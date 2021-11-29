using NUnit.Framework;
using Microsoft.AspNetCore.Hosting;
using SharedHost.Models.Cluster;
using Microsoft.AspNetCore.TestHost;
using SharedHost.Models.Device;
using NUnit.Framework;
using System.Net.Http;
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

namespace WebsocketTesting;

public class Tests
{
    private TestServer _server;
    private HttpClient _client;
    private WebSocketClient _ws;
    private IConfiguration _configuration;
    private SystemConfig _config;
    private string _token;

    [Test]
    public async Task Setup()
    {
        _configuration = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.json", false, false)
                .Build();
        // Arrange
        _server = new TestServer(new WebHostBuilder()
            .UseConfiguration(_configuration)
            .UseStartup<Startup>());
        

        var config = _server.Services.GetService<IOptions<SystemConfig>>();
        
        _ws = _server.CreateWebSocketClient();
        var uri = new Uri(_server.BaseAddress, "ws");

        var Ws = await _ws.ConnectAsync(new Uri("http://localhost/Hub/User"), CancellationToken.None);
        

        _config = _configuration.GetSection("SystemConfig").Get<SystemConfig>();
    }
}