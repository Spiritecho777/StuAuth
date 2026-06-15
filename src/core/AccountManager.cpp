#include "AccountManager.h"
#include "../commun/CryptoUtils.h"

#include <QDir>
#include <QFile>
#include <QTextStream>
#include <QStandardPaths>
#include <QRegularExpression>
#include <QMessageBox>
#include <QDebug>
#include <QUrl>

// ─────────────────────────────────────────────
//  Format du fichier Account.dat (déchiffré)
//
//  Chaque ligne :  Dossier\NomCompte;otpauth://totp/...
//  Ligne dossier vide :  Dossier\;
// ─────────────────────────────────────────────

AccountManager::AccountManager()
{
    m_dataDir = QStandardPaths::writableLocation(QStandardPaths::AppDataLocation);
    QDir().mkpath(m_dataDir);

    m_encryptedPath = m_dataDir + "/Account.dat";
    m_plainPath = m_dataDir + "/Account_decrypted.dat";

    if (QFile::exists(m_encryptedPath))
    {
        ensureDecrypted();
    }
    else
    {
        createEmptyFiles();
        encrypt();
    }
}

// ─────────────────────────────────────────────
//  Lecture
// ─────────────────────────────────────────────

QStringList AccountManager::readLines() const
{
    ensureDecrypted();

    QFile f(m_plainPath);
    if (!f.open(QIODevice::ReadOnly | QIODevice::Text))
        return {};

    QStringList lines;
    QTextStream in(&f);
    while (!in.atEnd())
    {
        QString line = in.readLine();
        if (!line.isEmpty())
            lines << line;
    }
    return lines;
}

QStringList AccountManager::getAccountsByFolder(const QString& folderName) const
{
    QStringList result;
    for (const QString& line : readLines())
    {
        QStringList parts = line.split(';');
        if (parts.size() < 2) continue;

        QString folder = parts[0].split('\\').value(0);
        if (folder == folderName)
            result << line;
    }
    return result;
}

QStringList AccountManager::getAllOtpSecrets() const
{
    QStringList secrets;
    static QRegularExpression re("secret=([^&]+)");

    for (const QString& line : readLines())
    {
        QStringList parts = line.split(';');
        if (parts.size() < 2) continue;

        QRegularExpressionMatch m = re.match(parts[1]);
        if (m.hasMatch())
            secrets << m.captured(1);
    }
    return secrets;
}

QMap<QString, int> AccountManager::countFolderOccurrences() const
{
    QMap<QString, int> occ;
    for (const QString& line : readLines())
    {
        QStringList parts = line.split(';');
        if (parts.size() < 2) continue;

        QString folder = parts[0].split('\\').value(0);
        occ[folder]++;
    }
    return occ;
}

QStringList AccountManager::getValidLines(const QMap<QString, int>& occurrences) const
{
    QStringList valid;
    for (const QString& line : readLines())
    {
        QStringList parts = line.split(';');
        if (parts.size() < 2) continue;

        QString folder = parts[0].split('\\').value(0);
        bool hasAccount = !parts[1].isEmpty();

        if (hasAccount || occurrences.value(folder) == 1)
            valid << line;
    }
    return valid;
}

// ─────────────────────────────────────────────
//  Écriture
// ─────────────────────────────────────────────

void AccountManager::addFolder(const QString& folderName)
{
    if (folderName.isEmpty()) return;

    QStringList lines = readLines();
    lines << folderName + "\\;";
    writeLines(lines);
    encrypt();
}

void AccountManager::addAccount(const QString& account)
{
    QFile f(m_plainPath);
    if (!f.open(QIODevice::Append | QIODevice::Text))
        return;

    QTextStream out(&f);
    out << account << "\n";
    f.close();

    encrypt();
}

bool AccountManager::deleteFolderOrAccount(const QString& name, bool isFolder, bool force)
{
    QStringList lines = readLines();
    bool deleted = false;

    for (int i = lines.size() - 1; i >= 0; --i)
    {
        QStringList parts = lines[i].split(';');
        if (parts.size() < 2) continue;

        QStringList pathParts = parts[0].split('\\');

        if (isFolder)
        {
            if (pathParts.value(0) != name) continue;

            bool hasAccount = !parts[1].isEmpty();
            if (hasAccount && !force)
                return false;

            lines.removeAt(i);
            deleted = true;
        }
        else
        {
            if (pathParts.size() > 1 && pathParts.value(1) == name)
            {
                lines.removeAt(i);
                deleted = true;
            }
        }
    }

    if (deleted)
    {
        writeLines(lines);
        encrypt();
    }

    return deleted;
}

