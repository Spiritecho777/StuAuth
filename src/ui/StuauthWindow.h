#pragma once

#include "../commun/TrayManager.h"
#include "../core/AccountManager.h"
#include "../network/HttpServer.h"

#include <QMainWindow>
#include <QStackedWidget>
#include <QListWidget>
#include <QPushButton>
#include <QLabel>
#include <QMenu>
#include <QAction>

class SelectAccountPage;
class NewAccountPage;
class NewAccount2Page;
class ImportPage;
class NetworkPage;

class StuauthWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit StuauthWindow(QWidget* parent = nullptr);
    ~StuauthWindow() { delete m_am; }

    // Appelé par les sous-pages pour rafraîchir la liste
    void updateFolderList();

private slots:
    void onAddClicked();
    void onDeleteClicked();
    void onRenameClicked();
    void onBackClicked();
    void onExportClicked();
    void onHelpClicked();
    void onTimeSynchro();
    void onServerClicked();
    void onItemDoubleClicked(QListWidgetItem* item);
    void retranslateUi();
	void onMasterPasswordClicked();

private:
    // ── Navigation ──────────────────────────────
    void showMainView();
    void navigateToAccount(const QString& name, const QString& otpUri);
    void navigateToNewAccount(const QString& folderName);
    void navigateToNaming(const QString& otpUri, const QString& folderName);
    void navigateToImport(const QStringList& accounts, const QString& folderName);
    void navigateToNetwork();

    // ── Vue principale ───────────────────────────
    void buildMainWidget();
    void showFolderView();
    void showAccountView(const QString& folderName);
    void exportToText();
    void exportToQRCode();
    void updateServerButton();
    void setLanguage(const QString& lang);
	void onLockStateChanged();
	void updateLangButton();

    void closeEvent(QCloseEvent* event) override;

    // ── Core ────────────────────────────────────
    AccountManager* m_am = nullptr;
    HttpServer* m_server = nullptr;

    // ── Widgets principaux ───────────────────────
    QStackedWidget* m_stack = nullptr;
    QWidget* m_main = nullptr;   // page principale (dossiers/comptes)

    // Toolbar ligne 1
    QPushButton* m_btnRename = nullptr;
    QPushButton* m_btnAdd = nullptr;
    QPushButton* m_btnDel = nullptr;
    QPushButton* m_btnExport = nullptr;
    QPushButton* m_btnHelp = nullptr;
	QPushButton* m_btnMasterPassword = nullptr;

    // Toolbar ligne 2
    QPushButton* m_btnServer = nullptr;
    QPushButton* m_btnSynchro = nullptr;
    QLabel* m_lblFolder = nullptr;
    QPushButton* m_btnBack = nullptr;

    // Liste
    QListWidget* m_list = nullptr;

    // Menu langue
    QMenu* m_langMenu = nullptr;
	QPushButton* m_btnLang = nullptr;
    QAction* m_actFr = nullptr;
    QAction* m_actEn = nullptr;
    QAction* m_actBz = nullptr;
    QAction* m_actJa = nullptr;

    // Tray
    TrayManager* m_tray = nullptr;

    // ── État ────────────────────────────────────
    QString     m_currentFolder;
    QStringList m_names;
    QStringList m_uris;
};