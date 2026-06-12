#pragma once

#include <QWidget>
#include <QLabel>
#include <QPushButton>
#include <QTimer>
#include <QProgressBar>

class SelectAccountPage : public QWidget
{
    Q_OBJECT

public:
    explicit SelectAccountPage(const QString& name, const QString& otpUri,
        QWidget* parent = nullptr);
    ~SelectAccountPage();

signals:
    void navigateBack();

private slots:
    void onTick();
    void onCopyClicked();
    void retranslateUi();

private:
    void buildUi();
    void refreshOtp();

    QString      m_name;
    QString      m_otpUri;

    QLabel* m_lblName = nullptr;
    QLabel* m_lblCode = nullptr;
    QLabel* m_lblTimer = nullptr;
    QProgressBar* m_progress = nullptr;
    QPushButton* m_btnCopy = nullptr;
    QPushButton* m_btnBack = nullptr;

    QTimer* m_timer = nullptr;
};