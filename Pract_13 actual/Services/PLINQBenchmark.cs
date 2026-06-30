using System.Diagnostics;

namespace DistributedWebCrawler.Services;

public class PLINQBenchmark
{
    public List<Product> GenerateLargeDataset(IEnumerable<Product> source, int targetSize)
    {
        var sourceList = source.ToList();
        var result = new List<Product>(targetSize);
        int index = 0;
        while (result.Count < targetSize)
        {
            var p = sourceList[index % sourceList.Count];
            result.Add(new Product
            {
                Article = p.Article,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ParsedAt = p.ParsedAt
            });
            index++;
        }
        return result;
    }

    public (long sequential, long parallel, double speedup) BenchmarkFilter(IEnumerable<Product> products, int iterations)
    {
        var data = products.ToList();
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = data
                .Where(p => p.Price > 5000)
                .ToList();
        }
        sw.Stop();
        long sequential = sw.ElapsedMilliseconds;
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _ = data
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .Where(p => p.Price > 5000)
                .ToList();
        }
        sw.Stop();
        long parallel = sw.ElapsedMilliseconds;
        return (
            sequential,
            parallel,
            (double)sequential / parallel
        );
    }

    public (long sequential, long parallel, double speedup) BenchmarkAggregation(IEnumerable<Product> products, int iterations)
    {
        var data = products.ToList();
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = data.Average(p => p.Price);
        }
        sw.Stop();

        long sequential = sw.ElapsedMilliseconds;
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _ = data
                .AsParallel()
                .Average(p => p.Price);
        }

        sw.Stop();
        long parallel = sw.ElapsedMilliseconds;
        return (
            sequential,
            parallel,
            (double)sequential / parallel
        );
    }

    public Dictionary<int, long> BenchmarkDifferentDegrees(IEnumerable<Product> products)
    {
        var data = products.ToList();
        var result = new Dictionary<int, long>();
        foreach (int degree in new[] { 1, 2, 4 })
        {
            var sw = Stopwatch.StartNew();
            _ = data
                .AsParallel()
                .WithDegreeOfParallelism(degree)
                .Where(p => p.Price > 3000)
                .OrderBy(p => p.Price)
                .ToList();
            sw.Stop();
            result[degree] = sw.ElapsedMilliseconds;
        }
        return result;
    }
}