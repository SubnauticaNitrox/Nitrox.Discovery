namespace Nitrox.Discovery;

public sealed record GameInfo
{
    public string Name { get; init; } = "";

    public string ExeName { get; init; } = "";

    public int ExeSearchDepth { get; set; }
}
