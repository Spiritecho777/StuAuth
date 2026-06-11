#define ExeName ProjectName + ".exe"

; ============================
; Configuration générale
; ============================
[Setup]
AppId={{#AppId}}
AppName={#ProjectName}
AppVersion={#ProjectVersion}

DefaultDirName={pf}\{#ProjectName}
DefaultGroupName={#ProjectName}

OutputDir=..\Deploy
OutputBaseFilename={#ProjectName}_Setup_{#ProjectVersion}

Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

; Icône de l’installeur
SetupIconFile=..\Asset\Icone.ico

; Icône affichée dans Ajout/Suppression de programmes
UninstallDisplayIcon={app}\{#ExeName}

; ============================
; Langue
; ============================
[Languages]
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"

; ============================
; Tâches optionnelles
; ============================
[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le bureau"; GroupDescription: "Raccourcis :"; Flags: unchecked

; ============================
; Fichiers à installer
; ============================
[Files]
Source: "..\Deploy\Windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; ============================
; Raccourcis
; ============================
[Icons]
; Menu démarrer
Name: "{group}\{#ProjectName}"; Filename: "{app}\{#ExeName}"

; Bureau
Name: "{commondesktop}\{#ProjectName}"; Filename: "{app}\{#ExeName}"; Tasks: desktopicon