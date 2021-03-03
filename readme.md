# DHHR API Dokumentasjon
Dokumentasjon for hvordan en skal benytte API-løsningen for å avlevere meldinger. Eksempelkode i C# vil også være mulig å laste ned på et senere tidspunkt.


## Innhold

- [Om tjenesten](#om-tjenesten)
- [Skisser/arkitektur](#systemskisse)
- [Overordnet flyt](#overordnet-flyt)
- [API definisjon](#api-definisjon)
- [Autentisering](#autentisering)
- [API metoder](#api-metoder)
- [API returobjekter](#api-returobjekter)
- [Miljøer](#miljøer)
- [Referanser](#referanser)

## Om tjenesten
HTTP-basert API for avlevering av meldinger. I første faste for IPLOS og HST hendelser.


## Skisser/arkitektur


### Systemskisse
![Systemskisse](/docs/images/konsept-skisse.png)

### Bruk av API (utvikling, qa & test)
![Systemskisse](/docs/images/flyt03.png)



### Bruk av API (prod)
![Systemskisse](/docs/images/flyt01.png)


### Felleskomponenter (alle miljøer)
![Systemskisse](/docs/images/flyt02.png)





## Overordnet flyt

Overordnet beskrives for hvordan en skal avlevere meldinger via API grensesnitt.

1. EPJ genererer og formaterer riktig JSON objekt som skal sendes til API’et.
2. JSON objektet krypteres og signeres (CMS/PKCS #7)
(Mottakers sertifikat for kryptering og avsenders sertifikat for signering – må stemme overens med oppføringer i Adresseregisteret)
3. EJP gjør nødvending autentisering med HelseID.
4. EPJ avleverer JSON objektet til angitt endepunkt for API’et.
5. API vil returnere følgende statuser:
	1. HTTP 200 – Ok (Meldingen er mottatt)
	2. HTTP 400 - Bad Request (Ugyldig meldingsformat)
	3. HTTP 500 - Server Error (Annen teknisk feil)
6.	Alle status-varianter vil også ha en data-struktur i body.

## API definisjon
Header-meldingen definerer avsender, mottaker, meldingstype, meldingsversjon og meldingsid. I selve payload så ligger meldingen kryptert og signert ved CMS/PKCS#7.


### Header-melding


	{   
		"messageHeader": {
			"fromHerId": 123, // fra-HerId - long  
			"toHerId": 1234, // til-HerId - long
			"messageType": "IPLOS", // meldingstype - string
			"messageVersion": "65.0.0", // meldingsversjon - string
			"messageId": "91b970e-ae6c-4316-99d2-1f034dd5c74f" // meldingsid - guid
		},
		"payload": "bnRSZXNwb25zZT48cmVzcG9uc2V" // byte-array
	}



### IPLOS melding

Dataformat for IPLOS melding


	Meldingstype: IPLOS
	Meldingsversjon: 56.0.0


Swagger-fil kan lastes ned herfra:

[https://app-messagein-dev.azurewebsites.net/swagger/index.html](https://app-messagein-dev.azurewebsites.net/swagger/index.html)


XSD for IPLOS kan lastes ner herfra:

[message-spesifications/iplos-56.0.0/XPR(ipl).xsd](message-spesifications/iplos-56.0.0/XPR(ipl).xsd)


### HST melding
Kommer så snart denne er tilgjenglig



## Autentisering

API tjenesten benytter HelseID for tilgangssikring (machine to machine). Dette realiseres ved hjelp av OAuth 2.0. Alle forespørsler til API-et blir dermed kontrollert mot HelseID.

EPJ må integrere sin tjeneste med HelseID for å få utstedt token med riktig claims. Utstedt token (JWT) må deretter benyttes i alle forespørsler til API-et.



Les mer om HelseID og teknisk dokumentasjon her:

[https://www.nhn.no/helseid/](https://www.nhn.no/helseid/)


![auth](/docs/images/auth.png)


## API metoder

Avlevering av meldinger gjøres med HTTP POST kall mot følgende endepunkt:

    /message/process

![Process message](/docs/images/process-message.png)



### Kryptering/signering

Innholdet i payload skal krypteres/signeres med CMS/PKCS#7.

For avsender (EPJ) av meldinger så vil dette bety følgende:

- En må hente mottakers krypteringssertifikat fra Adresseregisteret (public-del).
- Avsenders (EPJ) signeringssertifikat må være installert på enhet (privat-del.
- Signeringssertifikatet må være registrert i Adresseregisteret på avsenders HerId.
- Payload krypteres med krypteringssertifikatet og signeres med signeringssertifikatet.



Referanser:

- [https://tools.ietf.org/html/rfc5652](https://tools.ietf.org/html/rfc5652 "Cryptographic Message Syntax (CMS)")
- [https://en.wikipedia.org/wiki/Cryptographic_Message_Syntax](https://en.wikipedia.org/wiki/Cryptographic_Message_Syntax "Cryptographic Message Syntax")
- [SignedCms - Microsoft .NET](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.pkcs.signedcms.-ctor?view=dotnet-plat-ext-5.0#System_Security_Cryptography_Pkcs_SignedCms__ctor "SignedCms - Microsoft .NET")
- [PKCS#7 Java](http://www.docjar.com/docs/api/sun/security/pkcs/PKCS7.html "PKCS#7 Java")

Se også eksempelkode for implementasjon.



## API returobjekter

Følgende datamodeller returneres på de ulike HTTP statuskodene:



### HTTP 200 OK

	{   
		"messageId": "57211091-50cd-4ad7-993d-83fe2ff5dd3f", // Meldingsid på avlevert melding
		"refId": "7fdd8edd-1946-4266-bed8-9edc02f94eb3" // Referanseid for avlevering
	}



### HTTP 400

	{   
		"validationCode": 1234, // Valideringsfeil
		"validationMessage": "Feil signeringssertifikat", // Valideringsfeil
		"refId": "7fdd8edd-1946-4266-bed8-9edc02f94eb3" // Referanseid for avlevering
	}


### HTTP 500
	{   
		"errorCode": 1002, // Feilkode
		"errorMessage": "Teknisk feil på løsning. Prøv igjen senere.", // Feilmelding
		"refId": "7fdd8edd-1946-4266-bed8-9edc02f94eb3" // Referanseid for avlevering
	}


## Miljøer
Ulike miljøer og variabler pr miljø.


### API avlevering

- Test/Dev: https://app-messagein-dev.azurewebsites.net/api
- QA: n/a
- Prod: n/a

### Adresseregisteret

- Test/Dev: register-web.test.nhn.no
- QA: register.nhn.no
- Prod: register.nhn.no

### HerId
- Test/Dev: 8139520
- QA: n/a
- Prod: n/a



## Referanser


- [Registrering av IPLOS-data i kommunen](https://www.helsedirektoratet.no/veiledere/registrering-av-iplos-data-i-kommunen)
- [Rapportere data til KPR](https://www.helsedirektoratet.no/tema/statistikk-registre-og-rapporter/helsedata-og-helseregistre/kommunalt-pasient-og-brukerregister-kpr/rapportere-data-til-kpr)
- [Registrere omsorgsdata (IPLOS)](https://www.helsedirektoratet.no/tema/statistikk-registre-og-rapporter/helsedata-og-helseregistre/kommunalt-pasient-og-brukerregister-kpr/registrere-omsorgsdata)

