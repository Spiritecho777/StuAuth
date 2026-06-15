#include "TotpEngine.h"

#include <QDateTime>
#include <QUrl>
#include <QUrlQuery>

#include <openssl/hmac.h>

// ─────────────────────────────────────────────
//  API publique
// ─────────────────────────────────────────────

QString TotpEngine::generate(const QString& base32Secret, int digits, int step)
{
    QByteArray key = base32Decode(base32Secret);
    if (key.isEmpty())
        return QString(digits, '0');

    // Compteur TOTP : secondes Unix / step, encodé big-endian sur 8 octets
    quint64 counter = static_cast<quint64>(QDateTime::currentSecsSinceEpoch()) / step;

    QByteArray msg(8, 0);
    for (int i = 7; i >= 0; --i)
    {
        msg[i] = static_cast<char>(counter & 0xFF);
        counter >>= 8;
    }

    QByteArray hash = hmacSha1(key, msg);
    if (hash.isEmpty())
        return QString(digits, '0');

    // Dynamic truncation (RFC 4226 §5.4)
    int offset = hash[hash.size() - 1] & 0x0F;
    quint32 code =
        ((static_cast<quint8>(hash[offset]) & 0x7F) << 24) |
        ((static_cast<quint8>(hash[offset + 1]) & 0xFF) << 16) |
        ((static_cast<quint8>(hash[offset + 2]) & 0xFF) << 8) |
        ((static_cast<quint8>(hash[offset + 3]) & 0xFF));

    quint32 mod = 1;
    for (int i = 0; i < digits; ++i) mod *= 10;

    return QString::number(code % mod).rightJustified(digits, '0');
}

int TotpEngine::secondsRemaining(int step)
{
    qint64 now = QDateTime::currentSecsSinceEpoch();
    return static_cast<int>(step - (now % step));
}

QString TotpEngine::extractSecret(const QString& otpauthUri)
{
    // QUrl ne parse pas bien le scheme otpauth://
    // On extrait la query string manuellement
    int qmark = otpauthUri.indexOf('?');
    if (qmark == -1) return {};

    QUrlQuery query(otpauthUri.mid(qmark + 1));
    return query.queryItemValue("secret").toUpper().remove('=');
}

// ─────────────────────────────────────────────
//  Base32 decode (RFC 4648)
// ─────────────────────────────────────────────

QByteArray TotpEngine::base32Decode(const QString& input)
{
    // Alphabet Base32 standard (majuscules)
    static const char* alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    // Nettoie : majuscules, supprime padding et espaces
    QString clean = input.toUpper().remove('=').remove(' ').remove('-');

    QByteArray out;
    int buffer = 0;
    int bitsLeft = 0;

    for (QChar c : clean)
    {
        const char* pos = strchr(alphabet, c.toLatin1());
        if (!pos) return {}; // caractère invalide

        int val = static_cast<int>(pos - alphabet);
        buffer = (buffer << 5) | val;
        bitsLeft += 5;

        if (bitsLeft >= 8)
        {
            bitsLeft -= 8;
            out.append(static_cast<char>((buffer >> bitsLeft) & 0xFF));
        }
    }

    return out;
}

// ─────────────────────────────────────────────
//  HMAC-SHA1 via OpenSSL
// ─────────────────────────────────────────────

QByteArray TotpEngine::hmacSha1(const QByteArray& key, const QByteArray& data)
{
    unsigned char result[EVP_MAX_MD_SIZE];
    unsigned int  resultLen = 0;

    unsigned char* ok = HMAC(
        EVP_sha1(),
        reinterpret_cast<const unsigned char*>(key.constData()), key.size(),
        reinterpret_cast<const unsigned char*>(data.constData()), data.size(),
        result, &resultLen
    );

    if (!ok) return {};

    return QByteArray(reinterpret_cast<char*>(result), static_cast<int>(resultLen));
}