namespace MMLib.PostmanCollectionDiff.Comparer;
public sealed record EventDiff : IDiff
{
    public string Listen { get; private set; } = string.Empty;

    public ScriptDiff Script { get; private set; } = ScriptDiff.NoDiff;

    public bool HasDifferences { get; private set; }

    public static EventDiff Create(string listen, ScriptDiff script)
        => new()
        {
            Listen = listen,
            Script = script,
            HasDifferences = script.HasDifferences
        };

    public static EventDiff CreateDeleted(string listen, ScriptDiff script)
        => new()
        {
            Listen = listen,
            Script = script,
            HasDifferences = true
        };

    public static EventDiff CreateAdded(string listen, ScriptDiff script)
        => new()
        {
            Listen = listen,
            Script = script,
            HasDifferences = true
        };
}
