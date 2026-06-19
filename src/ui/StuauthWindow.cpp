#include "StuauthWindow.h"
#include "../commun/TranslationManager.h"
#include "../commun/security/CryptoUtils.h"
#include "../ui/SelectAccountPage.h"
#include "../ui/NewAccountPage.h"
#include "../ui/NewAccount2Page.h"
#include "../ui/ImportPage.h"
#include "../ui/NetworkPage.h"
#include "../ui/MasterPasswordPopup.h"

#include <QMenuBar>
#include <QActionGroup>
#include <QCloseEvent>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QInputDialog>
#include <QMessageBox>
#include <QFileDialog>
#include <QTextStream>
#include <QProcess>
#include <QApplication>
#include <QSettings>
#include <QCursor>
#include <QDebug>
#include <MultiFormatWriter.h>
#include <BitMatrix.h>

// ─────────────────────────────────────────────
//  Constructeur
// ─────────────────────────────────────────────

StuauthWindow::StuauthWindow(QWidget* parent) : QMainWindow(parent)
{
    setWindowTitle("StuAuth");
    resize(410, 610);

    m_am = new AccountManager();
    m_server = new HttpServer(m_am, this);

    // ── Stack + page principale ──────────────────
    m_stack = new QStackedWidget(this);
    setCentralWidget(m_stack);

    buildMainWidget();
    m_stack->addWidget(m_main);

    updateFolderList();

    // ── Tray ────────────────────────────────────
    m_tray = new TrayManager(this, this);

    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &StuauthWindow::retranslateUi);
}

// ─────────────────────────────────────────────
//  Construction de la page principale
// ─────────────────────────────────────────────

void StuauthWindow::buildMainWidget()
{
    m_main = new QWidget();

    // Toolbar ligne 1 : Renommer | + | - | Export | ?
    m_btnRename = new QPushButton(tr("Renommer"));
    m_btnAdd = new QPushButton("+");
    m_btnDel = new QPushButton("-");
    m_btnExport = new QPushButton(tr("Exporter"));
    m_btnHelp = new QPushButton("?");
#ifdef __linux__
    m_btnMasterPassword = new QPushButton();
    m_btnMasterPassword->setIcon(style()->standardIcon(QStyle::SP_FileDialogDetailedView));
#elif WIN32
    m_btnMasterPassword = new QPushButton("🔒", this);
#endif

    m_btnAdd->setFixedSize(30, 30);
    m_btnDel->setFixedSize(30, 30);
    m_btnHelp->setFixedSize(25, 30);
	m_btnMasterPassword->setFixedSize(40, 40);

	m_btnLang = new QPushButton("🌐");
    QFont f = m_btnLang->font();
    f.setPointSize(10);
    m_btnLang->setFont(f);
	m_btnLang->setFixedSize(50, 30);

	m_langMenu = new QMenu(this);
	m_actFr = m_langMenu->addAction("Français");
	m_actEn = m_langMenu->addAction("English");
	m_actBz = m_langMenu->addAction("Brezhoneg");
	m_actJa = m_langMenu->addAction("日本語");

	m_btnLang->setMenu(m_langMenu);

    QHBoxLayout* toolbar1 = new QHBoxLayout();
    toolbar1->addWidget(m_btnRename);
    toolbar1->addStretch();
    toolbar1->addWidget(m_btnAdd);
    toolbar1->addWidget(m_btnDel);
    toolbar1->addWidget(m_btnExport);
	toolbar1->addWidget(m_btnMasterPassword);
    toolbar1->addWidget(m_btnHelp);
	toolbar1->addWidget(m_btnLang);

    // Toolbar ligne 2 : Serveur | Synchro | NomDossier | <--
    m_btnServer = new QPushButton("⬤");
    m_btnSynchro = new QPushButton("⟳");
    m_lblFolder = new QLabel();
    m_btnBack = new QPushButton("<--");

    m_btnServer->setFixedSize(30, 30);
    m_btnSynchro->setFixedSize(30, 30);
    m_btnBack->setFixedSize(57, 30);
    m_lblFolder->setMinimumWidth(150);

    m_btnServer->setStyleSheet("QPushButton { background-color: red; border-radius: 5px; }");

    QHBoxLayout* toolbar2 = new QHBoxLayout();
    toolbar2->addWidget(m_btnServer);
    toolbar2->addWidget(m_btnSynchro);
    toolbar2->addWidget(m_lblFolder);
    toolbar2->addStretch();
    toolbar2->addWidget(m_btnBack);

    // Liste
    m_list = new QListWidget();
    m_list->setSelectionMode(QAbstractItemView::SingleSelection);

    // Layout principal
    QVBoxLayout* layout = new QVBoxLayout(m_main);
    layout->setContentsMargins(10, 10, 10, 10);
    layout->addLayout(toolbar1);
    layout->addLayout(toolbar2);
    layout->addWidget(m_list);

    // Connexions
    connect(m_btnAdd, &QPushButton::clicked, this, &StuauthWindow::onAddClicked);
    connect(m_btnDel, &QPushButton::clicked, this, &StuauthWindow::onDeleteClicked);
    connect(m_btnRename, &QPushButton::clicked, this, &StuauthWindow::onRenameClicked);
    connect(m_btnBack, &QPushButton::clicked, this, &StuauthWindow::onBackClicked);
    connect(m_btnExport, &QPushButton::clicked, this, &StuauthWindow::onExportClicked);
    connect(m_btnHelp, &QPushButton::clicked, this, &StuauthWindow::onHelpClicked);
    connect(m_btnSynchro, &QPushButton::clicked, this, &StuauthWindow::onTimeSynchro);
    connect(m_btnServer, &QPushButton::clicked, this, &StuauthWindow::onServerClicked);
    connect(m_list, &QListWidget::itemDoubleClicked, this, &StuauthWindow::onItemDoubleClicked);

    connect(m_btnMasterPassword, &QPushButton::clicked, this, &StuauthWindow::onMasterPasswordClicked);


	connect(m_actFr, &QAction::triggered, this, [this]() { setLanguage("fr"); });
	connect(m_actEn, &QAction::triggered, this, [this]() { setLanguage("en"); });
	connect(m_actBz, &QAction::triggered, this, [this]() { setLanguage("bz"); });
	connect(m_actJa, &QAction::triggered, this, [this]() { setLanguage("ja"); });
}

