#pragma once

#include <QWidget>
#include <QListWidget>
#include <QPushButton>
#include <QStringList>

class ImportPage : public QWidget
{
    Q_OBJECT

public:
    // accounts vide → ouvre un fichier texte
    // accounts rempli → vient d'un QR Google Auth
    explicit ImportPage(const QStringList& accounts, const QString& folderName,
        QWidget* parent = nullptr);

signals:
    void importDone();
    void navigateBack();

private slots:
    void onConfirmClicked();
    void retranslateUi();

private:
    void buildUi();
    void loadFromFile();
    void loadFromList(const QStringList& accounts);
    void parseAndAddItem(const QString& otpauthUri);

    QString      m_folderName;
    QListWidget* m_list = nullptr;
    QPushButton* m_btnOk = nullptr;
    QPushButton* m_btnBack = nullptr;
};