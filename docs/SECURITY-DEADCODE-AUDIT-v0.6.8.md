# Brendigo Web2APK Builder — vizualni, funkcionalni i sigurnosni pregled v0.6.8

Datum: 7. studenoga 2026. Autor: Brendigo.

## WPF: stvarna interakcija i pixel-regresijski test

Nakon pokretanja izvornog Windows WPF sučelja u izoliranom načinu
automatizirana provjera snima zaslone i klikom aktivira tri stvarna
navigacijska gumba: Welcome → Novi projekt, Dashboard → Moji projekti,
Projekti → Novi projekt. Dodatno uspoređuje 20 stvarnih WPF PNG snimki
s prethodno spremljenim verzijama: korak uzorkovanja 20 px, razlika RGB
kanala ukupno veća od 54 i prag od 40 % promijenjenih uzoraka.
Rezultat se pohranjuje u JSON artefakt brendigo-wpf-visual-audit.

**Granica:** ta metoda detektira veće neočekivane vizualne regresije, ali
ne dokazuje 1:1 podudarnost sa svim konceptnim referencama niti ispituje
svaki modalni dijalog, hover, focus, sistemsku temu i povećani font.

## Android emulator: stvarno pokretanje aplikacija

Neovisni CI postupak na Ubuntu emulatoru API 35 provjerava početni
aktivacijski ekran Android klijenta na tri profila zaslona, izgrađuje
zaseban offline WebView projekt iz lokalnog HTML-a i provjerava njegov
tekst u Android UI hijerarhiji. Sprema se nekoliko stvarnih PNG
screenshotova, odgovarajućih XML hijerarhija i filtrirani logcat.

**Granica:** ovo nisu fizički uređaji, validni komercijalni licencni
tokovi, testovi kamere/mikrofona/lokacije, stvarna dostava Firebase
poruka ili Google Play upload/proizvodno potpisivanje.

## Android build kompatibilnost

Generator u `strings.xml` sada uz XML kodiranje primjenjuje Android
resource escaping na posebne znakove u nazivu (apostrof, navodnik,
backslash, @, ? i prijelom retka). Provjera koristi naziv s apostrofom
i simbolima te kompilaciju stvarnog generiranog projekta.

## Sigurnost i dead-code zaključci

Prethodne provjere v0.6.7 uvele su provjeru točnog WebView podrijetla
(shema + host + port), čišćenje naslijeđenih env varijabli potpisivanja,
redakciju lozinki, Roslyn analizu i NuGet vulnerability query.
Nije identificirana sigurna metoda za uklanjanje bez provjere WPF XAML
event-veza i dodatne runtime pokrivenosti; cilj je spriječiti lažno
označavanje koda mrtvim.

## Preostali rizici

- Bez poslužitelja nije moguće centralno nametnuti ograničenje mjesta
  kod offline BL2 licenciranja na više računala/Android uređaja.
- Snimke WPF zaslona ne dokazuju pixel-perfect 1:1; potrebna je ručna
  i automatizirana usporedba s izvornim referentnim slikama i fontovima.
- Nije izveden neovisni penetracijski test niti potpuna provjera svih
  ovisnosti Maven/Gradle uz SBOM.
- Test emulatorskog početnog ekrana ne dokazuje sve korisničke funkcije.
- Za distribuciju su potrebni stvarni produkcijski Android potpisi i
  provjere fizičkih uređaja; nepotpisani AAB nije produkcijski artefakt.
- Neprovjereni su svi dijalozi, stanovi pogrešaka, 100/125/150/200 % fizički
  Windows DPI i potpuna pristupačnost svih zaslona.

Daljnje aktivnosti: testirati preostale funkcije s fizičkim uređajima,
usporediti originalne referentne slike pri identičnim viewportima,
uvesti formalni coverage i SBOM, izvršiti sigurnosno testiranje
neovisnim stručnjakom prije kritične produkcijske uporabe.
