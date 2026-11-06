# Brendigo Web2APK Builder — promjene

## v0.3.0 — Windows + Android, dozvole, AdMob, FCM i ključevi

- Nativni Windows WPF Studio zadržava lokalni Android Gradle generator i APK/AAB izradu.
- Android Mobile klijent: dashboard, lokalni projekti, uređivač, HTTPS pregled, uvoz/izvoz JSON-a.
- Opcionalna AdMob banner integracija s Google Mobile Ads SDK 25.5.0 i UMP 4.0.0, uz korisničke identifikatore.
- Opcionalni Firebase FCM SDK kroz Firebase BoM 35.1.0, Google Services 4.5.0 i servis za obavijesti, uz vlastiti google-services.json.
- Kamera, mikrofon, približna lokacija, Android datoteke i preuzimanja uz kontrolirane dozvole.
- Adaptivne ikone, monokromatska ikona, korisnički launcher PNG, native splash s trajanjem.
- Nasumični Android signing ključ preko lokalnog JDK keytool alata ili postojeći .p12/.jks.
- Korisnički odobreno preuzimanje Gradle 9.6.1 sa SHA-256 provjerom.
- Dodatni automatizirani testovi i Windows + Android build u CI-u.
- U izdanju: portable.exe, setup.exe, Android testni APK i nepotpisani AAB.

**Ograničenja:** Stvarni Google računi/konfiguracije potrebni su za AdMob i Firebase. Android klijent ne kompajlira druge aplikacije. Testni Android APK nije produkcijski, AAB je nepotpisan, Windows EXE nije kodno potpisan. Potpuno pixel-perfect podudaranje sa svim referentnim zaslonima nije potvrđeno.

## v0.2.0 — nativna Windows WPF aplikacija

Migracija s Electrona na .NET 10/WPF, izvoz Android Gradle projekata, Windows portable i setup.

## v0.1.0 — inicijalni Windows prototip

Početni Web2APK builder i renderer.
