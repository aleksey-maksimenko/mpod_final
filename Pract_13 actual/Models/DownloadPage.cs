namespace DistributedWebCrawler.Models;

public class DownloadedPage
{
    public required CrawlerTask Task { get; init; }

    public required string Html { get; init; }
}