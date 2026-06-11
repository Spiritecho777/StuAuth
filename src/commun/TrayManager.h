#pragma once

#include <QObject>
#include <QSystemTrayIcon>
#include <QMenu>

class StuauthWindow;

class TrayManager : public QObject
{
	Q_OBJECT

public:
	explicit TrayManager(StuauthWindow* main, QObject* parent = nullptr);

	void retranslate();

private:
	QSystemTrayIcon* tray;
	QMenu* trayMenu;
	QAction* actionShow;
	QAction* actionQuit;

	StuauthWindow* mainWindow;
};