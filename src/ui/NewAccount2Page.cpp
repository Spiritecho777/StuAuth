#include "NewAccount2Page.h"
#include "../core/AccountManager.h"
#include "../commun/TranslationManager.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QMessageBox>

NewAccount2Page::NewAccount2Page(const QString& otpUri, const QString& folderName, QWidget* parent)
    : QWidget(parent)
    , m_otpUri(otpUri)
    , m_folderName(folderName)
{
    buildUi();
    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &NewAccount2Page::retranslateUi);
}

void NewAccount2Page::buildUi()
{
    m_lblPrompt = new QLabel(tr("Nom du compte :"));
    m_editName = new QLineEdit();
    m_btnSave = new QPushButton(tr("Enregistrer"));
    m_btnBack = new QPushButton(tr("Retour"));

    QHBoxLayout* btnLayout = new QHBoxLayout();
    btnLayout->addWidget(m_btnBack);
    btnLayout->addStretch();
    btnLayout->addWidget(m_btnSave);

    QVBoxLayout* layout = new QVBoxLayout(this);
    layout->setContentsMargins(20, 20, 20, 20);
    layout->addStretch();
    layout->addWidget(m_lblPrompt);
    layout->addWidget(m_editName);
    layout->addStretch();
    layout->addLayout(btnLayout);

    connect(m_btnSave, &QPushButton::clicked, this, &NewAccount2Page::onSaveClicked);
    connect(m_btnBack, &QPushButton::clicked, this, &NewAccount2Page::navigateBack);
}

void NewAccount2Page::onSaveClicked()
{
    QString name = m_editName->text().trimmed();
    if (name.isEmpty())
    {
        QMessageBox::warning(this, tr("Erreur"), tr("Le nom du compte ne peut pas être vide."));
        return;
    }

    // Reconstruction propre : on garde la query string intacte
    // otpauth://totp/?secret=XXX  →  otpauth://totp/NOM?secret=XXX
    QUrl url(m_otpUri);
    QString query = url.query();   // "secret=XXX&digits=6&period=30"
    QString finalUri = "otpauth://totp/" + QUrl::toPercentEncoding(name) + "?" + query;
    QString line = m_folderName + "\\" + name + ";" + finalUri;

    AccountManager am;
    am.addAccount(line);

    emit accountSaved();
}

void NewAccount2Page::retranslateUi()
{
    m_lblPrompt->setText(tr("Nom du compte :"));
    m_btnSave->setText(tr("Enregistrer"));
    m_btnBack->setText(tr("Retour"));
}