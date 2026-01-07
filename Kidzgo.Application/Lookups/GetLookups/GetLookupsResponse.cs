namespace Kidzgo.Application.Lookups.GetLookups;

public sealed class GetLookupsResponse
{
    public Dictionary<string, List<LookupItemDto>> Lookups { get; init; } = new();
}

public sealed class LookupItemDto
{
    public string Value { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
}

