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

    // QTcpServer doit être créé dans son thread, pas ici
    connect(m_thread, &QThread::started, this, [this, ip, port]()
        {
            m_server = new QTcpServer();  // créé dans le bon thread

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
                Qt::QueuedConnection);

            emit started(ip, port);
        }, Qt::QueuedConnection);

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
    // Format : chaque entrée = "Dossier|otpauth://..."
    // On ignore les lignes sans URI (dossiers vides)
    QStringList lines = m_accounts->readLines();
    QStringList accounts;
    QStringList folders;

    for (const QString& line : lines)
    {
        int sep = line.indexOf(';');
        if (sep == -1) continue;

        QString path = line.left(sep);   // "Dossier\NomCompte"
        QString uri = line.mid(sep + 1).trimmed();
        if (uri.isEmpty()) continue;     // ignore dossiers vides

        QString folder = path.split('\\').value(0);
        accounts << uri;
        folders << folder;
    }

    QJsonObject obj;
    obj["Accounts"] = accounts.join('\n');
    obj["Folder"] = folders.join('\n');

    return QString::fromUtf8(QJsonDocument(obj).toJson(QJsonDocument::Compact));
}

QString HttpServer::getAccounts() const { return {}; }
QString HttpServer::getFolders()  const { return {}; }

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