namespace PartTracker.Models
{
    public class Part
    {
        public int Id { get; set; }
        public required string Code { get; set; }  // 5-digit code
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Metadata { get; set; }  // JSON for future expansion
        public DateTime ImportDate { get; set; }

        public override string ToString() => $"{Code} - {Name}";
    }
}