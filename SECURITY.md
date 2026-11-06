# Sigurnost — Brendigo Web2APK Builder

**Službeni kontakt za sigurnosne probleme: info@apk.com.hr.** Službeni web: apk.com.hr · razvoj: Brendigo / brendigo.com.

## Sigurnosna načela
- Projekti, aktivacije i keystore pohranjuju se lokalno, bez korisničkog računa i ugrađene telemetrije. Offline licencni tokeni su potpisani, ali nisu enkriptirani niti vezani uz jedan uređaj.
- Izdavatelj licenci koristi vlasnički privatni ključ koji se ne isporučuje korisnicima i ne nalazi se u repozitoriju. Zamjena ključevâ prvo verificira novopotpisani token.
- Brendigo Demo zahtijeva vlastitu ikonu i splash. Paketna prava provjeravaju se pri izvozu i gradnji te u Android editoru.
- Android WebView ograničava pristup datotekama, koristi HTTPS prema zadanim postavkama, provjerava podrijetlo i traži korisničko dopuštenje za kameru, mikrofon i lokaciju.
- Lokalni HTML izvoz odbacuje simboličke poveznice, skrivene datoteke i pretjerano velike mape.
- Potpisni ključ i zaporke ne pohranjuju se u projektne konfiguracije. Lokalni keytool koristi okolinsku varijablu za novu zaporku.
- JDK 17, Gradle 9.6.1 i Android SDK 36 mogu se na zahtjev preuzeti iz Windows aplikacije. Arhive se provjeravaju SHA-256, a Android SDK zahtijeva prihvaćanje Googleovih uvjeta.
- AdMob, Firebase, preuzeti alati i odabrana web-stranica mogu koristiti mrežu i obrađivati zasebne korisničke podatke; Windows studio nema vlastitu telemetriju.
- Windows .exe možda nije potpisan certifikatom izdavača. Android CI APK služi za testiranje, a AAB se mora potpisati prije produkcijske objave.

## Izvješće o pregledu

[Pregled sigurnosti, ovisnosti i mrtvog koda v0.6.7](docs/SECURITY-DEADCODE-AUDIT-v0.6.7.md) sadrži evidenciju pregledanih modula, promjene, testove, poznata ograničenja i daljnje provjere.

Kompilacija, NuGet upit za poznate ranjivosti, Roslyn statička analiza i WPF screenshot testovi nisu zamjena za potpuno neovisno penetracijsko testiranje ili provjeru na stvarnim Windows/Android uređajima.

Nikad ne šaljite privatan keystore, lozinke ili pune aktivacijske tokene javnim kanalima.
