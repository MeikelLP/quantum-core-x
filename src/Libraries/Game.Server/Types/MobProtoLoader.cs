using System.Text;
using BinarySerialization;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Core.Types;

public class MobProtoLoader
{
    static MobProtoLoader()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // register korean locale
    }

    public async Task<MonsterData[]> LoadAsync(string filePath)
    {
        await using var fs = File.OpenRead(filePath);
        var bs = new BinarySerializer
        {
            Options = SerializationOptions.ThrowOnEndOfStream
        };
        var result = await bs.DeserializeAsync<MonsterDataContainer>(fs);
        var items = new LzoXtea(result.Payload.RealSize, result.Payload.EncryptedSize, 0x497446, 0x4A0B, 0x86EB7,
            0x68189D);
        var itemsRaw = items.Decode(result.Payload.EncryptedPayload);
        return bs.Deserialize<MonsterData[]>(itemsRaw);
    }
}