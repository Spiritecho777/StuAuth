#pragma once

#include <QWidget>
#include <QPushButton>
#include <QLabel>
#include <QLineEdit>
#include <QComboBox>
#include <QProgressBar>
#include <QNetworkAccessManager>
#include <QNetworkInterface>

class AccountManager;
class HttpServer;

class NetworkPage : public QWidget
{
    Q_OBJECT

public:
    explicit NetworkPage(AccountManager* am, HttpServer* server, QWidget* parent = nullptr);

signals:
    void navigateBack();
    void serverStateChanged();

private slots:
    void onServerToggle();
    void onSyncClicked();
    void onInterfaceChanged(int index);
    void onScanSubnet();
    void onSyncFinished(const QByteArray& data);
    void retranslateUi();

private:
    void buildUi();
    void loadNetworkInterfaces();
    QString getLocalIp(const QString& interfaceName) const;
    bool isHostReachable(const QString& ip) const;
    void updateServerButton();

    AccountManager* m_am = nullptr;
    HttpServer* m_server = nullptr;
    QNetworkAccessManager* m_nam = nullptr;

    // Widgets
    QLabel* m_lblApp = nullptr;
    QLineEdit* m_editSubnet = nullptr;
    QComboBox* m_comboAppIp = nullptr;
    QLabel* m_lblServer = nullptr;
    QLabel* m_lblServerIp = nullptr;
    QComboBox* m_comboIface = nullptr;
    QPushButton* m_btnServer = nullptr;
    QPushButton* m_btnSync = nullptr;
    QPushButton* m_btnBack = nullptr;
    QProgressBar* m_progress = nullptr;

    QString      m_appIp;
    QString      m_serverIp;
};