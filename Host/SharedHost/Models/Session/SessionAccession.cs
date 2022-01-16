﻿using SharedHost.Models.Device;

namespace SharedHost.Models.Session
{
    public class SessionAccession
    {
        public Module Module { get; set; }

        public int ClientID { get; set; }

        public int WorkerID { get; set; }

        public int ID { get; set; }

        public override int GetHashCode()
        {
            return $"{this.Module}{this.ClientID}{this.WorkerID}{this.ID}".GetHashCode();
        }

        public override bool Equals(object obj) 
        { 
            return Equals(obj as SessionAccession); 
        }

        public bool Equals( SessionAccession y )
        {
            return ( ( this.ID == y.ID ) &&
                     ( this.Module == y.Module ) && 
                     ( this.ClientID == y.ClientID ) &&
                     ( this.WorkerID == y.WorkerID) );
        }
    }
}
