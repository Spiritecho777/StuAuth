#pragma once

#include <QWidget>
#include <QPushButton>
#include <QLineEdit>
#include <QLabel>
#include <QStringList>

class NewAccountPage : public QWidget
{
    Q_OBJECT

public:
    explicit NewAccountPage(const QString& folderName, QWidget* parent = nullptr);

signals:
    void navigateBack();
    void navigateToNaming(const QString& otpUri);
    void navigateToImport(const QStringList& accounts);

private slots:
    void onCaptureClicked();
    void onImportClicked();
    void onConfirmClicked();
    void retranslateUi();

private:
    void buildUi();

    QString      m_folderName;

    QLabel* m_lblSecret = nullptr;
    QLineEdit* m_editSecret = nullptr;
    QPushButton* m_btnCapture = nullptr;
    QPushButton* m_btnConfirm = nullptr;
    QPushButton* m_btnImport = nullptr;
    QPushButton* m_btnBack = nullptr;
};