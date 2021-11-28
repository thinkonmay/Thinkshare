using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using SharedHost.Models.Device;
using NUnit.Framework;
using System.Net.Http;
using Authenticator;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using SharedHost;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using SharedHost.Auth;
using SharedHost.Models.Session;

namespace TokenTesting
{
    [TestFixture]
    public class AuthenticatorTokenTesting
    {
        private TestServer _server;
        private HttpClient _client;
        private IConfiguration _configuration;
        private SystemConfig _config;
        private string _token;
        
        public AuthenticatorTokenTesting()
        {
            _configuration = new ConfigurationBuilder()
                   .AddJsonFile(@"appsettings.json", false, false)
                   .Build();
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseConfiguration(_configuration)
               .UseStartup<Startup>());
            _client = _server.CreateClient();

            _config = _configuration.GetSection("SystemConfig").Get<SystemConfig>();
        }

        [Test]
        [Order(1)]
        public async Task SetupToken()
        {
            Console.WriteLine("Testing login controller");
            var testLogin = new HttpRequestMessage(HttpMethod.Post, "/Account/Login");
            testLogin.Content = new StringContent(
                JsonConvert.SerializeObject(_config.AdminLogin),null,"application/json");


            var response = await _client.SendAsync(testLogin);

            var result = JsonConvert.DeserializeObject<AuthResponse>(await response.Content.ReadAsStringAsync());
            _token = result.Token;

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        [Order(2)]
        public async Task GetTokenValidationResult()
        {
            Console.WriteLine("Testing admin user token with value: "+_token);
            var token = new AuthenticationRequest
            {
                token = _token,
                Validator = "testcase",
            };

            var testLogin = new HttpRequestMessage(HttpMethod.Post, "/Token/Challenge");
            testLogin.Content = new StringContent(
                JsonConvert.SerializeObject(token), null, "application/json");
            var response = await _client.SendAsync(testLogin);

            var result = JsonConvert.DeserializeObject<AuthenticationResponse>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(result.IsAdmin, true);
            Assert.AreEqual(result.IsUser, true);
            Assert.AreEqual(result.IsManager, true);
        }


        [Test]
        [Order(2)]
        public async Task TestSessionToken()
        {
            Console.WriteLine("Setting session token");

            var accession = new SessionAccession
            {
                Module = Module.CORE_MODULE,
                ID = 100,
                WorkerID = 200,
                ClientID = 300,
            };

            var testLogin = new HttpRequestMessage(HttpMethod.Post, "/Token/GrantSession");
            testLogin.Content = new StringContent(
                JsonConvert.SerializeObject(accession), null, "application/json");
            var tokenResponse = await _client.SendAsync(testLogin);

            Assert.AreEqual(tokenResponse.StatusCode , HttpStatusCode.OK);
            var token = await tokenResponse.Content.ReadAsStringAsync();

            Console.WriteLine("Got token generated: "+ token);

            var testToken = new HttpRequestMessage(HttpMethod.Post, "/Token/ChallengeSession?token="+token);
            var resultResponse = await _client.SendAsync(testToken);

            Assert.AreEqual(resultResponse.StatusCode, HttpStatusCode.OK);
            var responseAccession = JsonConvert.DeserializeObject<SessionAccession>(await resultResponse.Content.ReadAsStringAsync());

            Assert.AreEqual(responseAccession.ClientID,accession.ClientID);
            Assert.AreEqual(responseAccession.WorkerID,accession.WorkerID);
            Assert.AreEqual(responseAccession.ID,accession.ID);
            Assert.AreEqual(responseAccession.Module,accession.Module);
        }
    }
}