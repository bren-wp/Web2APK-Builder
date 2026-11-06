# Sigurnosni pregled v0.4.0 — Brendigo Web2APK Builder

## Licence i kontrola paketa

**Developer · vlasnik** ima neograničeno aplikacija i sve opcije. Demo/Osnovno/Profesionalac vezani su uz potpisani Android `package ID`. Ograničenja se provjeravaju u Windows izvoznoj i Android build logici, a ne samo pri prikazu gumba. Mobilni editor provjerava paket prije spremanja/izvoza konfiguracije.

Offline potpis **ne sprječava kopiranje aktivacijskog tokena**, ne može pouzdano zaštititi od reverznog inženjeringa izvršnog programa i ne omogućuje opoziv već izdanog tokena bez dodatnog aktivacijskog servisa.

## Granice provjere

Ručno pregledani: .NET Core biblioteka, Android generator, WPF startup, Windows licenciranje, Android aktivacija, potpisni ključ, spremanje projekata, CI i dijelovi WPF/Android UI-ja. Automatski testovi i CI koriste se za kompilaciju, ali **nisu zamjena za neovisni penetracijski test, kompletni SAST ni testiranje na svim uređajima**.

| Prioritet | Zapažanje | Obrada |
|---|---|---|
| Visok | Skriveni parametar za snimanje omogućavao je izravno učitavanje ekrana | Parametar je ograničen na zaseban UI_CAPTURE CI build; produkcijski startup zahtijeva validnu licencu |
| Visok | Ugrađivanje popisa od 100 milijuna važećih licenci u binarni program omogućilo bi masovno kopiranje | Zamijenjeno offline RSA digitalnim potpisom i privatnim ključem koji **nije** u repozitoriju |
| Visok | Privatni ključ u izdavatelju mogao bi se pogrešno isporučiti korisnicima | Izdanja sadrže samo generator; vlasnički šifrirani PEM isporučuje se odvojeno |
| Srednji | Android licenciranje bez lokalne provjere prije otvaranja drugih stranica | Dodana provjera u onCreate, kontrola navigacije prije aktivacije i potpisna provjera pri svakom pokretanju |
| Srednji | Tajne za APK potpisivanje ne smiju biti u logovima ili projektu | Postojeći Android generator koristi varijable procesa; potrebno dodatno ručno pregledati sve integracije SDK-a |
| Srednji | Offline licenca je prenosiva i ne može se pouzdano opozvati | Jasno dokumentirano; za aktivaciju jednog uređaja potreban je zaseban autorizacijski servis |
| Srednji | Lokalni WebView/HTML može izvršavati korisnički web-kod | HTTPS zadano, file URL pristup ograničen, osjetljive dozvole uz potvrdu; URL i politike valja dodatno testirati |
| Nizak | Neki elementi forme mogu koristiti svijetli Windows sistemski popup | Doraditi ComboBox template i provjeriti kontrast na stvarnim 100%, 125%, 150% DPI snimkama |

## Dead-code pregled

Ukloniti samo nedvojbeno nekorišten kod. Klase za potpisivanje, testne fixture skripte i snimanje stvarnih zaslona označeni su prema namjeni i nisu mrtav kod. Nužno je nastaviti s Roslyn analyzer provjerama i pronaći stvarno nedostupne putanje i nepotrebne import-e. Nije razumno tvrditi da je provedena potpuna automatska analiza cijelog repozitorija samo na temelju kompilacije.

## Preostalo za produkciju

Potpisivanje Windows izvršnih datoteka izdavačkim certifikatom, trajni upload ključ za Android klijent, stvarni Android testovi kamera/mikrofona/FCM/AdMoba, UI 1:1 usporedba svakog zaslona s referencama te procjena ovisnosti i mogućnosti opoziva licenci.
