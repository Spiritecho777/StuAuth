#include "CryptoUtils.h"

#include <QCryptographicHash>
#include <QRandomGenerator>
#include <QSysInfo>
#include <QStorageInfo>
#include <QSettings>
#include <openssl/evp.h>
#include <openssl/rand.h>
#include <openssl/crypto.h>

static QString computeMachineGuid();
static void secureWipe(QByteArray& arr);

CryptoUtils::CryptoUtils()
{
    QByteArray master = loadMasterKey();
    if (!master.isEmpty()) {
        password = master;
        return;
    }
    else 
    {
        QString guid = computeMachineGuid();
        password = guid.toUtf8();
    }
}

QByteArray CryptoUtils::deriveKey(const QByteArray& password, const QByteArray& salt) const
{
    QByteArray key;
    key.resize(keySize);

    try {
        int ok = PKCS5_PBKDF2_HMAC(
            password.constData(),
            password.size(),
            reinterpret_cast<const unsigned char*>(salt.constData()),
            salt.size(),
            iterations,
            EVP_sha256(),
            keySize,
            reinterpret_cast<unsigned char*>(key.data())
        );

        if (!ok) {
            secureWipe(key);
            return{};
        }
    }
    catch (...) {
        secureWipe(key);
        throw;
	}

    return key;
}

QByteArray CryptoUtils::encryptBytes(const QByteArray& plain) const
{
    if (plain.isEmpty())
        return {};

    QByteArray iv(16, 0);
    if (RAND_bytes(reinterpret_cast<unsigned char*>(iv.data()), iv.size()) != 1)
        return {};

    QByteArray key = deriveKey(password, iv);
    if (key.isEmpty())
        return {};

    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    if (!ctx) {
        secureWipe(key);
        return {};
    }

    QByteArray encrypted;
    encrypted.reserve(plain.size() + 32);
    encrypted.append(iv);

    if (EVP_EncryptInit_ex(ctx, EVP_aes_256_cbc(), nullptr,
        reinterpret_cast<const unsigned char*>(key.data()),
        reinterpret_cast<const unsigned char*>(iv.data())) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

	int block = EVP_CIPHER_CTX_block_size(ctx);
    QByteArray buffer;
    buffer.resize(plain.size() + block);

    int outLen = 0;

    if (EVP_EncryptUpdate(ctx,
        reinterpret_cast<unsigned char*>(buffer.data()),
        &outLen,
        reinterpret_cast<const unsigned char*>(plain.data()),
        plain.size()) != 1) 
    {
        secureWipe(key);
        secureWipe(buffer);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    encrypted.append(buffer.constData(), outLen);

    int finalLen = 0;
    if (EVP_EncryptFinal_ex(ctx,
        reinterpret_cast<unsigned char*>(buffer.data()),
        &finalLen) != 1) 
    {
        secureWipe(key);
        secureWipe(buffer);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    encrypted.append(buffer.constData(), finalLen);

    secureWipe(key);
    secureWipe(buffer);
    EVP_CIPHER_CTX_free(ctx);

    return encrypted;
}

QByteArray CryptoUtils::decryptBytes(const QByteArray& encrypted) const
{
    if (encrypted.size() < 16)
        return {};

    QByteArray iv = encrypted.left(16);
    QByteArray cipher = encrypted.mid(16);

    QByteArray key = deriveKey(password, iv);
    if (key.isEmpty())
        return {};

    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    if (!ctx) {
        secureWipe(key);
        return {};
    }

    QByteArray decrypted;
    decrypted.resize(cipher.size() + 32);

    if (EVP_DecryptInit_ex(ctx, EVP_aes_256_cbc(), nullptr,
        reinterpret_cast<const unsigned char*>(key.data()),
        reinterpret_cast<const unsigned char*>(iv.data())) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    int outLen = 0;
    if (EVP_DecryptUpdate(ctx,
        reinterpret_cast<unsigned char*>(decrypted.data()),
        &outLen,
        reinterpret_cast<const unsigned char*>(cipher.data()),
        cipher.size()) != 1) {
        secureWipe(key);
        secureWipe(decrypted);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    int finalLen = 0;
    if (EVP_DecryptFinal_ex(ctx,
        reinterpret_cast<unsigned char*>(decrypted.data() + outLen),
        &finalLen) != 1) {
        secureWipe(key);
        secureWipe(decrypted);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    decrypted.resize(outLen + finalLen);

    secureWipe(key);
    EVP_CIPHER_CTX_free(ctx);

    return decrypted;
}

static QString computeMachineGuid()
{
    QString guid;

#ifdef _WIN32
    // MachineGuid Windows
    QSettings reg("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Cryptography",
        QSettings::NativeFormat);
    guid = reg.value("MachineGuid").toString();
    if (guid.isEmpty())
        guid = "FallbackMachineGuid";
#elif defined(__linux__)
    // Linux machine-id
    QFile f("/etc/machine-id");
    if (f.open(QIODevice::ReadOnly | QIODevice::Text))
        guid = QString::fromUtf8(f.readAll()).trimmed();
    if (guid.isEmpty())
        guid = "FallbackMachineId";
#else
   guid = "FallbackGenericId";
#endif

    // SHA‑256 pour uniformiser et éviter d’exposer l’ID brut
    QByteArray hash = QCryptographicHash::hash(guid.toUtf8(), QCryptographicHash::Sha256);
    return hash.toHex().toUpper();
}

static void secureWipe(QByteArray& arr)
{
    if (!arr.isEmpty())
        OPENSSL_cleanse(arr.data(), arr.size());
}

QByteArray CryptoUtils::loadMasterKey()
{
    QSettings s;

    QByteArray hash = s.value("masterHash").toByteArray();
    QByteArray salt = s.value("masterSalt").toByteArray();

    if (hash.isEmpty() || salt.isEmpty())
        return {};

    return hash + salt;
}

void CryptoUtils::saveMasterKey(const QByteArray& hash, const QByteArray& salt)
{
    QSettings s;
    s.setValue("masterHash", hash);
    s.setValue("masterSalt", salt);
}