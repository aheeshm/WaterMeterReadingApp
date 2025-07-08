namespace Api.Models
{
    public class RateConfig
    {
        public int Id { get; set; }
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}