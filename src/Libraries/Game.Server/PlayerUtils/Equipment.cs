using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Items;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.PlayerUtils;

public class Equipment : IEquipment
{
    public uint Owner { get; }
    public ItemInstance? Body { get; private set; }
    public ItemInstance? Head { get; private set; }
    public ItemInstance? Shoes { get; private set; }
    public ItemInstance? Bracelet { get; private set; }
    public ItemInstance? Weapon { get; private set; }
    public ItemInstance? Necklace { get; private set; }
    public ItemInstance? Earrings { get; private set; }
    public ItemInstance? Costume { get; private set; }
    public ItemInstance? Hair { get; private set; }

    private long _offset;

    public Equipment(uint owner, long offset)
    {
        Owner = owner;
        _offset = offset;
    }

    public bool SetItem(ItemInstance item)
    {
        return SetItem(item, (ushort)item.Position);
    }

    public bool SetItem(ItemInstance item, ushort position)
    {
        switch ((EquipmentSlot)(position - _offset))
        {
            case EquipmentSlot.Body:
                Body = item;
                return true;
            case EquipmentSlot.Head:
                Head = item;
                return true;
            case EquipmentSlot.Shoes:
                Shoes = item;
                return true;
            case EquipmentSlot.Bracelet:
                Bracelet = item;
                return true;
            case EquipmentSlot.Weapon:
                Weapon = item;
                return true;
            case EquipmentSlot.Necklace:
                Necklace = item;
                return true;
            case EquipmentSlot.Earring:
                Earrings = item;
                return true;
            case EquipmentSlot.Costume:
                Costume = item;
                return true;
            case EquipmentSlot.Hair:
                Hair = item;
                return true;
        }

        return false;
    }

    public ItemInstance? GetItem(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Body:
                return Body;
            case EquipmentSlot.Head:
                return Head;
            case EquipmentSlot.Shoes:
                return Shoes;
            case EquipmentSlot.Bracelet:
                return Bracelet;
            case EquipmentSlot.Weapon:
                return Weapon;
            case EquipmentSlot.Necklace:
                return Necklace;
            case EquipmentSlot.Earring:
                return Earrings;
            case EquipmentSlot.Costume:
                return Costume;
            case EquipmentSlot.Hair:
                return Hair;
        }

        return null;
    }

    public ItemInstance? GetItem(ushort position)
    {
        return GetItem((EquipmentSlot)(position - _offset));
    }

    public bool RemoveItem(ItemInstance item)
    {
        switch ((EquipmentSlot)(item.Position - _offset))
        {
            case EquipmentSlot.Body:
                Body = null;
                return true;
            case EquipmentSlot.Head:
                Head = null;
                return true;
            case EquipmentSlot.Shoes:
                Shoes = null;
                return true;
            case EquipmentSlot.Bracelet:
                Bracelet = null;
                return true;
            case EquipmentSlot.Weapon:
                Weapon = null;
                return true;
            case EquipmentSlot.Necklace:
                Necklace = null;
                return true;
            case EquipmentSlot.Earring:
                Earrings = null;
                return true;
            case EquipmentSlot.Costume:
                Costume = null;
                return true;
            case EquipmentSlot.Hair:
                Hair = null;
                return true;
        }

        return false;
    }

    public void Send(IPlayerEntity player)
    {
        if (Body != null)
        {
            player.SendItem(Body);
        }

        if (Head != null)
        {
            player.SendItem(Head);
        }

        if (Shoes != null)
        {
            player.SendItem(Shoes);
        }

        if (Bracelet != null)
        {
            player.SendItem(Bracelet);
        }

        if (Weapon != null)
        {
            player.SendItem(Weapon);
        }

        if (Necklace != null)
        {
            player.SendItem(Necklace);
        }

        if (Earrings != null)
        {
            player.SendItem(Earrings);
        }

        if (Costume != null)
        {
            player.SendItem(Costume);
        }

        if (Hair != null)
        {
            player.SendItem(Hair);
        }
    }

    public bool IsSuitable(IItemManager itemManager, ItemInstance item, ushort position)
    {
        var proto = itemManager.GetItem(item.ItemId);
        if (proto == null)
        {
            return false;
        }

        var wearFlags = (EWearFlags)proto.WearFlags;

        switch ((EquipmentSlot)(position - _offset))
        {
            case EquipmentSlot.Body:
                return wearFlags.HasFlag(EWearFlags.Body);
            case EquipmentSlot.Head:
                return wearFlags.HasFlag(EWearFlags.Head);
            case EquipmentSlot.Shoes:
                return wearFlags.HasFlag(EWearFlags.Shoes);
            case EquipmentSlot.Bracelet:
                return wearFlags.HasFlag(EWearFlags.Bracelet);
            case EquipmentSlot.Weapon:
                return wearFlags.HasFlag(EWearFlags.Weapon);
            case EquipmentSlot.Necklace:
                return wearFlags.HasFlag(EWearFlags.Necklace);
            case EquipmentSlot.Earring:
                return wearFlags.HasFlag(EWearFlags.Earrings);
            case EquipmentSlot.Costume:
                return proto.IsType(EItemType.Costume) && proto.IsSubtype(EItemSubtype.CostumeBody);
            case EquipmentSlot.Hair:
                return proto.IsType(EItemType.Costume) && proto.IsSubtype(EItemSubtype.CostumeHair);
            default:
                return false;
        }
    }

    public long GetWearPosition(IItemManager itemManager, uint itemId)
    {
        var proto = itemManager.GetItem(itemId);
        if (proto == null)
        {
            return _offset + (ushort)EquipmentSlot.Body;
        }

        var slot = proto.GetWearSlot()!;
        return (long)slot + _offset;
    }
}