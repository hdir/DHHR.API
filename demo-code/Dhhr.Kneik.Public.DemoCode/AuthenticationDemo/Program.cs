using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static IdentityModel.OidcConstants;

namespace AuthenticationDemo
{
    /* -- Authentication Demo --
     * This client will submit a test message to Kneik API.
     * The Kneik API requires a valid access token from the HelseId platform to verify the user.
     * Without this access token you will not be able to submit messages to the Kneik API.
     * 
     * The access token should be placed as a bearer token in the header values of requests towards Kneik.
     * 
     * The program will perform the following:
     *   - Requests a new access token from HelseId with the use of a private JWK (JSON Web Key)
     *   - Creates a POST request to Kneik API with test message
     *   
     * NOTES:
     *   - private_jwk.json needs to be replaced with a valid JWK issued by HelseId.
     */
    public class Program
    {
        private static readonly string _kneikBaseUrl = "https://app-messagein-test.azurewebsites.net";
        private static readonly string _helseIdBaseUrl = "https://helseid-sts.test.nhn.no";
        private static readonly string _helseIdClientId = "00a04013-c336-46bd-b72f-4cc3ccf64d0c";
        private static readonly string _pathToPrivateJWK = "private_jwk.json";
        private static readonly string _pathToTestMessage = "test_message.json";

        static async Task Main()
        {
            PrintTitle();

            PrintConfigurations();

            // Request for an access token
            Console.WriteLine("* Requesting for an Access Token from the HelseId platform ...");
            var token = await RequestToken().ConfigureAwait(false);
            Console.WriteLine("* Done requesting for an Access Token.");
            Console.WriteLine("** Access Token:");
            Console.WriteLine(token.AccessToken);
            Console.WriteLine();

            // Submit the test message
            Console.WriteLine("* Submit test message to the Kneik API ...");
            var httpResponseMessage = await SubmitMessage(token.AccessToken).ConfigureAwait(false);
            var httpResponseMessageContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine($"** Request Uri: {httpResponseMessage.RequestMessage.RequestUri} ({httpResponseMessage.RequestMessage.Method})");
            Console.WriteLine("* Submitted test message.");
            Console.WriteLine("** Response: ");
            Console.WriteLine($"*** StatusCode: {httpResponseMessage.StatusCode}");
            if(httpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"*** Content: {httpResponseMessageContent}");
            }
        }

        // Submits a message to the Kneik API
        private static async Task<HttpResponseMessage> SubmitMessage(string accessToken)
        {
            HttpClient client = new();
            client.BaseAddress = new Uri(_kneikBaseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var testMessage = GetTestMessage();
            var stringContent = new StringContent(testMessage, Encoding.UTF8, "application/json");

            return await client.PostAsync($"{_kneikBaseUrl}/messages", stringContent).ConfigureAwait(false);
        }

        // Requests a new access token from the HelseId platform
        private static async Task<IdentityModel.Client.TokenResponse> RequestToken()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(_helseIdBaseUrl).ConfigureAwait(false);

            return await client.RequestTokenAsync(new IdentityModel.Client.TokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _helseIdClientId,
                GrantType = GrantTypes.ClientCredentials,
                ClientAssertion = new ClientAssertion
                {
                    Type = ClientAssertionTypes.JwtBearer,
                    Value = BuildAssertion(disco, _helseIdClientId)
                }
            }).ConfigureAwait(false);
        }

        // Builds client assertion with private JWK.
        // Will be used by HelseId to verify the client and its scopes.
        // By adding custom claims, you should be able to request different scopes in the JWT returned from HelseId.
        private static string BuildAssertion(DiscoveryDocumentResponse disco, string clientId)
        {
            var _claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, _helseIdClientId),
                new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
                new Claim(JwtClaimTypes.Scope, "hdir:kneik/iplos/submit") // Custom claim
            };

            var creds = new JwtSecurityToken(clientId, disco.TokenEndpoint, _claims, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(60), GetCASigningCredentials());
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(creds);
        }

        // Reads the private JWK and creates signing credentials
        private static SigningCredentials GetCASigningCredentials()
        {
            if (File.Exists(_pathToPrivateJWK))
            {
                var keyfromfile = File.ReadAllLines(_pathToPrivateJWK);
                var key = new JsonWebKey(string.Join(string.Empty, keyfromfile));

                if (string.IsNullOrEmpty(key.P))
                {
                    throw new InvalidOperationException($"Private JWK is invalid. Please replace the local file with a valid JWK. Path: {_pathToPrivateJWK}");
                }

                return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            }
            else
            {
                throw new FileNotFoundException($"Could not find file for private JWK. Path: {_pathToPrivateJWK}");
            }
        }

        // Returns the local test message
        private static string GetTestMessage()
        {
            if (File.Exists(_pathToTestMessage))
            {
                return string.Join(string.Empty, File.ReadAllLines(_pathToTestMessage));
            }
            else
            {
                throw new FileNotFoundException($"Could not find file for test message. Path: {_pathToTestMessage}");
            }
        }

        // Prints title
        private static void PrintTitle()
        {
            Console.WriteLine(" _   __           _ _       ___        _   _                _   _           _   _              ______                     ");
            Console.WriteLine("| | / /          (_) |     / _ \\      | | | |              | | (_)         | | (_)             |  _  \\                    ");
            Console.WriteLine("| |/ / _ __   ___ _| | __ / /_\\ \\_   _| |_| |__   ___ _ __ | |_ _  ___ __ _| |_ _  ___  _ __   | | | |___ _ __ ___   ___  ");
            Console.WriteLine("|    \\| '_ \\ / _ \\ | |/ / |  _  | | | | __| '_ \\ / _ \\ '_ \\| __| |/ __/ _` | __| |/ _ \\| '_ \\  | | | / _ \\ '_ ` _ \\ / _ \\ ");
            Console.WriteLine("| |\\  \\ | | |  __/ |   <  | | | | |_| | |_| | | |  __/ | | | |_| | (_| (_| | |_| | (_) | | | | | |/ /  __/ | | | | | (_) |");
            Console.WriteLine("\\_| \\_/_| |_|\\___|_|_|\\_\\ \\_| |_/\\__,_|\\__|_| |_|\\___|_| |_|\\__|_|\\___\\__,_|\\__|_|\\___/|_| |_| |___/ \\___|_| |_| |_|\\___/ ");
            Console.WriteLine();
        }

        // Prints configurations
        private static void PrintConfigurations()
        {
            Console.WriteLine("* Configurations:");
            Console.WriteLine($"** Kneik base URL: {_kneikBaseUrl}");
            Console.WriteLine($"** HelseId base URL: {_helseIdBaseUrl}");
            Console.WriteLine($"** HelseId client id: {_helseIdClientId}");
            Console.WriteLine($"** Path to private JWK: {_pathToPrivateJWK}");
            Console.WriteLine($"** Path to test message: {_pathToTestMessage}");
            Console.WriteLine();
        }
    }
}
