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


        public async Task<EC2KeyPair> CreateKeyPair()
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


        public async Task<EC2Instance> LaunchInstances()
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

                keyPair = keyPair,
            };


            while (true)
            {
                var infor = await _ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest { InstanceIds = new List<string> { response.Reservation.Instances.First().InstanceId } });
                if (infor.Reservations.First().Instances.First().State.Name == InstanceStateName.Running &&
                    infor.Reservations.First().Instances.First().PublicIpAddress != null)
                {
                    result.IPAdress = infor.Reservations.First().Instances.First().PublicIpAddress;
                    break;
                }
                System.Threading.Thread.Sleep(1000);
            }

            return result;
        }




        public async Task<bool> EC2TerminateInstances(string ID)
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

            var prestaterequets  = await _ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest { InstanceIds = new List<string> { ID } });

            if(prestaterequets.Reservations.First().Instances.First().State.Name != InstanceStateName.Running)
            {
                return false;
            }

            var response = await _ec2Client.TerminateInstancesAsync(new TerminateInstancesRequest
            {
                InstanceIds = new List<string> {
                    ID
                }
            });


            while(true)
            {
                var infor = await _ec2Client.DescribeInstancesAsync(new DescribeInstancesRequest { InstanceIds = new List<string> { ID } });

                if (infor.Reservations.First().Instances.First().State.Name == InstanceStateName.Terminated )
                {
                    return true;
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        public async Task<ClusterInstance> SetupManagedCluster()
        {
            var result = new ClusterInstance((await LaunchInstances()));

            result.TurnUser = (new Random()).Next().ToString();
            result.TurnPassword = (new Random()).Next().ToString();


            // wait for coturn server to boot up until setup coturn script
            System.Threading.Thread.Sleep(30*1000);

            var coturn = SetupCoturnScript(result.TurnUser,result.TurnPassword);
            await AccessEC2Instance(result,coturn);

            var cluster = SetupClusterScript();
            await AccessEC2Instance(result,cluster);
            return result;
        }


        public async Task<ClusterInstance> SetupCoturnService()
        {
            var result = new ClusterInstance((await LaunchInstances()));

            result.TurnUser = (new Random()).Next().ToString();
            result.TurnPassword = (new Random()).Next().ToString();


            // wait for coturn server to boot up until setup coturn script
            System.Threading.Thread.Sleep(30*1000);

            var coturn = SetupCoturnScript(result.TurnUser,result.TurnPassword);
            await AccessEC2Instance(result,coturn);
            return result;
        }

        public async Task<List<string>?> AccessEC2Instance (EC2Instance instance, List<string> commands)
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










        List<string> SetupCoturnScript(string user, string password)
        {
            var script = new List<string>
            {
                "sudo apt-get -y update" ,
                "sudo apt-get -y install coturn" ,
                "echo \"TURNSERVER_ENABLED = 1\" >> sudo vi /etc/default/coturn" ,
                $"turnserver -a -o -v -n -u {user}:{password} -p 3478 -r someRealm --no-dtls --no-tls"
            };

            return script;
        }

        List<string> SetupClusterScript()
        {
            var script = new List<string>
            {
                "curl https://www.thinkmay.net/script/setup.sh > setup.sh && sudo sh setup.sh && rm setup.sh"
            };
            return script;
        }
    }
}