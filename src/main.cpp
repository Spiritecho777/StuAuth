#include <QCoreApplication>
#include <QApplication>
#include <QIcon>
#include <QFile>
#include <QStandardPaths>
#include <QDebug>
#include <QDir>

#include "commun/AppInitializer.h"
#include "ui/StuauthWindow.h"
#include "commun/TrayManager.h"
#include "commun/TranslationManager.h"

#include "core/AccountManager.h"
#include "network/HttpServer.h"

int main(int argc, char* argv[])
{
    QCoreApplication::setOrganizationName("Stusoft");
    QCoreApplication::setApplicationName("StuAuth");

    QApplication a(argc, argv);

    // Charge la langue de base
    TranslationManager::instance().loadSavedLanguage();

    if (!AppInitializer::init())
        return 0;

    StuauthWindow w;

    w.show();

    return a.exec();
}