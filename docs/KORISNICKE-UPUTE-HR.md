# Brendigo Web2APK Builder — potpune korisničke upute (HR)

**Službeni web:** https://apk.com.hr · **Podrška:** info@apk.com.hr · **Autor:** Brendigo / https://brendigo.com

## Vizualna dosljednost zaslona (v0.6.9)

CI snima stvarne Windows WPF zaslone na 1440 × 900 i dodatno sve glavne stranice i svih šest koraka čarobnjaka na najmanjoj podržanoj veličini prozora 840 × 570. Čarobnjak se dodatno testira na 900 × 650 i 1024 × 740. Provjeravaju se širine kartica, kompaktnog izbornika i dostupnost interaktivnih kontrola bez vodoravnog rezanja. Cijele PNG slike uspoređuju se piksel po piksel s prethodnim snimkama; izvještaj `native-full-pixel-audit.json` nalazi se u GitHub Actions artefaktima. Ako je prozor previsokim sadržajem ispunjen, upotrijebite okomito pomicanje. Uvijek se može vratiti puna širina izbornika povećavanjem prozora.

Stvarne snimke i CI testovi ne potvrđuju automatski potpuno podudaranje s izvornim konceptnim referencama, stvarnim povećanjem Windows fonta ili fizičkim DPI postavkama. Windows Portable i Setup i dalje **ne zahtijevaju licencu ni aktivacijski ključ**.

## 1. Instalacija Windows programa
Preuzmite `Brendigo-Web2APK-Setup.exe` za instalaciju ili `Brendigo-Web2APK-portable.exe` za pokretanje bez instalacije. Prozor se otvara u standardnoj veličini i može se smanjivati. Datoteke aplikacije i projekata spremaju se lokalno. Windows izvršne datoteke mogu biti nepotpisane; prije pokretanja provjerite izvor izdanja.

## 2. Windows pokretanje bez licence
Od verzije **v0.6.8** i Windows Builder Portable i Windows Builder Setup pokreću se bez licencnog ključa, aktivacije, prijave ili registracije. Pri pokretanju se više ne pojavljuje prozor za aktivaciju. U Postavkama piše **Windows Builder — bez licenci**. Sve funkcije za izradu APK/AAB paketa i izvornog koda dostupne su bez otključavanja. Zasebni Android Mobile klijent zadržava postojeće BL2 offline licenciranje; stare licencne datoteke na Windowsu ne brišu se automatski.

## 3. Dostupne funkcije bez ograničenja
Windows Builder omogućuje neograničen broj projekata, vlastite ikone, splash i brending, APK/AAB izvoze, ZIP izvornog koda, AdMob, Firebase, kameru, mikrofon, lokaciju i pull-to-refresh bez naplate ili aktivacije. Potrebni razvojni alati, certifikati, korisnička dopuštenja, ispravne konfiguracije oglasnih i Firebase servisa te Google Play pravila ostaju zasebni preduvjeti.

## 4. Izrada projekta
Otvorite Novi projekt i upišite naziv. Builder automatski generira jedinstven `applicationId` (primjer: `com.brendigo.moja_aplikacija_a1b2c3d4e5f6`), pa ga nije potrebno ručno upisivati. ID možete promijeniti prije prve objave. Ako ga promijenite na spremljenom projektu, sustav će upozoriti da je Android smatra drugom aplikacijom, da ne može ažurirati prethodno objavljeni paket bez trošenja ikakvog licencnog mjesta u Windows Builderu. Preimenovanje aplikacije ne mijenja package ID. Zatim unesite HTTPS URL ili lokalnu mapu s `index.html`, verziju i `versionCode`. Za ažuriranja povećajte `versionCode`. Ne radite izlaznu mapu unutar lokalnog HTML izvora.

## 5. Brending i ikone
Nova Android aplikacija po zadanim postavkama ima Brendigo vektorsku launcher ikonu i brendirani splash. U Windows izdanju možete odabrati PNG minimalno 192×192, postaviti vlastitu boju, isključiti Brendigo na splash ekranu ili promijeniti trajanje bez licence. Za konačni izgled obavezno testirajte uređaj različite gustoće zaslona.

## 6. WebView i responzivnost
WebView je prilagodljiv širini dostupnog zaslona i vodi računa o sistemskim rubovima. URL mora biti HTTPS, osim kad korisnik izričito uključi HTTP. JavaScript, DOM Storage, datoteke i vanjske poveznice podešavaju se u čarobnjaku. Ako je sama web-stranica napravljena s fiksnim CSS širinama, Builder to ne može automatski ispraviti; sadržaj web-stranice mora imati vlastiti responsivni CSS i viewport.

## 7. Dozvole
WebView kamera, mikrofon i lokacija dodjeljuju se samo istom točnom podrijetlu kao početni URL aplikacije (protokol, host, port). Na primjer, `https://primjer.hr:8443` nije isto podrijetlo kao `https://primjer.hr`; dopušteni HTTP također ne znači da HTTP smije naslijediti privilegije HTTPS stranice. Kada je uključeno vanjsko otvaranje poveznica, ostala podrijetla otvaraju se izvan WebViewa.
Uključujte samo potrebne dozvole. Kamera i mikrofon zahtijevaju potvrdu sustava Android. Lokacija se dodatno autorizira. Za datoteke koristite sistemski picker. U Windows Builderu sve navedene opcije dostupne su bez licence; Android operacijski sustav i dalje zasebno provjerava osjetljive dozvole.

