<div align="center">
<img src="assets/readme-banner.svg" width="100%" alt="Brendigo Web2APK Builder">
<h1>Brendigo Web2APK Builder</h1>
<strong>Nativni Windows Studio + Android Mobile klijent</strong>
<p>🌐 Web → APK/AAB · 🎨 Napredni brending · 🔏 Lokalno potpisivanje · 🛡️ Kontrolirane dozvole</p>
<p><a href="https://github.com/bren-wp/Web2APK-Builder/releases/latest">Preuzmi najnovije izdanje</a> · <a href="https://brendigo.com">brendigo.com</a> · <a href="SECURITY.md">Sigurnost</a></p>
</div>

---

## Licenciranje i Brendigo License Studio

Brendigo License Studio zasebna je **nativna Windows aplikacija** za izdavanje proizvoljnih serija licenci uz vlasnički šifrirani PEM i lokalnu RSA-3072 digitalnu potpisnu provjeru. Glavna Windows aplikacija i Android klijent prihvaćaju samo licence s točnim digitalnim potpisom. Nema internetske aktivacije, udaljenog računa ni popisa milijuna ključeva u binarnom programu.

**Privatni vlasnički ključ nije u ovom javnom GitHub repozitoriju niti u instalacijskom paketu.** Mora se čuvati odvojeno. Upute: [License Studio](docs/LICENSE-STUDIO-HR.md).

**Ograničenje:** offline licence moguće je kopirati; opoziv i ograničenje jednog uređaja zahtijevaju zasebnu online provjeru.

## Licencni paketi — po Android aplikaciji

| Paket | Cijena | Podržane funkcije |
|---|---:|---|
| **Demo** | **0 €** | Trajna licenca, APK izvoz, bez oglasa, zadani izgled |
| **Osnovno** | **49 €** | Sve iz Dema, APK/AAB, vlastita ikona, splash zaslon i brending |
| **Profesionalac** | **69 €** | Sve iz Osnovnog, povlačenje za osvježavanje i Firebase Push |
| **Developer · vlasnik** | **nije u prodaji** | **Neograničen broj aplikacija, sve funkcije, APK/AAB, AdMob, Push, napredne dozvole i sve postavke** |

Demo, Osnovno i Profesionalac su **doživotne licence za jedan točno definiran Android package ID**. Licenca može dopustiti rad u Windows aplikaciji, Android klijentu ili na obje platforme. Developer je vlasnička licenca sa znakom `*` umjesto package ID-ja, koristi se na obje platforme i **nema ograničenje broja aplikacija**. Svi paketi koriste lokalnu provjeru RSA potpisa; privatni potpisni ključ ostaje isključivo kod vlasnika.

**Napomena o Google Playu:** „kompatibilno” znači da program generira projekt s aktualnim API-jem i daje kontrolni popis. Odobrenje objave ovisi o pravilima, kvaliteti i stvarnim podacima aplikacije; nije zajamčeno kupnjom paketa. AdMob generiranje dopušteno je samo Developer paketu; ostali proizvode aplikacije bez ugrađenih oglasa iz Buildera.

Dodatne upute: [Izdavanje licenci i vlasnički Developer paket](docs/LICENSE-STUDIO-HR.md).

