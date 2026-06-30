using DistributedWebCrawler.Models;

namespace DistributedWebCrawler.Services;

public class CatalogCrawler
{
    private const string BaseUrl = "https://poryadok.ru/search/";

    public IEnumerable<CrawlerTask> CreateTasks(string query, int maxPages = 10)
    {
        var encodedQuery = Uri.EscapeDataString(query);
        for (int page = 1; page <= maxPages; page++)
        {
            var uri = page <= 1 ? new Uri($"{BaseUrl}?q={encodedQuery}"): new Uri($"{BaseUrl}?q={encodedQuery}&page={page}");
            yield return new CrawlerTask
            {
                Query = query,
                Page = page,
                Uri = uri
            };
        }
    }
}