// ─────────────────────────────────────────────
//  Langue
// ─────────────────────────────────────────────

void StuauthWindow::setLanguage(const QString& lang)
{
    TranslationManager::instance().setLanguage(lang);
}

void StuauthWindow::retranslateUi()
{
    m_langMenu->setTitle(tr("Langue"));
    m_btnRename->setText(tr("Renommer"));
    m_btnExport->setText(tr("Exporter"));
    m_tray->retranslate();
}

// ─────────────────────────────────────────────
//  Données
// ─────────────────────────────────────────────

void StuauthWindow::updateFolderList()
{
    m_names.clear();
    m_uris.clear();

    QMap<QString, int> occ = m_am->countFolderOccurrences();
    QStringList valid = m_am->getValidLines(occ);

    for (const QString& line : valid)
    {
        QStringList parts = line.split(';');
        if (parts.size() == 2)
        {
            m_names << parts[0];
            m_uris << parts[1];
        }
    }

    m_currentFolder.clear();
    m_lblFolder->clear();
    showFolderView();
}

void StuauthWindow::showFolderView()
{
    m_list->clear();
    QStringList seen;
    for (const QString& name : m_names)
    {
        QString folder = name.split('\\').value(0);
        if (!seen.contains(folder))
        {
            seen << folder;
            m_list->addItem(folder);
        }
    }
}

void StuauthWindow::showAccountView(const QString& folderName)
{
    m_names.clear();
    m_uris.clear();

    for (const QString& line : m_am->getAccountsByFolder(folderName))
    {
        QStringList parts = line.split(';');
        if (parts.size() == 2 && !parts[0].isEmpty() && !parts[1].isEmpty())
        {
            m_names << parts[0];
            m_uris << parts[1];
        }
    }

    m_list->clear();
    for (const QString& name : m_names)
    {
        QString accountName = name.split('\\').value(1);
        if (!accountName.isEmpty())
            m_list->addItem(accountName);
    }
}

// ─────────────────────────────────────────────
//  Navigation
// ─────────────────────────────────────────────

void StuauthWindow::showMainView()
{
    // Supprime toutes les pages empilées sauf la principale (index 0)
    while (m_stack->count() > 1)
    {
        QWidget* w = m_stack->widget(1);
        m_stack->removeWidget(w);
        w->deleteLater();
    }
    m_stack->setCurrentIndex(0);
    updateFolderList();
    updateServerButton();  // rafraîchit le bouton après retour depuis NetworkPage
}

