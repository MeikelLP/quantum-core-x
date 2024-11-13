using System.Collections.Immutable;
using System.Text;
using BinarySerialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Core.Types;

namespace QuantumCore.Game
{
    /// <summary>
    /// Manage all static data related to monster
    /// </summary>
    public class MonsterManager : IMonsterManager, ILoadable
    {
        private readonly ILogger<MonsterManager> _logger;
        private readonly IFileProvider _fileProvider;
        private ImmutableArray<MonsterData> _proto = [];

        static MonsterManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // register korean locale
        }

        public MonsterManager(ILogger<MonsterManager> logger, IFileProvider fileProvider)
        {
            _logger = logger;
            _fileProvider = fileProvider;
        }

        /// <summary>
        /// Try to load mob_proto file
        /// </summary>
        public async Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Loading mob_proto");

            await using var fs = _fileProvider.GetFileInfo("mob_proto").CreateReadStream();
            var bs = new BinarySerializer
            {
                Options = SerializationOptions.ThrowOnEndOfStream
            };
            var result = await bs.DeserializeAsync<MonsterDataContainer>(fs);
            var items = new LzoXtea(result.Payload.RealSize, result.Payload.EncryptedSize, 0x497446, 0x4A0B, 0x86EB7,
                0x68189D);
            var itemsRaw = items.Decode(result.Payload.EncryptedPayload);
            _proto = [..bs.Deserialize<MonsterData[]>(itemsRaw)];
            _logger.LogDebug("Loaded {Count} monsters", _proto.Length);
        }

        public MonsterData? GetMonster(uint id)
        {
            return _proto.FirstOrDefault(monster => monster.Id == id);
        }

        public ImmutableArray<MonsterData> GetMonsters()
        {
            return _proto;
        }
    }
}
