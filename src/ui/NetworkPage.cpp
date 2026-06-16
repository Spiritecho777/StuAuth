#include "NetworkPage.h"
#include "../core/AccountManager.h"
#include "../network/HttpServer.h"
#include "../commun/TranslationManager.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QGridLayout>
#include <QMessageBox>
#include <QSettings>
#include <QNetworkReply>
#include <QJsonDocument>
#include <QJsonObject>
#include <QTcpSocket>
#include <QRegularExpression>
#include <QProcess>
#include <QThread>
#include <QDebug>

// ─────────────────────────────────────────────
//  Worker de scan (classe séparée, pas de lambda)
// ─────────────────────────────────────────────

class ScanWorker : public QObject
{
    Q_OBJECT
public:
    explicit ScanWorker(const QString& subnet) : m_subnet(subnet) {}

signals:
    void finished(QStringList found);

public slots:
    void run()
    {
        QStringList found;

        // ── ARP d'abord : rapide et fiable sur le LAN ──
#ifdef _WIN32
        QStringList arpArgs = { "-a" };
#else
        QStringList arpArgs = { "-n" };
#endif
        QProcess arp;
        arp.start("arp", arpArgs);
        arp.waitForFinished(3000);
        QString arpOut = QString::fromLocal8Bit(arp.readAllStandardOutput());

        static QRegularExpression arpRe(R"((\d+\.\d+\.\d+\.\d+))");
        QRegularExpressionMatchIterator it = arpRe.globalMatch(arpOut);
        while (it.hasNext())
        {
            QString ip = it.next().captured(1);
            if (ip.startsWith(m_subnet + ".") && !found.contains(ip))
                found << ip;
        }

        // ── Ping parallèle sur les IP manquantes ────────
        // Lance tous les process simultanément
        QList<QPair<QString, QProcess*>> procs;
        for (int i = 1; i < 255; ++i)
        {
            QString ip = m_subnet + "." + QString::number(i);
            if (found.contains(ip)) continue;

            QProcess* p = new QProcess();
#ifdef _WIN32
            p->start("ping", { "-n", "1", "-w", "100", ip });
#else
            p->start("ping", { "-c", "1", "-W", "1", ip });
#endif
            procs << qMakePair(ip, p);
        }

        // Attend tous les résultats
        for (auto& pair : procs)
        {
            pair.second->waitForFinished(2000);
            if (pair.second->exitCode() == 0 && !found.contains(pair.first))
                found << pair.first;
            delete pair.second;
        }

        emit finished(found);
    }

private:
    QString m_subnet;
};

#include "NetworkPage.moc"

// ─────────────────────────────────────────────
//  Constructeur
// ─────────────────────────────────────────────

NetworkPage::NetworkPage(AccountManager* am, HttpServer* server, QWidget* parent)
    : QWidget(parent)
    , m_am(am)
    , m_server(server)
    , m_nam(new QNetworkAccessManager(this))
{
    buildUi();
    loadNetworkInterfaces();

    QSettings s;
    m_appIp = s.value("IPApplication").toString();
    m_comboAppIp->setCurrentText(m_appIp);

    int ifaceIdx = s.value("InterfaceSelect", 0).toInt();
    if (ifaceIdx < m_comboIface->count())
        m_comboIface->setCurrentIndex(ifaceIdx);

    onInterfaceChanged(m_comboIface->currentIndex());
    updateServerButton();

    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &NetworkPage::retranslateUi);
}

// ─────────────────────────────────────────────
//  UI
// ─────────────────────────────────────────────

