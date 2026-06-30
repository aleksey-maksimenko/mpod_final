namespace DistributedWebCrawler.Models;

public class WorkerResult
{
    public int WorkerId { get; init; }
    public required Product Product { get; init; }
}