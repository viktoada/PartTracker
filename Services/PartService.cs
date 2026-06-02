using PartTracker.Models;

namespace PartTracker.Services
{
    public class PartService
    {
        public async Task<Part> SearchPartAsync(string code)
        {
            // TODO: Implement actual database search
            await Task.Delay(300);
            return new Part
            {
                Code = code,
                Name = $"Díl {code}",
                Description = "Ukázkový díl",
                CreatedAt = DateTime.Now
            };
        }
    }
}
