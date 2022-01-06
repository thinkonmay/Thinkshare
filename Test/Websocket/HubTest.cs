using NUnit.Framework;
using Microsoft.AspNetCore.Hosting;
using SharedHost.Models.Cluster;
using Microsoft.AspNetCore.TestHost;
using SharedHost.Models.Device;
using NUnit.Framework;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using SharedHost;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SystemHub;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using SharedHost.Auth;
using SharedHost.Models.Session;
using System.Threading;
using DbSchema.DbSeeding;
using RestSharp;
using System.Net.WebSockets;
using Authenticator.Services;

namespace WebsocketTesting;

public class Tests
{
    private TestServer _server;
    private HttpClient _client;
    private WebSocketClient _ws;
    private IConfiguration _configuration;
    private SystemConfig _config;
    private string _token;
    private string _clusterToken;
    public Tests()
    {
        _configuration = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.json", false, false)
                .Build();
        // Arrange
        _server = new TestServer(new WebHostBuilder()
            .UseConfiguration(_configuration)
            .UseStartup<Startup>());
        

        _config = _configuration.GetSection("SystemConfig").Get<SystemConfig>();

        var login = new RestClient();
        var reqeust = new RestRequest("https://host.thinkmay.net/Account/Login")
            .AddJsonBody(_config.AdminLogin);
        reqeust.Method = Method.POST;

        var result = login.Execute(reqeust);

        _token = JsonConvert.DeserializeObject<AuthResponse>(result.Content).Token;




    }


    [Test]
    public async Task Setup()
    {
        _ws = _server.CreateWebSocketClient();
        var uri = new Uri(_server.BaseAddress, "ws");

        var Ws = await _ws.ConnectAsync(new Uri("http://localhost/Hub/User?token="+_token), CancellationToken.None);
        

        _config = _configuration.GetSection("SystemConfig").Get<SystemConfig>();

        var bytes = Encoding.UTF8.GetBytes("message");
        var buffer = new ArraySegment<byte>(bytes);
        await Ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

        Assert.AreEqual(Ws.State, WebSocketState.Open);
    }

}