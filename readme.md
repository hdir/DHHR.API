# DHHR.API Dokumentasjon
Dokumentasjon for hvordan en skal benytte API-løsningen for å avlevere meldinger. Eksempelkode i C# vil også være mulig å laste ned på et senere tidspunkt.



## Innhold

- [Om tjenesten](https://github.com/hdir/DHHR.API#om-tjenesten)
- [Systemskisse](https://github.com/hdir/DHHR.API#systemskisse)
- [Overordnet flyt](https://github.com/hdir/DHHR.API#overordnet-flyt)
- [API definisjon](https://github.com/hdir/DHHR.API#api-definisjon)
- [API metoder](https://github.com/hdir/DHHR.API#api-metoder)
- API metoder
- API returobjekter
- Kodeeksempler



## Om tjenesten
HTTP-basert API for avlevering av meldinger. I første faste for IPLOS og HST hendelser.


## Systemskisse

![Systemskisse](https://github.com/hdir/Dhhr.Api/blob/master/docs/images/konsept-skisse.png)

## Overordnet flyt

Overordnet beskrives for hvordan en skal avlevere meldinger via API grensesnitt.

1. EPJ genererer og formaterer riktig JSON objekt som skal sendes til API’et.
2. JSON objektet krypteres og signeres (CMS/PKCS #7)
(Mottakers sertifikat for kryptering og avsenders sertifikat for signering – må stemme overens med oppføringer i Adresseregisteret)
3. EPJ avleverer JSON objektet til angitt endepunkt for API’et.
4. API vil returnere følgende statuser:
	1. HTTP 200 – Ok (Meldingen er mottatt)
	2. HTTP 400 - Bad Request (Ugyldig meldingsformat)
	3. HTTP 500 - Server Error (Annen teknisk feil)
5.	Alle status-varianter vil også ha en data-struktur i body.

## API definisjon

TODO: Definere meldingsformatet
TODO: 


## API metoder

TODO: Definere API metoder
TODO: Definere kryptering/signering