void StuauthWindow::navigateToAccount(const QString& name, const QString& otpUri)
{
    auto* page = new SelectAccountPage(name, otpUri);
    connect(page, &SelectAccountPage::navigateBack, this, &StuauthWindow::showMainView);
    m_stack->addWidget(page);
    m_stack->setCurrentWidget(page);
}

void StuauthWindow::navigateToNewAccount(const QString& folderName)
{
    auto* page = new NewAccountPage(folderName);
    connect(page, &NewAccountPage::navigateBack, this, &StuauthWindow::showMainView);
    connect(page, &NewAccountPage::navigateToNaming, this, [this, folderName](const QString& uri) {
        navigateToNaming(uri, folderName);
        });
    connect(page, &NewAccountPage::navigateToImport, this, [this, folderName](const QStringList& acc) {
        navigateToImport(acc, folderName);
        });
    m_stack->addWidget(page);
    m_stack->setCurrentWidget(page);
}

void StuauthWindow::navigateToNaming(const QString& otpUri, const QString& folderName)
{
    auto* page = new NewAccount2Page(otpUri, folderName);
    connect(page, &NewAccount2Page::navigateBack, this, &StuauthWindow::showMainView);
    connect(page, &NewAccount2Page::accountSaved, this, &StuauthWindow::showMainView);
    m_stack->addWidget(page);
    m_stack->setCurrentWidget(page);
}

void StuauthWindow::navigateToImport(const QStringList& accounts, const QString& folderName)
{
    auto* page = new ImportPage(accounts, folderName);
    connect(page, &ImportPage::navigateBack, this, &StuauthWindow::showMainView);
    connect(page, &ImportPage::importDone, this, &StuauthWindow::showMainView);
    m_stack->addWidget(page);
    m_stack->setCurrentWidget(page);
}

void StuauthWindow::navigateToNetwork()
{
    auto* page = new NetworkPage(m_am, m_server);
    connect(page, &NetworkPage::navigateBack, this, &StuauthWindow::showMainView);
    connect(page, &NetworkPage::serverStateChanged, this, &StuauthWindow::updateServerButton);
    m_stack->addWidget(page);
    m_stack->setCurrentWidget(page);
}

// ─────────────────────────────────────────────
//  Slots boutons
// ─────────────────────────────────────────────

void StuauthWindow::onItemDoubleClicked(QListWidgetItem* item)
{
    if (!item) return;
    int row = m_list->row(item);

    if (m_currentFolder.isEmpty())
    {
        m_currentFolder = item->text();
        m_lblFolder->setText(m_currentFolder);
        showAccountView(m_currentFolder);
    }
    else
    {
        if (row >= 0 && row < m_names.size())
            navigateToAccount(m_names[row], m_uris[row]);
    }
}

void StuauthWindow::onAddClicked()
{
    if (!m_currentFolder.isEmpty())
    {
        navigateToNewAccount(m_currentFolder);
    }
    else
    {
        bool ok;
        QString name = QInputDialog::getText(this, tr("Nouveau dossier"),
            tr("Nom du dossier :"),
            QLineEdit::Normal, "", &ok);
        if (!ok) return;
        if (name.trimmed().isEmpty())
        {
            QMessageBox::warning(this, tr("Erreur"),
                tr("Le nom du dossier ne peut pas être vide."));
            return;
        }
        m_am->addFolder(name.trimmed());
        updateFolderList();
    }
}

void StuauthWindow::onDeleteClicked()
{
    QListWidgetItem* item = m_list->currentItem();
    if (!item) return;
    QString name = item->text();

    if (!m_currentFolder.isEmpty())
    {
        if (m_am->deleteFolderOrAccount(name, false))
        {
            QMessageBox::information(this, tr("Supprimé"),
                tr("Le compte \"%1\" a été supprimé.").arg(name));
            showAccountView(m_currentFolder);
        }
    }
    else
    {
        if (!m_am->deleteFolderOrAccount(name, true, false))
        {
            auto confirm = QMessageBox::warning(this, tr("Dossier non vide"),
                tr("Le dossier \"%1\" contient des comptes.\n"
                    "Voulez-vous le supprimer quand même ?").arg(name),
                QMessageBox::Yes | QMessageBox::No);

            if (confirm == QMessageBox::Yes)
                m_am->deleteFolderOrAccount(name, true, true);
        }
        updateFolderList();
    }
}