void NetworkPage::buildUi()
{
    m_lblApp = new QLabel(tr("IP de l'application distante :"));
    m_editSubnet = new QLineEdit();
    m_editSubnet->setPlaceholderText(tr("ex: 192.168.1  → Entrée pour scanner"));
    m_comboAppIp = new QComboBox();
    m_comboAppIp->setEditable(true);

    m_lblServer = new QLabel(tr("Serveur :"));
    m_lblServerIp = new QLabel(tr("—"));
    m_comboIface = new QComboBox();

    m_btnScan = new QPushButton(tr("Scanner"));
    m_btnServer = new QPushButton(tr("Serveur"));
    m_btnSync = new QPushButton(tr("Synchroniser"));
    m_btnBack = new QPushButton(tr("Retour"));
    m_progress = new QProgressBar();
    m_progress->setRange(0, 100);
    m_progress->setVisible(false);
    m_progress->setTextVisible(false);

    QGridLayout* grid = new QGridLayout();
    grid->addWidget(m_lblApp, 0, 0, 1, 2);
    grid->addWidget(m_editSubnet, 1, 0, 1, 2);
    grid->addWidget(m_comboAppIp, 2, 0, 1, 2);
    grid->addWidget(m_lblServer, 3, 0);
    grid->addWidget(m_lblServerIp, 4, 0);
    grid->addWidget(m_comboIface, 4, 1);

    QHBoxLayout* btnRow = new QHBoxLayout();
    btnRow->addWidget(m_btnScan);
    btnRow->addWidget(m_btnServer);
    btnRow->addWidget(m_btnSync);
    btnRow->addStretch();
    btnRow->addWidget(m_btnBack);

    QVBoxLayout* layout = new QVBoxLayout(this);
    layout->setContentsMargins(10, 10, 10, 10);
    layout->addLayout(grid);
    layout->addStretch();
    layout->addWidget(m_progress);
    layout->addLayout(btnRow);

    connect(m_btnServer, &QPushButton::clicked, this, &NetworkPage::onServerToggle);
    connect(m_btnSync, &QPushButton::clicked, this, &NetworkPage::onSyncClicked);
    connect(m_btnBack, &QPushButton::clicked, this, &NetworkPage::navigateBack);
    connect(m_btnScan, &QPushButton::clicked, this, &NetworkPage::onScanSubnet);
    connect(m_editSubnet, &QLineEdit::returnPressed, this, &NetworkPage::onScanSubnet);
    connect(m_comboIface, QOverload<int>::of(&QComboBox::currentIndexChanged),
        this, &NetworkPage::onInterfaceChanged);
    connect(m_comboAppIp, &QComboBox::currentTextChanged, this, [this](const QString& ip) {
        m_appIp = ip;
        QSettings s;
        s.setValue("IPApplication", ip);
        });
}

// ─────────────────────────────────────────────
//  Interfaces réseau
// ─────────────────────────────────────────────

void NetworkPage::loadNetworkInterfaces()
{
    m_comboIface->clear();
    for (const QNetworkInterface& iface : QNetworkInterface::allInterfaces())
    {
        if (iface.flags().testFlag(QNetworkInterface::IsUp) &&
            !iface.flags().testFlag(QNetworkInterface::IsLoopBack))
        {
            m_comboIface->addItem(iface.humanReadableName(), iface.name());
        }
    }
}

void NetworkPage::onInterfaceChanged(int index)
{
    if (index < 0) return;
    QString ifaceName = m_comboIface->itemData(index).toString();
    m_serverIp = getLocalIp(ifaceName);
    m_lblServerIp->setText(m_serverIp.isEmpty() ? tr("Introuvable") : m_serverIp);
    QSettings s;
    s.setValue("InterfaceSelect", index);
}

QString NetworkPage::getLocalIp(const QString& ifaceName) const
{
    QNetworkInterface iface = QNetworkInterface::interfaceFromName(ifaceName);
    for (const QNetworkAddressEntry& entry : iface.addressEntries())
    {
        if (entry.ip().protocol() == QAbstractSocket::IPv4Protocol &&
            !entry.ip().isLoopback())
            return entry.ip().toString();
    }
    return {};
}

// ─────────────────────────────────────────────
//  Scan subnet
// ─────────────────────────────────────────────

void NetworkPage::onScanSubnet()
{
    QString subnet = m_editSubnet->text().trimmed();
    static QRegularExpression re(R"(^\d{1,3}\.\d{1,3}\.\d{1,3}$)");
    if (!re.match(subnet).hasMatch())
    {
        QMessageBox::warning(this, tr("Erreur"),
            tr("Entrez un sous-réseau valide (ex: 192.168.1)"));
        return;
    }

    m_comboAppIp->clear();
    m_btnScan->setEnabled(false);

    QThread* thread = new QThread(this);
    ScanWorker* worker = new ScanWorker(subnet);
    worker->moveToThread(thread);

    connect(thread, &QThread::started, worker, &ScanWorker::run);
    connect(worker, &ScanWorker::finished, this, [this, thread, worker](const QStringList& found)
        {
            for (const QString& ip : found)
                m_comboAppIp->addItem(ip);

            m_btnScan->setEnabled(true);
            worker->deleteLater();
            thread->quit();
            thread->deleteLater();
        });

    thread->start();
}

