cmake_minimum_required(VERSION 3.16)

project(StuAuth
    VERSION 4.0.0
    LANGUAGES CXX
)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTORCC ON)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# -------------------------
# Version appli (unique)
# -------------------------
set(APP_VERSION "4.0.0")
string(REPLACE "." "," APP_VERSION_COMMA "${APP_VERSION}")
add_definitions(-DAPP_VERSION="${APP_VERSION}")

# ------------------------- 
# Génération du fichier RC 
# ------------------------- 
if (WIN32) 
    string(REPLACE "\\" "/" CMAKE_SOURCE_DIR_FIXED "${CMAKE_SOURCE_DIR}")
    configure_file( 
        ${CMAKE_SOURCE_DIR}/app.rc.in 
        ${CMAKE_BINARY_DIR}/app.rc 
        @ONLY 
    ) 
    # Copie l'icône dans le build dir pour que le .rc la trouve
    configure_file(
        ${CMAKE_SOURCE_DIR}/Asset/Icone.ico
        ${CMAKE_BINARY_DIR}/Asset/Icone.ico
        COPYONLY
    )

    #configure_file(
    #    ${CMAKE_SOURCE_DIR}/Asset/IconeOFF.ico
    #    ${CMAKE_BINARY_DIR}/Asset/IconeOFF.ico
    #    COPYONLY
    #)

    #configure_file(
    #    ${CMAKE_SOURCE_DIR}/Asset/IconeON.ico
    #    ${CMAKE_BINARY_DIR}/Asset/IconeON.ico
    #    COPYONLY
    #)
endif()

# -------------------------
# OpenSSL
# -------------------------
if(WIN32)
    set(OPENSSL_USE_STATIC_LIBS TRUE)
endif()
find_package(OpenSSL REQUIRED)
if (WIN32)
    add_definitions(-DOPENSSL_USE_STATIC_LIBS)
endif()

# -------------------------
# Qt Widgets (statique)
# -------------------------
find_package(Qt6 REQUIRED COMPONENTS Widgets)

# -------------------------
# Sources / Headers
# -------------------------
file(GLOB_RECURSE SOURCES
    "${CMAKE_SOURCE_DIR}/src/commun/*.cpp"
    "${CMAKE_SOURCE_DIR}/src/ui/*.cpp"
    "${CMAKE_SOURCE_DIR}/src/core/*.cpp"
    "${CMAKE_SOURCE_DIR}/src/network/*.cpp"
)

file(GLOB_RECURSE HEADERS
    "${CMAKE_SOURCE_DIR}/src/commun/*.h"
    "${CMAKE_SOURCE_DIR}/src/ui/*.h"
    "${CMAKE_SOURCE_DIR}/src/core/*.h"
    "${CMAKE_SOURCE_DIR}/src/network/*.h"
)

# -------------------------
# Exécutable
# -------------------------
qt_add_executable(${PROJECT_NAME}
    src/main.cpp
    ${SOURCES}
    ${HEADERS}
)

# -------------------------
# Copie Asset
# -------------------------
add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${CMAKE_SOURCE_DIR}/Asset
        $<TARGET_FILE_DIR:${PROJECT_NAME}>/Asset
)

# -------------------------
# Ressources Qt (Windows + Linux)
# -------------------------
qt_add_resources(${PROJECT_NAME} "resources"
    PREFIX "/icons"
    FILES
        Asset/Icone.png
        #Asset/IconeOFF.png
        #Asset/IconeON.png
)

qt_add_resources(${PROJECT_NAME} "translations"
    PREFIX "/translations"
    FILES
        Asset/translations/stuauth_fr.qm
        Asset/translations/stuauth_en.qm
        Asset/translations/stuauth_bz.qm
        Asset/translations/stuauth_ja.qm
)

# -------------------------
# Ressources Windows (.rc)
# -------------------------
if (WIN32)
    # Force le compilateur de ressources
    set_source_files_properties(${CMAKE_BINARY_DIR}/app.rc
        PROPERTIES
            LANGUAGE RC
    )
    target_sources(${PROJECT_NAME} PRIVATE app.rc "src/commun/LockManager.cpp" "src/commun/LockManager.h")
    set_target_properties(${PROJECT_NAME}
        PROPERTIES
            WIN32_EXECUTABLE TRUE
    )
endif()

# -------------------------
# Includes
# -------------------------
target_include_directories(${PROJECT_NAME}
    PRIVATE
        
)

# -------------------------
# Link
# -------------------------
target_link_libraries(${PROJECT_NAME}
    PRIVATE
        Qt6::Widgets
        OpenSSL::SSL
        OpenSSL::Crypto
        
)

# -------------------------
# Link Windows
# -------------------------
if (WIN32)
    target_link_libraries(${PROJECT_NAME}
        PRIVATE
            ws2_32
            advapi32
            crypt32
    )
endif()

# -------------------------
# Link Linux
# -------------------------
if (UNIX AND NOT APPLE)
    target_link_libraries(${PROJECT_NAME} 
        PRIVATE 
        
    )
endif()