void StuauthWindow::onRenameClicked()
{
    QListWidgetItem* item = m_list->currentItem();
    if (!item) return;

    QString oldName = item->text();
    bool ok;
    QString newName = QInputDialog::getText(this, tr("Renommer"),
        tr("Nouveau nom :"),
        QLineEdit::Normal, oldName, &ok);
    if (!ok || newName.trimmed().isEmpty()) return;

    bool isFolder = m_currentFolder.isEmpty();
    m_am->renameFolderOrAccount(oldName, newName.trimmed(), isFolder);

    if (isFolder) updateFolderList();
    else          showAccountView(m_currentFolder);
}

void StuauthWindow::onBackClicked()
{
    updateFolderList();
}

void StuauthWindow::onExportClicked()
{
    QMenu menu(this);
    menu.addAction(tr("Exporter en texte"), this, &StuauthWindow::exportToText);
    menu.addAction(tr("Exporter en QR Code"), this, &StuauthWindow::exportToQRCode);
    menu.exec(QCursor::pos());
}

void StuauthWindow::onHelpClicked()
{
    QString version = APP_VERSION;
    QMessageBox::information(this, tr("À propos"),
        tr("StuAuth — version: %1").arg(APP_VERSION));
}

void StuauthWindow::onTimeSynchro()
{
#ifdef _WIN32
    // Élévation UAC via PowerShell Start-Process -Verb RunAs
    QProcess::startDetached("powershell.exe", {
        "-Command",
        "Start-Process cmd.exe "
        "-ArgumentList '/C net start w32time & w32tm /resync' "
        "-Verb RunAs "
        "-WindowStyle Hidden"
        });
#else
    timeSynchroLinux();
#endif
}

void StuauthWindow::timeSynchroLinux()
{
    // timedatectl est fourni par systemd, présent sur toutes les distros modernes
    QProcess p;
    p.start("pkexec", { "timedatectl", "set-ntp", "true" });
    p.waitForFinished(5000);
}

void StuauthWindow::onServerClicked()
{
    navigateToNetwork();
}

void StuauthWindow::updateServerButton()
{
    if (m_server && m_server->isRunning())
        m_btnServer->setStyleSheet(
            "QPushButton { background-color: green; border-radius: 5px; }");
    else
        m_btnServer->setStyleSheet(
            "QPushButton { background-color: red; border-radius: 5px; }");
}

// ─────────────────────────────────────────────
//  Export
// ─────────────────────────────────────────────

void StuauthWindow::exportToText()
{
    QString path = QFileDialog::getSaveFileName(this, tr("Exporter"),
        "", tr("Fichiers texte (*.txt)"));
    if (path.isEmpty()) return;

    QFile file(path);
    if (!file.open(QIODevice::WriteOnly | QIODevice::Text)) return;

    QTextStream out(&file);
    int count = 0;
    for (const QString& line : m_am->readLines())
    {
        QString uri = line.split(';').value(1);
        if (!uri.isEmpty())
        {
            out << uri << "\n";
            count++;
        }
    }

    if (count == 0)
        QMessageBox::warning(this, tr("Export"), tr("Aucun compte à exporter."));
    else
        QMessageBox::information(this, tr("Export terminé"),
            tr("%1 compte(s) exporté(s).").arg(count));
}


void StuauthWindow::exportToQRCode()
{
    QString dir = QFileDialog::getExistingDirectory(this, tr("Dossier de destination"));
    if (dir.isEmpty()) return;

    QStringList lines = m_am->readLines();
    int count = 0;

    for (const QString& line : lines)
    {
        QStringList parts = line.split(';');
        if (parts.size() < 2 || parts[1].isEmpty()) continue;

        QString accountName = parts[0].split('\\').value(1);
        if (accountName.isEmpty()) continue;

        QString uri = parts[1];

        // ── Génération du QR code via ZXing ──
        try {
            ZXing::MultiFormatWriter writer(ZXing::BarcodeFormat::QRCode);
            ZXing::BitMatrix matrix = writer.encode(uri.toStdString(), 256, 256);

            // ── Conversion BitMatrix → QImage ──
            int w = matrix.width();
            int h = matrix.height();
            QImage img(w, h, QImage::Format_RGB32);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    QRgb color = matrix.get(x, y) ? qRgb(0, 0, 0)
                        : qRgb(255, 255, 255);
                    img.setPixel(x, y, color);
                }
            }


            // ── Mise à l'échelle pour lisibilité ──
            QImage scaled = img.scaled(512, 512, Qt::KeepAspectRatio,
                Qt::FastTransformation);

            QString path = dir + "/" + accountName + ".png";
            if (scaled.save(path, "PNG"))
                count++;
        }
        catch (...) {
            qDebug() << "Erreur QR pour:" << accountName;
        }
    }

    if (count == 0)
        QMessageBox::warning(this, tr("Export"), tr("Aucun compte à exporter."));
    else
        QMessageBox::information(this, tr("Export terminé"),
            tr("%1 QR code(s) exporté(s) dans :\n%2").arg(count).arg(dir));
}


