#include "AccountManager.h"
#include "../commun/security/CryptoUtils.h"

#include <QDir>
#include <QFile>
#include <QStandardPaths>
#include <QRegularExpression>
#include <QMessageBox>
#include <QUrl>

namespace
{
    struct AccountEntry
    {
        QString rawLine;
        QString fullName; // ex: "Dossier\Compte" ou "Dossier\"
        QString uri;
        bool valid = false;

        QString folder() const
        {
            return fullName.section('\\', 0, 0);
        }

        QString accountName() const
        {
            return fullName.section('\\', 1, 1);
        }

        bool hasAccount() const
        {
            return !uri.isEmpty();
        }
    };

    AccountEntry parseLine(const QString& line)
    {
        AccountEntry entry;
        entry.rawLine = line;

        const int sep = line.indexOf(';');
        if (sep < 0)
            return entry;

        entry.fullName = line.left(sep).trimmed();
        entry.uri = line.mid(sep + 1).trimmed();

        if (entry.fullName.isEmpty())
            return entry;

        entry.valid = true;
        return entry;
    }
}

AccountManager::AccountManager()
{
    m_dataDir = QStandardPaths::writableLocation(QStandardPaths::AppDataLocation);
    QDir().mkpath(m_dataDir);

    m_encryptedPath = m_dataDir + "/Account.dat";

    if (!QFile::exists(m_encryptedPath)) {
        QFile file(m_encryptedPath);
        file.open(QIODevice::WriteOnly);
        file.close();
    }
}

QStringList AccountManager::readLines() const
{
    QString text;
    if (!loadPlainText(text))
        return {};

    QStringList result;
    const QStringList lines = text.split('\n', Qt::SkipEmptyParts);

    for (const QString& line : lines) {
        const QString trimmed = line.trimmed();
        if (!trimmed.isEmpty())
            result << trimmed;
    }

    return result;
}

QStringList AccountManager::getAccountsByFolder(const QString& folderName) const
{
    QStringList result;

    const QStringList lines = readLines();
    for (const QString& line : lines) {
        const AccountEntry entry = parseLine(line);
        if (!entry.valid)
            continue;

        if (entry.folder() == folderName)
            result << line;
    }

    return result;
}

QStringList AccountManager::getAllOtpSecrets() const
{
    QStringList secrets;
    static const QRegularExpression re("secret=([^&]+)");

    const QStringList lines = readLines();
    for (const QString& line : lines) {
        const AccountEntry entry = parseLine(line);
        if (!entry.valid || !entry.hasAccount())
            continue;

        const QRegularExpressionMatch match = re.match(entry.uri);
        if (match.hasMatch())
            secrets << match.captured(1);
    }

    return secrets;
}

QMap<QString, int> AccountManager::countFolderOccurrences() const
{
    QMap<QString, int> occurrences;

    const QStringList lines = readLines();
    for (const QString& line : lines) {
        const AccountEntry entry = parseLine(line);
        if (!entry.valid)
            continue;

        occurrences[entry.folder()]++;
    }

    return occurrences;
}

QStringList AccountManager::getValidLines(const QMap<QString, int>& occurrences) const
{
    QStringList valid;

    const QStringList lines = readLines();
    for (const QString& line : lines) {
        const AccountEntry entry = parseLine(line);
        if (!entry.valid)
            continue;

        if (entry.hasAccount() || occurrences.value(entry.folder()) == 1)
            valid << line;
    }

    return valid;
}

void AccountManager::addFolder(const QString& folderName)
{
    const QString cleanFolder = folderName.trimmed();
    if (cleanFolder.isEmpty())
        return;

    QStringList lines = readLines();

    bool alreadyExists = false;
    for (const QString& line : lines) {
        const AccountEntry entry = parseLine(line);
        if (entry.valid && entry.folder() == cleanFolder) {
            alreadyExists = true;
            break;
        }
    }

    if (!alreadyExists) {
        lines << (cleanFolder + "\\;");
        writeLines(lines);
    }
}

void AccountManager::addAccount(const QString& account)
{
    const QString cleanAccount = account.trimmed();
    if (cleanAccount.isEmpty())
        return;

    QStringList lines = readLines();
    lines << cleanAccount;
    writeLines(lines);
}

