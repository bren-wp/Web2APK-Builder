# Brendigo Web2APK Builder — promjene

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

