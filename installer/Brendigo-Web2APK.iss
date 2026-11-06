#define MyAppName "Brendigo Web2APK Builder"
#define MyAppVersion "0.6.6"
#define MyAppPublisher "Brendigo"
#define MyAppExeName "Brendigo.Web2APK.exe"

[Setup]
AppId={{3B3545B5-D09B-46DB-B3C6-768149F2E602}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://brendigo.com
DefaultDirName={localappdata}\Programs\Brendigo Web2APK Builder
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\release
OutputBaseFilename=Brendigo-Web2APK-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
RestartApplications=yes
VersionInfoVersion=0.6.2.0

[Files]
Source: "..\publish\Brendigo.Web2APK.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Brendigo Web2APK Builder"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Brendigo Web2APK Builder"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Stvori ikonu na radnoj površini"; GroupDescription: "Dodatne opcije:"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Pokreni Brendigo Web2APK Builder"; Flags: nowait postinstall skipifsilent
