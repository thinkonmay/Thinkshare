using Renci.SshNet;
using System;
using Newtonsoft.Json;
using System.Text;
using RestSharp;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.IO;
using WorkerManager.Interfaces;
using SharedHost.Models.Cluster;
using SharedHost.Models.AWS;
using WorkerManager;
using System.Threading;
using Microsoft.Extensions.Options;
using SharedHost;



namespace WorkerManager.Services
{
    public class PortProxy : IPortProxy
    {
        private readonly IClusterInfor _infor;
        private readonly ILocalStateStore _cache;
        private readonly List<int> _ports;
        private SshClient _client;

        private readonly ClusterConfig _config;

        private bool Started;

        public PortProxy(IClusterInfor infor,
                         IOptions<ClusterConfig> config,
                         ILocalStateStore cache)
        {
            _config = config.Value;
            _cache = cache;
            _infor = infor;
            Started = false;
            _ports = new List<int>();
        }

        public async Task Start()
        {
            if(Started) { return; }
            await SetupSSHClient();
            SetupPortForward();
            Started = true;
        }


        private async Task SetupPortForward()
        {
            while (true)
            {
                if(!(await _infor.IsSelfHost()))
                {
                    try
                    {
                        var portForwards = (await _infor.Infor()).instance.portForwards.Where(x => !x.End.HasValue);
                        foreach (var item in portForwards)
                        {
                            if (!_ports.Contains(item.InstancePort))
                            {
                                Task.Run(() => Forward(item.InstancePort));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Information($"Fail to portforward {ex.Message} , {ex.StackTrace}");
                    }
                }
                Thread.Sleep((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            }
        }

        public async Task SetupSSHClient()
        {
                var cluster = await _infor.Infor();
                if(cluster.SelfHost){return;}
                Serilog.Log.Information($"Attempting to establish ssh connection with instance");
                ClusterInstance instance = cluster.instance;

                MemoryStream keyStream = new MemoryStream(Encoding.UTF8.GetBytes(instance.keyPair.PrivateKey));
                var keyFiles = new[] { new PrivateKeyFile(keyStream) };

                var methods = new List<AuthenticationMethod>();
                methods.Add(new PrivateKeyAuthenticationMethod("ubuntu", keyFiles));

            try
            {
                var con = new ConnectionInfo(instance.IPAdress, 22, "ubuntu", methods.ToArray());
                _client = new SshClient(con);
                _client.Connect();
                Serilog.Log.Information($"Sucessfully establish ssh connection with instance");
            }
            catch (Exception ex)
            {
                Serilog.Log.Information($"Attempting failed with error {ex.Message} {ex.StackTrace}");
                Thread.Sleep(10000);
                _ports.Clear();
                await SetupSSHClient();
            }
        }

        async Task Forward(int Port)
        {
            try
            {
                var agent = new ForwardedPortLocal("localhost",(uint)Port, "localhost", (uint)Port);
                _client.AddForwardedPort(agent);
                agent.Start();

                _ports.Add(Port);
                Serilog.Log.Information($"Successfully portforward on port {Port}");
                while (_ports.Contains(Port)) { Thread.Sleep(1000); }
                agent.Stop();
            }
            catch (Exception ex)
            {
                Serilog.Log.Information($"got exception {ex.Message} while port forward");
            }

            _ports.Remove(Port);
            var res = await (new RestClient()).ExecuteAsync(
                new RestRequest(_config.PortReleaseUrl,Method.GET)
                .AddHeader("Authorization",(await _cache.GetClusterInfor()).ClusterToken)
                .AddQueryParameter("InstancePort",Port.ToString()));
            if(res.StatusCode == HttpStatusCode.OK)
            {
                Serilog.Log.Information($"Processed port release with autoscaling");
            }
            else
            {
                Serilog.Log.Information($"fail to port release with autoscaling");
            }
        }
    }
}