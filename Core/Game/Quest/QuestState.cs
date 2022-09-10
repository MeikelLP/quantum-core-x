using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumCore.Cache;
using Serilog;
using SpanJson.Formatters.Dynamic;

namespace QuantumCore.Game.Quest;

public class QuestState
{
    private readonly Guid _playerId;
    private readonly string _questId;
    private Dictionary<string, object> _state = new();

    public QuestState(Guid playerId, string questId)
    {
        _playerId = playerId;
        _questId = questId;
    }

    public async Task Load()
    {
        _state = await CacheManager.Redis.Get<Dictionary<string, object>>($"quest:{_playerId}:{_questId}");
        if (_state == null)
        {
            _state = new Dictionary<string, object>();
        }
    }

    public async Task Save()
    {
        await CacheManager.Redis.Set($"quest:{_playerId}:{_questId}", _state);
    }
    
    public void Set<T>(string name, T value)
    {
        _state[name] = value;
    }

    public object Get(string name)
    {
        return !_state.ContainsKey(name) ? null : _state[name];
    }

    public IEnumerable<string> Keys {
        get {
            return _state.Keys;
        }
    }

    public T Get<T>(string name)
    {
        if (!_state.ContainsKey(name))
        {
            return default;
        }

        var value = _state[name];
        if (value is SpanJsonDynamicUtf8Number number)
        {
            if (number.TryConvert(typeof(T), out var converted))
            {
                return (T) converted;
            }
        }
        
        if (value is T ret)
        {
            return ret;
        }
        
        Log.Warning($"Type mismatch, state {name} expected {typeof(T).Name}, actual {value.GetType().Name}");
        return default;
    }
}