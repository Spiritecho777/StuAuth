#pragma once

#include <QString>
#include <QStringList>
#include <QMap>

class AccountManager
{
public:
	AccountManager();

	// Lecture
	QStringList readLines() const;
	QStringList getAccountsByFolder(const QString& folderName) const;
	QStringList getAllOtpSecrets() const;
	QMap<QString, int> countFolderOccurrences() const;
	QStringList getValidLines(const QMap<QString, int>& occurrences) const;

	// Écriture
	void addFolder(const QString& folderName);
	void addAccount(const QString& line);
	bool deleteFolderOrAccount(const QString& name, bool isFolder, bool force = false);
	void renameFolderOrAccount(const QString& oldName, const QString& newName, bool isFolder);

private:
	QString m_dataDir;
	QString m_encryptedPath;
	QString m_plainPath;

	// Helpers
	void ensureDecrypted() const;
	void encrypt() const;
	void createEmptyFiles() const;
	void writeLines(const QStringList& lines) const;
	bool fileExists() const;
	QString updateUri(const QString& uri, const QString& newName) const;
};