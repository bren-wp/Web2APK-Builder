# Brendigo Web2APK Builder — UI pixel audit v0.6.9

**Datum:** 7. studenoga 2026. **Projekt:** `bren-wp/Web2APK-Builder`.

## Što se stvarno uspoređuje

WPF CI pokreće stvarni `MainWindow` i snima `RenderTargetBitmap`, bez
generiranja dizajnerskih ilustracija ili zamjenskih tekstura. Za postojeće
screenshotove svaki piksel uspoređuje se s prethodno spremljenim stvarnim
WPF PNG-om. RGB razlika iznad 54 označava promjenu, a promjena udjela
piksela iznad 55 % zaustavlja izdanje. Uz to ostaje neovisna uzorkovana
kontrola s pragom 40 %.

JSON izvještaji u CI artefaktu `brendigo-wpf-visual-audit`:
- `native-full-pixel-audit.json`: broj uspoređenih i promijenjenih piksela
  po zaslonu, rezolucija i udio promjena.
- `wpf-visual-regression.json`: uzorkovana kontrola i popis konceptnih
  referenci čija apsolutna 1:1 podudarnost nije potvrđena.

## Inventar provjerenih zaslona

- Splash, Welcome, Dashboard, Projects, Preview, Tools, Devices,
  Settings, Documentation i About na standardnom desktop viewportu.
- Čarobnjak: Osnovno, Izgled, WebView, Funkcije, Dozvole i Izrada.
- Glavne stranice i svih šest koraka na 840 × 570 (minimalni podržani
  prozor), svi koraci dodatno na 900 × 650 i 1024 × 740.
- WPF čarobnjak pri rasterizaciji 125 %, 150 % i 200 % (ne stvarna
  promjena sistemskog per-monitor DPI-ja).
- License Studio pri standardnoj, 900 × 650 i 760 × 605 veličini prozora.

Automatske geometrijske provjere upozoravaju na horizontalno
odsijecanje interaktivnih elemenata, skriveni natpis kompaktne
navigacije, preskupe unutarnje razmake ili ostavljeni prazni stupac.
Obična tipkovnička navigacija dobiva vidljivi fokus.

## Što još nije moguće proglasiti potvrđenim

**Usporedba dva izdanja ne dokazuje 1:1 podudarnost s originalnim
dizajnerskim referencama.** `docs/screenshots/*.png` uključuje
konceptne slike, dok su `docs/screenshots/native/*.png` snimke
izvršenog WPF programa. Za objektivnu 1:1 potvrdu svaku relevantnu
konceptnu referencu treba poravnati na identičan viewport, font,
veličinu teksta, stanje kontrola i stvarnu metriku DPI, zatim
pregledati sve odstupajuće piksele. Bez toga se ne tvrdi da je takva
podudarnost postignuta.

U ovoj iteraciji nisu obavljeni fizički per-monitor DPI 100/125/150/200 %
testovi, svi modalni dijalozi, svi jezici, Windows accessibility
auditor ni potpuni manualni screenshot diff izvornog dizajna.
`License Studio` ostaje zaseban program za Android BL2 licence;
**Windows Builder Portable i Setup ne traže licencu**.

## Daljnji cilj

Nastaviti ekran po ekran s konkretnim referencama, proučiti izmjerena
odstupanja u PNG i JSON izvještajima, ispraviti svako odstupanje bez
poremećaja funkcionalnosti te dodatno testirati kombinacije hover/focus/
disabled, ekstremno dugačke nazive i više stvarnih monitora.
