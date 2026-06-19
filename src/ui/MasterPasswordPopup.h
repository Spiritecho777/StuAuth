#pragma once

#include <QDialog>
#include <QString>

class QLineEdit;
class QLabel;
class QPushButton;

class MasterPasswordPopup : public QDialog
{
    Q_OBJECT

public:
    explicit MasterPasswordPopup(const QString& title,
        const QString& prompt,
        QWidget* parent = nullptr);

    QString password() const;

private:
    QLineEdit* m_edit = nullptr;
    QLabel* m_label = nullptr;
    QPushButton* m_btnOk = nullptr;
    QPushButton* m_btnCancel = nullptr;
};