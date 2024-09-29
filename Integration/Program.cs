using Integration.Service;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        //For Single Server Integration Scenario
        //var service = new ItemIntegrationService();
        
        //For Distributed Server Integration Scenario
        var service = new DistributedItemIntegrationService();
        
        var items = new[] { "a", "b", "c" };
        var taskList = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            taskList.Add(Task.Run(() =>
            {
                var random = new Random(Guid.NewGuid().GetHashCode());
                var item = items[random.Next(items.Length)];
                var result = service.SaveItem(item);
                Console.WriteLine(result.Message);
            }));
        }

        Task.WaitAll(taskList.ToArray());
        
        // ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        // ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        // ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        //
        // Thread.Sleep(500);
        //
        // ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        // ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        // ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        //
        // Thread.Sleep(5000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();
    }
    
}