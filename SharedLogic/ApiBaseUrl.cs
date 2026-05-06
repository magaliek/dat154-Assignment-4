namespace SharedLogic;

public static class ApiBaseUrl
{
    public const string EnvironmentVariableName = "DAT154_API_BASE_URL";

    private const string LocalDevDefault = "http://localhost:5049";

    public static string Resolve()
    {
        var env = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(env))
            return env.Trim().TrimEnd('/');
        return LocalDevDefault;
    }

    public static Uri AsHttpClientBaseAddress()
    {
        var root = Resolve();
        if (!root.EndsWith('/'))
            root += "/";
        return new Uri(root);
    }

    public static bool IsLocalhost(Uri baseUri) =>
        string.Equals(baseUri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
        || string.Equals(baseUri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
}
