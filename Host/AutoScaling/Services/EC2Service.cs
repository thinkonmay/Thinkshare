using System;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Collections.Generic;
using System.Linq;
using SharedHost.Models.AWS;
using Renci.SshNet;
using AutoScaling.Interfaces;

namespace AutoScaling.Services
{
    public class EC2Service : IEC2Service
    {
        private readonly AmazonEC2Client _ec2Client;



        public EC2Service()
        {
            _ec2Client = new AmazonEC2Client();
        }




        public async Task<EC2Instance> LaunchInstances()
        {
            string keyName = "coturn";
            var response = await _ec2Client.RunInstancesAsync(new RunInstancesRequest
            {
                BlockDeviceMappings = new List<BlockDeviceMapping> {
                    new BlockDeviceMapping {
                        DeviceName = "/dev/sdh",
                        Ebs = new EbsBlockDevice { VolumeSize = 10 }
                    }
                },

                ImageId = "ami-055d15d9cfddf7bd3",
                InstanceType = "t3.micro",
                KeyName = keyName,
                
                MaxCount = 1,
                MinCount = 1,
                TagSpecifications = new List<TagSpecification> {
                    new TagSpecification {
                        ResourceType = "instance",
                        Tags = new List<Tag> {
                            new Tag {
                                Key = "Purpose",
                                Value = "test"
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

                Key = keyName
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
                System.Threading.Thread.Sleep(100);
            }

            return result;
        }




        public async Task<bool> EC2TerminateInstances(string ID)
        {
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
                System.Threading.Thread.Sleep(100);
            }
        }

        public async Task<ClusterInstance> SetupCoturnService()
        {
            var result = new ClusterInstance
            {
                TurnUser = (new Random()).Next().ToString(),
                TurnPassword = (new Random()).Next().ToString(),
            };
            result.instance = await LaunchInstances();


            // wait for coturn server to boot up until setup coturn script
            System.Threading.Thread.Sleep(30*1000);
            var script = SetupCoturnScript(result.TurnUser,result.TurnPassword);
            await AccessEC2Instance(result.instance,script);
            return result;
        }

        public async Task<List<string>?> AccessEC2Instance (EC2Instance ec2Instance, List<string> commands)
        {

            var pk = new PrivateKeyFile("/home/huyhoang/.ssh/coturn.pem");
            var keyFiles = new[] { pk };

            var methods = new List<AuthenticationMethod>();
            methods.Add(new PrivateKeyAuthenticationMethod("ubuntu", keyFiles));

            var con = new ConnectionInfo(ec2Instance.IPAdress, 22, "ubuntu", methods.ToArray());

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

        public List<string> SetupCoturnScript(string user, string password)
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

        public List<string> SetupRedisScript(string password)
        {
            var script = new List<string>
            {
            };

            return script;
        }
    }
}