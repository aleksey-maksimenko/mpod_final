using DistributedWebCrawler.Models;

namespace DistributedWebCrawler.Distributed;

public class MasterNode
{
    private readonly List<WorkerNode> _workers = new();

    private readonly TaskDistributor _taskDistributor;
    private readonly ResultCollector _resultCollector;

    // подключенные рабочие узлы
    public IReadOnlyList<WorkerNode> ConnectedWorkers => _workers;

    public MasterNode(ResultCollector resultCollector)
    {
        _resultCollector = resultCollector;
        _taskDistributor = new TaskDistributor();
    }

    // регистрирует рабочий узел
    public void RegisterWorker(WorkerNode worker)
    {
        _workers.Add(worker);
        // подписываемся на результаты рабочего узла.
        worker.ResultProduced += result =>
        {
            _resultCollector.CollectResult(result);
        };
    }

    // распределяет задачи между рабочими узлами.
    public async Task DistributeTasks(IEnumerable<CrawlerTask> tasks)
    {
        if (_workers.Count == 0)
            throw new InvalidOperationException("Нет зарегистрированных рабочих узлов.");
        var distribution = _taskDistributor.DistributeRoundRobin(tasks, _workers);
        var processingTasks = new List<Task>();
        foreach (var pair in distribution)
        {
            var worker = pair.Key;
            foreach (var task in pair.Value)
            {
                processingTasks.Add(worker.ProcessTask(task));
            }
        }
        await Task.WhenAll(processingTasks);
    }

    public void StartWorkers()
    {
        foreach (var worker in _workers)
        {
            worker.StartProcessing();
        }
    }
    public void StopWorkers()
    {
        foreach (var worker in _workers)
        {
            worker.StopProcessing();
        }
    }
}