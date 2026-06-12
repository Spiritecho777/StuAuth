#pragma once

#include <QWidget>
#include <QLineEdit>
#include <QPushButton>
#include <QLabel>

class NewAccount2Page : public QWidget
{
    Q_OBJECT

public:
    explicit NewAccount2Page(const QString& otpUri, const QString& folderName,
        QWidget* parent = nullptr);

signals:
    void accountSaved();
    void navigateBack();

private slots:
    void onSaveClicked();
    void retranslateUi();

private:
    void buildUi();

    QString      m_otpUri;
    QString      m_folderName;

    QLabel* m_lblPrompt = nullptr;
    QLineEdit* m_editName = nullptr;
    QPushButton* m_btnSave = nullptr;
    QPushButton* m_btnBack = nullptr;
};