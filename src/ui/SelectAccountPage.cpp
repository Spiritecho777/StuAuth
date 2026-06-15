#include "SelectAccountPage.h"
#include "../core/TotpEngine.h"
#include "../commun/TranslationManager.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QClipboard>
#include <QApplication>
#include <QFont>
#include <QDebug>

SelectAccountPage::SelectAccountPage(const QString& name, const QString& otpUri, QWidget* parent)
    : QWidget(parent)
    , m_name(name)
    , m_otpUri(otpUri)
{
    buildUi();
    refreshOtp();

    m_timer = new QTimer(this);
    m_timer->setInterval(1000);
    connect(m_timer, &QTimer::timeout, this, &SelectAccountPage::onTick);
    m_timer->start();

    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &SelectAccountPage::retranslateUi);
}

SelectAccountPage::~SelectAccountPage()
{
    if (m_timer) m_timer->stop();
}

void SelectAccountPage::buildUi()
{
    m_lblName = new QLabel(m_name.split('\\').value(1, m_name));
    QFont nameFont = m_lblName->font();
    nameFont.setPointSize(16);
    m_lblName->setFont(nameFont);
    m_lblName->setAlignment(Qt::AlignLeft);

    m_lblCode = new QLabel("------");
    QFont codeFont = m_lblCode->font();
    codeFont.setPointSize(40);
    codeFont.setBold(true);
    m_lblCode->setFont(codeFont);
    m_lblCode->setAlignment(Qt::AlignCenter);

    m_lblTimer = new QLabel("30");
    m_lblTimer->setAlignment(Qt::AlignCenter);

    m_progress = new QProgressBar();
    m_progress->setRange(0, 30);
    m_progress->setTextVisible(false);
    m_progress->setFixedHeight(8);

    m_btnCopy = new QPushButton(tr("Copier"));
    m_btnBack = new QPushButton(tr("Retour"));

    QHBoxLayout* btnLayout = new QHBoxLayout();
    btnLayout->addWidget(m_btnBack);
    btnLayout->addStretch();
    btnLayout->addWidget(m_btnCopy);

    QVBoxLayout* layout = new QVBoxLayout(this);
    layout->setContentsMargins(20, 20, 20, 20);
    layout->addWidget(m_lblName);
    layout->addStretch();
    layout->addWidget(m_lblCode);
    layout->addWidget(m_lblTimer);
    layout->addWidget(m_progress);
    layout->addStretch();
    layout->addLayout(btnLayout);

    connect(m_btnCopy, &QPushButton::clicked, this, &SelectAccountPage::onCopyClicked);
    connect(m_btnBack, &QPushButton::clicked, this, &SelectAccountPage::navigateBack);
}

void SelectAccountPage::refreshOtp()
{
    QString secret = TotpEngine::extractSecret(m_otpUri);
    QString code = TotpEngine::generate(secret);
    int remaining = TotpEngine::secondsRemaining();

    m_lblCode->setText(code);
    m_lblTimer->setText(QString::number(remaining) + "s");
    m_progress->setValue(remaining);
}

void SelectAccountPage::onTick()
{
    refreshOtp();
}

void SelectAccountPage::onCopyClicked()
{
    QApplication::clipboard()->setText(m_lblCode->text());
}

void SelectAccountPage::retranslateUi()
{
    m_btnCopy->setText(tr("Copier"));
    m_btnBack->setText(tr("Retour"));
}