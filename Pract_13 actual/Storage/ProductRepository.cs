using System.Collections.Concurrent;

namespace DistributedWebCrawler.Storage;

public class ProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public bool Add(Product product)
    {
        return _products.TryAdd(product.Id, product);
    }

    public bool Remove(Guid id)
    {
        return _products.TryRemove(id, out _);
    }

    public Product? Get(Guid id)
    {
        return _products.TryGetValue(id, out var product)? product: null;
    }

    public IReadOnlyCollection<Product> GetAll()
    {
        return _products.Values.ToList().AsReadOnly();
    }

    public int Count => _products.Count;

    public void Clear()
    {
        _products.Clear();
    }
}