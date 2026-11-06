# Brendigo License Studio — vlasnički postupak

**Nijedan privatni potpisni ključ nije u repozitoriju, Windows Builderu ili Android klijentu.** U obje aplikacije ugrađen je samo javni RSA-3072 ključ. Licence imaju vlastiti slučajni identifikator serije i jedinstveni brojač unutar serije, SHA-256/RSA potpis i platformsku namjenu.

## Paketi i izdavanje

| Paket | Cijena | Ograničenje |
|---|---:|---|
| Demo | 0 € | jedan package ID, APK, zadani izgled |
| Osnovno | 49 € | jedan package ID, APK/AAB, vlastiti brending i splash |
| Profesionalac | 69 € doživotno | do tri unaprijed potpisana package ID-ja; AAB, AdMob, napredne dozvole, Firebase, osvježavanje, izvorni kod |
| Agencija | 39 €/mj. ili 390 €/god. (prijedlog) | neograničeno klijentskih aplikacija, sve opcije, prodaja izrađenih aplikacija tijekom licence; nije prodaja izdavateljskih ključeva |
| **Developer · vlasnik** | **interno** | **neograničene Android aplikacije, sve opcije, obje platforme** |

Za nove licence **Demo, Osnovno i Profesionalac više nije potrebno upisivati Android package ID**. License Studio potpisuje automatski opseg `@`; Windows Builder automatski rezervira prvo slobodno mjesto pri izvozu izvornog projekta ili izradi APK/AAB-a. Demo i Osnovno imaju jedno mjesto, Profesionalac tri. Ponovna izrada istog package ID-ja ne troši novo mjesto. Promjena package ID-ja smatra se novom aplikacijom, a brisanje projekta ne vraća mjesto. Agencija i Developer koriste `*` i nemaju ograničenje. Stare licence s potpisanim package ID-jevima i dalje rade neizmijenjeno. Osim vremenske Agencije, licence nemaju istek.

**Nikada nemojte objaviti Developer licencu ili privatni potpisni ključ na javnom GitHubu.** Sam BL2 token može se kopirati, pa offline licenciranje ne može potpuno spriječiti dijeljenje tokena. Za stroga ograničenja instalacija i pouzdanu sinkronizaciju mjesta između računala potreban je aktivacijski servis. Windows evidencija automatskih licenci šifrirana je korisničkim DPAPI ključem, ali lokalni offline sustav ne može spriječiti brisanje ili vraćanje sigurnosne kopije evidencije. Android klijent zasebno evidentira konfiguracije i ne provjerava Windows mjesta.

## Korištenje izdavatelja

1. Otvorite **Brendigo License Studio** na pouzdanom Windows računalu.
2. Odaberite vlastitu datoteku **brendigo-license-issuer.encrypted.pem** koju ste dobili **odvojeno od javnog repozitorija**, pa unesite njezinu zaporku.
3. Odaberite broj licenci, trajanje (0 = trajno) i namjenu (Windows/Android/oba).
4. Odaberite novu CSV datoteku. Postupak radi protočno: redak po redak, nikada ne sprema cijelu seriju u memoriju.
5. Primatelju pošaljite samo token koji počinje s BL2. **Nemojte dijeliti PEM niti zaporku.**

## Kapacitet

Izdavanje velikih serija kao što je 100 milijuna licenci moguće je programski, ali nije trenutačno: RSA-3072 potpis za svaku licencu troši vrijeme i serija može zahtijevati desetke GB prostora. Program provjerava raspoloživi disk prije izvoza i omogućuje prekid postupka. *Ne postoji unaprijed generirana lista od 100 milijuna ključeva u kodu.*

## Sigurnost i ograničenja

- Vlasnički PEM je zaštićen lozinkom; otvoren je u memoriji samo tijekom izdavanja.
- Prilikom učitavanja provjerava se da privatni ključ odgovara javnom ključu u obje aplikacije; proizvoljno kreiran potpisni ključ neće moći izdavati valjane licence.
- Token nije tajna. Aktivacija je offline i jedan valjan token može se prenijeti na drugo računalo: **offline provjera bez servera ne može nametnuti ograničenje jednog uređaja, opozvati pojedinu licencu ili pouzdano spriječiti vraćanje sistemskog sata.** Za takve zahtjeve potreban je server za aktivaciju.
- Sigurnosno kopirajte vlasnički PEM i zaporku na dva odvojena sigurna mjesta. Gubitak vlasničkog ključa onemogućuje izdavanje novih licenci kompatibilnih s već distribuiranim aplikacijama.
- Nemojte učitavati privatni PEM ili CSV na GitHub Releases. U izdanju objavljujte samo izvršne programe.
- Windows glavni program i Android Mobile provjeravaju potpis i istek **pri svakom pokretanju** i lokalno spremaju token.

## Tehnički format

BL2.[base64url(JSON)].[base64url(RSA-SHA256 signature)] bez razmaka. Payload sadrži v=2, s (128-bitni serijski broj), a (platforma), t (paket), p (točan package ID, popis ID-jeva, `@` automatski ili `*` neograničeno), iat i exp=0. Potpis je RSA-3072 PKCS#1 v1.5 uz SHA-256. Zajednički format podržavaju .NET 10 i Android API 24+. Privatni PEM izvozi se kao šifrirani PKCS#8.

Javni ključ SHA-256 identifikator: 14b4a104359724ffeed91bbb08ccc99588afc7f676018f8cc5a4cd0788142424.

## Profesionalac, Agencija i vremensko izdavanje
Novi Profesionalac koristi potpisani opseg `@` i rezervira do tri različita package ID-ja pri upotrebi na lokalnom Windows računalu. Već izdani Profesionalac tokeni s jednim, dva ili tri unaprijed potpisana ID-ja ostaju kompatibilni. Potpisani opseg nije moguće proizvoljno mijenjati bez novog privatnog potpisa. Agencija i Developer koriste opseg `*`; Agencija ima odabir **1 mjesec (39 €)** ili **12 mjeseci (390 €)**. Datum isteka ulazi u RSA-potpisani token i CSV. Nakon isteka nema novih izvoza. Plaćanje pretplate i pravo izdavanja novih licenci nisu automatizirani.
