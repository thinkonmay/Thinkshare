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
        private SshClient _client;
        private readonly ILog _log;
        private readonly ClusterConfig _config;
        private readonly InstanceSetting _setting;
        private bool Started;

        public PortProxy(IClusterInfor infor,
                         IOptions<ClusterConfig> config,
                         IOptions<InstanceSetting> setting,
                         ILog log,
                         ILocalStateStore cache)
        {
            _log = log;
            _config = config.Value;
            _setting = setting.Value;
            _cache = cache;
            _infor = infor;
            Started = false;
        }

        public async Task Start()
        {
            if(Started) { return; }
            await SetupSSHClient();
            await SetupPortForward();
            Started = true;
        }


        private async Task SetupPortForward()
        {
            try
            {
                for (int i = _setting.PortMinValue; i < _setting.PortMaxValue-1; i++)
                {
                    Forward(i);
                }
            }
            catch (Exception ex)
            {
                _log.Information($"Fail to portforward {ex.Message} , {ex.StackTrace}");
            }
        }

        public async Task SetupSSHClient()
        {
                var cluster = await _infor.Infor();
                if(cluster.SelfHost){return;}
                _log.Information($"Attempting to establish ssh connection with instance");
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
                _log.Information($"Sucessfully establish ssh connection with instance");
            }
            catch (Exception ex)
            {
                _log.Information($"Attempting failed with error {ex.Message} {ex.StackTrace}");
                Thread.Sleep(10000);
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
                _log.Information($"Successfully portforward on port {Port}");
            }
            catch (Exception ex)
            {
                _log.Information($"got exception {ex.Message} while port forward");
            }
        }
    }
}