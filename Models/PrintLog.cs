namespace PartTracker.Models
{
    public class PrintLog
    {
        public int Id { get; set; }
        public required string PartCode { get; set; }
        public int UserId { get; set; }
        public DateTime PrintedAt { get; set; }
        public string Status { get; set; } = "Success";  // "Success" or "Failed"
        public string? ErrorMessage { get; set; }
    }
}