namespace SharedHost.Models.Error
{
    public class ReportedError
    {
        public int SlaveID { get; set; }
        public int Module { get; set; }
        public string ErrorMessage { get; set; }
    }
}