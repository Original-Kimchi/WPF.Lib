namespace WPFControls.UI.Models.DummyJson;

public sealed class DummyJsonProductPage
{
    public IReadOnlyList<DummyJsonProduct> Products { get; init; } = [];

    public int Total { get; init; }

    public int Skip { get; init; }

    public int Limit { get; init; }
}
