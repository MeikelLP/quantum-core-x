using System.Diagnostics;

namespace QuantumCore.API.Core.Models;

[DebuggerDisplay("{Name} ({Id})")]
public class ItemData
{
    public uint Id { get; set; }
    public uint Unknown { get; set; }
    public string Name { get; set; } = "";
    public string TranslatedName { get; set; } = "";
    public byte Type { get; set; }
    public byte Subtype { get; set; }
    public byte Unknown2 { get; set; }
    public byte Size { get; set; }
    public uint AntiFlags { get; set; }
    public uint Flags { get; set; }
    public uint WearFlags { get; set; }
    public uint ImmuneFlags { get; set; }
    public uint BuyPrice { get; set; }
    public uint SellPrice { get; set; }
    public List<ItemLimitData> Limits { get; set; } = new();
    public List<ItemApplyData> Applies { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public List<int> Sockets { get; set; } = new();
    public uint UpgradeId { get; set; }
    public ushort UpgradeSet { get; set; }
    public byte MagicItemPercentage { get; set; }
    public byte Specular { get; set; }
    public byte SocketPercentage { get; set; }
}
