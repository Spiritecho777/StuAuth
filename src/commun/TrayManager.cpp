#include "TrayManager.h"
#include "security/RuntimeSecret.h"
#include "security/CryptoUtils.h"
#include "../ui/StuauthWindow.h"
#include "../ui/MasterPasswordPopup.h"
#include "TranslationManager.h"

#include <QAction>
#include <QApplication>
#include <QMessageBox>

TrayManager::TrayManager(StuauthWindow* main, QObject* parent) : QObject(parent), mainWindow(main)
{
	tray = new QSystemTrayIcon(QIcon(":/icons/Asset/Icone.png"), this);
	trayMenu = new QMenu();
    
    trayMenu->addSeparator();

	actionLockToggle = trayMenu->addAction(tr("Verrouiller"));
	actionShow = trayMenu->addAction(tr("Ouvrir"));
	actionQuit = trayMenu->addAction(tr("Quitter"));

    tray->setContextMenu(trayMenu);

    connect(actionShow, &QAction::triggered, [main]() {
        main->show();
        main->raise();
        main->activateWindow();
    });

    connect(actionQuit, &QAction::triggered, [main]() { main->close();  qApp->quit(); });

    QObject::connect(tray, &QSystemTrayIcon::activated, [main](QSystemTrayIcon::ActivationReason reason) {
        if (reason == QSystemTrayIcon::DoubleClick) {
            main->show();
            main->raise();
            main->activateWindow();
        }
    });

    tray->show();

    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &TrayManager::retranslate);
	connect(actionLockToggle, &QAction::triggered, this, &TrayManager::onToggleLock);

	updateLockAction();
}

void TrayManager::retranslate()
{
	actionShow->setText(tr("Ouvrir"));
    actionQuit->setText(tr("Quitter"));
}

void TrayManager::onToggleLock()
{
    QWidget* parent = mainWindow;

    // UNLOCK
    if (!RuntimeSecret::instance().hasSecret())
    {
        MasterPasswordPopup dlg(
            QObject::tr("Déverrouillage"),
            QObject::tr("Entrez le mot de passe maître :"),
            parent
        );

        if (dlg.exec() != QDialog::Accepted)
            return;

        QString pwd = dlg.password();

        if (!CryptoUtils::verifyMasterPassword(pwd))
        {
            QMessageBox::critical(parent,
                QObject::tr("Erreur"),
                QObject::tr("Mot de passe incorrect."));
            return;
        }

        RuntimeSecret::instance().setSecret(pwd.toUtf8());

        // refresh UI
        emit lockStateChanged();
    }
    else
    {
        // LOCK
        RuntimeSecret::instance().clear();
    }

    updateLockAction();
    emit lockStateChanged();
}

void TrayManager::updateLockAction()
{
    if (!CryptoUtils::hasMasterPasswordConfigured())
    {
        actionLockToggle->setVisible(false);
        return;
    }

    actionLockToggle->setVisible(true);

    if (RuntimeSecret::instance().hasSecret())
        actionLockToggle->setText(QObject::tr("Verrouiller"));
    else
        actionLockToggle->setText(QObject::tr("Déverrouiller"));
}

void TrayManager::refreshLockState()
{
    updateLockAction();
}