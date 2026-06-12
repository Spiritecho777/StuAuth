#pragma once

#include <QString>
#include <QByteArray>

class TotpEngine
{
public:
    // Génère le code TOTP actuel (6 chiffres) à partir d'un secret Base32
    static QString generate(const QString& base32Secret, int digits = 6, int step = 30);

    // Secondes restantes avant le prochain code
    static int secondsRemaining(int step = 30);

    // Extrait le secret Base32 d'une URI otpauth://totp/...?secret=XXX&...
    static QString extractSecret(const QString& otpauthUri);

private:
    static QByteArray base32Decode(const QString& input);
    static QByteArray hmacSha1(const QByteArray& key, const QByteArray& data);
};