using DistributedWebCrawler.Models;

namespace DistributedWebCrawler.Parsing;

public class HtmlDownloader
{
    private readonly HttpClient _httpClient;

    public HtmlDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DownloadedPage> DownloadAsync(CrawlerTask task)
    {
        var html = await _httpClient.GetStringAsync(task.Uri);
        return new DownloadedPage
        {
            Task = task,
            Html = html
        };
    }
}