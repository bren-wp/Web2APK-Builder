# Brendigo Web2APK Builder — promjene

## v0.6.10 — stvarni pregled uređaja, izvorni kod i ZIP bez licence

- **Preview zaslon:** kontrola „WebView sadržaj” ne prikazuje aktivno stanje prije učitavanja preglednika; tipka „Pokreni test” uključuje živi pregled, a isključivanje vraća simulirani prikaz i oslobađa WebView. Prelazak na telefon, tablet ili offline poništava stanje aktivnog pregleda.
- **Više stvarnih WPF stanja:** nove snimke Preview telefona, tableta, offline prikaza i bez Splash ikone na 900/840 px; kontrola stvarnih promjena klikovima gumba za uređaje.
- **Source Viewer:** ispravljen zastarjeli tekst o plaćenim paketima, jer je Windows Builder od v0.6.8 bez licenci. Izvorni kod i ZIP dostupni su svima, a gumbi sada koriste standardnu WPF Studio tipografiju i vidljivi fokus.
- **Responzivni Source Viewer:** prilagođava širinu popisa datoteka i prebacuje ZIP/Zatvori gumbe u drugi red pri užem prozoru. CI sada snima stvarni Viewer pri standardnoj i najmanjoj širini 760 px i testira da nijedan gumb ne izlazi iz prozora.
- **Sigurnost ZIP izvoza:** generiranje ide u privremenu datoteku u odredišnoj mapi i atomarno se premješta nakon uspješnog završetka; odredišta unutar izvorne mape odbijaju se, a privremeni ostatci brišu se pri pogrešci.
- **Dokumentacija:** uklonjene su zavaravajuće upute da Windows Builder i dalje troši BL2 licencna mjesta; zasebni Android Mobile BL2 model opisan je odvojeno.
- Windows i License Studio 0.6.10, instaleri 0.6.10 te Android Mobile 0.6.10 (`versionCode 17`); svih šest standardnih release datoteka ostaje obvezno.
- **Windows Builder Portable i Setup i dalje ne traže licencu, aktivaciju, račun ni plaćanje.**
- Pixel-diff WPF testa uspoređuje dva stvarna izdanja; apsolutna 1:1 jednakost sa svim izvornim dizajnerskim referencama još nije potvrđena.


## v0.6.9 — responzivni 1:1 rad po zaslonima i puna usporedba piksela

- Ispravljena kompaktna bočna navigacija u Windows Builderu: logotip i ikone imaju prilagođene razmake, tekstualne oznake odjeljaka nestaju samo kada suženi izbornik nema dovoljno prostora, a svaki gumb zadržava vlastiti tooltip.
- Zaglavlje prilagođeno malim prozorima: smanjene unutarnje margine, skrivene nebitne statusne oznake pri užem prikazu i breadcrumb tekst se skraćuje uz dostupni puni tekst.
- Dodani vidljivi fokus pri upravljanju tipkovnicom za glavne WPF gumbe i License Studio; boje u čarobnjaku imaju jednoznačne pristupačne nazive i tooltipe.
- **Stvarna kontrola svakog piksela:** WPF CI uspoređuje sve piksele prethodnih i novih PNG snimki, računa broj promijenjenih piksela i udjel razlika te odbija velike neplanirane vizualne regresije. Strojno čitljiv JSON izvještaj objavljuje se u GitHub Actions artefaktu.
- Prošireno snimanje **svih devet glavnih ekrana i šest koraka čarobnjaka na 840 × 570** te dodatno svih šest koraka na 1024 × 740; u CI-u se provjeravaju uski sidebar, zaglavlje, kartice i vodoravno odrezane kontrole.
- License Studio dodatno snima stvarne WPF prikaze na 900 × 650 i minimalnom prozoru 760 × 605; screenshotovi su spremljeni u repozitorij.
- Sve oznake verzije usklađene na Windows/License Studio 0.6.9 te Android 0.6.9 (`versionCode 16`); šest standardnih Release artefakata ostaje nepromijenjeno.
- **Windows Builder Portable i Setup ostaju potpuno bez licenci, aktivacije i ograničenja funkcionalnosti**, prema prethodnom zahtjevu.
- Ograničenja: ovo nije potvrda apsolutne 1:1 podudarnosti sa svim dizajnerskim referencama ni potvrda fizičkog DPI skaliranja i svih stanja aplikacije.


## v0.6.8 — Windows Portable/Setup bez licenci, emulator i vizualne regresije

