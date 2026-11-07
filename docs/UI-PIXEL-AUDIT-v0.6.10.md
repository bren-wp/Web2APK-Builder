# Brendigo Web2APK Builder — pregled zaslona i sigurnost izvoza v0.6.10

**Datum:** 7. studenoga 2026. **Projekt:** `bren-wp/Web2APK-Builder`.

## Stvarni Windows zasloni i interaktivna stanja

Pored ranijih standardnih prikaza i svih šest koraka čarobnjaka, CI sada
snima stvarne WPF zaslone različitih stanja Preview preglednika:

| Zaslon / stanje | Rezolucija | Izvor |
|---|---|---|
| Preview telefon | 840 × 570 | `native/preview-phone-minimum-840.png` |
| Preview tablet | 900 × 650 | `native/preview-tablet-small-900.png` |
| Preview offline | 900 × 650 | `native/preview-offline-small-900.png` |
| Preview bez splasha | 900 × 650 | `native/preview-no-splash-small-900.png` |
| Source Viewer | stvarni prozor ~1130 × 790 | `native/source-viewer.png` |
| Source Viewer kompaktni | stvarni prozor ~760 × 520 | `native/source-viewer-minimum-760.png` |

Snimke se generiraju iz izvršavanih WPF kontrola unutar GitHub CI-ja, a
ne iz ilustracija. CI pritom fizički podiže izvorni WPF Source Viewer,
generira lokalni Android projekt, provjerava da se lista izvornih
datoteka prikazuje, da editor postoji te da footer kontrole ostaju unutar
prozora. Stanja uređaja dodatno se provjeravaju stvarnim pozivima
`Button.Click` događaja, bez pristupa vanjskom web-sadržaju.

Zadržana je provjera svakog piksela u odnosu na prethodne stvarne WPF
snimke i uzorkovana kontrola velikih odstupanja. Snimke novododanih
stanja prvi put služe kao **početna regresijska referenca**, a ne
potvrda apsolutnog 1:1 izgleda s konceptnim dizajnom.

## Funkcionalne ispravke

- Preview checkbox početno je isključen kada živi WebView nije učitan.
  Pokretanje, isključivanje i izmjena uređaja održavaju UI stanje usklađenim
  s aktivnim preglednikom.
- Izvorni kod i ZIP nisu zaključani licencom u Windows Portable i Setup.
  Stari tekst o plaćenim paketima je uklonjen.
- Source Viewer postaje kompaktniji pri maloj širini; akcijske kontrole
  ostaju dostupne.
- ZIP izvoz prvo se stvara kao privremena datoteka na istom datotečnom
  sustavu, a zatim se premješta bez prepisivanja postojećeg cilja.
  U slučaju neuspjeha privremeni se ZIP uklanja.

## Ograničenja i preostali posao

Potvrđena kompilacija i WPF CI-geometrija ne dokazuju apsolutnu
pixel-perfect 1:1 jednakost sa svim izvornim referentnim slikama. Za to
su potrebni ujednačena rezolucija, DPI, sistemski font, precizni izvori
i pregled svih pikselnih razlika. Također su otvoreni fizički Windows
DPI 100/125/150/200 %, sve modalne putanje, sve kombinacije
dugačkih tekstova i lokalizacije te potpuni instrumentirani Android
testovi kamera, lokacije, datoteka, licenci i potpisivanja.

**Windows Builder Portable i Setup ostaju trajno bez aktivacije.**