bool AccountManager::deleteFolderOrAccount(const QString& name, bool isFolder, bool force)
{
    QStringList lines = readLines();
    bool deleted = false;

    for (int i = lines.size() - 1; i >= 0; --i) {
        const AccountEntry entry = parseLine(lines[i]);
        if (!entry.valid)
            continue;

        if (isFolder) {
            if (entry.folder() != name)
                continue;

            if (entry.hasAccount() && !force)
                return false;

            lines.removeAt(i);
            deleted = true;
        }
        else {
            if (entry.accountName() == name) {
                lines.removeAt(i);
                deleted = true;
            }
        }
    }

    if (deleted)
        writeLines(lines);

    return deleted;
}

void AccountManager::renameFolderOrAccount(const QString& oldName, const QString& newName, bool isFolder)
{
    const QString cleanNewName = newName.trimmed();
    if (cleanNewName.isEmpty())
        return;

    QStringList lines = readLines();

    for (int i = 0; i < lines.size(); ++i) {
        AccountEntry entry = parseLine(lines[i]);
        if (!entry.valid)
            continue;

        QStringList pathParts = entry.fullName.split('\\');

        if (isFolder) {
            if (!pathParts.isEmpty() && pathParts[0] == oldName) {
                pathParts[0] = cleanNewName;
                entry.fullName = pathParts.join('\\');
            }
        }
        else {
            if (pathParts.size() > 1 && pathParts[1] == oldName) {
                pathParts[1] = cleanNewName;
                entry.fullName = pathParts.join('\\');

                if (!entry.uri.isEmpty())
                    entry.uri = updateUri(entry.uri, cleanNewName);
            }
        }

        lines[i] = entry.fullName + ";" + entry.uri;
    }

    writeLines(lines);
}

bool AccountManager::fileExists() const
{
    return QFile::exists(m_encryptedPath);
}

bool AccountManager::loadPlainText(QString& outText) const
{
    outText.clear();

    QFile file(m_encryptedPath);
    if (!file.open(QIODevice::ReadOnly))
        return false;

    const QByteArray encrypted = file.readAll();
    file.close();

    if (encrypted.isEmpty()) {
        outText.clear();
        return true;
    }

    CryptoUtils crypto;
    const QByteArray plain = crypto.decryptBytes(encrypted);

    if (plain.isEmpty()) {
        QMessageBox::critical(
            nullptr,
            QObject::tr("Erreur de déchiffrement"),
            QObject::tr("Impossible de déchiffrer Account.dat.\n"
                "Le fichier est corrompu ou la clé est incorrecte.")
        );
        return false;
    }

    outText = QString::fromUtf8(plain);
    return true;
}

bool AccountManager::savePlainText(const QString& text) const
{
    QFile file(m_encryptedPath);

    if (text.isEmpty()) {
        if (!file.open(QIODevice::WriteOnly | QIODevice::Truncate))
            return false;

        file.close();
        return true;
    }

    CryptoUtils crypto;
    const QByteArray cipher = crypto.encryptBytes(text.toUtf8());
    if (cipher.isEmpty())
        return false;

    if (!file.open(QIODevice::WriteOnly | QIODevice::Truncate))
        return false;

    const qint64 written = file.write(cipher);
    file.close();

    return written == cipher.size();
}

void AccountManager::writeLines(const QStringList& lines) const
{
    QString text;

    for (const QString& line : lines) {
        const QString trimmed = line.trimmed();
        if (!trimmed.isEmpty())
            text += trimmed + "\n";
    }

    savePlainText(text);
}

QString AccountManager::updateUri(const QString& uri, const QString& newName) const
{
    if (uri.isEmpty())
        return uri;

    QStringList uriParts = uri.split('/');
    if (uriParts.size() < 4)
        return uri;

    QString last = uriParts.last();
    QStringList nameDetails = last.split('?');
    if (nameDetails.size() < 2)
        return uri;

    nameDetails[0] = QUrl::toPercentEncoding(newName);
    uriParts.last() = nameDetails.join('?');

    return uriParts.join('/');
}

void AccountManager::rewriteWithCurrentKey(const QStringList& lines)
{
    if (lines.isEmpty() && fileExists())
    {
        qDebug() << "Refuse overwrite empty data";
        return;
    }

    writeLines(lines);
}
