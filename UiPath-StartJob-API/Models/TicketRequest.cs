namespace UiPath_StartJob_API.Models
{
    public class TicketRequest
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Phone { get; set; }
        public string Service { get; set; }
        public string Branch { get; set; }
        public string SuccessFlow { get; set; }
        public string FailureFlow { get; set; }
        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }
    }
}
