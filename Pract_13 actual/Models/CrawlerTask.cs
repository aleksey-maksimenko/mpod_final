namespace DistributedWebCrawler.Models;

public class CrawlerTask
{
    public required string Query { get; init; }

    public required int Page { get; init; }

    public required Uri Uri { get; init; }
}