## 8. Pregled i preuzimanje izvornog koda
U Windows izdanju svi korisnici imaju gumb **Pregledaj izvorni kod** u koraku Izrada i na popisu projekata, bez licence. Otvara se ugrađeni preglednik s Java, XML, Gradle i ostalim datotekama. Za čuvanje svih datoteka odaberite **Preuzmi izvorni kod (.zip)**.

## 9. Android razvojni alati unutar Windows programa
Pri prvom pokretanju izrade APK/AAB program sam provjerava razvojne alate i, ako nedostaju, nudi njihovu pripremu uz prihvaćanje uvjeta. Po želji u Razvojni alati kliknite **Pripremi JDK + Gradle + Android SDK unutar Studija**. Nakon što izričito prihvatite prikazane uvjete i preuzimanje, aplikacija koristi verificirane ZIP arhive iz službenih izvora i lokalno ih raspakira u Brendigo korisnički profil. Potrebni su internetska veza i diskovni prostor. Nije potrebna zasebna instalacija Android Studija. Ako preuzimanje ne uspije, moguće je koristiti već instalirani JAVA_HOME, Gradle i ANDROID_HOME. Pripremu prije produkcijske uporabe ispitajte na čistom Windows računalu.

## 9. APK debug, nepotpisani APK i AAB
U koraku Izrada birajte **Debug APK** za lokalno testiranje (potpisan razvojnim ključem), **Nepotpisani APK** za release bez keystorea, **Izradi APK** za potpisan ili nepotpisan release, te **Izradi AAB** za App Bundle. AAB bez keystorea ostaje nepotpisan. Nepotpisani APK nije instalabilan kao produkcijska aplikacija dok ne bude potpisan. Debug APK nije dopušten za produkcijsku objavu na Google Playu.

## 10. Trajni Android ključ
Možete učitati postojeći keystore ili lokalno generirati novi. Sačuvajte `.p12`/`.jks`, alias i zaporku izvan projekta. Gradle prima tajne samo kroz okolinske varijable procesa; ne stavljajte ih u README niti u javne repozitorije. Za buduća ažuriranja zadržite isti ključ.

## 11. Firebase i AdMob
Firebase zahtijeva odgovarajući `google-services.json` za točan package ID. AdMob zahtijeva vlastite App ID i banner ID te stvarnu suglasnost. Ovi SDK-ovi ostvaruju mrežne pozive i podliježu odgovarajućim Google pravilima.

## 12. Android Mobile klijent

Od v0.6.8 dostupan je zaseban Android emulator smoke test. Provjerava se aktivacijski ekran na tri veličine te pokretanje generiranog offline HTML WebView projekta. Stvarne snimke dostupne su u GitHub Actions artefaktu `brendigo-android-emulator-ui` nakon uspješne izgradnje. Fizičke kamere, mikrofon, GPS, Google Play i produkcijsko potpisivanje i dalje zahtijevaju zasebno testiranje.

Android klijent služi za planiranje i prijenos konfiguracija putem lokalnih JSON datoteka, prikaz licenci i dokumentacije. Sam Android telefon u ovom projektu ne kompajlira Gradle APK/AAB; takva izrada obavlja se u Windows Studiju. Na mobilnom uređaju izgled je prilagodljiv telefonu i tabletu.

## 13. Google Play
Provjerite pravo na objavu sadržaja, politiku privatnosti, Data safety, oglasne deklaracije, target SDK, certifikate i pravila naplate. Google Play zadržava pravo odbiti aplikaciju. Potpisani AAB nije jamstvo odobrenja.

## 14. Rješavanje poteškoća
Windows Builder ne zahtijeva aktivacijski ključ. Ako se ne može otvoriti projekt, provjerite lokalnu bazu `projects.json` i sigurnosnu kopiju `projects.json.bak`. Kod oštećenja program nudi obnovu kroz Postavke, uz očuvanje izvorne datoteke u `projects.json.recovery-*.json`. Za APK/AAB provjerite JDK 17, Gradle i Android SDK u odjeljku Razvojni alati, valjani package ID i ispravnu konfiguraciju projekta. Za potpisanu produkcijsku nadogradnju upotrijebite svoj postojeći keystore. Program ne ispisuje unesene potpisne lozinke u uobičajeni Gradle zapisnik, ali ne šaljite vlasničke ključeve ili neuređene zapisnike. Podrška: **info@apk.com.hr**.

## BL2 licence za zasebni Android Mobile klijent
Android Mobile klijent i dalje može tražiti vlastitu BL2 aktivaciju; to **ne utječe na besplatno korištenje Windows Builder Portable/Setup verzija**. Zasebni License Studio služi za stariji Android BL2 model. Nemojte dijeliti privatne izdavateljske ključeve ni aktivacijske tokene.

## 19. Novije podešavanje Android SDK platforme
Pri izradi Studio provjerava projektni Target SDK i preuzima platformu 36 ili 37 prema odabranom API-ju. Nije potreban Android Studio ni ručno upravljanje SDK managerom; preuzimanje se potvrđuje u programu, a provjera arhive koristi SHA-256. API 37 nije automatski preduvjet za projekte koji koriste API 36.
