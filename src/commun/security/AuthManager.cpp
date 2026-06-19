#include "AuthManager.h"

#include "CryptoUtils.h"
#include "../../ui/MasterPasswordPopup.h"
#include "RuntimeSecret.h"

#include <QApplication>
#include <QMessageBox>

bool AuthManager::init()
{
    if (!CryptoUtils::hasMasterPasswordConfigured())
        return true;

    QWidget* parent = QApplication::activeWindow();

    MasterPasswordPopup dlg(
        QObject::tr("Mot de passe maître"),
        QObject::tr("Entrez le mot de passe maître pour déverrouiller StuAuth :"),
        parent
    );

    if (dlg.exec() != QDialog::Accepted)
        return false;

    const QString pwd = dlg.password();
    if (!CryptoUtils::verifyMasterPassword(pwd)) {
        QMessageBox::critical(
            parent,
            QObject::tr("Erreur"),
            QObject::tr("Mot de passe maître incorrect.")
        );
        return false;
    }

    RuntimeSecret::instance().setSecret(pwd.toUtf8());
    return true;
}