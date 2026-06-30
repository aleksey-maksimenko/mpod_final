using DistributedWebCrawler.Models;
using DistributedWebCrawler.Parsing;
using DistributedWebCrawler.Services;
using DistributedWebCrawler.Storage;
using DistributedWebCrawler.Distributed;


using System.Threading.Tasks.Dataflow;

static void PlinqSequentalCompare(ProductRepository repository)
{
    int DATA_SIZE = 1_000_000;

    Console.WriteLine("PLINQ Benchmark");
    var benchmark = new PLINQBenchmark();

    Console.WriteLine($"Генерация данных {DATA_SIZE} записей...");
    var bigData = benchmark.GenerateLargeDataset(repository.GetAll(), DATA_SIZE);
    Console.WriteLine($"Размер набора: {bigData.Count:N0}");
    Console.WriteLine();

    var filter = benchmark.BenchmarkFilter(bigData, 10);
    Console.WriteLine(
        $"Фильтрация\n" +
        $"Последовательно : {filter.sequential} ms\n" +
        $"PLINQ           : {filter.parallel} ms\n" +
        $"Ускорение       : {filter.speedup:F2}x");
    Console.WriteLine();

    var aggregation = benchmark.BenchmarkAggregation(bigData, 10);
    Console.WriteLine(
        $"Средняя цена\n" +
        $"Последовательно : {aggregation.sequential} ms\n" +
        $"PLINQ           : {aggregation.parallel} ms\n" +
        $"Ускорение       : {aggregation.speedup:F2}x");
    Console.WriteLine();
    Console.WriteLine("Степень параллелизма");
    foreach (var pair in benchmark.BenchmarkDifferentDegrees(bigData))
    {
        Console.WriteLine($"{pair.Key} поток(а): {pair.Value} ms");
    }
}

static async Task DemoSingleWorker(ProductRepository repository)
{
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

    var crawler = new CatalogCrawler();
    var downloader = new HtmlDownloader(httpClient);
    var parser = new ProductParser();

    var collector = new ResultCollector(repository);
    var master = new MasterNode(collector);

    var worker =
        new WorkerNode(
            workerId: 1,
            downloader,
            parser);

    master.RegisterWorker(worker);
    master.StartWorkers();

    Console.WriteLine();
    Console.WriteLine("SINGLE WORKER TEST");
    Console.WriteLine();

    var tasks = new List<CrawlerTask>();
    foreach (var task in crawler.CreateTasks("дрель", 2))
    {
        tasks.Add(task);
    }

    await master.DistributeTasks(tasks);
    await Task.Delay(3000);

    master.StopWorkers();
    Console.WriteLine();
    Console.WriteLine($"Repository: {repository.Count} товаров.");
}

static async Task DemoMasterWorker(ProductRepository repository)
{
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

    var crawler = new CatalogCrawler();
    var collector = new ResultCollector(repository);
    var master = new MasterNode(collector);
    master.RegisterWorker(
        new WorkerNode(
            1,
            new HtmlDownloader(httpClient),
            new ProductParser()));
    master.RegisterWorker(
        new WorkerNode(
            2,
            new HtmlDownloader(httpClient),
            new ProductParser()));
    master.RegisterWorker(
        new WorkerNode(
            3,
            new HtmlDownloader(httpClient),
            new ProductParser()));

    master.StartWorkers();
    Console.WriteLine();
    Console.WriteLine("MASTER - WORKER (3 узла)");
    Console.WriteLine();

    var tasks = new List<CrawlerTask>();
    string[] queries =
    {
        "кресло",
        "лампа",
        "дрель"
    };
    foreach (var query in queries)
    {
        foreach (var task in crawler.CreateTasks(query, 10))
        {
            tasks.Add(task);
        }
    }

    await master.DistributeTasks(tasks);
    await Task.Delay(5000);
    master.StopWorkers();
    Console.WriteLine();
    Console.WriteLine($"Repository: {repository.Count} товаров.");
}

var repository = new ProductRepository();

//await DemoSingleWorker(repository);
//repository.Clear();

Console.WriteLine("\n==========================================\n");
await DemoMasterWorker(repository);

var analytics = new ProductAnalytics();
var allProducts = repository.GetAll();

Console.WriteLine($"Количество товаров: {analytics.CountProducts(allProducts)}");
Console.WriteLine($"Средняя цена: {analytics.AveragePrice(allProducts):F2} руб.\n");

PlinqSequentalCompare(repository);