// ─────────────────────────────────────────────
//  Close event → systray
// ─────────────────────────────────────────────

void StuauthWindow::closeEvent(QCloseEvent* event)
{
    if (m_tray)
    {
        hide();
        event->ignore();
    }
    else
    {
        event->accept();
    }
}

// ─────────────────────────────────────────────
//  Sécurité
// ─────────────────────────────────────────────

void StuauthWindow::onMasterPasswordClicked()
{
    QWidget* parent = this;

    const bool hasMaster = CryptoUtils::hasMasterPasswordConfigured();

    // ─────────────────────────────
    // Cas 1 : pas de mdp → création
    // ─────────────────────────────
    if (!hasMaster)
    {
        MasterPasswordPopup dlg(
            "Créer un mot de passe maître",
            "Entrez un mot de passe maître :",
            parent
        );

        if (dlg.exec() != QDialog::Accepted)
            return;

        QString pwd = dlg.password();
        if (pwd.isEmpty())
        {
            QMessageBox::warning(parent,
                "Erreur", "Le mot de passe ne peut pas être vide.");
            return;
        }

        if (!CryptoUtils::setOrChangeMasterPassword(pwd))
        {
            QMessageBox::critical(parent,
                "Erreur", "Impossible d’enregistrer le mot de passe.");
            return;
        }

        QMessageBox::information(parent,
            "Succès", "Mot de passe maître activé.");

        return;
    }

    // ─────────────────────────────
    // Cas 2 : mdp existe → choix
    // ─────────────────────────────
    QMessageBox msg(parent);
    msg.setWindowTitle("Mot de passe maître");
    msg.setText("Que souhaitez-vous faire ?");
    QPushButton* changeBtn = msg.addButton("Changer", QMessageBox::AcceptRole);
    QPushButton* disableBtn = msg.addButton("Désactiver", QMessageBox::DestructiveRole);
    msg.addButton("Annuler", QMessageBox::RejectRole);

    msg.exec();

    if (msg.clickedButton() == changeBtn)
    {
        // ── vérifier ancien mdp
        MasterPasswordPopup checkDlg(
            "Vérification",
            "Entrez le mot de passe actuel :",
            parent
        );

        if (checkDlg.exec() != QDialog::Accepted)
            return;

        if (!CryptoUtils::verifyMasterPassword(checkDlg.password()))
        {
            QMessageBox::critical(parent,
                "Erreur", "Mot de passe incorrect.");
            return;
        }

        // ── nouveau mdp
        MasterPasswordPopup newDlg(
            "Nouveau mot de passe",
            "Entrez le nouveau mot de passe :",
            parent
        );

        if (newDlg.exec() != QDialog::Accepted)
            return;

        QString newPwd = newDlg.password();
        if (newPwd.isEmpty())
        {
            QMessageBox::warning(parent,
                "Erreur", "Le mot de passe ne peut pas être vide.");
            return;
        }

        CryptoUtils::setOrChangeMasterPassword(newPwd);

        QMessageBox::information(parent,
            "Succès", "Mot de passe modifié.");
    }
    else if (msg.clickedButton() == disableBtn)
    {
        MasterPasswordPopup checkDlg(
            "Confirmation",
            "Entrez le mot de passe pour désactiver :",
            parent
        );

        if (checkDlg.exec() != QDialog::Accepted)
            return;

        if (!CryptoUtils::verifyMasterPassword(checkDlg.password()))
        {
            QMessageBox::critical(parent,
                "Erreur", "Mot de passe incorrect.");
            return;
        }

        CryptoUtils::disableMasterPassword();

        QMessageBox::information(parent,
            "Succès", "Mot de passe maître désactivé.");
    }
}
