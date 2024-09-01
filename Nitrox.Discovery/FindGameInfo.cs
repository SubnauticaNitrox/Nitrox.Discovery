namespace Nitrox.Discovery;

public sealed record FindGameInfo
{
    public string GameName { get; init; } = "";

    public string ExeName { get; init; } = "";

    public int ExeSearchDepth { get; set; }
}