<div align="center">
  <img src="assets/readme-banner.svg" width="100%" alt="Brendigo Web2APK Builder">
  <h1>BRENDIGO WEB2APK BUILDER</h1>
  <p><b>Website → Android Studio project → APK / AAB</b></p>
  <p>🖥️ Nativni Windows WPF · ⚡ .NET 10 · 🔒 Lokalna obrada · 🎨 Brendigo dizajn</p>
  <p><a href="https://github.com/bren-wp/Web2APK-Builder/releases/latest">⬇️ Preuzmi Windows</a> · <a href="https://brendigo.com">🌐 brendigo.com</a> · <a href="SECURITY.md">🛡️ Sigurnost</a></p>
</div>

---

## 🖥️ Windows Studio · Native Edition 0.2.0

Brendigo Web2APK Builder je **stvarna nativna Windows aplikacija** temeljena na Microsoft .NET 10 i WPF-u. Koristi Windows prozor s naslovnom trakom, standardnim Windows minimiziranjem/maksimiziranjem, promjenjivom veličinom i prilagodbom rasporeda; ne zahtijeva Electron ni Chromium.

Svi projekti ostaju na računalu. Umetnite HTTPS URL ili lokalni HTML projekt s `index.html`, konfigurirajte Android WebView i izvezite Android izvorni kod ili napravite APK/AAB putem lokalnog razvojnog lanca.

### ✨ Što je funkcionalno

- 📊 **Početna ploča** — broj projekata, HTTPS konfiguracija, nedavno korišteni projekti
- 📁 **Moji projekti** — kreiranje, uređivanje, spremanje i brisanje
- ⚙️ **Čarobnjak u šest koraka** — osnovno, izgled, WebView, funkcije, dozvole, izrada
- 🌍 **HTTPS ili offline HTML** — Android WebViewAssetLoader za lokalne mape
- 🎨 **Brending** — naziv, package ID, primarna boja, tema, orijentacija, verzije
- 🛡️ **Sigurnost** — zabrana file URL pristupa; HTTPS i minimalne Android dozvole prema zadanim postavkama
- 📦 **Android APK/AAB** — lokalni Gradle 9.6+, Android Gradle plugin 9.5, Android SDK 36+
- 🔐 **Opcionalno potpisivanje** — postojeći keystore, lozinke se ne čuvaju u projektima
- 🛠️ **Razvojni alati** — provjera Jave, Gradlea, SDK-a i ADB uređaja

### 🎨 Brendigo UI / UX

**Referentni Windows dizajn**, izrađen ranije u ovom projektu, služi za usporedbu boja, rasporeda i komponenti. Sljedeći prikazi su snimke **ranijeg v0.1.0 renderera**, a ne fotografije nativnog 0.2.0 WPF izdanja. Nativne snimke bit će objavljene nakon potvrđene vizualne provjere.

| Referenca: početna ploča | Referenca: novi projekt |
| :---: | :---: |
| ![Dashboard 0.1.0](docs/screenshots/dashboard.png) | ![Novi projekt 0.1.0](docs/screenshots/new-project.png) |
| Referenca: funkcionalnosti | Referenca: izrada |
| ![Funkcije 0.1.0](docs/screenshots/features.png) | ![Build 0.1.0](docs/screenshots/build.png) |

**Pixel-perfect 1:1 još nije verificiran na svim rezolucijama.** Dizajn se usklađuje zaslon po zaslon prema izvornim referencama 1440 × 900 te isprobava na drugim veličinama i DPI postavkama.

### ⬇️ Windows izdanja

| Paket | Namjena |
| --- | --- |
| **Brendigo-Web2APK-portable.exe** | Samostalni, self-contained Windows x64 EXE |
| **Brendigo-Web2APK-Setup.exe** | Instalacija za Windows x64 putem Inno Setupa |

➡️ **[Preuzmi najnovije Windows izdanje](https://github.com/bren-wp/Web2APK-Builder/releases/latest)**

Windows binarne datoteke nisu digitalno potpisane dok ne bude dostupan odgovarajući izdavački certifikat; SmartScreen može prikazati upozorenje.

### 🚀 Koraci za prvu Android aplikaciju

1. Otvorite **Novi projekt** i unesite naziv, Android package ID, HTTPS URL ili lokalnu HTML mapu.
2. Uredite **Izgled**, **WebView** i **Dozvole**.
3. Kliknite **Spremi projekt** — projekt se lokalno sprema u korisnički Windows profil.
4. U koraku **Izrada** odaberite izvoz Android Gradle projekta ili izradu APK/AAB.
5. Instalirajte JDK 17, Gradle 9.6+ i Android SDK 36 ili noviji te ih učinite dostupnima u Windows PATH i ANDROID_HOME.
6. Za potpisani Android paket odaberite vlastiti keystore i unesite lozinke.

**Važno:** nepodpisani Android release APK ne može se smatrati spremnim za distribuciju. Android build zahtijeva instalirane lokalne alate i za prve vanjske Maven ovisnosti može trebati internetsku vezu.

### 👨‍💻 Razvoj

Potreban je .NET SDK 10 na Windows 10/11:

~~~powershell
git clone https://github.com/bren-wp/Web2APK-Builder.git
cd Web2APK-Builder
dotnet run --project src/Brendigo.Web2APK.Windows/Brendigo.Web2APK.Windows.csproj
dotnet run -c Release --project tests/Brendigo.Web2APK.Core.Tests/Brendigo.Web2APK.Core.Tests.csproj
dotnet publish src/Brendigo.Web2APK.Windows/Brendigo.Web2APK.Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
~~~

Svako novo izdanje priprema se preko [Windows Release workflowa](.github/workflows/windows-release.yml) koji kompilira WPF, pokreće testove i zahtijeva oba EXE paketa. Novi broj verzije obvezan je prije sljedećeg izdanja.

### 🧩 Jasno definirane granice

Repozitorij razvija **samo Windows aplikaciju**. Nema zasebnog Brendigo Android klijenta. Generirani Android projekt nije zaseban desktop program. AdMob, Firebase Push, kamera, mikrofon, dinamički splash i napredne ikone trenutno **nisu implementirani** te nisu predstavljeni kao funkcionalne opcije. Uvođenje tih mogućnosti zahtijeva njihove prave integracije, testove i provjeru pravila Google Playa.

---

<div align="center"><img src="assets/brand-icon.svg" width="60" alt="Brendigo logo"><br><b>Made by Brendigo</b><br><a href="https://brendigo.com">brendigo.com</a></div>
