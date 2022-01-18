namespace SharedHost.Models.Cluster
{
    public class ClusterCredential
    {
        public int ID {get;set;}
        public int OwnerID {get;set;}
        public string ClusterName {get;set;}

        public override int GetHashCode()
        {
            return $"{this.ID}{this.OwnerID}{this.ClusterName}".GetHashCode();
        }

        public override bool Equals(object obj) 
        { 
            return Equals(obj as ClusterCredential); 
        }

        public bool Equals(ClusterCredential y )
        {
            return ( ( this.ID == y.ID ) &&
                   ( this.OwnerID == y.OwnerID) &&
                   ( this.ClusterName == y.ClusterName ) );
        }
    }
}