using DistributedWebCrawler.Models;
using DistributedWebCrawler.Parsing;
using DistributedWebCrawler.Services;
using System.Threading.Tasks.Dataflow;

namespace DistributedWebCrawler.Distributed;

public class WorkerNode
{
    private readonly ITargetBlock<CrawlerTask> _pipelineInput;
    private readonly ISourceBlock<Product> _pipelineOutput;

    public int WorkerId { get; }
    public bool IsRunning { get; private set; } // обрабатывает ли задачи

    public CrawlerTask? CurrentTask { get; private set; }

    // событие вызывается при получении результата.
    public event Action<WorkerResult>? ResultProduced;

    public WorkerNode(int workerId, HtmlDownloader downloader, ProductParser parser)
    {
        WorkerId = workerId;

        var pipeline =
            ParsingPipelineBuilder.Build(
                downloader,
                parser);
        _pipelineInput = pipeline.Input;
        _pipelineOutput = pipeline.Output;
        StartResultReader();
    }

    public void StartProcessing()
    {
        IsRunning = true;
        Console.WriteLine($"Worker {WorkerId}: запущен");
    }

    public void StopProcessing()
    {
        IsRunning = false;
        _pipelineInput.Complete();
        Console.WriteLine($"Worker {WorkerId}: остановлен");
    }

    public async Task ProcessTask(CrawlerTask task)
    {
        if (!IsRunning)
            throw new InvalidOperationException("Рабочий узел не запущен");
        CurrentTask = task;
        Console.WriteLine($"Worker {WorkerId}: обработка \"{task.Query}\", страница {task.Page}");
        await _pipelineInput.SendAsync(task);
        CurrentTask = null;
    }

    // считывает результаты конвейера
    private void StartResultReader()
    {
        _ = Task.Run(async () =>
        {
            while (await _pipelineOutput.OutputAvailableAsync())
            {
                var product = await _pipelineOutput.ReceiveAsync();
                Console.WriteLine($"Worker {WorkerId}: найден товар \"{product.Name}\"");
                ResultProduced?.Invoke(
                    new WorkerResult
                    {
                        WorkerId = WorkerId,
                        Product = product
                    });
            }
        });
    }
}