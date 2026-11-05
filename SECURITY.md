# Brendigo Web2APK Builder · Sigurnosne informacije

- Native Windows WPF/.NET program bez Electron renderera, vanjske telemetrije i udaljene korisničke baze.
- JSON baza projekata pohranjuje se u %LOCALAPPDATA%\Brendigo\Web2APK Builder\projects.json. Upis koristi privremenu datoteku i zamjenu.
- Android generator validira nazive, package ID, SDK, scheme i putanje prije izvoza.
- Standardno HTTPS; HTTP je moguće uključiti izričito i prikazuje se upozorenje.
- Android WebView: zabrana univerzalnog file URL pristupa, file access isključen, JavaScript prema konfiguraciji, WebView debugging isključen.
- Lokalni HTML se kopira bez simboličkih poveznica, razvojnih mapa, .env i skrivenih datoteka, s limitima veličine i broja datoteka.
- Android keystore lozinke ne pohranjuju se u JSON, niti se prikazuju u zapisniku. Prosljeđuju se samo procesu Gradlea kroz varijable okruženja.
- Gradle build pokreće se izravno lokalno na računalu; preporučuju se provjerene verzije JDK/SDK/Gradle.
- Windows EXE nije digitalno potpisan; zaštita od lažiranih datoteka zahtijeva potpisivanje službenim certifikatom.
- Android paket prije objave mora biti potpisan i testiran na uređaju; ne tretirajte nepotpisan APK kao produkcijski paket.

Za prijavu ranjivosti izbjegavajte javno objavljivati certifikate, privatne podatke i pristupne ključeve.
