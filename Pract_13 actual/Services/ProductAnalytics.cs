namespace DistributedWebCrawler.Services;

public class ProductAnalytics
{
    public int CountProducts(IEnumerable<Product> products)
    {
        return products.AsParallel().Count();
    }

    public double AveragePrice(IEnumerable<Product> products)
    {
        return products.AsParallel().Average(p => p.Price);
    }

    public Product? MostExpensive(IEnumerable<Product> products)
    {
        return products
            .AsParallel()
            .OrderByDescending(p => p.Price)
            .FirstOrDefault();
    }

    public Product? Cheapest(IEnumerable<Product> products)
    {
        return products
            .AsParallel()
            .OrderBy(p => p.Price)
            .FirstOrDefault();
    }

    public IEnumerable<Product> TopMostExpensive(IEnumerable<Product> products, int count)
    {
        return products
            .AsParallel()
            .OrderByDescending(p => p.Price)
            .Take(count)
            .ToList();
    }

    public IEnumerable<Product> ProductsAbovePrice(IEnumerable<Product> products, double minPrice)
    {
        return products
            .AsParallel()
            .Where(p => p.Price >= minPrice)
            .OrderByDescending(p => p.Price)
            .ToList();
    }

    public IEnumerable<IGrouping<string, Product>> GroupByFirstWord(IEnumerable<Product> products)
    {
        return products
            .AsParallel()
            .GroupBy(p =>
                p.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Без названия")
            .ToList();
    }
}