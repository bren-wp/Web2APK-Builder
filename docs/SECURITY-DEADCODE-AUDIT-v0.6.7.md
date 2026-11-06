# Brendigo Web2APK Builder — sigurnost, privatnost i dead-code pregled v0.6.7

**Datum:** 6. studenoga 2026. **Autor:** Brendigo.
**Repozitorij:** https://github.com/bren-wp/Web2APK-Builder

## Status i metodologija

Ovo je **prošireni pregled koda u aktualnoj iteraciji**, a ne certifikat da je
softver bez ranjivosti. Pregledani su jezgreni moduli, generiranje Android
WebView izvora, Gradle proces, lokalni keystore, licenciranje, Windows UI,
popis projekata, stvarni WPF capture mehanizam, postojeći regression testovi
i GitHub Actions. U dva odvojena statička pregleda analizirano je **10
jezgrenih datoteka (1.073 retka)** i **10 Windows/License Studio izvora
(2.447 redaka)** kao kandidata za neiskorištene privatne metode, uz zaseban
pregled dodatnih kritičnih modula AndroidBuildService, AndroidProjectGenerator
i IntegratedToolchainService. Ta analiza nije dokaz potpunog pokrivanja svakog
izvršnog puta.

CI dodatno pokreće C# Roslyn analizu u `latest-all` načinu i NuGet upit za
poznate ranjivosti, uključujući tranzitivne pakete. Ishod tih provjera
mjerodavan je tek nakon uspješnog završetka GitHub Actions za v0.6.7.
Potvrda builda nije potvrda sigurnosti svih runtime putanja.

## Sigurnosni nalazi i ispravci

| Nalaz | Rizik | Izvedena mjera |
|---|---|---|
| WebView podrijetlo prije je uspoređivano samo po hostu | **Visok**: ista domena na drugom portu/protokolu mogla je biti tretirana kao pouzdana za web dopuštenja | Provjerava se protokol, host i normalizirani port; pravilo vrijedi za kameru, mikrofon, geolokaciju i vanjske URL-ove |
| Naslijeđene `BRENDIGO_*` varijable potpisivanja | **Srednji**: build označen nepotpisanim mogao je naslijediti ključ iz roditeljskog procesa | Potpisne varijable se uklanjaju iz djetetova okruženja prije izričitog postavljanja odabranih vrijednosti |
| Mogućnost ispisa signing lozinki kroz Gradle izlaz | **Srednji**: lozinke su se mogle pojaviti u in-app zapisniku ako ih je alat ispisao | Sada se obje lozinke prikrivaju u stdout/stderr Gradle redovima, duži preklapajući nizovi prvi |
| Vremenski ograničena dijagnostika alata nije eksplicitno prekidala proces | **Nizak/srednji**: proces mogao ostati aktivan poslije timeouta | Prilikom isteka 12 s vremenskog ograničenja prekida se cijelo podređeno procesno stablo |
| Dugački nazivi i mnoštvo gumba u popisu projekata | **UI funkcionalnost**: moguće vodoravno preklapanje ili gubitak podataka na užem prozoru | Grid raspored s dvorednim akcijama, tooltip i skraćivanje teksta; test pri 900 px |

## Dead-code pregled

- Privatne metode pregledane su pretragom njihovih deklaracija i referenci.
  Nisu otkrivene metode za koje je na temelju tih dokaza **sigurno** da se
  mogu ukloniti bez rizika za funkcionalnost.
- Metode kao `Window_SizeChanged`, `TitleBar_Drag`, `Activate_Click`,
  `BrowseKey_Click`, `Export_Click` i druge imaju malo C# referenci jer
  su pozvane iz **WPF XAML događaja**; ne smiju se označiti mrtvima.
- `BuildStep`, `Wizard`, `Preview`, `Docs`, `About`, ostale
  navigacijske metode te callbackovi imaju valjane pozivne putanje.
- Kod za `UI_CAPTURE` koristi se samo tijekom zasebne CI izgradnje, ne u
  produkcijskom EXE-u; njegovo prividno nekorištenje u produkciji je
  namjerno.
- Nije provedeno neselektivno brisanje koda. Daljnja provjera treba
  uključiti Roslyn izvještaj, analizu XAML vezivanja, provjeru događaja
  tijekom rada i instrumentaciju testova.

## Izvršene/promijenjene provjere

1. Generirani Java izvor: prisutna usporedba HTTPS/HTTP protokola, hosta i
   efektivnog porta te usmjeravanje eksternih URL-ova.
2. Android signing: naslijeđene env varijable se uklanjaju, odabrani ključ
   se postavlja, lozinke su izostavljene iz standardnog Gradle loga.
3. Windows ProjectRow: adaptivni raspored na 900 px, gumbi ispod naziva,
   dugi nazivi dostupni tooltipom; provjera stvarne WPF geometrije s
   testnim redom koji se ne objavljuje kao snimka.
4. Stvarni WPF capture svakog koraka na 900 px, uz postojeće 1440 px,
   1024 px i raster DPI testove.
5. Roslyn i NuGet vulnerability audit u glavnom Quality workflowu.

## Poznati preostali rizici

- Offline licencna mjesta (Windows DPAPI i zasebni Android zapis) nisu
  globalno sinkronizirana. Nije moguće pouzdano spriječiti vraćanje kopije
  ili korištenje na drugom računalu bez aktivacijskog poslužitelja.
- Redakcija signing loga uklanja doslovne lozinke; ne može garantirati
  prepoznavanje hashiranih, kodiranih ili transformiranih oblika tajne.
  Podređeni Gradle proces i njegovi dodaci imaju pristup odabranom ključu.
- `cmd.exe`/Gradle/SDK manager procesi i preuzimanja vanjskog alatnog
  lanca ostaju dio napadne površine, čak uz provjeru SHA-256.
- NuGet analiza ne obuhvaća sve Maven/Google Play ovisnosti niti nove
  neobjavljene ranjivosti.
- Na stvarnom Android uređaju treba testirati dozvole, odabir datoteka,
  preuzimanja, ads consent, Firebase dostavu, cijeli potpisani APK/AAB i
  16 KB page-size kompatibilnost.
- Usporedba referentnih dizajna, fizički Windows 100/125/150/200 % DPI i
  povećanje sistemskog fonta ostaju nezavršene provjere. Testne snimke
  potvrđuju da se WPF prikazi mogu renderirati, ne pixel-perfect 1:1.
- Nije obavljen formalni full-system pentest, SBOM svih vanjskih
  komponenti, nezavisni audit poslovne logike ili kriptografski audit.

## Preporučene sljedeće radnje

Posebno provjeriti Maven i Gradle dependency CVE evidenciju i izraditi SBOM,
pokrenuti instrumentirane Android testove, provjeriti sva WPF stanja s
pravim projektima i dugim tekstovima, izvršiti usporedni screenshot diff
sa svakom izvornom 1:1 referencom, te testirati potpisivanje s vlasničkim
upload ključem samo u sigurnom izdvojenom okruženju. Ne unositi privatni
RSA izdavateljski ključ ili potpisni keystore u javni GitHub repozitorij.
