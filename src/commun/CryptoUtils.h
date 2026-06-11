#pragma once
#include <QByteArray>
#include <QString>

class CryptoUtils
{
public:
	CryptoUtils();

	QByteArray encryptBytes(const QByteArray& data) const;
	QByteArray decryptBytes(const QByteArray& encrypt) const;

	static QByteArray loadMasterKey();
	static void saveMasterKey(const QByteArray& hash, const QByteArray& salt);

private:
	QByteArray deriveKey(const QByteArray& password, const QByteArray& salt) const;

	QByteArray password;
	const int keySize = 32; // 256 bits
	const int iterations = 100000;
};

