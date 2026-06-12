#include "NewAccountPage.h"
#include "../core/ProtoParser.h"
#include "../commun/TranslationManager.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QMessageBox>
#include <QScreen>
#include <QGuiApplication>
#include <QRubberBand>
#include <QMouseEvent>

// ─────────────────────────────────────────────
//  Widget de sélection de zone écran
//  (remplace le Form WinForms de capture QR)
// ─────────────────────────────────────────────
class ScreenCapture : public QWidget
{
    Q_OBJECT
public:
    explicit ScreenCapture(const QPixmap& screen, QWidget* parent = nullptr)
        : QWidget(parent, Qt::Window | Qt::FramelessWindowHint)
        , m_screen(screen)
        , m_rubber(new QRubberBand(QRubberBand::Rectangle, this))
    {
        setWindowState(Qt::WindowFullScreen);
        setCursor(Qt::CrossCursor);
        setMouseTracking(true);
    }

signals:
    void regionSelected(const QPixmap& region);

protected:
    void paintEvent(QPaintEvent*) override
    {
        QPainter p(this);
        p.drawPixmap(0, 0, m_screen);
        p.fillRect(rect(), QColor(0, 0, 0, 80));
    }

    void mousePressEvent(QMouseEvent* e) override { m_origin = e->pos(); m_rubber->show(); }
    void mouseMoveEvent(QMouseEvent* e) override
    {
        if (e->buttons() & Qt::LeftButton)
            m_rubber->setGeometry(QRect(m_origin, e->pos()).normalized());
    }
    void mouseReleaseEvent(QMouseEvent* e) override
    {
        QRect sel = QRect(m_origin, e->pos()).normalized();
        hide();
        if (sel.isValid())
            emit regionSelected(m_screen.copy(sel));
        close();
    }

private:
    QPixmap     m_screen;
    QRubberBand* m_rubber;
    QPoint      m_origin;
};

#include "NewAccountPage.moc"   // nécessaire pour le Q_OBJECT dans le .cpp

// ─────────────────────────────────────────────
//  NewAccountPage
// ─────────────────────────────────────────────

NewAccountPage::NewAccountPage(const QString& folderName, QWidget* parent)
    : QWidget(parent)
    , m_folderName(folderName)
{
    buildUi();
    connect(&TranslationManager::instance(), &TranslationManager::languageChanged,
        this, &NewAccountPage::retranslateUi);
}

void NewAccountPage::buildUi()
{
    m_lblSecret = new QLabel(tr("Clé secrète :"));
    m_editSecret = new QLineEdit();
    m_editSecret->setPlaceholderText("JBSWY3DPEHPK3PXP");

    m_btnCapture = new QPushButton(tr("Scanner QR"));
    m_btnConfirm = new QPushButton(tr("Confirmer"));
    m_btnImport = new QPushButton(tr("Importer"));
    m_btnBack = new QPushButton(tr("Retour"));

    QHBoxLayout* topBar = new QHBoxLayout();
    topBar->addWidget(m_btnBack);
    topBar->addStretch();
    topBar->addWidget(m_btnImport);

    QHBoxLayout* botBar = new QHBoxLayout();
    botBar->addWidget(m_btnCapture);
    botBar->addStretch();
    botBar->addWidget(m_btnConfirm);

    QVBoxLayout* layout = new QVBoxLayout(this);
    layout->setContentsMargins(20, 20, 20, 20);
    layout->addLayout(topBar);
    layout->addStretch();
    layout->addWidget(m_lblSecret);
    layout->addWidget(m_editSecret);
    layout->addLayout(botBar);

    connect(m_btnCapture, &QPushButton::clicked, this, &NewAccountPage::onCaptureClicked);
    connect(m_btnConfirm, &QPushButton::clicked, this, &NewAccountPage::onConfirmClicked);
    connect(m_btnImport, &QPushButton::clicked, this, &NewAccountPage::onImportClicked);
    connect(m_btnBack, &QPushButton::clicked, this, &NewAccountPage::navigateBack);
}

void NewAccountPage::onConfirmClicked()
{
    QString secret = m_editSecret->text().trimmed().toUpper();

    // Validation : alphanumérique uniquement
    static QRegularExpression re("^[A-Z2-7]+$");
    if (secret.isEmpty() || !re.match(secret).hasMatch())
    {
        QMessageBox::warning(this, tr("Erreur"),
            tr("La clé secrète doit être en Base32 (lettres A-Z et chiffres 2-7)."));
        m_editSecret->clear();
        return;
    }

    QString uri = "otpauth://totp/?secret=" + secret + "&digits=6&period=30";
    emit navigateToNaming(uri);
}

void NewAccountPage::onImportClicked()
{
    emit navigateToImport({});
}

void NewAccountPage::onCaptureClicked()
{
    // Capture écran + détection QR
    QScreen* screen = QGuiApplication::primaryScreen();
    QPixmap  shot = screen->grabWindow(0);

    auto* capture = new ScreenCapture(shot);

    connect(capture, &ScreenCapture::regionSelected, this, [this](const QPixmap& region)
        {
            // TODO : décoder le QR avec ZXing-C++ ici
            // Exemple d'intégration ZXing :
            //   QImage img = region.toImage().convertToFormat(QImage::Format_RGB32);
            //   ZXing::ImageView view(img.bits(), img.width(), img.height(), ZXing::ImageFormat::XRGB);
            //   auto result = ZXing::ReadBarcode(view);
            //   if (result.isValid()) processQrText(result.text());

            // Stub temporaire : demande manuelle si ZXing non intégré
            Q_UNUSED(region)
                QMessageBox::information(this, tr("QR Code"),
                    tr("Intégrez ZXing-C++ pour activer la détection automatique."));
        });

    window()->hide();
    capture->showFullScreen();

    connect(capture, &QWidget::destroyed, this, [this]() {
        window()->show();
        });
}

void NewAccountPage::retranslateUi()
{
    m_lblSecret->setText(tr("Clé secrète :"));
    m_btnCapture->setText(tr("Scanner QR"));
    m_btnConfirm->setText(tr("Confirmer"));
    m_btnImport->setText(tr("Importer"));
    m_btnBack->setText(tr("Retour"));
}