namespace DistributedWebCrawler.Models;

public class WorkerInfo
{
    public int WorkerId { get; init; }
    public int ActiveTasks { get; set; }
    public bool IsConnected { get; set; }
}