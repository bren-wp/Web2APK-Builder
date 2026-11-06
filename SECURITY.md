# Sigurnost — Brendigo Web2APK Builder

- Windows i Android Builder spremaju projekte lokalno. Ručni prijenos konfiguracija koristi JSON, ne šalje privatne ključeve.
- Nasumični Android keystore izrađuje lokalni JDK keytool s kriptografski sigurnom lozinkom. Korisnik mora izraditi sigurnosnu kopiju keystorea i lozinke radi budućih ažuriranja.
- AdMob i Firebase ovisnosti uključuju se samo uz vlastite valjane konfiguracije; Google servisi koriste mrežu i mogu obrađivati korisničke podatke ovisno o postavkama.
- AdMob zahtijeva odgovarajući UMP pristanak. Firebase FCM zahtijeva stvarni Firebase projekt i Android dozvole za obavijesti.
- WebView ima HTTPS prema zadanim postavkama, isključen univerzalni pristup lokalnim datotekama, provjeru podrijetla i Android runtime odobrenja za osjetljive dozvole.
- Lokalni HTML se kopira bez simboličkih poveznica i skrivenih/razvojnih datoteka; s ograničenjem veličine.
- Gradle se na zahtjev preuzima iz službenog izvora i provjerava SHA-256. JDK i Android SDK ostaju zasebne lokalne ovisnosti.
- Windows paket nema certifikat proizvođača, a mobilni CI APK je testni. Za produkcijsko potpisivanje postavite trajni keystore.
- CI, generator testovi i kompilacija ne zamjenjuju sveobuhvatni security audit i testove na stvarnim uređajima.

Ne objavljujte keystore, lozinke, tokene, privatne certifikate i druge tajne u javnom repozitoriju.
