using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace QuantumCore.Caching.InMemory;

/// <summary>
/// Basic implementation of a in-memory equivalent of redis
/// </summary>
public class InMemoryRedisStore : IRedisStore
{
    private readonly Dictionary<string, (object Value, DateTime? Expiry)> _dict = new();
    private readonly List<InMemoryRedisSubscriber> _subscribers = [];

    public IRedisListWrapper<T> CreateList<T>(string name)
    {
        if (_dict.TryGetValue(name, out var value))
        {
            if (value.Expiry is not null && value.Expiry < DateTime.UtcNow)
            {
                _dict.Remove(name);
            }
            else
            {
                Debug.Assert(value.Value.GetType().IsAssignableTo(typeof(IRedisListWrapper<T>)), "type mismatch");
                return (IRedisListWrapper<T>)value.Value;
            }
        }

        var list = new InMemoryRedisListWrapper<T>();
        _dict.Add(name, (list, null));
        return list;
    }

    public ValueTask<long> Del(string key)
    {
        _dict.Remove(key);
        return ValueTask.FromResult(1L);
    }

    public ValueTask<string> Set(string key, object item)
    {
        _dict[key] = (item, null);
        return ValueTask.FromResult(key);
    }

    public ValueTask<T> Get<T>(string key)
    {
        if (_dict.TryGetValue(key, out var value))
        {
            if (value.Expiry is not null && value.Expiry < DateTime.UtcNow)
            {
                _dict.Remove(key);
                return default;
            }

            return ValueTask.FromResult((T)value.Value);
        }

        return default;
    }

    public async ValueTask<long> Exists(string key)
    {
        if (_dict.TryGetValue(key, out var value))
        {
            if (value.Expiry is not null && value.Expiry < DateTime.UtcNow)
            {
                _dict.Remove(key);
                return 0;
            }

            if (value.Value is IRedisListWrapper enumerable && await enumerable.Len() == 0)
            {
                // empty list equals no result
                return 0;
            }

            return 1;
        }

        return 0;
    }

    public ValueTask<long> Expire(string key, TimeSpan seconds)
    {
        if (_dict.TryGetValue(key, out var tuple))
        {
            tuple.Expiry = DateTime.UtcNow + seconds;
            _dict[key] = tuple;
        }

        return ValueTask.FromResult(1L);
    }

    public ValueTask<bool> Ping()
    {
        return ValueTask.FromResult(true);
    }

    public ValueTask<long> Publish(string key, object obj)
    {
        var callbacks = _subscribers
            .SelectMany(x => x.Callbacks)
            .Where(x => x.Key == key)
            .Select(x => x.Value)
            .ToArray();
        foreach (var callback in callbacks)
        {
            var actionType = typeof(Action<>).MakeGenericType(obj.GetType());
            var methodInfo = actionType.GetMethod(nameof(Action<object>.Invoke))!;

            foreach (var action in (IEnumerable)callback)
            {
                methodInfo.Invoke(action, [obj]);
            }
        }

        return ValueTask.FromResult((long)callbacks.Length);
    }

    public IRedisSubscriber Subscribe()
    {
        var sub = new InMemoryRedisSubscriber();
        _subscribers.Add(sub);
        return sub;
    }

    public ValueTask<string[]> Keys(string key)
    {
        var regex = RedisPatternToRegex(key);
        var matchedKeys = _dict.Keys.Where(x => regex.IsMatch(key)).ToArray();
        return ValueTask.FromResult(matchedKeys);
    }

    public ValueTask<long> Persist(string key)
    {
        // TODO implement persistence
        return ValueTask.FromResult(1L);
    }

    public ValueTask<string> FlushAll()
    {
        _dict.Clear();

        return ValueTask.FromResult("");
    }

    public void DelAllAsync(string pattern)
    {
        var regex = RedisPatternToRegex(pattern);

        var keys = _dict.Keys.Where(x => regex.IsMatch(x)).ToArray();
        foreach (var key in keys)
        {
            _dict.Remove(key);
        }
    }

    public ValueTask<long> Incr(string key)
    {
        if (!_dict.TryGetValue(key, out var tuple) || tuple.Expiry is not null && tuple.Expiry > DateTime.UtcNow)
        {
            // according to redis docs the value is set to 0 if it does not exist before incrementing
            tuple = (0, null);
        }

        _dict[key] = ((int)tuple.Value + 1, tuple.Expiry);
        return ValueTask.FromResult(Convert.ToInt64(_dict[key].Value));
    }

    /// <summary>
    /// replace * (redis match all) with regex .*
    /// </summary>
    public static Regex RedisPatternToRegex(string key)
    {
        var regexPattern = new StringBuilder();
        var currentIndex = 0;
        int asterixIndex;
        while ((asterixIndex = key.IndexOf('*', currentIndex)) >= 0)
        {
            regexPattern.Append(Regex.Escape($"{key[currentIndex..asterixIndex]}"));
            regexPattern.Append(".*");
            currentIndex = asterixIndex + 1;
        }

        if (currentIndex < key.Length - 1)
        {
            regexPattern.Append(Regex.Escape($"{key[currentIndex..]}"));
        }

        return new Regex(regexPattern.ToString());
    }
}
