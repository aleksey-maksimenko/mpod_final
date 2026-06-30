public class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Article { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public double Price { get; init; }

    public DateTime ParsedAt { get; init; } = DateTime.UtcNow;
}