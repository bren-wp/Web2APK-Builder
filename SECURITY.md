# Sigurnost — Brendigo Web2APK Builder

**Službeni kontakt za sigurnosne probleme: info@apk.com.hr.** Službeni web: apk.com.hr · razvoj: Brendigo / brendigo.com.

## Sigurnosna načela
- **Windows Builder Setup i Portable od v0.6.8 nemaju aktivaciju, ograničenja licenci ni zahtjev za korisničkim računom.** Projekti i keystore pohranjuju se lokalno, bez ugrađene telemetrije. Zasebni Android Mobile klijent zadržava BL2 offline potpisane tokene koji nisu šifrirani niti vezani uz uređaj.
- Izdavatelj licenci koristi vlasnički privatni ključ koji se ne isporučuje korisnicima i ne nalazi se u repozitoriju. Zamjena ključevâ prvo verificira novopotpisani token.
- Windows Builder omogućuje vlastitu ikonu, splash, APK/AAB i izvorni kod bez licence. Zasebni Android editor i dalje provjerava prava svojih BL2 paketa.
- Android WebView ograničava pristup datotekama, koristi HTTPS prema zadanim postavkama, provjerava podrijetlo i traži korisničko dopuštenje za kameru, mikrofon i lokaciju.
- Lokalni HTML izvoz odbacuje simboličke poveznice, skrivene datoteke i pretjerano velike mape.
- Potpisni ključ i zaporke ne pohranjuju se u projektne konfiguracije. Lokalni keytool koristi okolinsku varijablu za novu zaporku.
- JDK 17, Gradle 9.6.1 i Android SDK 36 mogu se na zahtjev preuzeti iz Windows aplikacije. Arhive se provjeravaju SHA-256, a Android SDK zahtijeva prihvaćanje Googleovih uvjeta.
- AdMob, Firebase, preuzeti alati i odabrana web-stranica mogu koristiti mrežu i obrađivati zasebne korisničke podatke; Windows studio nema vlastitu telemetriju.
- Windows .exe možda nije potpisan certifikatom izdavača. Android CI APK služi za testiranje, a AAB se mora potpisati prije produkcijske objave.

## Izvješće o pregledu

[Prošireni pregled sigurnosti i otvorenih rizika v0.6.8](docs/SECURITY-DEADCODE-AUDIT-v0.6.8.md) opisuje testni opseg, izmjene i ograničenja. Detaljni nalazi iz v0.6.7 ostaju dio dokumentacije.

Kompilacija, NuGet upit za poznate ranjivosti, Roslyn statička analiza i WPF screenshot testovi nisu zamjena za potpuno neovisno penetracijsko testiranje ili provjeru na stvarnim Windows/Android uređajima.

Nikad ne šaljite privatan keystore, lozinke ili pune aktivacijske tokene javnim kanalima.