- **Po zahtjevu vlasnika**, Windows Builder Portable.exe i Setup.exe pokreću se bez BL2 licence, aktivacijskog prozora, registracije ili prijave. Sve mogućnosti APK/AAB/izvornog koda/brandinga/AdMob/Firebase dostupne su bez rezerviranja mjesta.
- Windows prikaz Postavki i ugrađena Dokumentacija više ne nude aktivaciju niti zastarjele pakete. Vlasničke BL2 korisničke datoteke ostaju netaknute; odvojeni Android Mobile klijent i License Studio i dalje podržavaju postojeće BL2 licence.
- Dodani regresijski testovi da Windows edition radi bez licence, da su dostupni osnovni i napredni izvozi, te da Android klijent nije implicitno otključan.
- Ispravljen CI build-pogreška CS0136 (ponovljena lokalna varijabla `root` u emulator fixtureu) i Android SDK postavljanje na dostupni paket `platform-tools`.

- Novi zasebni GitHub Actions workflow izrađuje i pokreće stvarni Android klijent i generiranu offline WebView aplikaciju na API 35 emulatoru.
- Testira se aktivacijski ekran mobilnog klijenta na tri rezolucijska/gustoćna profila i učitavanje stvarnog lokalnog HTML-a; PNG snimke i XML hijerarhije pohranjuju se u CI artefakt.
- WPF testovi sada stvarno aktiviraju navigacijske gumbe Welcome → Novi projekt, Dashboard → Projekti i Projekti → Novi projekt.
- U Release postupak dodana je usporedba uzorkovanih RGB piksela 20 stvarnih Windows zaslona s prethodno spremljenim snimkama te izvozni JSON izvještaj; velika neplanirana vizualna regresija zaustavlja izdanje.
- Generator Android resursa obrađuje apostrofe, navodnike i početne znakove `@`/`?`, uz dodatni C# regresijski test i build-fixture s nazivom koji sadrži posebne znakove.
- Metapodaci: Windows i License Studio 0.6.8, oba instalera 0.6.8, Android klijent 0.6.8 (`versionCode 15`).
- Pravila potpisanih BL2 licenci zadržana su za zasebni Android Mobile klijent; Windows Builder izdanje bez licenci ne primjenjuje ograničenja i cijene. Emulator nije fizički uređaj i WPF regresijska kontrola ne potvrđuje pixel-perfect podudarnost.


## v0.6.7 — WebView origin sigurnost, potpisivanje i detaljniji pregled Windows zaslona

- Generirane Android aplikacije prije dodjele WebView dozvola sada provjeravaju **HTTPS/HTTP shemu, host i stvarni/default port**, a ne samo domenu; ista domena na drugom portu ili preko HTTP-a više ne dijeli automatsko povjerenje.
- Usmjeravanje vanjskih poveznica slijedi istu provjeru podrijetla, čime se izbjegava zadržavanje drukčijeg porta/protokola unutar web aplikacije kada je uključeno otvaranje izvana.
- AndroidBuildService eksplicitno čisti naslijeđene varijable `BRENDIGO_*` za potpisivanje prije pokretanja Gradlea; samo odabrani keystore postavlja nove vrijednosti. Izlaz Gradlea prikriva unijete lozinke.
- Dijagnostika JDK/Gradle/ADB alata prekida podređeni proces pri isteku vremenskog ograničenja.
- Popis Windows projekata prelazi na raspored u dva reda u užim prozorima; duga imena, URL-ovi i package ID-jevi koriste skraćivanje i tooltip, a tablični prikaz Dashboarda skraćuje predugačke vrijednosti.
- Dodane realne WPF snimke svih šest koraka na 900 px te zaslona Uređaji i O programu na 900 px; test reda s dugim nazivom koristi isključivo testni UI element, **ne objavljuje se kao screenshot**.
- Prošireni automatizirani testovi izvornog Java koda, varijabli potpisivanja i redakcije lozinki; Quality CI uključuje Roslyn analizu i provjeru poznatih NuGet ranjivosti.
- Usklađene Windows, License Studio i oba Inno Setup instalera na verziju 0.6.7; Android klijent 0.6.7 (`versionCode 14`).
- Zadržana pravila i valjanost prethodno izdanih BL2 licenci, sve paketne mogućnosti i dosadašnji cjenik.
- Preostale granice: automatizirani UI screenshotovi nisu potvrda pixel-perfect 1:1 prema svim originalnim referencama; nisu odrađeni fizički DPI/Android testovi ni vanjski penetracijski test.


## v0.6.6 — oporavak oštećenih projekata i privatniji lokalni HTML izvoz

