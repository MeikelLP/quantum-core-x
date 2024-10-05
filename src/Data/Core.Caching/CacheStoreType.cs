using System.Runtime.Serialization;

namespace QuantumCore.Caching;

public enum CacheStoreType
{
    
    /// <summary>
    /// Redis database for common data
    /// </summary>
    [EnumMember(Value = "Common")]
    Shared = 0,
    
    /// <summary>
    /// Redis database for current server
    /// </summary>
    [EnumMember(Value = "Server")]
    Server = 1, // todo: read from config
    
}