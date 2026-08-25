namespace WPFControls.UI.Models.DummyJson;

public sealed class DummyJsonProduct
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public double Rating { get; init; }

    public int Stock { get; init; }

    public string? Brand { get; init; }

    public string Thumbnail { get; init; } = string.Empty;
}
