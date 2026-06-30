namespace DistributedWebCrawler.Models;

public class ValidatedPage
{
    public required DownloadedPage Page { get; init; }

    public bool IsValid { get; init; }

    public string? Error { get; init; }
}