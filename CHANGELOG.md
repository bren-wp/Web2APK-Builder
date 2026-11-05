# Brendigo Web2APK Builder – izdavanja

## v0.2.0 — Native Windows WPF Edition

**Velika nadogradnja:** iz aktivnog desktop projekta uklonjen Electron. Novi Windows Studio implementiran je nativno u Microsoft .NET 10/WPF-u.

- Normalni prozor (1360 × 820, promjenjiv, minimalno 850 × 560), ne otvara se preko cijelog zaslona.
- Tamni Brendigo vizualni stil, prilagodljivi bočni izbornik, kartice, čarobnjak u šest koraka.
- Lokalna baza projekata i uređivanje identiteta, URL-a, lokalnih HTML projekata, SDK-a, tema i WebView konfiguracije.
- Generator Android projekta izdvojen u .NET Core biblioteku s testovima.
- Izrada Android APK/AAB paketa pomoću Android SDK + Gradle 9.6+ i potpisivanje lokalnim keystoreom.
- Provjera Java/Gradle/ADB alata i lista Android uređaja.
- Native Windows x64 portable.exe i setup.exe (Inno Setup), self-contained .NET.
- Nema telemetrije ni korisničkog računa.

**Poznata ograničenja:** stvarni rad APK/AAB ovisi o instaliranom Android SDK-u i alatima; AdMob, Firebase, potpuno upravljanje kamerom, dozvolama, animacijama i custom launcher ikonama još nisu implementirani. Nije verificirano poklapanje 1:1 sa svim 16 referentnih zaslona. Windows paketi nisu kodno potpisani.

## v0.1.0 — Electron prototip

Prethodni Windows prototip sa sučeljem, izvozom Android projekata, Gradle postupkom i inicijalnim referentnim screenshotovima. Zamijenjen nativnom verzijom 0.2.0.
