using System;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Collections.Generic;
using System.Linq;
using SharedHost.Models.AWS;
using Renci.SshNet;
using AutoScaling.Interfaces;
using SharedHost.Logging;
using Microsoft.Extensions.Options;
using Amazon.Runtime;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using AutoScaling.Models;


namespace AutoScaling.Services
{
    public class EC2Service : IEC2Service
    {
        private readonly AWSSetting _aws;

        private readonly CredentialProfileStoreChain _cred;

        private readonly string defaultProfile;

        private readonly ILog _log;

        public EC2Service(IOptions<AWSSetting> aws,
                          ILog log)
        {
            defaultProfile = "default";
            _aws = aws.Value;
            _log = log;
        }


        private async Task<EC2KeyPair> CreateKeyPair(string region)
        {
            AmazonEC2Client _ec2Client = AWSregion.GetRegionClient(_aws,region);

            CreateKeyPairResponse response =
                await _ec2Client.CreateKeyPairAsync(new CreateKeyPairRequest{
                    KeyName = (new Random()).Next().ToString()
                });            

            return new EC2KeyPair {
                PrivateKey = response.KeyPair.KeyMaterial,
                Name = response.KeyPair.KeyName
            };
        }




        private async Task<EC2Instance> LaunchInstances(string region)
        {
            AmazonEC2Client _ec2Client = AWSregion.GetRegionClient(_aws,region);

            var keyPair = await CreateKeyPair(region);


            var response = await _ec2Client.RunInstancesAsync(new RunInstancesRequest
            {
                BlockDeviceMappings = new List<BlockDeviceMapping> {
                    new BlockDeviceMapping {
                        DeviceName = "/dev/sdh",
                        Ebs = new EbsBlockDevice { VolumeSize = 10 }
                    }
                },

                ImageId = _aws.regions.Where(x => x.region == region).First().AMI,
                InstanceType = _aws.InstanceType,
                KeyName = keyPair.Name,
                
                MaxCount = 1,
                MinCount = 1,
                TagSpecifications = new List<TagSpecification> {
                    new TagSpecification {
                        ResourceType = "instance",
                        Tags = new List<Tag> {
                            new Tag {
                                Key = "Launcher",
                                Value = "AutoScaling"
                            }
                        }
                    }
                },
            });

            var result = new EC2Instance
            {
                InstanceName = response.Reservation.Instances.First().KeyName,

                InstanceID = response.Reservation.Instances.First().InstanceId,

                PrivateIP = response.Reservation.Instances.First().PrivateIpAddress,

                Start = DateTime.Now,

                keyPair = keyPair,
            };

            int waitingTime = 0;
            while (true)
            {
                var infor = await _ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest { InstanceIds = new List<string> { response.Reservation.Instances.First().InstanceId } });
                if (infor.Reservations.First().Instances.First().State.Name == InstanceStateName.Running &&
                    infor.Reservations.First().Instances.First().PublicIpAddress != null)
                {
                    result.IPAdress = infor.Reservations.First().Instances.First().PublicIpAddress;
                    break;
                }
                _log.Information("waiting for ec2 instance to get desired state: "+ waitingTime);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                waitingTime++;
            }
            _log.Information("EC2 instance create finished after : "+ waitingTime);

            return result;
        }




