# Brendigo Web2APK Builder — potpune korisničke upute (HR)

**Službeni web:** https://apk.com.hr · **Podrška:** info@apk.com.hr · **Autor:** Brendigo / https://brendigo.com

## 1. Instalacija Windows programa
Preuzmite `Brendigo-Web2APK-Setup.exe` za instalaciju ili `Brendigo-Web2APK-portable.exe` za pokretanje bez instalacije. Prozor se otvara u standardnoj veličini i može se smanjivati. Datoteke aplikacije i projekata spremaju se lokalno. Windows izvršne datoteke mogu biti nepotpisane; prije pokretanja provjerite izvor izdanja.

## 2. Aktivacija i zamjena licenci
Prilikom prvog pokretanja unesite BL2 licencu. U Postavke → Licence vidi se paket, package ID i **maskirani** ključ; kompletan tekst ključa nije prikazan. Klikom na **Zamijeni ovu licencu** unesite novu. Novi token mora imati valjan RSA potpis i pravo korištenja na Windowsu; stari se briše tek nakon prihvaćanja novog. Ako želite dodati još jednu licencu, izaberite **Dodaj novu licencu**. Ne objavljujte licencu javno.

## 3. Paketi i ograničenja
Demo: 0 €, jedna aplikacija, trajno, APK, obavezna Brendigo ikona i splash. Osnovno: 49 €, jedna aplikacija, APK/AAB i vlastiti brending. Profesionalac: 69 €, jedna doživotna licenca za najviše tri aplikacije (novi automatski opseg `@` ili stari unaprijed potpisani package ID-jevi), s AdMobom, kamerom, mikrofonom, lokacijom, Pushom i osvježavanjem. Agencija: predloženo 39 €/mj. ili 390 €/god., neograničeni projekti i pravo izrade te komercijalne isporuke aplikacija klijentima tijekom aktivne pretplate. Developer (vlasnička licenca): neograničeno Android aplikacija, sve dostupne funkcije, obje platforme. Paket se provjerava pri generiranju; opcije samo vizualno skrivene bez zaštite ne bi bile dovoljne.

## 4. Izrada projekta
Otvorite Novi projekt. Unesite naziv, `applicationId` u obliku `com.tvrtka.aplikacija`, HTTPS URL ili lokalnu mapu s `index.html`, verziju i `versionCode`. Za ažuriranja povećajte `versionCode`. Ne radite izlaznu mapu unutar lokalnog HTML izvora.

## 5. Brending i ikone
Nova Android aplikacija po zadanim postavkama ima Brendigo vektorsku launcher ikonu i brendirani splash. Demo to ne može promijeniti. Viši paketi mogu odabrati PNG minimalno 192×192, postaviti vlastitu boju, isključiti Brendigo na splash ekranu ili promijeniti trajanje. Za konačni izgled obavezno testirajte uređaj različite gustoće zaslona.

## 6. WebView i responzivnost
WebView je prilagodljiv širini dostupnog zaslona i vodi računa o sistemskim rubovima. URL mora biti HTTPS, osim kad korisnik izričito uključi HTTP. JavaScript, DOM Storage, datoteke i vanjske poveznice podešavaju se u čarobnjaku. Ako je sama web-stranica napravljena s fiksnim CSS širinama, Builder to ne može automatski ispraviti; sadržaj web-stranice mora imati vlastiti responsivni CSS i viewport.

## 7. Dozvole
Uključujte samo potrebne dozvole. Kamera i mikrofon zahtijevaju potvrdu sustava Android. Lokacija se dodatno autorizira. Za datoteke koristite sistemski picker. Osjetljive dozvole i AdMob dostupni su već u Profesionalcu, kao i svi skuplji paketi. Firebase i refresh dostupni su u Profesionalcu, Agenciji i Developeru.

## 8. Pregled i preuzimanje izvornog koda
Osnovno, Profesionalac, Agencija i Developer imaju gumb **Pregledaj izvorni kod** u koraku Izrada i na popisu projekata. Otvara se ugrađeni Windows preglednik s Java, XML, Gradle i ostalim datotekama. Za čuvanje svih datoteka odaberite **Preuzmi izvorni kod (.zip)**. Prije otvaranja i izvoza program ponovno provjerava licencu. Demo nema pristup ni ZIP izvozu.

