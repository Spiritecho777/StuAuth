#pragma once

#include <QByteArray>

class RuntimeSecret
{
public:
    static RuntimeSecret& instance();

    void setSecret(const QByteArray& secret);
    QByteArray secret() const;
    bool hasSecret() const;
    void clear();

private:
    RuntimeSecret() = default;
    ~RuntimeSecret();

    RuntimeSecret(const RuntimeSecret&) = delete;
    RuntimeSecret& operator=(const RuntimeSecret&) = delete;

    QByteArray m_secret;
};