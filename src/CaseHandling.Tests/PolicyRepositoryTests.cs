using CaseHandling.PolicyMcp;

namespace CaseHandling.Tests;

public sealed class PolicyRepositoryTests
{
    private readonly PolicyRepository _repository =
        new(Path.Combine(AppContext.BaseDirectory, "data", "policies"));

    [Fact]
    public void Loads_all_policies_with_metadata()
    {
        Assert.Equal(5, _repository.All.Count);
        Assert.Equal("Water damage", _repository.Get("POL-WATER-002")?.Title);
    }

    [Fact]
    public void Search_by_category_finds_policy()
    {
        var results = _repository.Search("TravelDelay");

        Assert.Contains(results, p => p.Id == "POL-TRAVEL-004");
    }
}
