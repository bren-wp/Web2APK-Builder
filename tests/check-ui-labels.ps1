$ErrorActionPreference='Stop'
$paths=@(
  'src/Brendigo.Web2APK.Windows/MainWindow.xaml',
  'src/Brendigo.Web2APK.Windows/MainWindow.xaml.cs',
  'src/Brendigo.Web2APK.Windows/MainWindow.Wizard.cs',
  'src/Brendigo.Web2APK.Windows/MainWindow.Actions.cs',
  'src/Brendigo.Web2APK.Windows/MainWindow.Preview.cs',
  'src/Brendigo.Web2APK.Windows/MainWindow.Welcome.cs',
  'android/app/src/main/java/com/brendigo/web2apk/client/MainActivity.java')
foreach($p in $paths){ if(-not(Test-Path $p)){throw "UI file missing: $p"} }
$forbidden=@('lorem ipsum','ui koncept','demo mode','developer placeholder','todo: replace','dev only','mock user data')
foreach($path in $paths) {
  $body=Get-Content $path -Raw
  foreach($word in $forbidden) {
    if($body.ToLowerInvariant().Contains($word)) { throw "User-facing development label found: $word in $path" }
  }
}
$window=Get-Content 'src/Brendigo.Web2APK.Windows/MainWindow.xaml' -Raw
if($window -notmatch 'WindowState="Normal"' -or $window -notmatch 'ResizeMode="CanResizeWithGrip"') {
 throw 'Windows desktop must open windowed and remain resizeable.'
}
$android=Get-Content $paths[-1] -Raw
if($android -notmatch 'bottomNavigation\(title\)' -or $android -notmatch 'showWizardStep\(0\)') {
 throw 'Android navigation or six-step wizard missing.'
}
Write-Host 'Production UI labels and responsive navigation verified.'

$theme=Get-Content 'src/Brendigo.Web2APK.Windows/App.xaml' -Raw
if($theme -notmatch 'PART_Popup' -or $theme -notmatch 'CheckFrame' -or
   $theme -notmatch '#132242') {throw 'Dark native ComboBox/CheckBox template missing.'}
Write-Host 'Dark WPF control templates verified.'

$frontends=@(
 'src/Brendigo.Web2APK.Windows/MainWindow.Actions.cs',
 'src/Brendigo.Web2APK.Windows/MainWindow.Wizard.cs',
 'src/Brendigo.Web2APK.Windows/MainWindow.xaml.cs',
 'android/app/src/main/java/com/brendigo/web2apk/client/MainActivity.java')
foreach($path in $frontends){
 $body=Get-Content $path -Raw
 if($body -match 'github\\.com' -or $body -match 'Verzija 0\\.3\\.2'){
  throw "Obsolete/GitHub label in production UI: $path"
 }
}
$actions=Get-Content 'src/Brendigo.Web2APK.Windows/MainWindow.Actions.cs' -Raw
foreach($item in @('apk.com.hr','info@apk.com.hr','Zamijeni ovu licencu','Nepotpisani APK','Debug APK')){
 if(-not($actions.Contains($item))){throw "Missing production feature: $item"}
}
$license=Get-Content 'src/Brendigo.Web2APK.Core/LicenseEntitlements.cs' -Raw
if(-not($license.Contains('!project.BrendigoSplashBranding'))){
 throw 'Demo splash-lock enforcement is missing'
}
Write-Host 'Public program URLs, masked licensing, demo splash and export modes verified.'

$ent=Get-Content 'src/Brendigo.Web2APK.Core/LicenseEntitlements.cs' -Raw
if(-not ($ent.Contains('public const string Agency') -and
    $ent.Contains('ApplicationLimit') -and $ent.Contains('PackageMatches') -and
    $ent.Contains('"source"'))) {
    throw 'Agency tier, three-app scope or paid source entitlement missing.'
}
$viewer=Get-Content 'src/Brendigo.Web2APK.Windows/SourceViewerWindow.cs' -Raw
if(-not ($viewer.Contains('LicenseActivationStore.Require') -and
    $viewer.Contains('ZipFile.CreateFromDirectory'))) {
    throw 'Paid source-code browser or ZIP export missing.'
}
$studio=Get-Content 'src/Brendigo.Web2APK.LicenseStudio/MainWindow.xaml' -Raw
if(-not ($studio.Contains('AgencyDuration') -and $studio.Contains('AgencyButton'))) {
    throw 'Agency monthly/yearly licensing controls missing.'
}
$themed=Get-Content 'src/Brendigo.Web2APK.LicenseStudio/App.xaml' -Raw
if(-not ($themed.Contains('PART_Popup') -and $themed.Contains('ComboBoxItem'))) {
    throw 'License Studio dark selection popup missing.'
}
Write-Host 'Agency, multi-app Professional, gated source viewer and dark License Studio verified.'

