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

    // Reconstruction de l'URI avec le nom : otpauth://totp/NOM?secret=...
    QStringList parts = m_otpUri.split('/');
    if (parts.size() >= 4)
    {
        // Remplace la partie label dans l'URI
        parts[3] = QUrl::toPercentEncoding(name) + (parts.size() > 4 ? "" : "");
        if (parts.size() == 4)
        {
            // URI courte : otpauth://totp/?secret=...  → on insère le nom
            QString query = parts[3].section('?', 1);
            parts[3] = QUrl::toPercentEncoding(name) + "?" + query;
        }
    }

    QString finalUri = parts.join('/');
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