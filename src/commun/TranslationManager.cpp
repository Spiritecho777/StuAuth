#include "TranslationManager.h"
#include <QApplication>

TranslationManager& TranslationManager::instance() {
    static TranslationManager inst;
    return inst;
}

void TranslationManager::setLanguage(const QString& lang) {
    qApp->removeTranslator(&m_translator);

    if (m_translator.load(":/translations/Asset/translations/stuauth_" + lang + ".qm")) {
        qApp->installTranslator(&m_translator);
		m_currentLanguage = lang;
		m_settings.setValue("language", lang);
        emit languageChanged();
    }
}

QString TranslationManager::currentLanguage() const {
    return m_currentLanguage;
}

void TranslationManager::loadSavedLanguage() {
    QString lang = m_settings.value("language", "en").toString();
    setLanguage(lang);
}