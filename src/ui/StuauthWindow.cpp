#include "StuauthWindow.h"
#include "../commun/TranslationManager.h"

#include <QMenuBar>
#include <QProcess>
#include <QActionGroup>
#include <QCloseEvent>

StuauthWindow::StuauthWindow(QWidget* parent) : QMainWindow(parent)
{
    this->setWindowTitle("StuAuth");
    this->resize(410, 610);

    // --- Menu ---
    langMenu = menuBar()->addMenu(tr("Langue"));

    QActionGroup* langGroup = new QActionGroup(this);
    langGroup->setExclusive(true);

    QAction* actFr = langMenu->addAction("Français");
    QAction* actEn = langMenu->addAction("English");
    QAction* actBz = langMenu->addAction("Brezhoneg");
    QAction* actJa = langMenu->addAction("日本語");

    actFr->setCheckable(true);
    actEn->setCheckable(true);
    actBz->setCheckable(true);
    actJa->setCheckable(true);

    langGroup->addAction(actFr);
    langGroup->addAction(actEn);
    langGroup->addAction(actBz);
    langGroup->addAction(actJa);

    connect(actFr, &QAction::triggered, this, [this]() { setLanguage("fr"); });
    connect(actEn, &QAction::triggered, this, [this]() { setLanguage("en"); });
    connect(actBz, &QAction::triggered, this, [this]() { setLanguage("bz"); });
    connect(actJa, &QAction::triggered, this, [this]() { setLanguage("ja"); });

    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &StuauthWindow::retranslateUi);

    // --- Barre d’état ---
	m_tray = new TrayManager(this, this);
	connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
		this, &StuauthWindow::retranslateUi);
}

// Langue
void StuauthWindow::setLanguage(const QString& lang)
{
    TranslationManager::instance().setLanguage(lang);
}

void StuauthWindow::retranslateUi()
{
    langMenu->setTitle(tr("Langue"));

    m_tray->retranslate();
}

void StuauthWindow::closeEvent(QCloseEvent* event)
{
    if (m_tray) {
        this->hide();
        event->ignore();  // Ignore la fermeture, cache juste la fenêtre
    }
    else {
        event->accept();  // Sinon ferme vraiment
    }
}