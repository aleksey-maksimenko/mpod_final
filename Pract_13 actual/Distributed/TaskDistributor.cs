using DistributedWebCrawler.Models;

namespace DistributedWebCrawler.Distributed;

public class TaskDistributor
{
    private int _nextWorkerIndex;

    // round robin-распределение задачи между рабочими узлами 
    public Dictionary<WorkerNode, List<CrawlerTask>> DistributeRoundRobin(
        IEnumerable<CrawlerTask> tasks,
        IReadOnlyList<WorkerNode> workers)
    {
        if (workers.Count == 0)
            throw new InvalidOperationException(
                "Нет доступных рабочих узлов.");
        var distribution = new Dictionary<WorkerNode, List<CrawlerTask>>();
        foreach (var worker in workers)
        {
            distribution[worker] = new List<CrawlerTask>();
        }
        foreach (var task in tasks)
        {
            var worker = workers[_nextWorkerIndex];
            distribution[worker].Add(task);
            _nextWorkerIndex++;
            if (_nextWorkerIndex >= workers.Count)
            {
                _nextWorkerIndex = 0;
            }
        }
        return distribution;
    }
}