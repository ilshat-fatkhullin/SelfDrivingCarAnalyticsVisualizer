using CsvHelper.Configuration.Attributes;

public class CsvFrameEntity
{
    [Name("X"), Index(3)]
    public float X { get; set; }

    [Name("Y"), Index(4)]
    public float Y { get; set; }

    [Name("Z"), Index(5)]
    public float Z { get; set; }

    [Name("timestamp"), Index(11)]
    public float Timestamp { get; set; }
}
