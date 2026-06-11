#pragma once
#include <QObject>
#include <QTranslator>
#include <QSettings>

class TranslationManager : public QObject {
    Q_OBJECT

public:
    static TranslationManager& instance();
    void setLanguage(const QString& lang);
    QString currentLanguage() const;
    void loadSavedLanguage();

signals:
    void languageChanged();

private:
    TranslationManager() = default;
    QTranslator m_translator;
	QSettings m_settings;
	QString m_currentLanguage;
};