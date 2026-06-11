#include <QCoreApplication>
#include <QApplication>
#include <QIcon>

#include "commun/AppInitializer.h"
#include "ui/StuauthWindow.h"
#include "commun/TrayManager.h"
#include "commun/TranslationManager.h"

/*#ifdef _WIN32
#include <winsock2.h>
#endif*/

int main(int argc, char* argv[])
{
//#ifdef _WIN32
//    WSADATA wsadata;
//    WSAStartup(MAKEWORD(2, 2), &wsadata);
//#endif

    QCoreApplication::setOrganizationName("Stusoft");
    QCoreApplication::setApplicationName("StuAuth");

    QApplication a(argc, argv);

    // Charge la langue de base
    TranslationManager::instance().loadSavedLanguage();

    if (!AppInitializer::init())
        return 0;

    StuauthWindow w;

    w.show();

    int ret = a.exec();

//#ifdef _WIN32
//    WSACleanup();
//#endif

    return ret;
}