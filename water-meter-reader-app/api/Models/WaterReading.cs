namespace Api.Models
{
    public class WaterReading
    {
        public int Id { get; set; }
        public int Reading { get; set; }
        public DateTime Date { get; set; }
        public int UnitId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}