using System;
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
using Microsoft.Extensions.Options;
using Amazon.Runtime;
using Amazon;
using Amazon.Runtime.CredentialManagement;


namespace AutoScaling.Services
{
    public class EC2Service : IEC2Service
    {
        private readonly AWSSetting _aws;

        private readonly RegionEndpoint _defaultRegion;

        private readonly CredentialProfileStoreChain _cred;

        private readonly string defaultProfile;

        public EC2Service(IOptions<AWSSetting> aws)
        {
            _aws = aws.Value;

            _defaultRegion = RegionEndpoint.APSoutheast1;

            defaultProfile = "default";

            _cred = new CredentialProfileStoreChain(_aws.CredentialPath);
        }


        private async Task<EC2KeyPair> CreateKeyPair()
        {
            AmazonEC2Client _ec2Client;
            if (_cred.TryGetAWSCredentials(defaultProfile, out AWSCredentials awsCredentials))
            {
                _ec2Client = new AmazonEC2Client(awsCredentials,_defaultRegion);
            }
            else
            {
                return null;
            }

            CreateKeyPairResponse response =
                await _ec2Client.CreateKeyPairAsync(new CreateKeyPairRequest{
                    KeyName = (new Random()).Next().ToString()
                });            


            return new EC2KeyPair {
                PrivateKey = response.KeyPair.KeyMaterial,
                Name = response.KeyPair.KeyName
            };
        }




        private async Task<EC2Instance> LaunchInstances()
        {
            AmazonEC2Client _ec2Client;
            if (_cred.TryGetAWSCredentials(defaultProfile, out AWSCredentials awsCredentials))
            {
                _ec2Client = new AmazonEC2Client(awsCredentials,_defaultRegion);
            }
            else
            {
                return null;
            }

            var keyPair = await CreateKeyPair();


            var response = await _ec2Client.RunInstancesAsync(new RunInstancesRequest
            {
                BlockDeviceMappings = new List<BlockDeviceMapping> {
                    new BlockDeviceMapping {
                        DeviceName = "/dev/sdh",
                        Ebs = new EbsBlockDevice { VolumeSize = 10 }
                    }
                },

                ImageId = _aws.AMI,
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
                Serilog.Log.Information("waiting for ec2 instance to get desired state: "+ waitingTime);
                System.Threading.Thread.Sleep(1000);
                waitingTime++;
            }
            Serilog.Log.Information("EC2 instance create finished after : "+ waitingTime);

            return result;
        }




        public async Task<bool> TerminateInstance(ClusterInstance instance)
        {
            AmazonEC2Client _ec2Client;
            if (_cred.TryGetAWSCredentials(defaultProfile, out AWSCredentials awsCredentials))
            {
                _ec2Client = new AmazonEC2Client(awsCredentials,_defaultRegion);
            }
            else
            {
                return false;
            }

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

        public async Task<ClusterInstance> SetupManagedCluster()
        {
            var instance = await LaunchInstances();
            var result = new ClusterInstance
            {
                IPAdress = instance.IPAdress,
                InstanceID = instance.InstanceID,
                InstanceName = instance.InstanceName,
                PrivateIP = instance.PrivateIP,
                keyPair = instance.keyPair,
                portForwards = new List<PortForward>(),
            };


            result.TurnUser = (new Random()).Next().ToString();
            result.TurnPassword = (new Random()).Next().ToString();


            // wait for coturn server to boot up until setup coturn script
            System.Threading.Thread.Sleep(30*1000);

            var coturn = SetupTurnScript(result.TurnUser,result.TurnPassword,instance.IPAdress);
            await AccessEC2Instance(instance,coturn);

            var cluster = SetupClusterScript("development","2022-01-03");
            await AccessEC2Instance(instance,cluster);
            return result;
        }


        public async Task<ClusterInstance> SetupCoturnService()
        {
            var instance = await LaunchInstances();
            var result = new ClusterInstance
            {
                IPAdress = instance.IPAdress,
                InstanceID = instance.InstanceID,
                InstanceName = instance.InstanceName,
                PrivateIP = instance.PrivateIP,
                keyPair = instance.keyPair,
                portForwards = new List<PortForward>(),
            };

            result.TurnUser = (new Random()).Next().ToString();
            result.TurnPassword = (new Random()).Next().ToString();


            // wait for coturn server to boot up until setup coturn script
            System.Threading.Thread.Sleep(30*1000);

            var coturn = SetupTurnScript(result.TurnUser,result.TurnPassword,instance.IPAdress);
            await AccessEC2Instance(instance,coturn);
            return result;
        }

        private async Task<List<string>?> AccessEC2Instance (EC2Instance instance, List<string> commands)
        {
            MemoryStream keyStream = new MemoryStream(Encoding.UTF8.GetBytes(instance.keyPair.PrivateKey));
            var keyFiles = new[] { new PrivateKeyFile(keyStream) };

            var methods = new List<AuthenticationMethod>();
            methods.Add(new PrivateKeyAuthenticationMethod("ubuntu", keyFiles));

            var con = new ConnectionInfo(instance.IPAdress, 22, "ubuntu", methods.ToArray());

            var _client = new SshClient(con);
            _client.Connect();

            if(_client.IsConnected == false)
            {
                return null;
            }

            var result = new List<string>();
            foreach (var command in commands)
            {
                result.Add(_client.RunCommand(command).Result);
            }

            return result;
        }










        List<string> SetupTurnScript(string user, string password, string IP)
        {
            var script = new List<string>
            {
                $"export DEBIAN_FRONTEND=noninteractive" ,
                $"export TURN_USERNAME=${user}" ,
                $"export TURN_PASSWORD=${password}" ,
                $"export PUBLIC_IP=${IP}" ,

                "curl https://www.thinkmay.net/script/pion.sh > setup.sh && sudo sh setup.sh & disown"
            };

            return script;
        }

        List<string> SetupClusterScript(string version, string uiVersion)
        {
            var script = new List<string>
            {
                $"export MANAGER_VERSION=${version}" ,
                $"export UI_VERSION=${uiVersion}" ,

                "curl https://www.thinkmay.net/script/install.sh > install.sh && sudo sh install.sh" ,
                "curl https://www.thinkmay.net/script/docker-compose.yaml > docker-compose.yaml && sudo docker-compose up -d"
            };
            return script;
        }
    }
}