# Brendigo Web2APK Builder — izdanja

## v0.1.0 — Windows Studio

- Originalni tamni Brendigo UI za Windows, navigacija, dashboard i čarobnjak kroz šest koraka.
- Lokalno spremanje Android projekata.
- Podrška za HTTPS URL i lokalni HTML folder.
- Izvoz Android Gradle projekta, Android SDK 36+, Gradle 9.6+.
- Gradle generiranje APK/AAB paketa kada je lokalni alatni lanac instaliran.
- Opcionalno lokalno potpisivanje Android paketa keystoreom; lozinke nisu spremljene.
- Sigurniji WebView i osnovne validacije.
- Automatizirana provjera i dvije Windows datoteke po izdanju.

Windows datoteke: Brendigo-Web2APK-portable.exe i Brendigo-Web2APK-Setup.exe.

Ograničenja v0.1.0: Windows datoteke nisu digitalno potpisane; nezavisne AdMob/Firebase integracije nisu izrađene; nije potvrđeno 1:1 poklapanje svih 16 zaslona s konceptima. Za APK/AAB potreban je JDK/Android SDK/Gradle. Android release APK je nepotpisan bez vlastitog keystorea.
