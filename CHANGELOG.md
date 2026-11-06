# Brendigo Web2APK Builder — promjene

## v0.4.0 — Brendigo License Studio, četiri paketa i potpisane licence

- Demo (0 €), Osnovno (49 €), Profesionalac (69 €) i **Developer · vlasnik**.
- Developer koristi trajnu BL2 licencu za neograničen broj Android aplikacija na Windowsu i Androidu te otključava sve opcije.
- Ostale licence vežu se uz jedan Android package ID; Demo proizvodi samo APK, Osnovno dodaje AAB/ikone/splash, a Profesionalac dodaje Firebase Push i povlačenje za osvježavanje.
- Generator stvarno implementira AndroidX SwipeRefreshLayout 1.2.0 kada je opcija uključena.
- Offline provjera RSA potpisa, zaštita privatnog ključa, više instaliranih licenci, kontrole pri izvozu i gradnji.


- Dodana **zasebna Windows aplikacija** License Studio za kreiranje licenci za Windows, Android ili obje platforme.
- Protočni CSV izvoz proizvoljno velikih serija (ovisno o vremenu i dostupnom disku), RSA-3072/SHA-256 potpis svake licence.
- Šifrirani vlasnički PEM ne nalazi se u javnom repozitoriju ni izvršnom paketu.
- Windows i Android imaju offline ekran za aktivaciju, provjeru potpisa i isteka.
- Onemogućen ulaz bez licence preko javnog parametra za UI snimke; snimke se rade samo izdvojenim CI buildom.
- Zatamnjeni Windows ComboBoxovi i dosljedne boje Brendigo sučelja.
- Dodani sigurnosni pregled, testovi nevaljanih i krivotvorenih licenci, upute i dva nova Windows paketa.


## v0.3.2 — Brendigo Windows i Android ekran po ekran

- Windows: novi splash i prvi welcome zaslon, poboljšana nadzorna ploča i tablica projekata.
- Windows: stvarni pregled aplikacije uz kontrolirani lokalni WebView2, tri tipa prikaza i dodatne kontrole.
- Windows: usklađene boje, gradijenti, okviri, razmaci, izbornik i forma po referencama.
- Android: onboarding, stalna donja navigacija, nadzorna ploča s podacima iz projekata.
- Android: čarobnjak sa šest koraka, detalji projekta, popis projekata i prikaz uređaja, prilagodba rubovima sustava.
- Android: kamera, mikrofon, lokacija, prijenos/preuzimanje datoteka, Firebase zahtjev, AdMob i postavke privatnosti u konfiguraciji.
- Uklonjeni privremeni/dev tekstovi iz glavnog korisničkog sučelja.
- Kontrola UI tekstova u CI-u, najmanje 16 stvarnih Windows screenshotova.
- Obavezni Windows portable i Setup, Android test APK i nepotpisani AAB.

**Napomena:** pixel-perfect 1:1 na svim zaslonima nije moguće tvrditi bez usporednih mjerenja na ciljanim uređajima. CI kompilacija nije isto što i end-to-end test Firebase/AdMob/ADB funkcija ili Google Play certifikacija.


## v0.3.1 — provjere integracija i prilagodba Windows/Android zaslona

- CI stvarno kompajlira generirane Android projekte s AdMob+UMP i FCM (testni, neprodukcijski identifikatori).
- Ispravljeno pokretanje WPF snimanja iz GitHub Actions; omogućena lokalna dijagnostika pogrešaka pri startupu.
- Početna Windows nadzorna ploča bliža referenci: gradijentni hero, ilustrirani telefon iz stvarne aplikacijske ikone i tablični popis projekata.
- Dodan zaseban WPF resource za Brendigo ikonice, kako bi slike bile dostupne u self-contained EXE.
- Android Mobile editor podržava konfiguraciju kamere, mikrofona, lokacije, datoteka, dinamičkog splasha i zahtjev za Firebase.
- Zamijenjena upotreba java.time.Instant radi kompatibilnosti s Android API 24.
- Svi paketni brojevi povećani na 0.3.1; Windows+Android release zahtijeva uspješan CI.



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
