#pragma once

#include <QString>
#include <QStringList>
#include <QByteArray>
#include <QList>

// Représente un compte OTP extrait du QR de migration Google Authenticator
struct OtpEntry
{
    QByteArray secret;   // secret brut (bytes)
    QString    name;     // nom du compte
    QString    issuer;   // émetteur
    int        digits = 6;
    int        period = 30;

    // URI otpauth prête à l'emploi
    QString toOtpauthUri() const;
};

// ─────────────────────────────────────────────
//  ProtoParser
//
//  Parse le payload protobuf du QR de migration Google Authenticator
//  sans dépendance externe (wire format proto3 manuel).
//
//  Usage :
//    QByteArray raw = ...;   // bytes décodés du QR (après URL-decode + base64)
//    QList<OtpEntry> entries = ProtoParser::parse(raw);
// ─────────────────────────────────────────────
class ProtoParser
{
public:
    // Parse le payload binaire, retourne la liste des comptes trouvés
    static QList<OtpEntry> parse(const QByteArray& payload);

    // Extrait et décode le payload depuis une URI de migration complète
    // "otpauth-migration://offline?data=..."
    static QList<OtpEntry> parseFromMigrationUri(const QString& uri);

    // Encode un QByteArray en Base32 (pour reconstruire l'URI otpauth)
    static QString base32Encode(const QByteArray& data);

private:
    // Wire format proto3
    struct Field {
        quint32    number;  // numéro de champ
        int        type;    // 0=varint, 1=64bit, 2=length-delimited, 5=32bit
        quint64    varint;
        QByteArray bytes;
    };

    static QList<Field> decodeMessage(const QByteArray& data);
    static quint64      readVarint(const QByteArray& data, int& pos);
    static OtpEntry     parseOtpParameters(const QByteArray& data);
};