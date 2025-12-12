;汉化:MonKeyDu 
;由 Inno Setup 脚本向导 生成的脚本,有关创建 INNO SETUP 脚本文件的详细信息，请参阅文档！!

#include "CodeDependencies.iss"
#define MyAppName "bp-idvevent"
; Extract File Version from EXE
#define MyAppVersion GetFileVersion("..\build\neo-bpsys-wpf\neo-bpsys-wpf.exe")
#define MyAppPublisher "罗澜, PLFJY"
#define MyAppURL "https://bpsys.plfjy.top/"
#define MyAppExeName "neo-bpsys-wpf.exe"

[Setup]
;注意:AppId 的值唯一标识此应用程序。请勿在安装程序中对其他应用程序使用相同的 AppId 值。
;（若要生成新的 GUID，请单击“工具”|”在 IDE 中生成 GUID）。
AppId={{842859C0-E6A4-4997-BA10-0933EC09444F}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName}-v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
DisableWelcomePage=no
DisableReadypage=yes
;下行注释，指定安装程序无法运行，除 Arm 上的 x64 和 Windows 11 之外的任何平台上.
ArchitecturesAllowed=x64compatible
WizardImageFile=侧图186x356.bmp
;WizardSmallImageFile=顶图165x54.bmp
WizardSmallImageFile=顶图54x54.bmp
;下行注释，强制安装程序在 64 位系统上，但不强制以 64 位模式运行.
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
;下面两行注释是License文件和InfoShown
LicenseFile=隐私政策.txt
InfoBeforeFile=StartInfo.txt
;取消下行前面 ; 符号，在非管理安装模式下运行（仅为当前用户安装）.
;PrivilegesRequired=lowest
OutputDir=..\build\
OutputBaseFilename=bp-idvevent_Installer
SetupIconFile=..\neo-bpsys-wpf\favicon.ico
SolidCompression=yes
WizardStyle=modern
WizardResizable=yes
WizardSizePercent=125,125

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkablealone

[Files]
Source: "..\build\neo-bpsys-wpf\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\build\neo-bpsys-wpf\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[code]
procedure URLLabelOnClick(Sender: TObject);
var
  ResultCode: Integer;
begin
  ShellExec('', '{#MyAppURL}', '', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
end;

procedure InitializeWizard();
var
  TitleLabel, SubTitleLabel, NoteLabel, FeaturesLabel, WebsiteLabel: TLabel;
begin
  WizardForm.LICENSEACCEPTEDRADIO.Checked := True;
  WizardForm.WelcomeLabel1.Visible := False;
  WizardForm.WelcomeLabel2.Visible := False;

  TitleLabel := TLabel.Create(WizardForm);
  TitleLabel.Parent := WizardForm.WelcomePage;
  TitleLabel.Left := 24;
  TitleLabel.Top := 24;
  TitleLabel.Font.Size := 20;
  TitleLabel.Font.Name := 'Segoe UI';
  try
    TitleLabel.Font.Style := TitleLabel.Font.Style + [fsBold];
  except
  end;
  TitleLabel.Font.Color := clNavy;
  TitleLabel.Caption := '{#MyAppName}  v{#MyAppVersion}';

  SubTitleLabel := TLabel.Create(WizardForm);
  SubTitleLabel.Parent := WizardForm.WelcomePage;
  SubTitleLabel.Left := 24;
  SubTitleLabel.Top := TitleLabel.Top + TitleLabel.Height + 8;
  SubTitleLabel.Font.Size := 10;
  SubTitleLabel.Font.Name := 'Segoe UI';
  SubTitleLabel.Caption := '开源、轻量、可扩展的桌面应用';
  SubTitleLabel.AutoSize := True;

  FeaturesLabel := TLabel.Create(WizardForm);
  FeaturesLabel.Parent := WizardForm.WelcomePage;
  FeaturesLabel.Left := 24;
  FeaturesLabel.Top := SubTitleLabel.Top + SubTitleLabel.Height + 10;
  FeaturesLabel.Font.Size := 10;
  FeaturesLabel.Font.Name := 'Segoe UI';
  FeaturesLabel.Caption := '• 快速安装' + #13#10 + '• 轻量无广告' + #13#10 + '• 自动安装依赖';
  FeaturesLabel.AutoSize := True;

  NoteLabel := TLabel.Create(WizardForm);
  NoteLabel.Parent := WizardForm.WelcomePage;
  NoteLabel.Left := 24;
  NoteLabel.Top := FeaturesLabel.Top + FeaturesLabel.Height + 10;
  NoteLabel.Font.Size := 9;
  NoteLabel.Font.Name := 'Segoe UI';
  NoteLabel.Caption := '安装完成后将创建开始菜单和桌面快捷方式。';
  NoteLabel.AutoSize := True;

  WebsiteLabel := TLabel.Create(WizardForm);
  WebsiteLabel.Parent := WizardForm.WelcomePage;
  WebsiteLabel.Left := 24;
  WebsiteLabel.Top := NoteLabel.Top + NoteLabel.Height + 10;
  WebsiteLabel.Font.Size := 9;
  WebsiteLabel.Font.Name := 'Segoe UI';
  WebsiteLabel.Font.Color := clBlue;
  WebsiteLabel.Cursor := crHand;
  WebsiteLabel.Caption := '{#MyAppURL}';
  WebsiteLabel.OnClick := @URLLabelOnClick;
end;
function InitializeSetup: Boolean;
begin
Dependency_AddDotNet90Desktop;
Result := True;
end;
//卸载时删除用户数据
procedure CurUninstallStepChanged (CurUninstallStep: TUninstallStep);
var
    mres : integer;
begin
   case CurUninstallStep of
     usUninstall:
       begin
         mres := MsgBox('是否删除用户数据？(包括日志、自定义UI、自定义设置)', mbConfirmation, MB_YESNO or MB_DEFBUTTON2)
         if mres = IDYES then
           DelTree(ExpandConstant('{userappdata}\bp-idvevent'), True, True, True);
      end;
  end;
end;

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: postinstall shellexec skipifdoesntexist
