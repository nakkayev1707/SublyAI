namespace SublyAI.Models;

public class TranscriptionResult
{
    public string Text { get; set; } = string.Empty;
    public List<TranscriptionSegment> Segments { get; set; } = new();
}

public class TranscriptionSegment
{
    public double Start { get; set; }
    public double End { get; set; }
    public string Text { get; set; } = string.Empty;
}

