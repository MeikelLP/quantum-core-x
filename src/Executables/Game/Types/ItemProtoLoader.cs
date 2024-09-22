using System.Text;
using BinarySerialization;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Core.Types;

public class ItemProtoLoader
{
    static ItemProtoLoader()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // register korean locale
    }

    public async Task<ItemData[]> LoadAsync(string filePath)
    {
        await using var fs = File.OpenRead(filePath);
        var bs = new BinarySerializer
        {
            Options = SerializationOptions.AllowIncompleteObjects
        };
        var result = await bs.DeserializeAsync<ItemDataContainer>(fs);
        var items = new LzoXtea(result.Payload.RealSize, result.Payload.EncryptedSize, 0x2A4A1, 0x45415AA, 0x185A8BE7,
            0x1AAD6AB);
        var itemsRaw = items.Decode(result.Payload.EncryptedPayload);
        return bs.Deserialize<ItemData[]>(itemsRaw);
    }
}
