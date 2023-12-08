namespace MMLib.PostmanCollectionDiff.Comparer;

public class CollectionDiffResult : IDiff
{
    private readonly List<ItemDiff> _items = new();

    public required string OriginalName { get; set; }

    public required string UpdatedName { get; set; }

    public bool HasDifferences { get; private set; }

    public DiffChunk Info { get; private set; } = DiffChunk.NoDiff;

    public VariablesDiff Variables { get; } = new VariablesDiff();

    public IEnumerable<EventDiff> Events { get; private set; } = Enumerable.Empty<EventDiff>();

    public DiffChunk Auth { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk ProtocolProfileBehavior { get; private set; } = DiffChunk.NoDiff;

    public int UnchangedItemCount { get; private set; }

    public IEnumerable<ItemDiff> Items => _items;

    public void AddInfo(DiffChunk diff)
    {
        Info = diff;
        if (diff.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    protected internal void AddVariables(IEnumerable<VariableDiff> variables)
    {
        Variables.AddVariables(variables);
        if (Variables.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    public void AddEvents(IEnumerable<EventDiff> diff)
    {
        Events = diff;
        if (diff.Any(d => d.HasDifferences))
        {
            HasDifferences = true;
        }
    }

    public void AddAuth(DiffChunk diff)
    {
        Auth = diff;
        if (diff.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    public void AddProtocolProfileBehavior(DiffChunk diff)
    {
        ProtocolProfileBehavior = diff;
        if (diff.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    public void AddItem(ItemDiff item)
    {
        _items.Add(item);
        if (item.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    public void AddUnchangedItem()
    {
        UnchangedItemCount++;
    }
}