- Ispravljen prekid pri otvaranju Windows Studija ako je lokalna baza `projects.json` oštećena: dostupne su Postavke i kontrolirani oporavak.
- Dodan gumb za sigurno vraćanje projekata iz `projects.json.bak`, uz prethodnu validaciju, izričitu potvrdu i arhiviranje izvorne datoteke `projects.json.recovery-*.json`; automatska licencna mjesta ostaju netaknuta.
- Testiran oporavak 23 projekta nakon oštećenja baze; nevaljana kopija blokira vraćanje i čuva postojeće podatke.
- Spriječeno uključivanje u lokalni Android HTML sadržaj tipičnih privatnih ključeva i konfiguracija (`.pem`, `.pfx`, `.p12`, `.jks`, `.keystore`, `credentials.json`, `google-services.json`, `service-account.json`, `firebase-adminsdk*`).
- Ugrađene korisničke upute o oporavku i osvježena dokumentacija izvornog koda; dodan sigurnosni pregled s otvorenim pitanjima i ograničenjima.
- Proširene stvarne WPF snimke zaslona Razvojni alati i Dokumentacija na 900 px; Release sada provjerava najmanje 30 stvarnih snimaka.
- Usklađene verzije programa i oba Windows instalera na 0.6.6 te Android klijenta na versionName 0.6.6 i versionCode 13.
- Zadržana postojeća BL2 kompatibilnost, cijene, paketi i pravila zauzetih mjesta.


## v0.6.5 — automatski Android identitet, transparentno licenciranje i WPF DPI provjere

- Novi projekti u Windows čarobnjaku automatski dobivaju jedinstven package ID iz naziva i internog GUID-a; hrvatski znakovi pretvaraju se u valjani ASCII, ručni unos nije obvezan.
- Preimenovanje postojećeg projekta čuva package ID. Promjena ID-ja već spremljenog projekta zahtijeva potvrdu jer može značiti novu Play aplikaciju i novo licencno mjesto.
- Postavke prikazuju točno vezane package ID-jeve automatskih BL2 licenci bez trošenja mjesta. Stari potpisani BL2 tokeni ostaju kompatibilni.
- Za ZIP izvornog koda najprije se bira odredište, a zatim ponovno provjerava ovlaštenje prije spremanja.
- Dodani testovi stabilnog identiteta, hrvatskih slova, kolizija između projekata i čitanja licencnih mjesta te stvarne WPF snimke čarobnjaka pri 125 %, 150 % i 200 % gustoće bitmapa.
- Ispravljeni Inno Setup metapodaci oba Windows instalera. Windows i License Studio nose verziju 0.6.5; Android klijent 0.6.5, versionCode 12.
- Ograničenja: offline DPAPI evidencija ne jamči sinkronizaciju mjesta na više računala; rasterizacijski DPI test nije ekvivalent ručnom testu na monitorima s različitim Windows skaliranjem.

## v0.6.4 — sigurnija baza i usklađene binarne verzije

- `projects.json.bak` čuva prethodno valjano stanje; više procesa serijalizira spremanje.
- Ograničenje veličine baze, nevaljanih i dupliciranih projekata te test 24 paralelna upisa.
- Stvarni WPF prikazi Projekata i Postavki na 900 px; Windows/Android assembly verzioniranje usklađeno.


## v0.6.3 — licencna mjesta i responzivni Windows zasloni

- Odabir izlazne mape i otkazivanje pripreme alata više ne zauzimaju automatsko licencno mjesto unaprijed; stvarni izvoz ili početak izrade i dalje provjeravaju potpisana prava i broj mjesta.
- Ojačana obrada oštećene lokalne DPAPI evidencije te test koji provjerava izmijenjeni interni serijski broj i odbija nevaljane podatke.
- Početni zaslon i Dashboard na uskom prozoru oslobađaju cijelu širinu skrivene ilustracije, umjesto ostavljanja praznog stupca.
- U sažetku projekta dugi nazivi i package ID-jevi skraćuju se vizualno, a cjeloviti tekst dostupan je preko tooltipa.
- Uvedeno stvarno automatizirano snimanje zaslona License Studija i dvije dodatne provjere rasporeda pri 900 px. Snimke se spremaju u repozitorij, ne generiraju se umjetno.
- Poseban quality workflow pokreće se i na izravne promjene grane main, bez obveze otvaranja pull requesta.


## v0.6.2 — automatske licence bez ručnog package ID-ja

