#pragma once

#include <QByteArray>
#include <QString>

struct MasterPasswordData
{
    QByteArray hash;
    QByteArray salt;
};

class CryptoUtils
{
public:
    CryptoUtils() = default;

    QByteArray encryptBytes(const QByteArray& plain) const;
    QByteArray decryptBytes(const QByteArray& encrypted) const;

    static bool hasMasterPasswordConfigured();
    static bool verifyMasterPassword(const QString& password);
    static bool setOrChangeMasterPassword(const QString& password);
    static void disableMasterPassword();

private:
    QByteArray deriveKey(const QByteArray& secret, const QByteArray& salt) const;
    QByteArray currentSecret() const;

    static QByteArray computeFallbackSecret();
    static MasterPasswordData loadMasterPasswordData();
    static void saveMasterPasswordData(const MasterPasswordData& data);

private:
    const int m_keySize = 32;       // 256 bits
    const int m_iterations = 100000;
};