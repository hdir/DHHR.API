# DHHR.API Dokumentasjon
Dokumentasjon for hvordan en skal benytte API-løsningen for å avlevere meldinger. Eksempelkode i C# vil også være mulig å laste ned på et senere tidspunkt.



## Innhold

- Om tjenesten
- Systemskisse
- Overordnet flyt
- API definisjon
- API metoder
- API returobjekter
- Kodeeksempler



## Om tjenesten
TODO


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
