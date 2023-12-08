namespace MMLib.PostmanCollectionDiff.Comparer;
public sealed record BodyDiff : IDiff
{
    public DiffValueChunk Mode { get; private set; } = DiffValueChunk.NoDiff;

    public DiffChunk Raw { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk Urlencoded { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk Formdata { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk File { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk Options { get; private set; } = DiffChunk.NoDiff;

    public DiffValueChunk Disabled { get; private set; } = DiffValueChunk.NoDiff;

    public bool HasDifferences { get; private set; }

    internal void AddMode(DiffValueChunk mode)
    {
        Mode = mode;
        if (mode.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddRaw(DiffChunk raw)
    {
        Raw = raw;
        if (raw.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddUrlencoded(DiffChunk urlencoded)
    {
        Urlencoded = urlencoded;
        if (urlencoded.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddFormdata(DiffChunk formdata)
    {
        Formdata = formdata;
        if (formdata.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddFile(DiffChunk file)
    {
        File = file;
        if (file.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddOptions(DiffChunk options)
    {
        Options = options;
        if (options.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddDisabled(DiffValueChunk disabled)
    {
        Disabled = disabled;
        if (disabled.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    public static BodyDiff NoDiff { get; } = new();
}