## 9. Android razvojni alati unutar Windows programa
Pri prvom pokretanju izrade APK/AAB program sam provjerava razvojne alate i, ako nedostaju, nudi njihovu pripremu uz prihvaćanje uvjeta. Po želji u Razvojni alati kliknite **Pripremi JDK + Gradle + Android SDK unutar Studija**. Nakon što izričito prihvatite prikazane uvjete i preuzimanje, aplikacija koristi verificirane ZIP arhive iz službenih izvora i lokalno ih raspakira u Brendigo korisnički profil. Potrebni su internetska veza i diskovni prostor. Nije potrebna zasebna instalacija Android Studija. Ako preuzimanje ne uspije, moguće je koristiti već instalirani JAVA_HOME, Gradle i ANDROID_HOME. Pripremu prije produkcijske uporabe ispitajte na čistom Windows računalu.

## 9. APK debug, nepotpisani APK i AAB
U koraku Izrada birajte **Debug APK** za lokalno testiranje (potpisan razvojnim ključem), **Nepotpisani APK** za release bez keystorea, **Izradi APK** za potpisan ili nepotpisan release, te **Izradi AAB** za App Bundle. AAB bez keystorea ostaje nepotpisan. Nepotpisani APK nije instalabilan kao produkcijska aplikacija dok ne bude potpisan. Debug APK nije dopušten za produkcijsku objavu na Google Playu.

## 10. Trajni Android ključ
Možete učitati postojeći keystore ili lokalno generirati novi. Sačuvajte `.p12`/`.jks`, alias i zaporku izvan projekta. Gradle prima tajne samo kroz okolinske varijable procesa; ne stavljajte ih u README niti u javne repozitorije. Za buduća ažuriranja zadržite isti ključ.

## 11. Firebase i AdMob
Firebase zahtijeva odgovarajući `google-services.json` za točan package ID. AdMob zahtijeva vlastite App ID i banner ID te stvarnu suglasnost. Ovi SDK-ovi ostvaruju mrežne pozive i podliježu odgovarajućim Google pravilima.

## 12. Android Mobile klijent
Android klijent služi za planiranje i prijenos konfiguracija putem lokalnih JSON datoteka, prikaz licenci i dokumentacije. Sam Android telefon u ovom projektu ne kompajlira Gradle APK/AAB; takva izrada obavlja se u Windows Studiju. Na mobilnom uređaju izgled je prilagodljiv telefonu i tabletu.

## 13. Google Play
Provjerite pravo na objavu sadržaja, politiku privatnosti, Data safety, oglasne deklaracije, target SDK, certifikate i pravila naplate. Google Play zadržava pravo odbiti aplikaciju. Potpisani AAB nije jamstvo odobrenja.

## 14. Rješavanje poteškoća
Ako korisnik odustane od izbora izlazne mape ili pripreme razvojnih alata, novo automatsko licencno mjesto ne zauzima se. Kada izvoz stvarno započne, package ID rezervira se i za neuspjeli pokušaj izrade, jer se mjesto dodjeljuje prije pokretanja Gradlea. Ako build ne uspije, u Razvojni alati provjerite Java, Gradle i Android SDK. Pogrešan package ID ili licenca blokiraju izvoz. Za potpis upotrijebite isti keystore kao u prethodnoj verziji. Pri prijavi na **info@apk.com.hr** pošaljite opis, verziju i anonimizirani zapisnik, bez lozinki, keystorea ili punih licencnih tokena.

## Agencijska pretplata i tri aplikacije
Za nove licence Profesionalac u License Studiju ne unosite package ID-jeve. Paket automatski povezuje tri različita Android package ID-ja pri prvom stvarnom izvozu ili izradi na Windowsu. Brisanje projekta ne oslobađa licencno mjesto; ponovno otvaranje ili ažuriranje istog package ID-ja ne troši novo. Stari tokeni s unaprijed potpisanim ID-jevima ostaju podržani. Evidencija je lokalno zaštićena Windows DPAPI-jem, bez centralne sinkronizacije između računala. Agencija koristi `*` i može izvoziti aplikacije za klijente dok je licenca valjana; na datum isteka novi izvozi se blokiraju. Ova offline pretplata ne uključuje automatsku naplatu ni pouzdanu zaštitu od vraćanja sistemskog sata. Pri obnovi u Postavke → Licence unesite novu potpisanu licencu.

## 19. Novije podešavanje Android SDK platforme
Pri izradi Studio provjerava projektni Target SDK i preuzima platformu 36 ili 37 prema odabranom API-ju. Nije potreban Android Studio ni ručno upravljanje SDK managerom; preuzimanje se potvrđuje u programu, a provjera arhive koristi SHA-256. API 37 nije automatski preduvjet za projekte koji koriste API 36.
