namespace PartTracker.Models
{
    public class RemovedPart
    {
        public int Id { get; set; }
        public required string PartCode { get; set; }
        public int UserId { get; set; }
        public DateTime RemovedAt { get; set; }
        public string? AdminNotes { get; set; }
        public string? MechanikName { get; set; }
        
        // Part details (joined from Parts table)
        public string? PartName { get; set; }
        public string? PartDescription { get; set; }
    }
}