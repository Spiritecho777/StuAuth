#include "MasterPasswordPopup.h"

#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QLineEdit>
#include <QLabel>
#include <QPushButton>

MasterPasswordPopup::MasterPasswordPopup(const QString& title,
    const QString& prompt,
    QWidget* parent)
    : QDialog(parent)
{
    setWindowTitle(title);
    setModal(true);
    resize(340, 130);

    auto* layout = new QVBoxLayout(this);

    m_label = new QLabel(prompt, this);
    m_edit = new QLineEdit(this);
    m_edit->setEchoMode(QLineEdit::Password);

    auto* buttons = new QHBoxLayout();
    m_btnOk = new QPushButton(tr("Valider"), this);
    m_btnCancel = new QPushButton(tr("Annuler"), this);

    buttons->addStretch();
    buttons->addWidget(m_btnOk);
    buttons->addWidget(m_btnCancel);

    layout->addWidget(m_label);
    layout->addWidget(m_edit);
    layout->addLayout(buttons);

    connect(m_btnOk, &QPushButton::clicked, this, [this]() {
        accept();
        });

    connect(m_btnCancel, &QPushButton::clicked, this, [this]() {
        reject();
        });
}

QString MasterPasswordPopup::password() const
{
    return m_edit->text();
}