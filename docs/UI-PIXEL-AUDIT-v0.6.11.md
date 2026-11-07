# Brendigo Web2APK Builder — vizualni i sadržajni audit v0.6.11

**Datum:** 7. studenoga 2026.  
**Autor i brend:** Brendigo — https://brendigo.com

## Stvarni Windows zasloni

Nova kontrola pokriva Splash (790 × 490), devet glavnih Windows zaslona
(Welcome, Dashboard, Projects, Preview, Tools, Devices, Settings,
Documentation i About) te svih šest koraka čarobnjaka.

Glavni zasloni automatski se snimaju i provjeravaju na:

| Prikaz | Logička veličina |
|---|---|
| Desktop | 1440 × 900 |
| Međukorak (široki sidebar) | 1024 × 740 |
| Kompaktni prikaz | 900 × 650 |
| Minimalni podržani prozor | 840 × 570 |

Čarobnjak se dodatno snima pri 1024, 900 i 840 px. Postojeće
rasterizacije pri 125 %, 150 % i 200 % nisu fizički per-monitor
Windows DPI test.

**Snimke su stvarni renderi WPF programa iz GitHub Actions postupka,
bez generiranja dizajnerskih slika.** Raniji automatizirani pixel-diff
uspoređuje svaki piksel s prethodnim stvarnim izdanjem. Geometrijski
testovi provjeravaju horizontalni overflow kontrola, širine kartica,
margine i kompaktni navigacijski izbornik. Preview i Source Viewer
dodatno imaju provjere izmjene stanja i minimalne širine.

## Ispravci vidljivog sučelja

- Iconografija tri značajke na Welcome zamijenjena je dosljednim
  jednobojnim Segoe MDL2 simbolima.
- Windows naslovna traka prikazuje sustavne ikone; ikona za povećanje
  i tooltip mijenjaju se pri povratku iz maksimiziranog stanja.
- Fokus tipkovnice na TextBox kontrolama ima kontrastni obrub.
- Postavke i dijalog oporavka više ne spominju Windows licencna mjesta.
- U About uklonjen je zastarjeli rezervni broj verzije.

## Marketinški README i kontrola izvora

README je ponovno napisan za korisnike, s jasnom ponudom i preuzimanjima.
Promotivne slike referenciraju postojeće lokalne brand SVG resurse.
Galerije sadrže isključivo stvarne PNG snimke iz
docs/screenshots/native/. Nisu dodavani vanjski slikovni servisi.
Automatizirani test provjerava postojanje slikovnih datoteka,
minimalan broj lokalnih vizuala i odsutnost razvojnih placeholdera.

Sadržaj razlikuje Windows Builder bez licenci od zasebnog Android
Mobile BL2 licenciranja i navodi uvjete za potpisivanje APK/AAB
paketa bez obećavanja Google Play odobrenja.

## Preostale provjere

**Usporedba stvarnih izdanja nije dokaz potpune 1:1 podudarnosti s
izvornim dizajnerskim referencama.** Potrebno je uskladiti sve
originalne grafičke reference, fontove, rezolucije, DPI,
fokus/hover/disabled stanja te provjeriti stvarne Windows monitore
na 100/125/150/200 % skali. CI ne zamjenjuje ručno testiranje
svake opcije ni neovisni sigurnosni audit.

**Windows Builder Portable i Setup ne zahtijevaju licence, registraciju
ni aktivacijske ključeve.**
