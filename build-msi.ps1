param(
  [ValidateSet('msix','msi')]
  [string]$Format = 'msix',
  [string]$PublisherDisplayName = '罗澜嘎嘎',
  [string]$PublisherCN = '罗澜嘎嘎',
  [string]$IdentityName,
  [string]$DisplayName,
  [bool]$SelfContained = $true,
  [string]$RuntimeId = 'win-x64'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root
$AppName = 'bp-idvevent'
$AppIdName = ($AppName -replace '[^A-Za-z0-9]', '')
if (-not $IdentityName) { $IdentityName = $AppIdName }
if (-not $DisplayName) { $DisplayName = $AppName }
$BuildPath = Join-Path $root 'build/neo-bpsys-wpf'
$ProjPath = Join-Path $root 'neo-bpsys-wpf/neo-bpsys-wpf.csproj'
New-Item -ItemType Directory -Path $BuildPath -Force | Out-Null
if ($SelfContained) {
  dotnet publish $ProjPath -c Release -o $BuildPath -r $RuntimeId --self-contained true
} else {
  dotnet publish $ProjPath -c Release -o $BuildPath
}
$exePath = Join-Path $BuildPath 'neo-bpsys-wpf.exe'
$version = '1.0.0.0'
try {
  $fv = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath).FileVersion
  if ($fv) {
    $parts = $fv.Split('.')
    if ($parts.Length -ge 3) { $version = "$($parts[0]).$($parts[1]).$($parts[2]).0" } else { $version = "$fv.0.0.0" }
  }
} catch {}
if ($Format -ieq 'msix') {
  $staging = Join-Path $root ("build/msix/$AppName")
  New-Item -ItemType Directory -Path $staging -Force | Out-Null
  Copy-Item -Path (Join-Path $BuildPath '*') -Destination $staging -Recurse -Force
  $assetsDir = Join-Path $staging 'Assets'
  New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null
  $iconSrc = Join-Path $root 'neo-bpsys-wpf/Assets/icon.png'
  if (Test-Path $iconSrc) {
    Copy-Item $iconSrc (Join-Path $assetsDir 'Square150x150Logo.png') -Force
    Copy-Item $iconSrc (Join-Path $assetsDir 'Square44x44Logo.png') -Force
    Copy-Item $iconSrc (Join-Path $assetsDir 'StoreLogo.png') -Force
  }
  $manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
         IgnorableNamespaces="uap desktop rescap">
  <Identity Name="$IdentityName" Publisher="CN=$PublisherCN" Version="$version" />
  <Properties>
    <DisplayName>$DisplayName</DisplayName>
    <PublisherDisplayName>$PublisherDisplayName</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0" />
  </Dependencies>
  <Resources>
    <Resource Language="zh-CN" />
  </Resources>
  <Applications>
    <Application Id="$AppIdName" Executable="neo-bpsys-wpf.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="$DisplayName" Description="$DisplayName" BackgroundColor="transparent" Square150x150Logo="Assets\Square150x150Logo.png" Square44x44Logo="Assets\Square44x44Logo.png" />
      <Extensions>
        <desktop:Extension Category="windows.fullTrustProcess">
          <desktop:FullTrustProcess />
        </desktop:Extension>
      </Extensions>
    </Application>
  </Applications>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
"@
  Set-Content -Path (Join-Path $staging 'AppxManifest.xml') -Value $manifest -Encoding UTF8
  $makeappxCmd = Get-Command makeappx.exe -ErrorAction SilentlyContinue
  if ($makeappxCmd) { $makeappx = $makeappxCmd.Path } else {
    $makeappx = $null
    $bins = @('C:\Program Files (x86)\Windows Kits\10\bin','C:\Program Files\Windows Kits\10\bin','C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages\microsoft.windows.sdk.buildtools')
    foreach ($b in $bins) {
      if (Test-Path $b) {
        $m = Get-ChildItem -Path $b -Recurse -Filter 'makeappx.exe' -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match '\\x64\\makeappx\.exe$' } | Select-Object -First 1
        if (-not $m) { $m = Get-ChildItem -Path $b -Recurse -Filter 'makeappx.exe' -ErrorAction SilentlyContinue | Select-Object -First 1 }
        if ($m) { $makeappx = $m.FullName; break }
      }
    }
    if (-not $makeappx) { throw 'makeappx.exe not found' }
  }
  $msixOut = Join-Path $root ("build/$AppName.msix")
  & $makeappx pack /o /d $staging /p $msixOut
  $signtoolCmd = Get-Command signtool.exe -ErrorAction SilentlyContinue
  if ($signtoolCmd) { $signtool = $signtoolCmd.Path } else {
    $signtool = $null
    $bins = @('C:\Program Files (x86)\Windows Kits\10\bin','C:\Program Files\Windows Kits\10\bin','C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages\microsoft.windows.sdk.buildtools')
    foreach ($b in $bins) {
      if (Test-Path $b) {
        $s = Get-ChildItem -Path $b -Recurse -Filter 'signtool.exe' -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match '\\x64\\signtool\.exe$' } | Select-Object -First 1
        if (-not $s) { $s = Get-ChildItem -Path $b -Recurse -Filter 'signtool.exe' -ErrorAction SilentlyContinue | Select-Object -First 1 }
        if ($s) { $signtool = $s.FullName; break }
      }
    }
    if (-not $signtool) { throw 'signtool.exe not found' }
  }
  $pfxPath = Join-Path $root 'build/msix_signing.pfx'
  $cerPath = Join-Path $root 'build/msix_signing.cer'
  $pwd = [Guid]::NewGuid().ToString('N')
  $cert = New-SelfSignedCertificate -Subject "CN=$PublisherCN" -Type CodeSigningCert -CertStoreLocation 'Cert:\CurrentUser\My'
  Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password (ConvertTo-SecureString -String $pwd -Force -AsPlainText)
  Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
  & $signtool sign /fd SHA256 /f $pfxPath /p $pwd $msixOut
  & $signtool verify /pa $msixOut
  Get-Item $msixOut | Format-List FullName,Length,LastWriteTime
} else {
  $heatCmd = Get-Command heat.exe -ErrorAction SilentlyContinue
  if ($null -ne $heatCmd) { $wixDir = Split-Path $heatCmd.Path } else {
    $existingHeat = $null
    $candidate1 = Join-Path $root 'build/wix311/heat.exe'
    $candidate2 = Join-Path $root 'build/wix/heat.exe'
    if (Test-Path $candidate1) { $existingHeat = $candidate1 } elseif (Test-Path $candidate2) { $existingHeat = $candidate2 }
    if ($null -ne $existingHeat) { $wixDir = Split-Path $existingHeat } else {
      $zipUrl = 'https://ghproxy.net/https://github.com/wixtoolset/wix3/releases/download/wix3112rtm/wix311-binaries.zip'
      $zipPath = Join-Path $root 'build/wix311-binaries.zip'
      $extractPath = Join-Path $root 'build/wix311'
      Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath
      Expand-Archive -Path $zipPath -DestinationPath $extractPath -Force
      $heatCmd = Get-ChildItem -Path $extractPath -Recurse -Filter 'heat.exe' | Select-Object -First 1
      if (-not $heatCmd) { throw 'WiX heat.exe not found' }
      $wixDir = Split-Path $heatCmd.FullName
    }
  }
  $heat = Join-Path $wixDir 'heat.exe'
  $candle = Join-Path $wixDir 'candle.exe'
  $light = Join-Path $wixDir 'light.exe'
  $installerDir = Join-Path $root 'build/installer'
  New-Item -ItemType Directory -Path $installerDir -Force | Out-Null
  & $heat dir $BuildPath -cg FilesGroup -dr INSTALLDIR -sreg -srd -ag -var var.SourceDir -o (Join-Path $installerDir 'harvest.wxs')
  $productWxs = @"
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="$AppName" Manufacturer="PLFJY" Version="$version" Language="2052" UpgradeCode="{980E3856-4BEE-470D-A73D-6507CA035EF8}" Codepage="936">
    <Package InstallerVersion="500" Compressed="yes" InstallScope="perMachine" SummaryCodepage="936" />
    <MajorUpgrade DowngradeErrorMessage="已存在更高版本，无法降级安装。" />
    <MediaTemplate EmbedCab="yes" CompressionLevel="high" />
    <Feature Id="MainFeature" Title="$AppName" Level="1">
      <ComponentGroupRef Id="FilesGroup" />
      <ComponentRef Id="StartMenuShortcuts" />
      <ComponentRef Id="DesktopShortcutComponent" />
    </Feature>
    <UIRef Id="WixUI_InstallDir" />
    <Property Id="WIXUI_INSTALLDIR" Value="INSTALLDIR" />
  </Product>
  <Fragment>
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFiles64Folder">
        <Directory Id="INSTALLDIR" Name="$AppName" />
      </Directory>
      <Directory Id="ProgramMenuFolder">
        <Directory Id="ApplicationProgramsFolder" Name="$AppName" />
      </Directory>
      <Directory Id="DesktopFolder" />
    </Directory>
  </Fragment>
  <Fragment>
    <Component Id="StartMenuShortcuts" Directory="ApplicationProgramsFolder" Guid="*">
      <Shortcut Id="StartMenuShortcut" Directory="ApplicationProgramsFolder" Name="$AppName" WorkingDirectory="INSTALLDIR" Target="[INSTALLDIR]neo-bpsys-wpf.exe" />
      <RemoveFolder Id="ApplicationProgramsFolder" On="uninstall" />
      <RegistryValue Root="HKCU" Key="Software\$AppName" Name="startmenu" Type="integer" Value="1" KeyPath="yes" />
    </Component>
    <Component Id="DesktopShortcutComponent" Directory="DesktopFolder" Guid="*">
      <Shortcut Id="DesktopShortcut" Directory="DesktopFolder" Name="$AppName" WorkingDirectory="INSTALLDIR" Target="[INSTALLDIR]neo-bpsys-wpf.exe" />
      <RegistryValue Root="HKCU" Key="Software\$AppName" Name="desktop" Type="integer" Value="1" KeyPath="yes" />
    </Component>
  </Fragment>
</Wix>
"@
  Set-Content -Path (Join-Path $installerDir 'product.wxs') -Value $productWxs -Encoding UTF8
  $locPath = Join-Path $installerDir 'zh-cn.wxl'
  $locContent = '<WixLocalization xmlns="http://schemas.microsoft.com/wix/2006/localization" Culture="zh-CN" Codepage="936" />'
  Set-Content -Path $locPath -Value $locContent -Encoding UTF8
  Set-Location $root
  Remove-Item (Join-Path $root 'harvest.wixobj') -ErrorAction SilentlyContinue
  Remove-Item (Join-Path $root 'product.wixobj') -ErrorAction SilentlyContinue
  & $candle -arch x64 -ext WixUIExtension -dSourceDir="$BuildPath" (Join-Path $installerDir 'harvest.wxs') (Join-Path $installerDir 'product.wxs')
  & $light -ext WixUIExtension -loc $locPath (Join-Path $root 'harvest.wixobj') (Join-Path $root 'product.wixobj') -out (Join-Path $root 'build/bp-idvevent_Installer.msi')
  Get-Item (Join-Path $root 'build/bp-idvevent_Installer.msi') | Format-List FullName,Length,LastWriteTime
}
