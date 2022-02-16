using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Amazon.EC2;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;
using Amazon;
using SharedHost.Models.AWS;

namespace AutoScaling.Models
{
    public class AWSregion
    {

        public static AmazonEC2Client GetRegionClient(AWSSetting setting, string region)
        {
            RegionEndpoint AWSregion = null;
            switch (region)
            {
                case Region.US_West:
                    AWSregion = RegionEndpoint.USWest1;
                    break;
                case Region.US_East:
                    AWSregion = RegionEndpoint.USEast1;
                    break;
                case Region.Canada:
                    AWSregion = RegionEndpoint.CACentral1;
                    break;
                case Region.Singapore:
                    AWSregion = RegionEndpoint.APSoutheast1;
                    break;
                case Region.India:
                    AWSregion = RegionEndpoint.APSouth1;
                    break;
                case Region.South_Korea:
                    AWSregion = RegionEndpoint.APNortheast2;
                    break;
                case Region.Australia:
                    AWSregion = RegionEndpoint.APSoutheast2;
                    break;
                case Region.Tokyo:
                    AWSregion = RegionEndpoint.APNortheast1;
                    break;
            }

            if(AWSregion == null)
                throw new Exception("Invalid region");

            var _cred = new CredentialProfileStoreChain(setting.CredentialPath);
            var success = _cred.TryGetAWSCredentials("default", out AWSCredentials awsCredentials);

            if(!success)
                throw new Exception("Fail to get aws credential");


            return new AmazonEC2Client(awsCredentials,AWSregion);
        }
    }
}
