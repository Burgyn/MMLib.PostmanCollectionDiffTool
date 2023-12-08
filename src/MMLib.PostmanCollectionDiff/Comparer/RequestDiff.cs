namespace MMLib.PostmanCollectionDiff.Comparer;

public sealed class RequestDiff : IDiff
{
    public static RequestDiff NoDiff { get; } = new();

    public DiffValueChunk Url { get; private set; } = DiffValueChunk.NoDiff;

    public DiffChunk Auth { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk Proxy { get; private set; } = DiffChunk.NoDiff;

    public DiffChunk Certificate { get; private set; } = DiffChunk.NoDiff;

    public DiffValueChunk Method { get; private set; } = DiffValueChunk.NoDiff;

    public DiffChunk Description { get; private set; } = DiffChunk.NoDiff;

    public bool HasDifferences { get; private set; }

    public BodyDiff Body { get; private set; } = BodyDiff.NoDiff;

    public HeadersDiff Headers { get; set; } = new HeadersDiff();

    internal void AddUrl(DiffValueChunk url)
    {
        Url = url;
        if (url.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddAuth(DiffChunk auth)
    {
        Auth = auth;
        if (auth.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddProxy(DiffChunk proxy)
    {
        Proxy = proxy;
        if (proxy.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddCertificate(DiffChunk certificate)
    {
        Certificate = certificate;
        if (certificate.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddMethod(DiffValueChunk method)
    {
        Method = method;
        if (method.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddDescription(DiffChunk description)
    {
        Description = description;
        if (description.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddBody(BodyDiff body)
    {
        Body = body;
        if (body.HasDifferences)
        {
            HasDifferences = true;
        }
    }

    internal void AddHeaders(IEnumerable<HeaderDiff> headers)
    {
        Headers.AddHeader(headers);
        if (Headers.HasDifferences)
        {
            HasDifferences = true;
        }
    }
}
