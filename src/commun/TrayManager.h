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
	void refreshLockState();

signals:
	void lockStateChanged();

private:
	QSystemTrayIcon* tray;
	QMenu* trayMenu;
	QAction* actionShow;
	QAction* actionQuit;
	QAction* actionLockToggle = nullptr;

	StuauthWindow* mainWindow;

	void updateLockAction();

private slots:
	void onToggleLock();
};