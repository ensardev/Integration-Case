using Integration.Backend;
using Integration.Common;
using StackExchange.Redis;

namespace Integration.Service;

public sealed class DistributedItemIntegrationService
{
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();
    
    private static readonly ConnectionMultiplexer _redis;
    private static readonly IDatabase _db;

    static DistributedItemIntegrationService()
    {
        _redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
        _db = _redis.GetDatabase();
    }
    
    private static readonly TimeSpan _lockExpiry = TimeSpan.FromSeconds(30);

    public Result SaveItem(string itemContent)
    {
        string lockKey = $"lock:item:{itemContent}";
        string uniqueLockId = Guid.NewGuid().ToString();
        
        bool lockAcquired = _db.StringSet(lockKey, uniqueLockId, _lockExpiry, When.NotExists);

        if (!lockAcquired)
        {
            return new Result(false, $"Item with content '{itemContent}' is being processed by another instance.");
        }

        try
        {
            string existingItemKey = $"item:{itemContent}";
            if (_db.KeyExists(existingItemKey))
            {
                return new Result(false, $"Duplicate item received with content '{itemContent}'.");
            }

            var item = ItemIntegrationBackend.SaveItem(itemContent);
            
            _db.StringSet(existingItemKey, item.Id.ToString());

            return new Result(true, $"Item with content '{itemContent}' saved with id {item.Id}");
        }
        finally
        {
            string currentValue = _db.StringGet(lockKey);
            if (currentValue == uniqueLockId)
            {
                _db.KeyDelete(lockKey);
            }
        }
    }
    
    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}