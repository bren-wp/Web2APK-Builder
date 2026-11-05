# Brendigo Web2APK Builder · sigurnosne napomene

- Windows projektne datoteke lokalno se spremaju u userData direktorij. Ne koristi se udaljena aplikacijska baza.
- Renderer radi s contextIsolation, sandbox načinom rada, isključenim nodeIntegration i lokalnim Content Security Policyjem.
- Android projekt prema zadanim postavkama zahtijeva HTTPS i onemogućuje WebView pristup file:// datotekama.
- Lokalni HTML projekti kopiraju se uz ograničenje broja i veličine datoteka; preskaču se skrivene i razvojne datoteke te se ne slijede simboličke poveznice.
- Android keystore i lozinke nisu dio project.json datoteka.
- Izrađene Windows izvršne datoteke nisu potpisane izdavačkim certifikatom.
- Java/Gradle/SDK alate korisnik instalira i ažurira sam.
- Kod i Android aplikacije prije produkcije moraju proći dodatne sigurnosne i funkcionalne testove.

Ne objavljujte keystore, pristupne podatke i privatne putanje u javnim GitHub issues.
