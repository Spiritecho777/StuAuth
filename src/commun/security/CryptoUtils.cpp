#include "CryptoUtils.h"

#include "RuntimeSecret.h"

#include <QCryptographicHash>
#include <QSettings>
#include <QFile>

#include <openssl/evp.h>
#include <openssl/rand.h>
#include <openssl/crypto.h>

namespace
{
    constexpr int SALT_SIZE = 16;
    constexpr int IV_SIZE = 12;   // recommandé pour GCM
    constexpr int TAG_SIZE = 16;

    void secureWipe(QByteArray& arr)
    {
        if (!arr.isEmpty())
            OPENSSL_cleanse(arr.data(), arr.size());
    }
}

QByteArray CryptoUtils::computeFallbackSecret()
{
    QString guid;

#ifdef _WIN32
    QSettings reg(
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Cryptography",
        QSettings::NativeFormat
    );
    guid = reg.value("MachineGuid").toString();
    if (guid.isEmpty())
        guid = "FallbackMachineGuid";
#elif defined(__linux__)
    QFile f("/etc/machine-id");
    if (f.open(QIODevice::ReadOnly | QIODevice::Text))
        guid = QString::fromUtf8(f.readAll()).trimmed();
    if (guid.isEmpty())
        guid = "FallbackMachineId";
#else
    guid = "FallbackGenericId";
#endif

    return QCryptographicHash::hash(guid.toUtf8(), QCryptographicHash::Sha256);
}

QByteArray CryptoUtils::currentSecret() const
{
    if (RuntimeSecret::instance().hasSecret())
        return RuntimeSecret::instance().secret();

    return computeFallbackSecret();
}

QByteArray CryptoUtils::deriveKey(const QByteArray& secret, const QByteArray& salt) const
{
    QByteArray key(m_keySize, 0);

    const int ok = PKCS5_PBKDF2_HMAC(
        secret.constData(),
        secret.size(),
        reinterpret_cast<const unsigned char*>(salt.constData()),
        salt.size(),
        m_iterations,
        EVP_sha256(),
        m_keySize,
        reinterpret_cast<unsigned char*>(key.data())
    );

    if (!ok) {
        secureWipe(key);
        return {};
    }

    return key;
}

