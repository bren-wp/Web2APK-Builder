<div align="center">

<img src="assets/readme-banner.svg" alt="Brendigo Web2APK Builder" width="100%"/>

# Brendigo Web2APK Builder

### Vaša web-stranica. Vaša Android aplikacija.

**Nativni Windows Studio za stvaranje Android APK/AAB aplikacija s vlastitim identitetom, dozvolama i potpisivanjem.**

<img src="assets/brand-icon.svg" alt="Originalna Brendigo ikona" width="90"/>

[**Preuzmi Windows Studio**](https://github.com/bren-wp/Web2APK-Builder/releases/latest) · [**Službena stranica: apk.com.hr**](https://apk.com.hr) · **Podrška: info@apk.com.hr**

Razvoj i autorstvo: **Brendigo** · [brendigo.com](https://brendigo.com)

</div>

---

## Od weba do Android aplikacije

Pretvorite HTTPS web-stranicu ili lokalni HTML projekt u Android aplikaciju. Uređujte izgled u nativnom Windows sučelju, pregledajte ponašanje, konfigurirajte dozvole i izradite APK/AAB. Vaši projekti i potpisni ključevi ostaju na vašem računalu, a web-sadržaj se otvara izravno s njegove odabrane adrese.

| U koracima | Što dobivate |
|---|---|
| **01 · Projekt** | Naziv, package ID, HTTPS URL ili vlastiti lokalni HTML |
| **02 · Brending** | Zadana Brendigo ikona, početni splash, boje i vlastiti PNG |
| **03 · WebView** | Sigurnosne postavke preglednika i vanjske poveznice |
| **04 · Funkcionalnosti** | AAB, datoteke, preuzimanja, Firebase Push, AdMob i osvježavanje prema paketu |
| **05 · Dozvole** | Kamera, mikrofon, lokacija, pristup datotekama i privatnost |
| **06 · Izrada** | Debug APK, nepotpisani APK/AAB ili produkcijski potpis |

### Stvarni Windows zasloni

Snimke niže nastaju **automatskim renderiranjem stvarnog nativnog WPF sučelja** tijekom GitHub Actions provjera. Nisu umjetno generirane ilustracije.

| Početna ploča | Novi projekt |
|:--:|:--:|
| ![Stvarna početna ploča Brendigo Studija](docs/screenshots/native/dashboard.png) | ![Stvarni WPF obrazac za novi projekt](docs/screenshots/native/wizard-1-basics.png) |
| **Brending i splash** | **Pregled Android aplikacije** |
| ![Stvarni WPF zaslon brendinga](docs/screenshots/native/wizard-2-appearance.png) | ![Stvarni WPF zaslon pregleda](docs/screenshots/native/preview.png) |
| **Dozvole i privatnost** | **APK/AAB izrada** |
| ![Stvarni WPF zaslon dozvola](docs/screenshots/native/wizard-5-permissions.png) | ![Stvarni WPF zaslon za izradu](docs/screenshots/native/wizard-6-build.png) |

**Responzivni Windows raspored:** [1024 px – čarobnjak](docs/screenshots/native/wizard-compact-1024.png) · [900 px – čarobnjak](docs/screenshots/native/wizard-small-900.png) · [1024 px – pregled](docs/screenshots/native/preview-compact-1024.png) · [900 px – pregled](docs/screenshots/native/preview-small-900.png).

## Vaš brending odmah pri prvom projektu

Svaka nova aplikacija po zadanim postavkama dobiva originalnu **Brendigo launcher ikonu** i **Brendigo splash** sa službenom oznakom `apk.com.hr`. U paketu **Demo** to ostaje zaključano. U paketima **Osnovno**, **Profesionalac** i **Developer** možete prilagoditi ikonicu, splash, trajanje i boje.

Android WebView koristi širinu zaslona, viewport i sigurne rubove; konačni izgled udaljene web-stranice ovisi i o responzivnom HTML/CSS-u samog izvora.

## Doživotni licencni paketi

| Funkcija | Demo · 0 € | Osnovno · 49 € | Profesionalac · 69 € | Developer · vlasnik |
|---|:--:|:--:|:--:|:--:|
| Neograničeno vrijeme korištenja | ✓ | ✓ | ✓ | ✓ |
| APK | ✓ | ✓ | ✓ | ✓ |
| AAB | — | ✓ | ✓ | ✓ |
| Vlastita ikona i splash | — | ✓ | ✓ | ✓ |
| Vlastiti brending | — | ✓ | ✓ | ✓ |
| Povlačenje za osvježavanje | — | — | ✓ | ✓ |
| Firebase Push | — | — | ✓ | ✓ |
| AdMob i napredne dozvole | — | — | — | ✓ |
| Broj različitih Android aplikacija | 1 | 1 | 1 | **Neograničeno** |

Licenca je potpisani **BL2** token vezan uz jedan Android package ID ili uz neograničena vlasnička prava. U **Postavke → Licence** možete vidjeti aktivni paket, **maskirani licencni ključ**, dodati novi ili **zamijeniti odabrani ključ**. Nema obvezne online aktivacije.

Zaseban **Brendigo License Studio** za Windows omogućuje vlasniku generiranje proizvoljnog broja doživotnih licenci (Demo, Osnovno, Profesionalac i Developer) uz privatni šifrirani RSA ključ. Privatni ključ **nije** ugrađen u javni program. [Upute za License Studio](docs/LICENSE-STUDIO-HR.md).

*Nijedan paket ne dodaje Builderove vlastite oglase u generiranu aplikaciju. Ako sama web-stranica prikazuje oglase, Builder ih ne uklanja.*

## Jedan Windows Studio za cijeli tijek rada

Windows aplikacija može sama preuzeti **Eclipse Temurin JDK 17, Gradle 9.6.1 i Android SDK 36** iz službenih izvora u lokalni korisnički profil, uz potvrdu uvjeta i provjeru SHA-256. Ne morate zasebno instalirati Android Studio. **Za prvo preuzimanje potrebni su internet, slobodan prostor i prihvaćanje Googleovih SDK uvjeta.** Priprema novog alatnog lanca još zahtijeva provjeru na čistoj Windows instalaciji.

Izaberite oblik izvoza prema svrsi:
- **Debug APK:** potpisan Android razvojnim ključem, za lokalno testiranje.
- **Nepotpisani APK:** release APK bez produkcijskog potpisa.
- **APK s vlastitim potpisom:** odaberite trajni keystore ili generirajte novi lokalni ključ.
- **AAB:** s vlastitim keystoreom ili nepotpisani AAB za naknadno potpisivanje.

**Debug i nepotpisani paketi nisu spremni za javnu distribuciju na Google Playu.** Sačuvajte ključ za buduća ažuriranja.

## Android Mobile

Android klijent služi za pregled i uređivanje konfiguracija, prijenos projekata preko lokalnog JSON-a, pregled maskiranih licenci, zamjenu licenci i čitanje uputa. APK/AAB proizvodi Windows aplikacija. Mobilni klijent koristi vlastito responzivno sučelje za telefon i tablet te ne zahtijeva korisnički račun.

## Privatnost, sigurnost i objava

- Projektne datoteke pohranjuju se lokalno; nema ugrađene telemetrije.
- Osjetljive Android dozvole zahtijevaju korisnikov pristanak.
- HTTPS je zadana postavka, WebView ograničava pristup lokalnim datotekama, a izvor aplikacije validira se.
- Android keystore i zaporke nisu u repozitoriju; zaporke se ne spremaju u projekt.
- Službene ovisnosti i Google SDK-ovi preuzimaju se samo uz odobrenje. AdMob, Firebase i odabrani web-sadržaj mogu samostalno obrađivati mrežne podatke.
- Napredne Google Play značajke ovise o pravilima, korisničkoj privoli, stvarnim vjerodajnicama i dodatnim uređajskim testovima; izrada AAB-a **ne jamči odobrenje** aplikacije.

Pročitajte [sigurnosna pravila](SECURITY.md), [pregled sigurnosti i dead codea](docs/SECURITY-DEADCODE-AUDIT-v0.5.0.md) te [detaljne korisničke upute](docs/KORISNICKE-UPUTE-HR.md).

## Preuzimanja

Najnoviji [GitHub Release](https://github.com/bren-wp/Web2APK-Builder/releases/latest) sadrži Windows **portable.exe** i **setup.exe** za glavni Builder i License Studio, kao i **Android APK/AAB** klijenta. Windows i Android paketi mogu biti nepotpisani/testno potpisani, što je izričito navedeno uz izdanje.

---

<div align="center">
**Brendigo Web2APK Builder** · [apk.com.hr](https://apk.com.hr) · **info@apk.com.hr**

Razvoj: **Brendigo** · [brendigo.com](https://brendigo.com)
</div>
