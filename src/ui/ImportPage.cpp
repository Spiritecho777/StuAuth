#include "ImportPage.h"
#include "../core/AccountManager.h"
#include "../commun/TranslationManager.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QFileDialog>
#include <QFile>
#include <QTextStream>
#include <QCheckBox>
#include <QListWidgetItem>
#include <QMessageBox>
#include <QUrl>

ImportPage::ImportPage(const QStringList& accounts, const QString& folderName, QWidget* parent)
    : QWidget(parent)
    , m_folderName(folderName)
{
    buildUi();

    if (accounts.isEmpty())
        loadFromFile();
    else
        loadFromList(accounts);

    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &ImportPage::retranslateUi);
}

void ImportPage::buildUi()
{
    m_list = new QListWidget();
    m_btnOk = new QPushButton(tr("Confirmer"));
    m_btnBack = new QPushButton(tr("Retour"));

    QHBoxLayout* btnLayout = new QHBoxLayout();
    btnLayout->addWidget(m_btnBack);
    btnLayout->addStretch();
    btnLayout->addWidget(m_btnOk);

    QVBoxLayout* layout = new QVBoxLayout(this);
    layout->setContentsMargins(10, 10, 10, 10);
    layout->addWidget(m_list);
    layout->addLayout(btnLayout);

    connect(m_btnOk, &QPushButton::clicked, this, &ImportPage::onConfirmClicked);
    connect(m_btnBack, &QPushButton::clicked, this, &ImportPage::navigateBack);
}

// ─────────────────────────────────────────────
//  Chargement depuis fichier texte
// ─────────────────────────────────────────────

void ImportPage::loadFromFile()
{
    QString path = QFileDialog::getOpenFileName(this, tr("Ouvrir"),
        "", tr("Fichiers texte (*.txt)"));
    if (path.isEmpty()) return;

    QFile file(path);
    if (!file.open(QIODevice::ReadOnly | QIODevice::Text)) return;

    QTextStream in(&file);
    while (!in.atEnd())
    {
        QString line = in.readLine().trimmed();
        if (line.startsWith("otpauth:"))
            parseAndAddItem(line);
    }
}

// ─────────────────────────────────────────────
//  Chargement depuis liste (import Google Auth QR)
// ─────────────────────────────────────────────

void ImportPage::loadFromList(const QStringList& accounts)
{
    for (const QString& line : accounts)
        if (line.startsWith("otpauth:"))
            parseAndAddItem(line);
}

void ImportPage::parseAndAddItem(const QString& otpauthUri)
{
    // Extraire le label depuis l'URI  otpauth://totp/LABEL?secret=...
    QUrl url(otpauthUri);
    QString label = QUrl::fromPercentEncoding(url.path().mid(1).toUtf8()); // supprime le "/" initial
    if (label.isEmpty()) label = otpauthUri;

    auto* cb = new QCheckBox(label + ";" + otpauthUri);
    auto* item = new QListWidgetItem(m_list);
    item->setSizeHint(cb->sizeHint());
    m_list->setItemWidget(item, cb);
}

// ─────────────────────────────────────────────
//  Confirmation : ajouter les comptes cochés
// ─────────────────────────────────────────────

void ImportPage::onConfirmClicked()
{
    AccountManager am;

    for (int i = 0; i < m_list->count(); ++i)
    {
        QListWidgetItem* item = m_list->item(i);
        auto* cb = qobject_cast<QCheckBox*>(m_list->itemWidget(item));
        if (!cb || !cb->isChecked()) continue;

        QString content = cb->text();
        // Format : "LABEL;otpauth://..."
        int sep = content.indexOf(';');
        if (sep == -1) continue;

        QString label = content.left(sep);
        QString uri = content.mid(sep + 1);
        QString line = m_folderName + "\\" + label + ";" + uri;

        try {
            QUrl check(uri);
            if (check.isValid())
                am.addAccount(line);
        }
        catch (...) {
            QMessageBox::warning(this, tr("Erreur"),
                tr("Impossible d'importer : %1").arg(label));
        }
    }

    emit importDone();
}

void ImportPage::retranslateUi()
{
    m_btnOk->setText(tr("Confirmer"));
    m_btnBack->setText(tr("Retour"));
}