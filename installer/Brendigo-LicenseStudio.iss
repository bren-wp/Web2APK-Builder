#define MyAppName "Brendigo License Studio"
#define MyAppVersion "0.6.7"
#define MyAppPublisher "Brendigo"
#define MyAppExeName "Brendigo.LicenseStudio.exe"
[Setup]
AppId={{0C0EF207-3A5F-4BC6-B56B-056724D974C1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://brendigo.com
DefaultDirName={localappdata}\Programs\Brendigo License Studio
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\release
OutputBaseFilename=Brendigo-LicenseStudio-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
VersionInfoVersion=0.6.2.0
UninstallDisplayIcon={app}\{#MyAppExeName}
[Files]
Source: "..\publish-license\Brendigo.LicenseStudio.exe"; DestDir: "{app}"; Flags: ignoreversion
[Icons]
Name: "{autoprograms}\Brendigo License Studio"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Brendigo License Studio"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Stvori prečac na radnoj površini"; GroupDescription: "Dodatne opcije:"; Flags: unchecked
[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Pokreni Brendigo License Studio"; Flags: nowait postinstall skipifsilent
