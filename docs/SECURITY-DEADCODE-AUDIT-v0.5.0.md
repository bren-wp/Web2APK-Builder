# Sigurnosni i dead-code pregled — v0.5.0

**Projekt:** Brendigo Web2APK Builder · **Službena podrška:** info@apk.com.hr

## Obuhvat
Pregledani su lokalni .NET WPF sustav i aktivacija, generator Android projekata, prijenos JSON datoteka, gradnja i potpisivanje, izdavanje licenci, Windows/Android klijent, Gradle instalacija, novi integrirani JDK/SDK downloader, te GitHub Actions objava. Provjere u CI-u uključuju .NET testove, Windows kompilaciju, generirane Android buildove (WebView, AdMob, Firebase i refresh), Android Mobile APK/AAB te stvarne WPF snimke.

## Zapažanja i obrada
| Ozbiljnost | Područje | Nalaz / status |
|---|---|---|
| Visoka | License Store | Zamjena licence sada prvo verificira novi token i aktivira ga, pa uklanja stari. Krivi potpis ili publika odbijaju se. |
| Visoka | Demo ograničenja | Obavezna Brendigo ikona i splash provjeravaju se na razini licencnih prava, ne samo UI-ja. |
| Visoka | Windows keytool | Tajna se predaje u varijabli okruženja, ne kao argument CLI-ja; podržan je lokalno instalirani JDK. |
| Srednja | Integrirani SDK | Google command-line ZIP provjerava se SHA-256 zbrojem sa službene stranice. JDK se preuzima prema Adoptium metapodacima i pripadajućem SHA-256. Zbog mreže ostaje rizik kompromitiranih upstream objava ili promjene distribucije. |
| Srednja | Android gradnja | Razlikuju se debug potpis, nepotpisani release i trajni produkcijski potpis; debug APK nije Play proizvod. |
| Srednja | WebView | HTTPS zadano, izolacija pristupa lokalnim datotekama, validacija izvora, runtime dopuštenja za kameru/mikrofon/lokaciju. Sadržaj svake uključene web-stranice ostaje njezina vlastita odgovornost. |
| Srednja | Mobilna privatnost | Projektni JSON nalazi se lokalno; prikaz i zamjena tokena maskirani su u korisničkom sučelju. Token ostaje prenosiv i potpisan, ne šifriran. |
| Niska | Dead code | Uklonjeni nekorišteni generirani Java `PULL` konstantni atribut, stara substitucija i nepotrebni C# importi. |
| Niska | Tekst i UI | O programu više ne prikazuje zastarjelu verziju; developer Brendigo, službeni web apk.com.hr i podrška info@apk.com.hr. |

## Preostali rizici i granice

1. **Offline licenciranje:** ne postoji hardverski vezana aktivacija ni mrežni opoziv. Valjani token može se kopirati na druga računala; to se ne može riješiti samo potpisom.
2. **Potpisivanje Android paketa:** Android Gradle Plugin, adb, Firebase, AdMob i mrežni alati obrađuju podatke ovisno o namjeni i konfiguraciji. Novi alatni lanac mora se ispitati na čistoj Windows instalaciji; njegova instalacija zahtijeva internet i korisnikov pristanak na licence.
3. **SDK paketi:** pri uključivanju vanjskih SDK-ova potreban je ponovljeni audit ovisnosti i točna deklaracija Play Console Data safety.
4. **Responzivnost:** WPF provjerava 1440×900, 1024×740 i 900×650 u CI-u, ali automatizirani screenshot nije dokaz pixel-perfect 1:1 podudarnosti svih zaslona; potrebno je uspoređivanje s referencama i testiranje stvarnog Android uređaja.
5. **Nema potvrđenog neovisnog penetracijskog testa ili potpunog SAST nalaza.** Ovaj dokument bilježi provedeni ručni i automatizirani pregled, ne certifikaciju potpune sigurnosti.

## Preporučene sljedeće sigurnosne provjere
- SAST skeniranje cijelog .NET i Java projekta te skeniranje transitive dependency stabla, uz osvježavanje Google i Microsoft SDK-ova;
- test legalnih i zlonamjernih Android dozvola, uključujući WebRTC i file picker;
- instrumentirani Windows UI test bijelih sistemskih dijaloga, visokog DPI-ja i rada nakon promjene licence;
- test preuzetih JDK/Gradle/SDK arhiva, timeouta, otkazivanja i oporavka nakon prekida mreže;
- zasebno testirati potpisivanje release APK/AAB, debug i nepotpisani build na čistom Windows računalu.
