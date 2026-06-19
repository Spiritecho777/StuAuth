#pragma once

#include <QString>
#include <QStringList>
#include <QMap>

class AccountManager
{
public:
    AccountManager();

    QStringList readLines() const;
    QStringList getAccountsByFolder(const QString& folderName) const;
    QStringList getAllOtpSecrets() const;

    QMap<QString, int> countFolderOccurrences() const;
    QStringList getValidLines(const QMap<QString, int>& occurrences) const;

    void addFolder(const QString& folderName);
    void addAccount(const QString& account);

    bool deleteFolderOrAccount(const QString& name, bool isFolder, bool force = false);
    void renameFolderOrAccount(const QString& oldName, const QString& newName, bool isFolder);

    bool fileExists() const;

private:
    bool loadPlainText(QString& outText) const;
    bool savePlainText(const QString& text) const;
    void writeLines(const QStringList& lines) const;

    QString updateUri(const QString& uri, const QString& newName) const;

private:
    QString m_dataDir;
    QString m_encryptedPath;
};
