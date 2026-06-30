using DistributedWebCrawler.Models;
using DistributedWebCrawler.Parsing;
using System.Threading.Tasks.Dataflow;

namespace DistributedWebCrawler.Services;

public static class ParsingPipelineBuilder
{
    public static (ITargetBlock<CrawlerTask> Input, ISourceBlock<Product> Output, Task Completion) Build
            (HtmlDownloader downloader,
            ProductParser parser)
    {
        // 1 - входная очередь заданий
        var taskBuffer = new BufferBlock<CrawlerTask>();
        // 2 - загрузка html
        var downloadBlock = new TransformBlock<CrawlerTask, DownloadedPage>(downloader.DownloadAsync);
        // 3 - проверка html
        var validateBlock =
            new TransformBlock<DownloadedPage, ValidatedPage>(page =>
            {
                if (string.IsNullOrWhiteSpace(page.Html))
                {
                    return new ValidatedPage
                    {
                        Page = page,
                        IsValid = false,
                        Error = "HTML-документ пуст."
                    };
                }
                if (!page.Html.Contains("<html", StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidatedPage
                    {
                        Page = page,
                        IsValid = false,
                        Error = "Отсутствует тег <html>"
                    };
                }
                if (!page.Html.Contains("<body", StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidatedPage
                    {
                        Page = page,
                        IsValid = false,
                        Error = "Отсутствует тег <body>"
                    };
                }
                return new ValidatedPage
                {
                    Page = page,
                    IsValid = true
                };
            });
        // блок 4 - разбор html
        var parseBlock =
            new TransformBlock<ValidatedPage, IEnumerable<Product>>(page =>
            {
                return parser.Parse(page.Page.Html);
            });
        // 5 - разворачивание коллекции товаров
        var flattenBlock = new TransformManyBlock<IEnumerable<Product>, Product>(products => products);

        taskBuffer.LinkTo(
            downloadBlock,
            new DataflowLinkOptions
            {
                PropagateCompletion = true
            });
            
        downloadBlock.LinkTo(
            validateBlock,
            new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

        validateBlock.LinkTo(
            parseBlock,
            new DataflowLinkOptions
            {
                PropagateCompletion = true
            },
            page => page.IsValid);

        validateBlock.LinkTo(
            DataflowBlock.NullTarget<ValidatedPage>(),
            new DataflowLinkOptions
            {
                PropagateCompletion = true
            },
            page => !page.IsValid);

        parseBlock.LinkTo(
            flattenBlock,
            new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

        return (
            taskBuffer,
            flattenBlock,
            flattenBlock.Completion);
    }
}