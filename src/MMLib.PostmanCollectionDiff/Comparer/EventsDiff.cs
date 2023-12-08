using System.Collections;

namespace MMLib.PostmanCollectionDiff.Comparer;
public sealed record EventsDiff : IDiff, IEnumerable<EventDiff>
{
    public bool HasDifferences { get; private set; }

    public IEnumerable<EventDiff> Events { get; private set; } = new List<EventDiff>();

    public void AddEvent(IEnumerable<EventDiff> events)
    {
        Events = events;
        if (events.Any(v => v.HasDifferences))
        {
            HasDifferences = true;
        }
    }

    public IEnumerator<EventDiff> GetEnumerator() => Events.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Events.GetEnumerator();
}
