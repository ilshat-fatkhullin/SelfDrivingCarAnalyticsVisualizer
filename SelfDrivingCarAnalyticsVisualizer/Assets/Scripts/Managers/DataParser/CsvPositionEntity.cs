using CsvHelper.Configuration.Attributes;

public class CsvPositionEntity
{
    [Name("time"), Index(3)]
    public float Timestamp { get; set; }

    [Name("Points:0"), Index(17)]
    public float X { get; set; }

    [Name("Points:1"), Index(18)]
    public float Y { get; set; }

    [Name("Points:2"), Index(19)]
    public float Z { get; set; }
}