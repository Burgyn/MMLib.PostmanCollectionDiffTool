using DiffPlex.DiffBuilder.Model;

namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed record ItemDiff : IDiff
{
    public ChangeType ChangeType { get; set; }

    public DiffValueChunk Id { get; private set; } = DiffValueChunk.NoDiff;

    public bool HasDifferences => ChangeType != ChangeType.Unchanged;

    public required string Name { get; set; }

    public required string FullName { get; set; }

    public DiffChunk Description { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk Auth { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk ProtocolProfileBehavior { get; private set; } = DiffChunk.NoDiff;

    public VariablesDiff Variables { get; } = new VariablesDiff();

    public RequestDiff Request { get; private set; } = RequestDiff.NoDiff;

    public DiffChunk Response { get; private set; } = DiffChunk.NoDiff;

    public EventsDiff Events { get; private set; } = new EventsDiff();

    public bool IsFolder { get; internal set; } = false;

    internal void AddDescription(DiffChunk description)
    {
        Description = description;
        if (!HasDifferences && description.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }

    internal void AddAuth(DiffChunk auth)
    {
        Auth = auth;
        if (!HasDifferences && auth.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }

    internal void AddProtocolProfileBehavior(DiffChunk protoProfileBehavior)
    {
        ProtocolProfileBehavior = protoProfileBehavior;
        if (!HasDifferences && protoProfileBehavior.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }

    internal void AddVariables(IEnumerable<VariableDiff> variables)
    {
        Variables.AddVariables(variables);
        if (!HasDifferences && Variables.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }

    internal void AddId(DiffValueChunk id)
    {
        Id = id;
        if (!HasDifferences && id.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }

    internal void AddResponse(DiffChunk response)
    {
        Response = response;
        if (!HasDifferences && response.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }

    internal void AddRequest(RequestDiff request)
    {
        Request = request;
        if (!HasDifferences && request.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }

    internal void AddEvents(IEnumerable<EventDiff> events)
    {
        Events.AddEvent(events);
        if (!HasDifferences && Events.HasDifferences)
        {
            ChangeType = ChangeType.Modified;
        }
    }
}
