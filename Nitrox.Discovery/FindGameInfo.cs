namespace Nitrox.Discovery;

public sealed record FindGameInfo
{
    public string Name { get; init; } = "";

    public string ExeName { get; init; } = "";

    public int ExeSearchDepth { get; set; }
}