void AccountManager::renameFolderOrAccount(const QString& oldName, const QString& newName, bool isFolder)
{
    QStringList lines = readLines();

    for (int i = 0; i < lines.size(); ++i)
    {
        QStringList parts = lines[i].split(';');
        if (parts.size() < 2) continue;

        QStringList pathParts = parts[0].split('\\');

        if (isFolder)
        {
            if (pathParts.value(0) == oldName)
            {
                pathParts[0] = newName;
                parts[0] = pathParts.join('\\');
            }
        }
        else
        {
            if (pathParts.size() > 1 && pathParts.value(1) == oldName)
            {
                pathParts[1] = newName;
                parts[0] = pathParts.join('\\');
                parts[1] = updateUri(parts[1], newName);
            }
        }

        lines[i] = parts.join(';');
    }

    writeLines(lines);
    encrypt();
}

// ─────────────────────────────────────────────
//  Helpers privés
// ─────────────────────────────────────────────

void AccountManager::ensureDecrypted() const
{
    if (!QFile::exists(m_encryptedPath)) return;

    QFile enc(m_encryptedPath);
    if (!enc.open(QIODevice::ReadOnly)) return;
    QByteArray cipher = enc.readAll();
    enc.close();

    // Fichier vide = aucun compte enregistré, rien à déchiffrer
    if (cipher.isEmpty())
    {
        QFile out(m_plainPath);
        out.open(QIODevice::WriteOnly | QIODevice::Truncate);
        return;
    }

    CryptoUtils crypto;
    QByteArray plain = crypto.decryptBytes(cipher);

    if (plain.isEmpty())
    {
        QMessageBox::critical(nullptr,
            QObject::tr("Erreur de déchiffrement"),
            QObject::tr("Impossible de déchiffrer Account.dat.\n"
                "Le fichier est corrompu ou la clé est incorrecte."));
        return;
    }

    QFile out(m_plainPath);
    if (out.open(QIODevice::WriteOnly | QIODevice::Truncate))
    {
        out.write(plain);
        out.close();
    }
}

void AccountManager::encrypt() const
{
    QFile f(m_plainPath);
    if (!f.open(QIODevice::ReadOnly)) return;
    QByteArray plain = f.readAll();
    f.close();

    // Fichier vide (dernier dossier supprimé) : on tronque directement Account.dat
    if (plain.isEmpty())
    {
        QFile enc(m_encryptedPath);
        enc.open(QIODevice::WriteOnly | QIODevice::Truncate);
        return;
    }

    CryptoUtils crypto;
    QByteArray cipher = crypto.encryptBytes(plain);

    if (cipher.isEmpty()) return;

    QFile enc(m_encryptedPath);
    if (enc.open(QIODevice::WriteOnly | QIODevice::Truncate))
    {
        enc.write(cipher);
        enc.close();
    }
}

void AccountManager::createEmptyFiles() const
{
    QFile(m_encryptedPath).open(QIODevice::WriteOnly);
    QFile(m_plainPath).open(QIODevice::WriteOnly);
}

void AccountManager::writeLines(const QStringList& lines) const
{
    QFile f(m_plainPath);
    if (!f.open(QIODevice::WriteOnly | QIODevice::Truncate | QIODevice::Text))
        return;

    QTextStream out(&f);
    for (const QString& line : lines)
        out << line << "\n";
}

bool AccountManager::fileExists() const
{
    return QFile::exists(m_plainPath);
}

QString AccountManager::updateUri(const QString& uri, const QString& newName) const
{
    if (uri.isEmpty()) return uri;

    QStringList uriParts = uri.split('/');
    if (uriParts.size() < 4) return uri;

    QString last = uriParts.last();
    QStringList nameDetails = last.split('?');
    if (nameDetails.size() < 2) return uri;

    nameDetails[0] = QUrl::toPercentEncoding(newName);
    uriParts.last() = nameDetails.join('?');

    return uriParts.join('/');
}