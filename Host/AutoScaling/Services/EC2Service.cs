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
                KeyName = "test",
                
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
                }
            });

            var result = new EC2Instance
            {
                InstanceName = response.Reservation.Instances.First().KeyName,

                InstanceID = response.Reservation.Instances.First().InstanceId,

                PrivateIP = response.Reservation.Instances.First().PrivateIpAddress,
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
            var instance = await LaunchInstances();
            var result = new ClusterInstance
            {
                instance = instance,
                TurnUser = (new Random()).Next().ToString(),
                TurnPassword = (new Random()).Next().ToString(),
            };
            try
            {
                var success = await AccessEC2Instance(result.instance, new List<string>
                {
                    SetupCoturnScript(
                        result.TurnUser,
                        result.TurnPassword)
                });
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;

        }


        public async Task<bool> AccessEC2Instance (EC2Instance ec2Instance, List<string> commands)
        {

            var pk = new PrivateKeyFile("C:/Users/huyho/Desktop/pcc/private/personal-cloud-computing/Host/AutoScaling/key.txt");
            var keyFiles = new[] { pk };

            var methods = new List<AuthenticationMethod>();
            methods.Add(new PrivateKeyAuthenticationMethod("ubuntu", keyFiles));

            var con = new ConnectionInfo(ec2Instance.IPAdress, 22, "ubuntu", methods.ToArray());

            var _client = new SshClient(con);
            _client.Connect();

            if(_client.IsConnected == false)
            {
                return false;
            }

            foreach (var command in commands)
            {
                _client.RunCommand(command);
            }

            return true;
        }


        public string SetupCoturnScript(string user, string password)
        {
            var script =
            $"sudo apt-get -y update\n" +
            $"sudo apt-get install coturn\n" +
            $"echo \"TURNSERVER_ENABLED = 1\" >> sudo vi /etc/default/coturn\n" +
            $"turnserver -a -o -v -n -u {user}:{password} -p 3478 -r someRealm --no-dtls --no-tls";
            return script;
        }
    }
}