#include "HttpServer.h"
#include "../core/AccountManager.h"

#include <QTcpSocket>
#include <QJsonObject>
#include <QJsonDocument>
#include <QHostAddress>
#include <QThread>

// ─────────────────────────────────────────────
//  Constructeur / Destructeur
// ─────────────────────────────────────────────

HttpServer::HttpServer(AccountManager* accountManager, QObject* parent)
    : QObject(parent)
    , m_accounts(accountManager)
{}

HttpServer::~HttpServer()
{
    stop();
}

// ─────────────────────────────────────────────
//  Start / Stop
// ─────────────────────────────────────────────

bool HttpServer::start(const QString& ip, quint16 port)
{
    if (m_server && m_server->isListening())
        return true;

    // Le serveur tourne dans un thread dédié
    m_thread = new QThread(this);
    m_server = new QTcpServer();      // pas de parent : on le déplace dans le thread

    m_server->moveToThread(m_thread);

    connect(m_thread, &QThread::started, this, [this, ip, port]()
        {
            QHostAddress addr = (ip.isEmpty() || ip == "0.0.0.0")
                ? QHostAddress::AnyIPv4
                : QHostAddress(ip);

            if (!m_server->listen(addr, port))
            {
                emit errorOccurred(tr("Impossible de démarrer le serveur : %1")
                    .arg(m_server->errorString()));
                return;
            }

            connect(m_server, &QTcpServer::newConnection,
                this, &HttpServer::onNewConnection,
                Qt::DirectConnection);

            emit started(ip, port);
        });

    m_running = true;
    m_thread->start();
    return true;
}

void HttpServer::stop()
{
    m_running = false;
    if (m_server)
    {
        m_server->close();
        m_server->deleteLater();
        m_server = nullptr;
    }

    if (m_thread)
    {
        m_thread->quit();
        m_thread->wait(2000);
        m_thread = nullptr;
    }

    emit stopped();
}

bool HttpServer::isRunning() const
{
    return m_running;
}

// ─────────────────────────────────────────────
//  Slots réseau
// ─────────────────────────────────────────────

void HttpServer::onNewConnection()
{
    while (m_server->hasPendingConnections())
    {
        QTcpSocket* socket = m_server->nextPendingConnection();

        connect(socket, &QTcpSocket::readyRead,
            this, &HttpServer::onReadyRead,
            Qt::DirectConnection);

        // Nettoyage automatique quand la connexion se ferme
        connect(socket, &QTcpSocket::disconnected,
            socket, &QTcpSocket::deleteLater,
            Qt::DirectConnection);
    }
}

void HttpServer::onReadyRead()
{
    QTcpSocket* socket = qobject_cast<QTcpSocket*>(sender());
    if (!socket) return;

    QByteArray request = socket->readAll();

    // On répond à n'importe quel GET — le serveur C# ne vérifiait pas non plus
    if (request.startsWith("GET"))
    {
        QByteArray body = buildJsonResponse().toUtf8();
        socket->write(makeHttpResponse(body));
    }

    socket->disconnectFromHost();
}

// ─────────────────────────────────────────────
//  Construction de la réponse JSON
//  { "Accounts": "otpauth://...\n...", "Folder": "Dossier1\n..." }
// ─────────────────────────────────────────────

QString HttpServer::buildJsonResponse() const
{
    QJsonObject obj;
    obj["Accounts"] = getAccounts();
    obj["Folder"] = getFolders();

    return QString::fromUtf8(QJsonDocument(obj).toJson(QJsonDocument::Compact));
}

QString HttpServer::getAccounts() const
{
    // Retourne la partie après ";" de chaque ligne (l'URI otpauth)
    QStringList lines = m_accounts->readLines();
    QStringList results;

    for (const QString& line : lines)
    {
        int idx = line.indexOf(';');
        if (idx != -1)
            results << line.mid(idx + 1).trimmed();
    }

    return results.join('\n');
}

QString HttpServer::getFolders() const
{
    // Retourne la partie avant "\" de chaque ligne (le nom du dossier)
    QStringList lines = m_accounts->readLines();
    QStringList results;

    for (const QString& line : lines)
    {
        int idx = line.indexOf('\\');
        if (idx != -1)
            results << line.left(idx).trimmed();
        else
            results << line.trimmed();
    }

    return results.join('\n');
}

// ─────────────────────────────────────────────
//  Réponse HTTP/1.1 minimale avec CORS
// ─────────────────────────────────────────────

QByteArray HttpServer::makeHttpResponse(const QByteArray& body)
{
    QByteArray response;
    response += "HTTP/1.1 200 OK\r\n";
    response += "Content-Type: application/json; charset=utf-8\r\n";
    response += "Access-Control-Allow-Origin: *\r\n";
    response += "Access-Control-Allow-Methods: GET\r\n";
    response += "Access-Control-Allow-Headers: *\r\n";
    response += "Content-Length: " + QByteArray::number(body.size()) + "\r\n";
    response += "Connection: close\r\n";
    response += "\r\n";
    response += body;
    return response;
}