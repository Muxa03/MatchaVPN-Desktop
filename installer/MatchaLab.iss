#define AppVer "1.3.5"

[Setup]
AppId={{B7C51E86-52D4-4B7A-9F3E-6A1D22C4A9B1}
AppName=MatchaLab VPN
AppVersion={#AppVer}
AppPublisher=MatchaLab
AppPublisherURL=https://matchavpn.space
DefaultDirName={sd}\MatchaLab
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputDir=Output
OutputBaseFilename=MatchaLab-Setup-{#AppVer}
SetupIconFile=..\src\MatchaLab.App\Assets\appicon.ico
LicenseFile=terms.txt
UninstallDisplayIcon={app}\MatchaLab.exe
UninstallDisplayName=MatchaLab VPN
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
Compression=lzma2
SolidCompression=yes

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Дополнительно:"
Name: "autostart"; Description: "Запускать при входе в Windows (свёрнутым)"; GroupDescription: "Дополнительно:"; Flags: unchecked

[Files]
Source: "..\src\MatchaLab.App\bin\Release\net8.0\win-x64\publish\MatchaLab.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\native\tunnel.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\native\wintun.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\native\sing-box.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\MatchaLab VPN"; Filename: "{app}\MatchaLab.exe"
Name: "{autodesktop}\MatchaLab VPN"; Filename: "{app}\MatchaLab.exe"; Tasks: desktopicon

[Run]
Filename: "schtasks"; Parameters: "/Create /F /RL HIGHEST /SC ONLOGON /TN ""MatchaLab"" /TR ""\""{app}\MatchaLab.exe\"" --min"""; Flags: runhidden; Tasks: autostart
Filename: "{app}\MatchaLab.exe"; Description: "Запустить MatchaLab"; Flags: nowait postinstall skipifsilent runascurrentuser

[UninstallRun]
Filename: "taskkill"; Parameters: "/IM MatchaLab.exe /F"; Flags: runhidden; RunOnceId: "KillApp"
Filename: "sc.exe"; Parameters: "stop MatchaLabTunnel"; Flags: runhidden; RunOnceId: "StopSvc"
Filename: "sc.exe"; Parameters: "delete MatchaLabTunnel"; Flags: runhidden; RunOnceId: "DelSvc"
Filename: "schtasks"; Parameters: "/Delete /F /TN ""MatchaLab"""; Flags: runhidden; RunOnceId: "DelTask"
