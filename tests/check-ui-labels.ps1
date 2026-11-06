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
