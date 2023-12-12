using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed class CollectionComparerV21 : ICollectionComparer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CollectionComparerV21()
    {
        _options.Converters.Add(new RawJsonConverterFactory(typeof(RawJson), new RawJsonConverter()));
        _options.Converters.Add(new RawJsonConverterFactory(typeof(Variable), new RawJsonConverter()));
        _options.Converters.Add(new RawJsonConverterFactory(typeof(Information), new RawJsonConverter()));
    }

    public CollectionDiffResult Compare(JsonDocument? original, JsonDocument? updated)
    {
        var originalCollection = Deserialize(original);
        var updatedCollection = Deserialize(updated);
        var result = new CollectionDiffResult()
        {
            OriginalName = originalCollection.Name,
            UpdatedName = updatedCollection.Name
        };

        CompareCollection(originalCollection, updatedCollection, result);

        foreach ((var fullName, var item) in updatedCollection.Items)
        {
            if (originalCollection.Items.TryGetValue(fullName, out var originalItem))
            {
                var diff = item.Compare(originalItem);
                if (diff.HasDifferences)
                {
                    result.AddItem(diff);
                }
                else
                {
                    result.AddUnchangedItem();
                }
            }
            else
            {
                result.AddItem(item.ToDiff(ChangeType.Inserted));
            }
        }

        foreach ((var fullName, var item) in originalCollection.Items)
        {
            if (!updatedCollection.Items.ContainsKey(fullName))
            {
                result.AddItem(item.ToDiff(ChangeType.Deleted));
            }
        }

        return result;
    }

    private static void CompareCollection(
        PostmanCollection originalCollection,
        PostmanCollection updatedCollection,
        CollectionDiffResult result)
    {
        var diff = originalCollection.Info.Compare(updatedCollection.Info);
        if (diff.HasDifferences)
        {
            result.AddInfo(diff);
        }

        result.AddVariables(Compare(originalCollection.Variable, updatedCollection.Variable));
        result.AddEvents(CompareEvents(originalCollection.Event, updatedCollection.Event));
        result.AddAuth(originalCollection.Auth.Compare(updatedCollection.Auth));
        result.AddProtocolProfileBehavior(
            originalCollection.ProtocolProfileBehavior.Compare(updatedCollection.ProtocolProfileBehavior));
    }

    private static List<EventDiff> CompareEvents(ICollection<Event> originalEvents, ICollection<Event> updatedEvent)
    {
        var events = originalEvents.Select(origin =>
        {
            var updated = updatedEvent.FirstOrDefault(e => e.Listen == origin.Listen);
            if (updated == null)
            {
                return EventDiff.CreateDeleted(
                    origin.Listen,
                    ScriptDiff.SingleValue(
                        origin.Id,
                        origin.Script.Name,
                        origin.Script.Type,
                        origin.Script.Exec,
                        ChangeType.Deleted));
            }

            return EventDiff.Create(updated.Listen,
                ScriptDiff.Create(updated.Script.Id, updated.Script.Name, updated.Script.Type,
                origin.Script.Exec, updated.Script.Exec));
        }).ToList();

        foreach (var updated in updatedEvent)
        {
            if (!originalEvents.Any(e => e.Listen == updated.Listen))
            {
                events.Add(EventDiff.CreateAdded(
                    updated.Listen,
                    ScriptDiff.Create(updated.Script.Id, updated.Script.Name, updated.Script.Type,
                        Array.Empty<string>(), updated.Script.Exec)));
            }
        }

        return events;
    }

    private PostmanCollection Deserialize(JsonDocument? document)
    {
        if (document is null)
        {
            return PostmanCollection.Empty;
        }

        var root = document.RootElement;
        var collection = new PostmanCollection
        {
            Info = Deserialize<Information>(root.GetProperty("info")),
            Variable = Deserialize<List<Variable>>(root, "variable"),
            Event = Deserialize<List<Event>>(root, "event"),
            Auth = Deserialize<RawJson>(root, "auth"),
            ProtocolProfileBehavior = Deserialize<RawJson>(root, "protocolProfileBehavior"),
            Name = root.GetProperty("info").GetProperty("name").GetString() ?? string.Empty,
            Items = new Dictionary<string, ItemBase>(StringComparer.OrdinalIgnoreCase)
        };

        DeserializeItem(root.GetProperty("item"), string.Empty, collection.Items);

        return collection;
    }

    private IEnumerable<ItemBase> DeserializeItem(JsonElement element, string path, Dictionary<string, ItemBase> allItems)
    {
        var items = new List<ItemBase>();
        string FullName(string n) => path + "/" + n;

        foreach (var item in element.EnumerateArray())
        {
            if (item.TryGetProperty("request", out var _))
            {
                var request = Deserialize<Item>(item);
                request.FullName = FullName($"({request.Request.Method}) {request.Name}");
                items.Add(request);
                AddItemToDic(allItems, request);
            }
            else
            {
                var folder = Deserialize<Folder>(item);
                folder.FullName = FullName(folder.Name);
                folder.Item = DeserializeItem(item.GetProperty("item"), folder.FullName, allItems);
                items.Add(folder);
                AddItemToDic(allItems, folder);
            }
        }

        return items;
    }

    private static void AddItemToDic(Dictionary<string, ItemBase> allItems, ItemBase item)
    {
        var key = item.FullName;
        int i = 1;

        while (allItems.ContainsKey(key))
        {
            key = $"{item.FullName} ({i})";
            i++;
        }

        allItems.Add(key, item);
    }

    [return: NotNull]
    private T? Deserialize<T>(JsonElement element, string propertyName)
        where T : new()
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            return Deserialize<T>(property);
        }

        return new T();
    }

    [return: NotNull]
    private T Deserialize<T>(JsonElement element)
    {
        var el = element.Deserialize<T>(_options)!;

        if (el is RawJson rawJson)
        {
            rawJson.Raw = element.GetRawText();
        }

        return el;
    }

    private static IEnumerable<VariableDiff> Compare(IEnumerable<Variable> original, IEnumerable<Variable> updated)
    {
        var diff = updated
            .Select(updatedVar =>
            {
                var originalVar = (original ?? Array.Empty<Variable>())
                    .FirstOrDefault(o => updatedVar.Key.Equals(o.Key, StringComparison.OrdinalIgnoreCase));

                return originalVar is not null ?
                    VariableDiff.Create(updatedVar.Key, originalVar.Value, updatedVar.Value) :
                    VariableDiff.NewCreated(updatedVar.Key, updatedVar.Value);
            }).ToList();

        foreach (var originalVar in original ?? Enumerable.Empty<Variable>())
        {
            var updatedVar = updated
                .FirstOrDefault(u => u.Key.Equals(originalVar.Key, StringComparison.OrdinalIgnoreCase));

            if (updatedVar is null)
            {
                diff.Add(VariableDiff.Removed(originalVar.Key, originalVar.Value));
            }
        }

        return diff;
    }

    private record RawJson : IEquatable<RawJson>
    {
        public static RawJson Empty { get; } = new RawJson();

        public string Raw { get; set; } = string.Empty;

        public virtual bool Equals(RawJson? other)
            => other != null && Raw.Equals(other.Raw);

        public override int GetHashCode() => Raw.GetHashCode();

        public virtual DiffChunk Compare(RawJson? other)
        {
            if (other == null)
            {
                return DiffChunk.NoDiff;
            }

            return SideBySideDiffBuilder.Instance.CreateDiff(other.Raw, Raw);
        }
    }

    private record Url
    {
        public string? Raw { get; set; }

        public virtual DiffValueChunk Compare(Url? other)
        {
            if (other == null)
            {
                return DiffValueChunk.NoDiff;
            }

            return new DiffValueChunk(other.Raw, Raw);
        }
    }

    private record Information : RawJson
    {
        public required string Name { get; set; }
        public required string Schema { get; set; }
    }

    private abstract record ItemBase
    {
        public required string Name { get; set; }
        public string FullName { get; set; } = string.Empty;
        public RawJson Description { get; set; } = RawJson.Empty;
        public List<Variable> Variable = new();
        public List<Event> Event { get; set; } = new List<Event>();
        public RawJson Auth { get; set; } = RawJson.Empty;
        public RawJson ProtocolProfileBehavior { get; set; } = RawJson.Empty;
        [JsonIgnore]
        public IEnumerable<ItemBase> Item { get; set; } = Enumerable.Empty<ItemBase>();

        public virtual ItemDiff ToDiff(ChangeType changeType)
        {
            var ret = new ItemDiff
            {
                FullName = FullName,
                Name = Name,
                ChangeType = changeType
            };
            ret.AddDescription(new DiffChunk(Description?.Raw));
            ret.AddAuth(new DiffChunk(Auth?.Raw));
            ret.AddProtocolProfileBehavior(new DiffChunk(ProtocolProfileBehavior?.Raw));

            ret.AddVariables(Variable.Select(v => VariableDiff.Create(v.Key, v.Value)));

            ret.AddEvents(Event.Select(
                e => EventDiff.Create(
                    e.Listen,
                    ScriptDiff.SingleValue(e.Script.Id, e.Script.Name, e.Script.Type, e.Script.Exec, changeType))));

            return ret;
        }

        public virtual ItemDiff Compare(ItemBase other)
        {
            var diff = new ItemDiff() { FullName = FullName, Name = Name };

            diff.AddDescription(Description.Compare(other.Description));
            diff.AddAuth(Auth.Compare(other.Auth));
            diff.AddProtocolProfileBehavior(ProtocolProfileBehavior.Compare(other.ProtocolProfileBehavior));
            diff.AddVariables(CollectionComparerV21.Compare(Variable, other.Variable));

            diff.AddEvents(CompareEvents(other.Event, Event));

            return diff;
        }
    }

    private record Item : ItemBase
    {
        public string? Id { get; set; }
        public required Request Request { get; set; }
        public RawJson Response { get; set; } = RawJson.Empty;

        public override ItemDiff ToDiff(ChangeType changeType)
        {
            var diff = base.ToDiff(changeType);

            diff.AddId(DiffValueChunk.SingleValue(Id));
            diff.AddResponse(DiffChunk.SingleValue(Response.Raw));
            diff.AddRequest(Request.ToDiff(changeType));

            return diff;
        }

        public override ItemDiff Compare(ItemBase other)
        {
            var diff = base.Compare(other);

            if (other is Item otherItem)
            {
                diff.AddId(Id.CompareDiff(otherItem.Id));
                diff.AddResponse(Response.Compare(otherItem.Response));
                diff.AddRequest(Request.Compare(otherItem.Request));
            }

            return diff;
        }
    }

    private record Request
    {
        public required Url Url { get; set; }
        public RawJson Auth { get; set; } = RawJson.Empty;
        public RawJson Proxy { get; set; } = RawJson.Empty;
        public RawJson Certificate { get; set; } = RawJson.Empty;
        public required string Method { get; set; }
        public List<Header> Header { get; set; } = new List<Header>();
        public Body? Body { get; set; }
        public RawJson Description { get; set; } = RawJson.Empty;

        public RequestDiff ToDiff(ChangeType changeType)
        {
            var diff = new RequestDiff();

            diff.AddUrl(DiffValueChunk.SingleValue(Url.Raw));
            diff.AddAuth(new DiffChunk(Auth?.Raw));
            diff.AddProxy(new DiffChunk(Proxy?.Raw));
            diff.AddCertificate(new DiffChunk(Certificate?.Raw));
            diff.AddMethod(DiffValueChunk.SingleValue(Method));
            diff.AddDescription(new DiffChunk(Description?.Raw));
            diff.AddHeaders(Header.Select(h => HeaderDiff.Create(h.Key, h.Value, h.Description?.Raw, h.Disabled)));

            if (Body is not null)
            {
                diff.AddBody(Body.ToDiff(changeType));
            }

            return diff;
        }

        public RequestDiff Compare(Request other)
        {
            var diff = new RequestDiff();

            diff.AddUrl(Url.Compare(other.Url));
            diff.AddAuth(Auth.Compare(other.Auth));
            diff.AddProxy(Proxy.Compare(other.Proxy));
            diff.AddCertificate(Certificate.Compare(other.Certificate));
            diff.AddMethod(Method.CompareDiff(other.Method));
            diff.AddDescription(Description.Compare(other.Description));
            diff.AddHeaders(Compare(other.Header, Header));
            if (Body is not null)
            {
                diff.AddBody(other.Body!.Compare(Body));
            }
            else if (other.Body is not null)
            {
                diff.AddBody(other.Body.ToDiff(ChangeType.Unchanged));
            }

            return diff;
        }

        private static IEnumerable<HeaderDiff> Compare(IEnumerable<Header> original, IEnumerable<Header> updated)
        {
            var diff = updated
                .Select(updatedHeader =>
                {
                    var originalHeader = (original ?? Array.Empty<Header>())
                        .FirstOrDefault(o => updatedHeader.Key.Equals(o.Key, StringComparison.OrdinalIgnoreCase));

                    return originalHeader is not null ?
                        new HeaderDiff(updatedHeader.Key, new DiffValueChunk(updatedHeader.Value, originalHeader.Value),
                            DiffChunk.Create(updatedHeader.Description.Raw, originalHeader.Description.Raw),
                            new DiffValueChunk(updatedHeader.Disabled.ToString(), originalHeader.Disabled.ToString())) :
                        HeaderDiff.NewCreated(updatedHeader.Key, updatedHeader.Value,
                            updatedHeader.Description.Raw, updatedHeader.Disabled);
                }).ToList();

            foreach (var originalHeader in original ?? Enumerable.Empty<Header>())
            {
                var updatedVar = updated
                    .FirstOrDefault(u => u.Key.Equals(originalHeader.Key, StringComparison.OrdinalIgnoreCase));

                if (updatedVar is null)
                {
                    diff.Add(HeaderDiff.Removed(originalHeader.Key, originalHeader.Value,
                        originalHeader.Description.Raw, originalHeader.Disabled));
                }
            }

            return diff;
        }
    }

    private record Folder : ItemBase
    {
        public override ItemDiff Compare(ItemBase other)
        {
            var diff = base.Compare(other);
            diff.IsFolder = true;

            return diff;
        }

        public override ItemDiff ToDiff(ChangeType changeType)
        {
            var diff = base.ToDiff(changeType);
            diff.IsFolder = true;

            return diff;
        }
    }

    private record Event
    {
        public string Id { get; set; } = string.Empty;
        public string Listen { get; set; } = string.Empty;
        public bool Disabled { get; set; }
        public Script Script { get; set; } = Script.Empty;
    }

    private record Script
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public required string[] Exec { get; set; }

        public static Script Empty { get; } = new Script() { Exec = Array.Empty<string>() };
    }

    private record Variable : RawJson
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
    }

    private record Header
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public bool Disabled { get; set; }
        public RawJson Description { get; set; } = RawJson.Empty;
    }

    private record Body
    {
        public required string Mode { get; set; }
        public string? Raw { get; set; }
        public RawJson Urlencoded { get; set; } = RawJson.Empty;
        public RawJson Formdata { get; set; } = RawJson.Empty;
        public RawJson File { get; set; } = RawJson.Empty;
        public RawJson Options { get; set; } = RawJson.Empty;
        public bool Disabled { get; set; }

        public BodyDiff ToDiff(ChangeType changeType)
        {
            var diff = new BodyDiff();

            diff.AddMode(DiffValueChunk.SingleValue(Mode));
            if (changeType == ChangeType.Inserted)
            {
                diff.AddRaw(DiffChunk.Create(Raw, string.Empty));
            }
            else
            {
                diff.AddRaw(DiffChunk.SingleValue(Raw));
            }
            diff.AddUrlencoded(DiffChunk.SingleValue(Urlencoded?.Raw));
            diff.AddFormdata(DiffChunk.SingleValue(Formdata?.Raw));
            diff.AddFile(DiffChunk.SingleValue(File?.Raw));
            diff.AddOptions(DiffChunk.SingleValue(Options?.Raw));

            return diff;
        }

        public BodyDiff Compare(Body? other)
        {
            if (other is not null)
            {
                var diff = new BodyDiff();
                diff.AddMode(Mode.CompareDiff(other.Mode));
                diff.AddRaw(DiffChunk.Create(other.Raw, Raw));
                diff.AddUrlencoded(Urlencoded.Compare(other.Urlencoded));
                diff.AddFormdata(Formdata.Compare(other.Formdata));
                diff.AddFile(File.Compare(other.File));
                diff.AddOptions(Options.Compare(other.Options));
                return diff;
            }
            else
            {
                return ToDiff(ChangeType.Unchanged);
            }
        }
    }

    private record PostmanCollection
    {
        public required Information Info { get; set; }
        public List<Variable> Variable = null!;
        public List<Event> Event { get; set; } = null!;
        public RawJson Auth { get; set; } = RawJson.Empty;
        public RawJson ProtocolProfileBehavior { get; set; } = RawJson.Empty;
        public required Dictionary<string, ItemBase> Items { get; set; }
        public required string Name { get; set; }

        public static PostmanCollection Empty { get; } = new PostmanCollection()
        {
            Info = new() { Name = string.Empty, Schema = string.Empty },
            Items = [],
            Name = string.Empty,
            Event = [],
            Variable = []
        };
    }

    private class RawJsonConverter : JsonConverter<RawJson>
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public override RawJson Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var snapshot = reader;

            if (reader.TokenType == JsonTokenType.String)
            {
                return new()
                {
                    Raw = reader.GetString() ?? string.Empty
                };
            }

            if (typeToConvert == typeof(RawJson))
            {
                using var jsonDocument = JsonDocument.ParseValue(ref reader);
                var jsonText = jsonDocument.RootElement.GetRawText();
                return new()
                {
                    Raw = jsonText
                };
            }

            var obj = JsonSerializer.Deserialize(ref reader, typeToConvert, _jsonSerializerOptions)!;
            ((RawJson)obj).Raw = JsonSerializer.Deserialize<JsonDocument>(ref snapshot)!.RootElement.GetRawText();

            return (RawJson)obj;
        }

        public override void Write(Utf8JsonWriter writer, RawJson value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class RawJsonConverterFactory : JsonConverterFactory
    {
        private readonly JsonConverter _converter;

        public RawJsonConverterFactory(Type typeToConvert, JsonConverter converter)
        {
            TypeToConvert = typeToConvert;
            _converter = converter;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == TypeToConvert;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return _converter;
        }

        public Type TypeToConvert { get; }
    }
}
