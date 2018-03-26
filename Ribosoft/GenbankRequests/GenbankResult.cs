namespace Ribosoft.GenbankRequests
{
    public class GenbankResult
    {
        public string AccessionId { get; set; }
        public string Sequence { get; set; }
        public int OpenReadingFrameStart { get; set; }
        public int OpenReadingFrameEnd { get; set; }
    }
}