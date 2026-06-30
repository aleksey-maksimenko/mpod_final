using DistributedWebCrawler.Models;
using DistributedWebCrawler.Storage;

namespace DistributedWebCrawler.Distributed;

public class ResultCollector
{
    private readonly ProductRepository _repository;

    public ResultCollector(ProductRepository repository)
    {
        _repository = repository;
    }

    // результат от рабочего узла
    public void CollectResult(WorkerResult result)
    {
        Console.WriteLine($"Master: считано \"{result.Product.Name}\" от Worker {result.WorkerId}");
        _repository.Add(result.Product);
    }

    // несколько результатов
    public void CollectResults(IEnumerable<WorkerResult> results)
    {
        foreach (var result in results)
        {
            CollectResult(result);
        }
    }
}