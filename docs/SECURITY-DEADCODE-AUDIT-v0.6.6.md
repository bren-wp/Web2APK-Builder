# Brendigo Web2APK Builder — sigurnosne i kvalitativne bilješke v0.6.6

Datum: 6. studenoga 2026. Autor: Brendigo.

## Opseg ove iteracije

Ručni pregled ključnih dijelova izvornog koda: `ProjectStore.cs`,
`AndroidProjectGenerator.cs`, `MainWindow.xaml.cs`,
`MainWindow.Actions.cs` te automatizirani testovi u `Program.cs`.
Provedena je provjera regresije generatora i pohrane kroz GitHub Actions Windows
runner te kompilacija generiranih Android projekata. Ovo je **ciljani pregled**;
nije provedeno neovisno penetracijsko testiranje ni cjelokupni audit svih datoteka.

## Nalazi i mjere

1. **Oštećenje lokalne baze pri pokretanju — ispravljeno.**
   Prijašnji `MainWindow` poziv `_store.List()` mogao je baciti
   `InvalidDataException` prije otvaranja GUI-ja. Sada se iznimka obrađuje
   i otvaraju Postavke s putom za oporavak.
2. **Ručna obnova sigurnosne kopije — implementirana.**
   `ProjectStore.RestoreBackup` validira `.bak` prije radnji, koristi
   isto međuprocesno zaključavanje kao spremanje i atomsku zamjenu u istoj mapi.
   Original se prije zamjene arhivira u posebnu `recovery` datoteku.
   Odbijanje oštećenog `.bak` ne smije mijenjati glavni JSON; test je dodan.
3. **Nenamjerno pakiranje privatnih ključeva u APK — ublaženo.**
   Pored postojećeg isključivanja skrivenih i razvojnih mapa, lokalni kopirni
   postupak sada preskače `.pem`, `.key`, `.p12`, `.pfx`, `.jks`,
   `.keystore`, `.license`, `credentials.json`, `service-account.json`,
   `google-services.json` te `firebase-adminsdk*`. Dodane su regresijske
   provjere sadržaja stvarno generirane Android assets mape.

## Preostali rizici i granice zaštite

- Filtar nije DLP sustav: tajne sadržane u `.js`, `.html`, `.json`
  neprepoznatog naziva ili drugim dopuštenim datotekama moraju se ručno pregledati.
- DPAPI offline licenciranje štiti lokalni zapis od običnog mijenjanja,
  ali ne sprječava pouzdano brisanje, vraćanje stare kopije ili iskorištavanje
  istog tokena na više uređaja. Za globalno praćenje potreban je poslužitelj.
- Uvezeni `projects.json` može sadržavati korisničke URL-ove i lokalne putanje.
  Arhivirana `recovery` kopija nije šifrirana; korisnik je treba čuvati u
  privatnoj, pristupno ograničenoj mapi.
- Automatske WPF snimke i 900 px testovi nisu potvrda stvarne podudarnosti
  s referencama na svim DPI, fontovima, Windows temama i fizičkim uređajima.
- Nisu obuhvaćeni puna analiza svih ovisnosti i CVE-a, sve interakcije na
  fizičkim Android uređajima, vanjsko potpisivanje certifikata, niti produkcijsko
  izdavanje AAB-a s trajnim upload ključem.

## Preporučena daljnja provjera

U CI uvesti automatski SBOM i skeniranje poznatih ovisnosti,
izolirane testove certifikata i putanja datoteka, stvarni test oporavka u
Windows GUI-ju te mjerenje odstupanja svih referentnih zaslona.
Ne uključivati vlasnički RSA privatni ključ u repozitorij ni artefakte.
