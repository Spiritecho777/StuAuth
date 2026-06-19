#include "RuntimeSecret.h"
#include <openssl/crypto.h>

static void secureWipe(QByteArray& arr)
{
    if (!arr.isEmpty())
        OPENSSL_cleanse(arr.data(), arr.size());
}

RuntimeSecret& RuntimeSecret::instance()
{
    static RuntimeSecret inst;
    return inst;
}

void RuntimeSecret::setSecret(const QByteArray& secret)
{
    clear();
    m_secret = QByteArray(secret.constData(), secret.size());
}

QByteArray RuntimeSecret::secret() const
{
    return QByteArray(m_secret.constData(), m_secret.size());
}

bool RuntimeSecret::hasSecret() const
{
    return !m_secret.isEmpty();
}

void RuntimeSecret::clear()
{
    secureWipe(m_secret);
    m_secret.clear();
}

RuntimeSecret::~RuntimeSecret()
{
    clear();
}