**Google Play i naplata:** Android klijent koji se distribuira preko Google Playa i otključava plaćene digitalne funkcije mora poštovati [Google Play Payments pravila](https://support.google.com/googleplay/android-developer/answer/9858738). Ovisno o modelu distribucije, kupnji i regiji, to može zahtijevati Play Billing ili odobrenu alternativu. **Ova offline aktivacija ne implementira sustav kupnje niti jamči odobrenje na Google Playu.** U aplikaciji nema poveznica za alternativnu kupnju.

## Windows Studio (.NET 10 / WPF)

Nativni Windows program za kreiranje i konfiguriranje Android WebView aplikacija i lokalnu izradu APK/AAB paketa uz Android SDK. Ne zahtijeva Electron ni Chromium. Prozor se ne pokreće maksimiziran i prilagođava se širini zaslona.

**Ključne značajke:** lokalno upravljanje projektima, HTTPS i offline HTML, tema i prilagodljive ikone, dinamički splash, kamera, mikrofon, približna lokacija, Android odabir i preuzimanje datoteka, AdMob banner s UMP korisničkim pristankom, Firebase Cloud Messaging i lokalno potpisivanje s vlastitim ili slučajno generiranim trajnim ključem.

### Stvarni nativni zasloni

Za v0.3.2 uključeno je snimanje svih **16 Windows zaslona**, uključujući splash, dobrodošlicu, novi pregled aplikacije i sve korake čarobnjaka. Snimke proizvodi GitHub Actions iz stvarnog WPF sučelja, bez generiranja marketinških prikaza.

| Splash | Dobrodošli |
|---|---|
| ![Brendigo splash](docs/screenshots/native/splash.png) | ![Dobrodošli](docs/screenshots/native/welcome.png) |

| Pregled aplikacije | Postavke |
|---|---|
| ![Pregled aplikacije](docs/screenshots/native/preview.png) | ![Postavke](docs/screenshots/native/settings.png) |

### Stvarni nativni zasloni

Snimke se automatski stvaraju iz WPF renderera tijekom CI-ja, a ne umjetnim generiranjem:

| Početni zaslon | Novi projekt |
|---|---|
| ![Nativni Windows dashboard](docs/screenshots/native/dashboard.png) | ![Novi projekt](docs/screenshots/native/wizard-1-basics.png) |
| Izgled i splash | AdMob i Firebase |
| ![Windows appearance](docs/screenshots/native/wizard-2-appearance.png) | ![Windows features](docs/screenshots/native/wizard-4-features.png) |
| Dozvole | Potpisivanje i izrada |
| ![Windows permissions](docs/screenshots/native/wizard-5-permissions.png) | ![Windows build](docs/screenshots/native/wizard-6-build.png) |

Ako screenshotovi još nisu prisutni, postupak snimanja nije potvrđen. **Pixel-perfect 1:1 podudaranje sa svim referencama još nije verificirano.**

## Android Mobile klijent

Android aplikacija sada uključuje **početno predstavljanje, nadzornu ploču, donju navigaciju, čarobnjak od šest koraka, detalje projekta, pregled, uređaje i postavke**. Vrijednosti konfiguracije ostaju u obrascu pri prelasku između koraka. Izgled se prilagođava širini telefona i tableta, a prikaz poštuje sistemske rubove na novijem Androidu.



Projekt **android/** sadrži nativnu Android aplikaciju s Brendigo brendingom, početnim zaslonom, projektima, editorom, pregledom HTTPS sadržaja i lokalnim JSON uvozom/izvozom. Konfiguracije se mogu ručno prenositi u Windows Studio i obratno. Privatni ključevi i lozinke se ne prenose.

**Android Mobile ne kompajlira APK/AAB na telefonu.** Izradu generiranih Android paketa izvodi Windows Studio. AdMob i FCM opcije odnose se na generirane aplikacije i nisu oglasi ili obavijesti ugrađeni u sam Builder klijent.

## Izdanje i preuzimanje

| Paket | Namjena |
|---|---|
| **Brendigo-Web2APK-portable.exe** | Nativni Windows x64 bez instalacije |
| **Brendigo-Web2APK-Setup.exe** | Instalacijski program za Windows x64 |
| **Brendigo-LicenseStudio-portable.exe** | Zaseban program za izdavanje licenci, bez privatnog ključa |
| **Brendigo-LicenseStudio-Setup.exe** | Instalacijska verzija programa za licence |
| **Brendigo-Web2APK-Android-test.apk** | Testni Android klijent s debug potpisom; za testiranje |
| **Brendigo-Web2APK-Android-unsigned.aab** | Nepotpisani AAB; za produkciju potreban je vlastiti trajni ključ |

[GitHub izdanja](https://github.com/bren-wp/Web2APK-Builder/releases)

Windows EXE nije digitalno potpisan izdavačkim certifikatom. Debug Android potpis ne jamči nadogradnju u budućim izdanjima, a nepotpisani AAB nije spreman za Google Play.

## Kako izraditi aplikaciju

1. U Windows Studiju kliknite **Novi projekt** i unesite naziv, identifikator aplikacije i HTTPS URL ili HTML mapu.
2. Podesite temu, PNG launcher ikonu, splash, WebView i potrebne Android dozvole.
3. Za **AdMob** unesite stvarni App ID i Banner Unit ID; za **Firebase** odaberite vlastiti google-services.json sa zadanim Android package ID-jem.
4. Za potpis odaberite postojeći keystore ili kliknite **Generiraj novi slučajni ključ**. **Istog ključa i lozinke morate se držati u svim budućim ažuriranjima!**
5. Instalirajte JDK 17, Android SDK Platform 36 i Gradle 9.6+. Gradle je moguće instalirati kroz Studio uz privolu i SHA-256 provjeru.
6. Izradite APK/AAB, provjerite dozvole i rad na stvarnom Android uređaju te ispunite aktualna Google Play pravila.

**Stvarna dostava FCM poruka i prikaz AdMob oglasa zahtijevaju valjan Firebase odnosno AdMob račun, odgovarajuće konfiguracije, korisnički pristanak i testiranje.** Ove ovisnosti dolaze iz Googleova repozitorija i ne obrađuju se potpuno offline.

## Razvoj i testovi

Na Windowsu koristite .NET 10, a za Android JDK 17, SDK 36 i Gradle 9.6+.

Windows: dotnet run --project src/Brendigo.Web2APK.Windows/Brendigo.Web2APK.Windows.csproj

Testovi: dotnet run -c Release --project tests/Brendigo.Web2APK.Core.Tests/Brendigo.Web2APK.Core.Tests.csproj

Android: gradle -p android :app:assembleDebug :app:bundleRelease

Ikonica: powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File scripts/make-icon.ps1

Automatizacija: [Windows/Android Release](.github/workflows/windows-release.yml) · [Quality](.github/workflows/quality.yml).

## Provjere u izdanju v0.3.1

Uz Android Mobile i osnovni WebView projekt, CI sada zasebno kompajlira generirani projekt s Google AdMob + UMP integracijom te primjer s Firebase Cloud Messagingom. Firebase konfiguracija u CI-ju sadrži samo testne identifikatore, **nije produkcijski Firebase račun**. Za korisničke projekte još su potrebni stvarni ID-jevi i test stvarne dostave oglasa/poruka.

Nativne WPF snimke pokušavaju se snimiti izvršavanjem pravog Windows programa; nedostajuće ili neuspjele snimke više se ne smiju prikazivati kao uspješno verificirane.

Na Android klijentu sada je moguće konfigurirati kameru, mikrofon, lokaciju, prijenos datoteka, splash i zatražiti Firebase postavljanje. Firebase konfiguracijsku datoteku i završno potpisivanje postavlja se u Windows Studiju.

## Status i sigurnosni pregled

U verziji 0.4.0 aktivacija je obvezna u produkcijskom Windows programu i Android klijentu. Interni UI capture način zaseban je CI build i **nije dostupan u prodajnoj verziji**. Sigurnosni i dead-code pregled (opseg i preostale provjere): [Izvještaj](docs/SECURITY-DEADCODE-AUDIT-v0.4.0.md).

## Status

U izdanju v0.3.2 proširen je UI/UX na Windowsu i Androidu. Uklonjeni su razvojni i privremeni tekstovi iz korisničkih stranica; tehničke informacije ostaju samo ondje gdje su potrebne za izradu i sigurnost. Reference su vizualni cilj, no potpuno 1:1 podudaranje pojedinačnih elemenata zahtijeva dodatnu usporedbu stvarnih snimki na istim DPI postavkama i uređajima.



Poboljšanja UI/UX-a i funkcionalna testiranja napreduju zaslon po zaslon. CI kompilacija ne jamči besprijekoran rad svih integracija na svim uređajima, niti su sve vizualne reference provjerene pixel po pixel. Pogledajte [CHANGELOG.md](CHANGELOG.md) i [SECURITY.md](SECURITY.md).

<div align="center"><img src="assets/brand-icon.svg" width="58" alt="Brendigo logo"><p><strong>Brendigo · brendigo.com</strong></p></div>