QByteArray CryptoUtils::encryptBytes(const QByteArray& plain) const
{
    if (plain.isEmpty())
        return {};

    QByteArray salt(SALT_SIZE, 0);
    QByteArray iv(IV_SIZE, 0);

    if (RAND_bytes(reinterpret_cast<unsigned char*>(salt.data()), salt.size()) != 1)
        return {};

    if (RAND_bytes(reinterpret_cast<unsigned char*>(iv.data()), iv.size()) != 1)
        return {};

    QByteArray secret = currentSecret();
    QByteArray key = deriveKey(secret, salt);
    secureWipe(secret);

    if (key.isEmpty())
        return {};

    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    if (!ctx) {
        secureWipe(key);
        return {};
    }

    QByteArray ciphertext(plain.size(), 0);
    QByteArray tag(TAG_SIZE, 0);

    int len = 0;
    int ciphertextLen = 0;

    if (EVP_EncryptInit_ex(ctx, EVP_aes_256_gcm(), nullptr, nullptr, nullptr) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    if (EVP_CIPHER_CTX_ctrl(ctx, EVP_CTRL_GCM_SET_IVLEN, iv.size(), nullptr) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    if (EVP_EncryptInit_ex(
        ctx,
        nullptr,
        nullptr,
        reinterpret_cast<const unsigned char*>(key.constData()),
        reinterpret_cast<const unsigned char*>(iv.constData())) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    if (EVP_EncryptUpdate(
        ctx,
        reinterpret_cast<unsigned char*>(ciphertext.data()),
        &len,
        reinterpret_cast<const unsigned char*>(plain.constData()),
        plain.size()) != 1) {
        secureWipe(key);
        secureWipe(ciphertext);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    ciphertextLen = len;

    if (EVP_EncryptFinal_ex(
        ctx,
        reinterpret_cast<unsigned char*>(ciphertext.data()) + ciphertextLen,
        &len) != 1) {
        secureWipe(key);
        secureWipe(ciphertext);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    ciphertextLen += len;
    ciphertext.resize(ciphertextLen);

    if (EVP_CIPHER_CTX_ctrl(ctx, EVP_CTRL_GCM_GET_TAG, TAG_SIZE, tag.data()) != 1) {
        secureWipe(key);
        secureWipe(ciphertext);
        secureWipe(tag);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    EVP_CIPHER_CTX_free(ctx);
    secureWipe(key);

    QByteArray output;
    output.reserve(SALT_SIZE + IV_SIZE + TAG_SIZE + ciphertext.size());
    output.append(salt);
    output.append(iv);
    output.append(tag);
    output.append(ciphertext);

    return output;
}

QByteArray CryptoUtils::decryptBytes(const QByteArray& encrypted) const
{
    const int headerSize = SALT_SIZE + IV_SIZE + TAG_SIZE;
    if (encrypted.size() < headerSize)
        return {};

    int offset = 0;
    const QByteArray salt = encrypted.mid(offset, SALT_SIZE);
    offset += SALT_SIZE;

    const QByteArray iv = encrypted.mid(offset, IV_SIZE);
    offset += IV_SIZE;

    const QByteArray tag = encrypted.mid(offset, TAG_SIZE);
    offset += TAG_SIZE;

    const QByteArray ciphertext = encrypted.mid(offset);
    if (ciphertext.isEmpty())
        return {};

    QByteArray secret = currentSecret();
    QByteArray key = deriveKey(secret, salt);
    secureWipe(secret);

    if (key.isEmpty())
        return {};

    EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
    if (!ctx) {
        secureWipe(key);
        return {};
    }

    QByteArray plain(ciphertext.size(), 0);

    int len = 0;
    int plainLen = 0;

    if (EVP_DecryptInit_ex(ctx, EVP_aes_256_gcm(), nullptr, nullptr, nullptr) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    if (EVP_CIPHER_CTX_ctrl(ctx, EVP_CTRL_GCM_SET_IVLEN, iv.size(), nullptr) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    if (EVP_DecryptInit_ex(
        ctx,
        nullptr,
        nullptr,
        reinterpret_cast<const unsigned char*>(key.constData()),
        reinterpret_cast<const unsigned char*>(iv.constData())) != 1) {
        secureWipe(key);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    if (EVP_DecryptUpdate(
        ctx,
        reinterpret_cast<unsigned char*>(plain.data()),
        &len,
        reinterpret_cast<const unsigned char*>(ciphertext.constData()),
        ciphertext.size()) != 1) {
        secureWipe(key);
        secureWipe(plain);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    plainLen = len;

    if (EVP_CIPHER_CTX_ctrl(ctx, EVP_CTRL_GCM_SET_TAG, TAG_SIZE,
        const_cast<char*>(tag.constData())) != 1) {
        secureWipe(key);
        secureWipe(plain);
        EVP_CIPHER_CTX_free(ctx);
        return {};
    }

    const int finalOk = EVP_DecryptFinal_ex(
        ctx,
        reinterpret_cast<unsigned char*>(plain.data()) + plainLen,
        &len
    );

    EVP_CIPHER_CTX_free(ctx);
    secureWipe(key);

    if (finalOk != 1) {
        secureWipe(plain);
        return {};
    }

    plainLen += len;
    plain.resize(plainLen);
    return plain;
}

MasterPasswordData CryptoUtils::loadMasterPasswordData()
{
    QSettings s;
    MasterPasswordData data;
    data.hash = s.value("masterHash").toByteArray();
    data.salt = s.value("masterSalt").toByteArray();
    return data;
}

void CryptoUtils::saveMasterPasswordData(const MasterPasswordData& data)
{
    QSettings s;
    s.setValue("masterHash", data.hash);
    s.setValue("masterSalt", data.salt);
}

bool CryptoUtils::hasMasterPasswordConfigured()
{
    const MasterPasswordData data = loadMasterPasswordData();
    return !data.hash.isEmpty() && !data.salt.isEmpty();
}

bool CryptoUtils::verifyMasterPassword(const QString& password)
{
    if (password.isEmpty())
        return false;

    const MasterPasswordData data = loadMasterPasswordData();
    if (data.hash.isEmpty() || data.salt.isEmpty())
        return false;

    QByteArray derived(32, 0);

    const QByteArray pwdBytes = password.toUtf8();
    const int ok = PKCS5_PBKDF2_HMAC(
        pwdBytes.constData(),
        pwdBytes.size(),
        reinterpret_cast<const unsigned char*>(data.salt.constData()),
        data.salt.size(),
        100000,
        EVP_sha256(),
        32,
        reinterpret_cast<unsigned char*>(derived.data())
    );

    if (!ok) {
        secureWipe(derived);
        return false;
    }

    const bool match =
        derived.size() == data.hash.size() &&
        CRYPTO_memcmp(derived.constData(), data.hash.constData(), data.hash.size()) == 0;

    secureWipe(derived);
    return match;
}

bool CryptoUtils::setOrChangeMasterPassword(const QString& password)
{
    if (password.isEmpty())
        return false;

    MasterPasswordData data;
    data.salt = QByteArray(16, 0);
    data.hash = QByteArray(32, 0);

    if (RAND_bytes(reinterpret_cast<unsigned char*>(data.salt.data()), data.salt.size()) != 1)
        return false;

    const QByteArray pwdBytes = password.toUtf8();

    const int ok = PKCS5_PBKDF2_HMAC(
        pwdBytes.constData(),
        pwdBytes.size(),
        reinterpret_cast<const unsigned char*>(data.salt.constData()),
        data.salt.size(),
        100000,
        EVP_sha256(),
        32,
        reinterpret_cast<unsigned char*>(data.hash.data())
    );

    if (!ok) {
        secureWipe(data.hash);
        secureWipe(data.salt);
        return false;
    }

    saveMasterPasswordData(data);
    RuntimeSecret::instance().setSecret(pwdBytes);

    secureWipe(data.hash);
    secureWipe(data.salt);
    return true;
}

void CryptoUtils::disableMasterPassword()
{
    QSettings s;
    s.remove("masterHash");
    s.remove("masterSalt");
    RuntimeSecret::instance().clear();
}