- Brendigo License Studio generira nove BL2 licence s automatskim opsegom `@` za Demo, Osnovno i Profesionalac, bez unosa package ID-ja.
- Windows Builder pri izvozu izvornog projekta ili izradi APK/AAB-a automatski rezervira licencno mjesto za točan Android package ID. Ažuriranje istog package ID-ja ne troši novo mjesto, dok brisanje projekta ne vraća mjesto.
- Lokalna evidencija mjesta koristi Windows DPAPI i provjeru cjelovitosti; izmijenjeni zapisi blokiraju izvoz. To nije centralni aktivacijski servis i ne sprječava brisanje ili vraćanje lokalne evidencije.
- Postavke prikazuju broj zauzetih i preostalih mjesta. Čarobnjak prikazuje prava novih licenci bez preuranjenog zauzimanja mjesta.
- Android klijent prihvaća novi tip potpisane licence i vodi zasebnu lokalnu evidenciju konfiguracija; ne izrađuje APK/AAB i ne sinkronizira korištenje s Windowsom.
- Kompatibilnost s već izdanim BL2 licencama i postojeće cijene, prava paketa i rokovi ostaju očuvani.
- Dodani testovi automatskog opsega, granica licenci, ponovne upotrebe i DPAPI evidencije.


## v0.6.1 — precizniji Windows zasloni i puni odabir Android API-ja

- Nativni Windows naslovni okvir s Brendigo ikonicom, gumbima minimiziranja, povećanja i zatvaranja te mogućnošću normalne promjene veličine prozora.
- Usklađena visina gornjeg zaglavlja i razmaci na ekranu Novi projekt prema originalnoj referenci 1440×900.
- Tamne WPF liste i označavanje datoteka unutar preglednika izvornog koda, bez bijelog zadanog izgleda.
- Integrirani Android SDK sada prema projektnom Target SDK odabire platformu **36 ili 37**. JDK 17 i Gradle 9.6.1 ostaju dostupni kroz Studio.
- Zadržana kompatibilnost s potpisanim BL2 licencama v0.6.0, paketima Profesionalac (3 unaprijed potpisane aplikacije), Agencija i Developer.
- Provjera svih postojećih Windows/Android buildova i stvarnih WPF snimki ostaje uvjet objave.


## v0.6.1 — Profesionalac 3 aplikacije, Agencija i izvorni kod

- Profesionalac 69 € doživotno za do 3 različita package ID-ja u vlasnički potpisanom BL2 tokenu.
- Profesionalac sada dopušta AdMob, kameru, mikrofon, lokaciju, Firebase i napredne opcije.
- Novi Agencija paket s potpisanim trajanjem od mjesec dana ili 12 mjeseci; predložene cijene 39 €/mj. ili 390 €/god., neograničeno projekata i izrada aplikacija za klijente.
- Android klijent i Windows provjeravaju novi opseg i istek agencijske licence, uz kompatibilnost ranijih BL2 tokena.
- Ugrađeni Windows preglednik izvornog koda i ZIP izvoz (Osnovno, Profesionalac, Agencija, Developer).
- Provjera JDK/Gradle/SDK alata pri APK/AAB buildu, uz korisnikovu izričitu potvrdu početnog preuzimanja.
- Tamni dropdown i polje Minimalni Android SDK preko pune širine u Windows čarobnjaku.
- Testovi za četvrtu Pro aplikaciju, datume isteka Agencije, pravilna prava za AdMob i ograničenja Demo izvoza.


## v0.5.0 — Brendigo brending, licence, ugrađeni razvojni alati i dokumentacija

- Zadana originalna Brendigo ikona i splash s oznakom apk.com.hr za svaku novu Android aplikaciju.
- Demo: brendirani splash i launcher ikona zaključani u Windows i Android provjerama; napredniji paketi smiju ih prilagođavati.
- U Postavkama prikaz maskiranog licencnog ključa te sigurna zamjena pojedine BL2 licence bez brisanja prije verifikacije nove.
- APK debug sa standardnim Android razvojnim potpisom, nepotpisani APK release i postojeći signed/unsigned AAB.
- Iz Windows Studija upravljanje lokalnom instalacijom Temurin JDK 17, Gradle 9.6.1, Android SDK 36 i ADB uz korisničku potvrdu i SHA-256 provjeru.
- WebView uključen pregled za raspoloživu širinu zaslona i Android insets; ne mijenja CSS udaljenog web-sadržaja.
- Tamne Windows forme, PasswordBox, kontekstni izbornici i jasniji status Android SDK-a.
- Ažurirani O programu, opširna ugrađena dokumentacija i kontakt: program apk.com.hr; podrška info@apk.com.hr; razvoj Brendigo / brendigo.com.
- README sada sadrži stvarne screenshotove, opise paketa i stvarne uvjete distribucije. Nema GitHub poveznica u korisničkom sučelju.
- Uklonjeni nekorišteni generator atributi i uvozi; dokumentiran cilj i ograničenja sigurnosnog/dead-code pregleda.
- Svako izdanje ostaje uvjetovano uspješnom GitHub Actions gradnjom Windows i Android paketa te stvarnim UI snimkama.