        public async Task<bool> TerminateInstance(ClusterInstance instance)
        {
            try
            {
                AmazonEC2Client _ec2Client = AWSregion.GetRegionClient(_aws,instance.Region);

                var prestaterequets  = await _ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest { InstanceIds = new List<string> { instance.InstanceID } });
                if(prestaterequets.Reservations.First().Instances.First().State.Name != InstanceStateName.Running)
                {
                    return false;
                }

                await _ec2Client.DeleteKeyPairAsync(new DeleteKeyPairRequest( instance.keyPair.Name ));            
                await _ec2Client.TerminateInstancesAsync(new TerminateInstancesRequest
                {
                    InstanceIds = new List<string> {
                        instance.InstanceID
                    }
                });


                while(true)
                {
                    var infor = await _ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest { InstanceIds = new List<string> { instance.InstanceID } });

                    if (infor.Reservations.First().Instances.First().State.Name == InstanceStateName.Terminated )
                    {
                        return true;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch(Exception ex)
            {
                _log.Error("Fail to terminate instance",ex);
                return false;
            }
        }

        public async Task<ClusterInstance> SetupManagedCluster(string region)
        {
            var script = new List<string>();
            var instance = await LaunchInstances(region);
            var result = new ClusterInstance
            {
                Start = DateTime.Now,
                IPAdress = instance.IPAdress,
                InstanceID = instance.InstanceID,
                InstanceName = instance.InstanceName,
                PrivateIP = instance.PrivateIP,
                keyPair = instance.keyPair,
                Region = region,
                portForwards = new List<PortForward>(),
            };


            result.TurnUser = (new Random()).Next().ToString();
            result.TurnPassword = (new Random()).Next().ToString();


            // wait for coturn server to boot up until setup coturn script
            var coturn = SetupTurnScript(result.TurnUser,result.TurnPassword);
            await AccessEC2Instance(instance,coturn);

            var cluster = SetupClusterScript(_aws.ClusterManagerVersion,_aws.ClusterUIVersion);
            await AccessEC2Instance(instance,cluster);
            return result;
        }


        public async Task<ClusterInstance> SetupCoturnService(string region)
        {
            var instance = await LaunchInstances(region);
            var result = new ClusterInstance
            {
                Start = DateTime.Now,
                IPAdress = instance.IPAdress,
                InstanceID = instance.InstanceID,
                InstanceName = instance.InstanceName,
                PrivateIP = instance.PrivateIP,
                keyPair = instance.keyPair,
                Region = region,
                portForwards = new List<PortForward>(),
            };

            result.TurnUser = (new Random()).Next().ToString();
            result.TurnPassword = (new Random()).Next().ToString();


            // wait for coturn server to boot up until setup coturn script
            var coturn = SetupTurnScript(result.TurnUser,result.TurnPassword);
            await AccessEC2Instance(instance,coturn);
            return result;
        }

        private async Task AccessEC2Instance (EC2Instance instance, List<string> commands)
        {
            MemoryStream keyStream = new MemoryStream(Encoding.UTF8.GetBytes(instance.keyPair.PrivateKey));
            var keyFiles = new[] { new PrivateKeyFile(keyStream) };
            var methods = new List<AuthenticationMethod>();
            methods.Add(new PrivateKeyAuthenticationMethod("ubuntu", keyFiles));
            var con = new ConnectionInfo(instance.IPAdress, 22, "ubuntu", methods.ToArray());

            SshClient _client = null;
            int attemption = 0;
            while (true)
            {
                _client = new SshClient(con);
                _client.ConnectionInfo.Timeout = TimeSpan.FromMinutes(2);

                if(attemption == 3)
                    throw new Exception("Fail to connect to instance multiple times");

                try
                {
                    _client.Connect();

                    if(!_client.IsConnected)
                        throw new Exception("connection is not setup properly");

                    foreach (var command in commands)
                    {
                        _log.Information("Executing command " +command);
                        Task.Run(() => _client.RunCommand(command));
                    }

                    return;
                }
                catch (Exception exception)
                {
                    _log.Error("Fail to connect to EC2 instance",exception);
                }
                _client.Dispose();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                attemption++;
            }
        }










        List<string> SetupTurnScript(string user, string password)
        {
            var script = new List<string>
            {
                "export TURN_USERNAME="+user + " && " +
                "export TURN_PASSWORD="+password + " && " +

                "sh /home/ubuntu/pion.sh"
            };

            return script;
        }

        List<string> SetupClusterScript(string version, string uiVersion)
        {
            var script = new List<string>
            {
                "export MANAGER_VERSION=" +version + " && " +
                "export UI_VERSION=" + uiVersion + " && " +

                "sh /home/ubuntu/setup.sh",
            };
            return script;
        }
    }
}