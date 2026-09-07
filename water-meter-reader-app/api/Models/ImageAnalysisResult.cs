namespace Api.Models
{
    public class ImageAnalysisResult
    {
        public bool IsMeterImage { get; set; }
        public int Reading { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
