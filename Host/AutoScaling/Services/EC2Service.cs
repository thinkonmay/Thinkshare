using System;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Collections.Generic;

namespace AutoScaling.Services
{
    public class EC2Service
    {
        private readonly AmazonEC2Client _ec2Client;
        public EC2Service ()
        {
            _ec2Client = new AmazonEC2Client();


        }

        public async Task<List<string>> LaunchInstances(RunInstancesRequest requestLaunch)
        {
            var instanceIds = new List<string>();

            RunInstancesResponse responseLaunch =
                await _ec2Client.RunInstancesAsync(new RunInstancesRequest{
                    
                });

            Console.WriteLine("\nNew instances have been created.");
            foreach (Instance item in responseLaunch.Reservation.Instances)
            {
                instanceIds.Add(item.InstanceId);
                Console.WriteLine($"  New instance: {item.InstanceId}");
            }

            return instanceIds;
        }
    }
}