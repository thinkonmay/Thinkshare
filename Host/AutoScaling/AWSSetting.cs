namespace AutoScaling
{
    public class AWSSetting
    {
        public string Keyname { get; set; }
        public string InstanceType {get;set;}
        public string AMI{get;set;}
        public string SSHkeyPath {get;set;}
        public string CredentialPath{get;set;}
        public string ConfigPath{get;set;}
        public string ClusterManagerVersion {get;set;}
        public string ClusterUIVersion {get;set;}
    }
}