// ─────────────────────────────────────────────
//  Toggle serveur
// ─────────────────────────────────────────────

void NetworkPage::onServerToggle()
{
    if (!m_server->isRunning())
        m_server->start(m_serverIp);
    else
        m_server->stop();

    updateServerButton();
    emit serverStateChanged();
    emit navigateBack();
}

void NetworkPage::updateServerButton()
{
    if (m_server->isRunning())
        m_btnServer->setStyleSheet("QPushButton { background-color: green; border-radius:5px; }");
    else
        m_btnServer->setStyleSheet("QPushButton { background-color: red; border-radius:5px; }");
}

// ─────────────────────────────────────────────
//  Synchronisation
// ─────────────────────────────────────────────

void NetworkPage::onSyncClicked()
{
    if (m_appIp.isEmpty())
    {
        QMessageBox::warning(this, tr("Erreur"), tr("Aucune IP d'application configurée."));
        return;
    }

    auto answer = QMessageBox::question(this, tr("Synchronisation"),
        tr("Synchroniser les comptes depuis %1 ?").arg(m_appIp),
        QMessageBox::Yes | QMessageBox::No);
    if (answer != QMessageBox::Yes) return;

    if (!isHostReachable(m_appIp))
    {
        QMessageBox::warning(this, tr("Erreur"),
            tr("L'hôte %1 est inaccessible.").arg(m_appIp));
        return;
    }

    m_btnBack->setEnabled(false);
    m_btnServer->setEnabled(false);
    m_btnSync->setEnabled(false);
    m_progress->setVisible(true);
    m_progress->setValue(0);

    QNetworkReply* reply = m_nam->get(
        QNetworkRequest(QUrl(QString("http://%1:19755/").arg(m_appIp))));

    connect(reply, &QNetworkReply::finished, this, [this, reply]()
        {
            if (reply->error() != QNetworkReply::NoError)
            {
                QMessageBox::warning(this, tr("Erreur"), reply->errorString());
                reply->deleteLater();
                m_progress->setVisible(false);
                m_btnBack->setEnabled(true);
                m_btnServer->setEnabled(true);
                m_btnSync->setEnabled(true);
                return;
            }
            onSyncFinished(reply->readAll());
            reply->deleteLater();
        });
}

void NetworkPage::onSyncFinished(const QByteArray& data)
{
    QJsonObject obj = QJsonDocument::fromJson(data).object();

    QStringList accounts = obj["Accounts"].toString().split('\n', Qt::SkipEmptyParts);
    QStringList folders = obj["Folder"].toString().split('\n', Qt::SkipEmptyParts);
    QStringList existing = m_am->getAllOtpSecrets();

    int total = accounts.size();
    for (int i = 0; i < total; ++i)
    {
        QString uri = accounts[i].trimmed();
        QString folder = (i < folders.size()) ? folders[i].trimmed() : "Default";

        static QRegularExpression re("secret=([^&]+)");
        QRegularExpressionMatch m = re.match(uri);
        if (!m.hasMatch() || existing.contains(m.captured(1))) continue;

        QUrl    qurl(uri);
        QString label = QUrl::fromPercentEncoding(qurl.path().mid(1).toUtf8());
        if (label.isEmpty()) label = "Unknown";

        m_am->addAccount(folder + "\\" + label + ";" + uri);
        m_progress->setValue(((i + 1) * 100) / total);
    }

    m_progress->setVisible(false);
    m_btnBack->setEnabled(true);
    m_btnServer->setEnabled(true);
    m_btnSync->setEnabled(true);
    emit navigateBack();
}

// ─────────────────────────────────────────────
//  Ping simple
// ─────────────────────────────────────────────

bool NetworkPage::isHostReachable(const QString& ip) const
{
    QTcpSocket sock;
    sock.connectToHost(ip, 19755);
    return sock.waitForConnected(1000);
}

// ─────────────────────────────────────────────
//  Traduction
// ─────────────────────────────────────────────

void NetworkPage::retranslateUi()
{
    m_lblApp->setText(tr("IP de l'application distante :"));
    m_lblServer->setText(tr("Serveur :"));
    m_btnSync->setText(tr("Synchroniser"));
    m_btnBack->setText(tr("Retour"));
    m_btnScan->setText(tr("Scanner"));
    m_btnServer->setText(tr("Serveur"));
}