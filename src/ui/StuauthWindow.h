#pragma once

#include "../commun/TrayManager.h"

#include <QMainWindow>
#include <QProcess>


class StuauthWindow : public QMainWindow
{
	Q_OBJECT

public:
	explicit StuauthWindow(QWidget* parent = nullptr);

private:
	QMenu* langMenu;

	TrayManager* m_tray = nullptr;

	void setLanguage(const QString& lang);
	void retranslateUi();
	void closeEvent(QCloseEvent* event) override;
};