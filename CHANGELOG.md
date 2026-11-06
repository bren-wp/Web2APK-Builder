# Brendigo Web2APK Builder — promjene

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

