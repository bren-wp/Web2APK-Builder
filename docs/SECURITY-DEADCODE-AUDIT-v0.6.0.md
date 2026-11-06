# Sigurnosni i dead-code pregled — v0.6.0

## Promjene i provjere
- **BL2 potpisi:** Pro opseg dopušta jedan do tri točno potpisana Android package ID-ja; četvrti ili ponovljeni ID odbija se. Stari Pro token za jedan package ID ostaje valjan.
- **Agencijska pretplata:** token s oznakom `agency` ima potpisani `exp` datum i opseg `*`; Windows i Android provjeravaju iste pravila. Stari `developer` tokeni ostaju doživotni.
- **Prava:** Profesionalac ima AdMob, kameru, mikrofon, lokaciju, Push i osvježavanje; Osnovno ima pristup izvornom kodu, Demo ne. Gate se provjerava u korisničkom UI-ju i metodama izvoza.
- **Izvorni kod:** novi WPF preglednik lokalno generira privremenu kopiju Gradle projekta; ograničava tekstualni pregled na 1 MB po datoteci i dopuštene podputanje. Pri ZIP izvozu ponovno se provjerava potpis licence. Privremena mapa briše se na zatvaranje.
- **SDK/JDK/Gradle:** izrada APK/AAB sada može sama pokrenuti pripremu alata unutar Windows Studija, nakon korisnikova pristanka i Googleovih SDK uvjeta. Arhive se provjeravaju SHA-256, SDK se ne uključuje u javni setup.
- **Tamne forme:** License Studio preuzima tamne ComboBox i ComboBoxItem predloške; osnovni čarobnjak daje minimalnom SDK polju punu raspoloživu širinu.

## Preostala ograničenja
- Offline licenca nije vezana uz uređaj i može se kopirati; lokalno vraćanje vremena može ograničiti pouzdanost mjesečne/godišnje pretplate. Za naplatu i opoziv potrebna je online komponenta s pravnom dokumentacijom.
- Google SDK nema dopuštenje za slobodnu redistribuciju kao zatvorena komponenta u našem EXE-u. Prva priprema zahtijeva mrežu i prihvaćanje Googleovih uvjeta.
- Proces automatske pripreme i ZIP izvoza treba testirati na čistom Windows računalu, a Android runtime dozvole na fizičkim uređajima. Kompilacija nije dokaz potpunog UX/1:1 i penetracijskog testiranja.
- Sve aplikacije trebaju vlastitu politiku privatnosti, pravilno konfiguriran Firebase/AdMob i usklađene Play Console deklaracije.

**Napomena:** ovo je statički i automatizirani pregled poznatog koda te povezani funkcionalni testovi, a ne certificiran sveobuhvatni sigurnosni audit. Kontakt: info@apk.com.hr.
