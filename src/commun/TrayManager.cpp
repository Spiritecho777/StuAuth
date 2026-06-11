#include "TrayManager.h"
#include "../ui/StuauthWindow.h"
#include "TranslationManager.h"

#include <QAction>
#include <QApplication>

TrayManager::TrayManager(StuauthWindow* main, QObject* parent) : QObject(parent), mainWindow(main)
{
	tray = new QSystemTrayIcon(QIcon(":/icons/Asset/Icone.png"), this);
	trayMenu = new QMenu();
    
    trayMenu->addSeparator();

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
}

void TrayManager::retranslate()
{
	actionShow->setText(tr("Ouvrir"));
    actionQuit->setText(tr("Quitter"));
}