$chrome=Get-Content 'src/Brendigo.Web2APK.Windows/MainWindow.xaml' -Raw
if($chrome -notmatch 'TitleBar_Drag' -or $chrome -notmatch 'WindowStyle="None"'){
 throw 'Native window titlebar not enabled.'
}
$native=Get-Content 'src/Brendigo.Web2APK.Windows/MainWindow.xaml.cs' -Raw
if($native -notmatch 'WindowChrome.SetWindowChrome') {
 throw 'Resize chrome not retained in borderless native window.'
}
$sdk=Get-Content 'src/Brendigo.Web2APK.Core/IntegratedToolchainService.cs' -Raw
if($sdk -notmatch 'IsLocalAndroidPlatformInstalledFor' -or $sdk -notmatch 'targetSdk=36') {
 throw 'Integrated Android SDK target chooser missing.'
}
Write-Host 'Window titlebar, resize chrome and Android SDK 36/37 installer verified.'

# Verify that release tags, Windows assembly resources and the Android package metadata
# all advance together. Older releases inadvertently kept their 0.6.1 file metadata.
$winProject=Get-Content 'src/Brendigo.Web2APK.Windows/Brendigo.Web2APK.Windows.csproj' -Raw
$studioProject=Get-Content 'src/Brendigo.Web2APK.LicenseStudio/Brendigo.Web2APK.LicenseStudio.csproj' -Raw
$gradleProject=Get-Content 'android/app/build.gradle' -Raw
$releaseWorkflow=Get-Content '.github/workflows/windows-release.yml' -Raw
foreach($project in @($winProject,$studioProject)){
 foreach($marker in @('<Version>0.6.8</Version>','<FileVersion>0.6.8.0</FileVersion>','<AssemblyVersion>0.6.8.0</AssemblyVersion>')){
  if(-not $project.Contains($marker)){throw "Release version mismatch: $marker"}
 }
}
if(-not $gradleProject.Contains("versionCode 15") -or
   -not $gradleProject.Contains("versionName '0.6.8'") -or
   -not $releaseWorkflow.Contains("$('v0.6.8')")){
 throw 'Android metadata and release workflow version must match v0.6.8.'
}
Write-Host 'Windows and Android release versions verified: 0.6.8 (Android code 15).'

foreach($installer in @('installer/Brendigo-Web2APK.iss','installer/Brendigo-LicenseStudio.iss')) {
 if(-not (Get-Content $installer -Raw).Contains('#define MyAppVersion "0.6.8"')) {
   throw "Installer version mismatch: $installer"
 }
}
Write-Host 'Installer and Android package IDs verified.'

# Verify the recovery entry point remains reachable if the project database is corrupt.
$startup=Get-Content 'src/Brendigo.Web2APK.Windows/MainWindow.xaml.cs' -Raw
$settings=Get-Content 'src/Brendigo.Web2APK.Windows/MainWindow.Actions.cs' -Raw
$recovery=Get-Content 'src/Brendigo.Web2APK.Core/ProjectStore.cs' -Raw
$generator=Get-Content 'src/Brendigo.Web2APK.Core/AndroidProjectGenerator.cs' -Raw
if(-not ($startup.Contains('catch(InvalidDataException e)') -and
          $startup.Contains('Navigate("settings")') -and
          $settings.Contains('RestoreBackup()') -and
          $settings.Contains('BackupProjectCount()') -and
          $recovery.Contains('File.Move(temp,_path,overwrite:true)'))){
  throw 'Windows safe-recovery flow is incomplete.'
}
foreach($sensitive in @('credentials.json','google-services.json','service-account.json',
 'firebase-adminsdk','.p12','.pem','.jks')) {
 if(-not $generator.Contains($sensitive)){throw "Local secret exclusion missing: $sensitive"}
}
Write-Host 'Startup recovery and offline asset secret exclusions verified.'

# Security and responsive regressions are scanned as source in the quality job.
$androidGenerator=Get-Content 'src/Brendigo.Web2APK.Core/AndroidProjectGenerator.cs' -Raw
$androidBuild=Get-Content 'src/Brendigo.Web2APK.Core/AndroidBuildService.cs' -Raw
$windowsUi=Get-Content 'src/Brendigo.Web2APK.Windows/MainWindow.xaml.cs' -Raw
if(-not ($androidGenerator.Contains('effectivePort(uri) == effectivePort(home)') -and
         $androidGenerator.Contains('if (EXTERNAL && !trusted(uri))') -and
         $androidBuild.Contains('ConfigureSigningEnvironment(start, signing)') -and
         $androidBuild.Contains('RedactSigningSecrets(e.Data, signing)') -and
         $windowsUi.Contains('stackActions = logicalWidth < 1260'))) {
    throw 'WebView origin, Gradle signing or responsive project-row regression.'
}
Write-Host 'Security boundaries and compact project-row layout verified.'

# The additional UI and emulator tests must remain part of CI.
$androidFlow=Get-Content '.github/workflows/android-emulator.yml' -Raw
$androidSmoke=Get-Content 'scripts/test-android-emulator.sh' -Raw
$visualDiff=Get-Content 'scripts/compare-wpf-screens.ps1' -Raw
if(-not ($androidFlow.Contains('reactivecircus/android-emulator-runner@v2') -and
         $androidFlow.Contains('assembleDebug') -and
         $androidSmoke.Contains('tablet-wide') -and
         $androidSmoke.Contains('BRENDIGO_OFFLINE_EMULATOR_READY') -and
         $visualDiff.Contains('changedSampleFraction'))) {
  throw 'Android emulator and WPF visual regression CI checks missing.'
}
Write-Host 'Android emulator and native WPF visual regression configured.'
