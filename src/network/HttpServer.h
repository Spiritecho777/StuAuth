#pragma once

#include <QObject>
#include <QThread>
#include <QTcpServer>
#include <QTcpSocket>
#include <QByteArray>
#include <QString>

class AccountManager;

// ─────────────────────────────────────────────
//  HttpServer
//
//  Serveur HTTP minimaliste sur le port 19755.
//  Répond à GET / avec un JSON :
//    { "Accounts": "...", "Folder": "..." }
//
//  Tourne dans un QThread dédié pour ne pas
//  bloquer l'UI.
// ─────────────────────────────────────────────
class HttpServer : public QObject
{
    Q_OBJECT

public:
    explicit HttpServer(AccountManager* accountManager, QObject* parent = nullptr);
    ~HttpServer();

    bool start(const QString& ip, quint16 port = 19755);
    void stop();
    bool isRunning() const;

signals:
    void started(const QString& ip, quint16 port);
    void stopped();
    void errorOccurred(const QString& message);

private slots:
    void onNewConnection();
    void onReadyRead();

private:
    QTcpServer* m_server = nullptr;
    QThread* m_thread = nullptr;
    AccountManager* m_accounts = nullptr;

    QString buildJsonResponse() const;
    QString getAccounts() const;
    QString getFolders() const;

    static QByteArray makeHttpResponse(const QByteArray& body);
};