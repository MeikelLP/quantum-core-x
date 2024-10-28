using System.Diagnostics;
using BinarySerialization;

namespace QuantumCore.API.Core.Models;

[DebuggerDisplay("{Name} ({Id})")]
public class ItemData
{
    [FieldOrder(0)] public uint Id { get; set; }
    [FieldOrder(1)] public uint Unknown { get; set; }

    [FieldOrder(2), FieldLength(25), FieldEncoding("EUC-KR")]
    [SerializeAs(SerializedType.TerminatedString)]
    public string Name { get; set; } = "";

    [FieldOrder(3), FieldLength(25), FieldEncoding("EUC-KR")]
    [SerializeAs(SerializedType.TerminatedString)]
    public string TranslatedName { get; set; } = "";

    [FieldOrder(4)] public byte Type { get; set; }
    [FieldOrder(5)] public byte Subtype { get; set; }
    [FieldOrder(6)] public byte Unknown2 { get; set; }
    [FieldOrder(7)] public byte Size { get; set; }
    [FieldOrder(8)] public uint AntiFlags { get; set; }
    [FieldOrder(9)] public uint Flags { get; set; }
    [FieldOrder(10)] public uint WearFlags { get; set; }
    [FieldOrder(11)] public uint ImmuneFlags { get; set; }
    [FieldOrder(12)] public uint BuyPrice { get; set; }
    [FieldOrder(13)] public uint SellPrice { get; set; }
    [FieldOrder(14), FieldLength(5 * 2)] public List<ItemLimitData> Limits { get; set; } = [];
    [FieldOrder(15), FieldLength(5 * 3)] public List<ItemApplyData> Applies { get; set; } = [];
    [FieldOrder(16), FieldLength(6 * 4)] public List<int> Values { get; set; } = [];
    [FieldOrder(17), FieldLength(3 * 4)] public List<int> Sockets { get; set; } = [];
    [FieldOrder(18)] public uint UpgradeId { get; set; }
    [FieldOrder(19)] public ushort UpgradeSet { get; set; }
    [FieldOrder(20)] public byte MagicItemPercentage { get; set; }
    [FieldOrder(21)] public byte Specular { get; set; }
    [FieldOrder(22)] public byte SocketPercentage { get; set; }
}
