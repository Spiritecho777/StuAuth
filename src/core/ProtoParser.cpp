#include "ProtoParser.h"

#include <QUrl>
#include <QUrlQuery>

// ─────────────────────────────────────────────
//  OtpEntry
// ─────────────────────────────────────────────

QString OtpEntry::toOtpauthUri() const
{
    QString secret = ProtoParser::base32Encode(this->secret);
    QString label = name.isEmpty() ? "Unknown" : name;

    QString uri = "otpauth://totp/" + QUrl::toPercentEncoding(label)
        + "?secret=" + secret
        + "&digits=" + QString::number(digits)
        + "&period=" + QString::number(period);

    if (!issuer.isEmpty())
        uri += "&issuer=" + QUrl::toPercentEncoding(issuer);

    return uri;
}

// ─────────────────────────────────────────────
//  API publique
// ─────────────────────────────────────────────

QList<OtpEntry> ProtoParser::parseFromMigrationUri(const QString& uri)
{
    // "otpauth-migration://offline?data=BASE64_URL_ENCODED"
    QUrl    url(uri);
    QString encoded = QUrlQuery(url.query()).queryItemValue("data");

    if (encoded.isEmpty())
        return {};

    // URL-decode puis base64-decode
    QString urlDecoded = QUrl::fromPercentEncoding(encoded.toUtf8());
    QByteArray raw = QByteArray::fromBase64(urlDecoded.toUtf8());

    if (raw.isEmpty())
        return {};

    return parse(raw);
}

QList<OtpEntry> ProtoParser::parse(const QByteArray& payload)
{
    QList<OtpEntry> result;

    // Le Payload a un seul champ repeated : otp_parameters (field 1, wire type 2)
    QList<Field> fields = decodeMessage(payload);

    for (const Field& f : fields)
    {
        // field 1 = otp_parameters (length-delimited = embedded message)
        if (f.number == 1 && f.type == 2)
        {
            OtpEntry entry = parseOtpParameters(f.bytes);
            if (!entry.secret.isEmpty())
                result << entry;
        }
        // fields 2-5 (version, batch_size, batch_index, batch_id) : ignorés
    }

    return result;
}

// ─────────────────────────────────────────────
//  Parse un OtpParameters embedded message
//
//  Champs proto :
//    1 = secret    (bytes)
//    2 = name      (string)
//    3 = issuer    (string)
//    4 = algorithm (varint) — ignoré, on force SHA1
//    5 = digits    (varint) : 1=6, 2=8
//    6 = type      (varint) — ignoré, on gère TOTP uniquement
//    7 = counter   (varint) — ignoré
// ─────────────────────────────────────────────

OtpEntry ProtoParser::parseOtpParameters(const QByteArray& data)
{
    OtpEntry entry;
    entry.digits = 6;
    entry.period = 30;

    QList<Field> fields = decodeMessage(data);

    for (const Field& f : fields)
    {
        switch (f.number)
        {
        case 1: entry.secret = f.bytes;                                       break;
        case 2: entry.name = QString::fromUtf8(f.bytes);                   break;
        case 3: entry.issuer = QString::fromUtf8(f.bytes);                   break;
        case 5: entry.digits = (f.varint == 2) ? 8 : 6;                      break;
        default: break;
        }
    }

    return entry;
}

// ─────────────────────────────────────────────
//  Décodeur wire format proto3
//
//  Wire types :
//    0 = varint
//    1 = 64-bit (fixed64, sfixed64, double)
//    2 = length-delimited (string, bytes, embedded messages)
//    5 = 32-bit (fixed32, sfixed32, float)
// ─────────────────────────────────────────────

QList<ProtoParser::Field> ProtoParser::decodeMessage(const QByteArray& data)
{
    QList<Field> fields;
    int pos = 0;
    int len = data.size();

    while (pos < len)
    {
        // Tag = (field_number << 3) | wire_type
        quint64 tag = readVarint(data, pos);
        if (pos > len) break;

        Field f;
        f.number = static_cast<quint32>(tag >> 3);
        f.type = static_cast<int>(tag & 0x07);

        switch (f.type)
        {
        case 0: // varint
            f.varint = readVarint(data, pos);
            break;

        case 1: // 64-bit little-endian
            if (pos + 8 > len) goto done;
            f.varint = 0;
            for (int i = 0; i < 8; ++i)
                f.varint |= (static_cast<quint64>(static_cast<quint8>(data[pos + i])) << (8 * i));
            pos += 8;
            break;

        case 2: // length-delimited
        {
            quint64 sz = readVarint(data, pos);
            if (pos + static_cast<int>(sz) > len) goto done;
            f.bytes = data.mid(pos, static_cast<int>(sz));
            pos += static_cast<int>(sz);
            break;
        }

        case 5: // 32-bit little-endian
            if (pos + 4 > len) goto done;
            f.varint = 0;
            for (int i = 0; i < 4; ++i)
                f.varint |= (static_cast<quint64>(static_cast<quint8>(data[pos + i])) << (8 * i));
            pos += 4;
            break;

        default: // wire type inconnu, on abandonne
            goto done;
        }

        fields << f;
    }

done:
    return fields;
}

quint64 ProtoParser::readVarint(const QByteArray& data, int& pos)
{
    quint64 result = 0;
    int     shift = 0;

    while (pos < data.size())
    {
        quint8 b = static_cast<quint8>(data[pos++]);
        result |= (static_cast<quint64>(b & 0x7F) << shift);
        if (!(b & 0x80)) break;  // MSB à 0 = dernier octet du varint
        shift += 7;
        if (shift >= 64) break;  // protection débordement
    }

    return result;
}

// ─────────────────────────────────────────────
//  Base32 encode (RFC 4648, sans padding)
// ─────────────────────────────────────────────

QString ProtoParser::base32Encode(const QByteArray& data)
{
    static const char* alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    QString result;
    int buffer = 0;
    int bitsLeft = 0;

    for (int i = 0; i < data.size(); ++i)
    {
        buffer = (buffer << 8) | static_cast<quint8>(data[i]);
        bitsLeft += 8;

        while (bitsLeft >= 5)
        {
            bitsLeft -= 5;
            result += alpha[(buffer >> bitsLeft) & 0x1F];
        }
    }

    // Bits restants
    if (bitsLeft > 0)
    {
        buffer <<= (5 - bitsLeft);
        result += alpha[buffer & 0x1F];
    }

    return result;
}