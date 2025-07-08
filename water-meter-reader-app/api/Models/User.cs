namespace Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PropertyAddress { get; set; }
        public ICollection<WaterReading> WaterReadings { get; set; }
    }
}
