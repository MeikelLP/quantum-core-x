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
            case EquipmentSlot.BODY:
                Body = item;
                return true;
            case EquipmentSlot.HEAD:
                Head = item;
                return true;
            case EquipmentSlot.SHOES:
                Shoes = item;
                return true;
            case EquipmentSlot.BRACELET:
                Bracelet = item;
                return true;
            case EquipmentSlot.WEAPON:
                Weapon = item;
                return true;
            case EquipmentSlot.NECKLACE:
                Necklace = item;
                return true;
            case EquipmentSlot.EARRING:
                Earrings = item;
                return true;
            case EquipmentSlot.COSTUME:
                Costume = item;
                return true;
            case EquipmentSlot.HAIR:
                Hair = item;
                return true;
        }

        return false;
    }

    public ItemInstance? GetItem(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.BODY:
                return Body;
            case EquipmentSlot.HEAD:
                return Head;
            case EquipmentSlot.SHOES:
                return Shoes;
            case EquipmentSlot.BRACELET:
                return Bracelet;
            case EquipmentSlot.WEAPON:
                return Weapon;
            case EquipmentSlot.NECKLACE:
                return Necklace;
            case EquipmentSlot.EARRING:
                return Earrings;
            case EquipmentSlot.COSTUME:
                return Costume;
            case EquipmentSlot.HAIR:
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
            case EquipmentSlot.BODY:
                Body = null;
                return true;
            case EquipmentSlot.HEAD:
                Head = null;
                return true;
            case EquipmentSlot.SHOES:
                Shoes = null;
                return true;
            case EquipmentSlot.BRACELET:
                Bracelet = null;
                return true;
            case EquipmentSlot.WEAPON:
                Weapon = null;
                return true;
            case EquipmentSlot.NECKLACE:
                Necklace = null;
                return true;
            case EquipmentSlot.EARRING:
                Earrings = null;
                return true;
            case EquipmentSlot.COSTUME:
                Costume = null;
                return true;
            case EquipmentSlot.HAIR:
                Hair = null;
                return true;
        }

        return false;
    }

    public void Send(IPlayerEntity player)
    {
        if (Body is not null)
        {
            player.SendItem(Body);
        }

        if (Head is not null)
        {
            player.SendItem(Head);
        }

        if (Shoes is not null)
        {
            player.SendItem(Shoes);
        }

        if (Bracelet is not null)
        {
            player.SendItem(Bracelet);
        }

        if (Weapon is not null)
        {
            player.SendItem(Weapon);
        }

        if (Necklace is not null)
        {
            player.SendItem(Necklace);
        }

        if (Earrings is not null)
        {
            player.SendItem(Earrings);
        }

        if (Costume is not null)
        {
            player.SendItem(Costume);
        }

        if (Hair is not null)
        {
            player.SendItem(Hair);
        }
    }

    public bool IsSuitable(IItemManager itemManager, ItemInstance item, ushort position)
    {
        var proto = itemManager.GetItem(item.ItemId);
        if (proto is null)
        {
            return false;
        }

        var wearFlags = (EWearFlags)proto.WearFlags;

        switch ((EquipmentSlot)(position - _offset))
        {
            case EquipmentSlot.BODY:
                return wearFlags.HasFlag(EWearFlags.BODY);
            case EquipmentSlot.HEAD:
                return wearFlags.HasFlag(EWearFlags.HEAD);
            case EquipmentSlot.SHOES:
                return wearFlags.HasFlag(EWearFlags.SHOES);
            case EquipmentSlot.BRACELET:
                return wearFlags.HasFlag(EWearFlags.BRACELET);
            case EquipmentSlot.WEAPON:
                return wearFlags.HasFlag(EWearFlags.WEAPON);
            case EquipmentSlot.NECKLACE:
                return wearFlags.HasFlag(EWearFlags.NECKLACE);
            case EquipmentSlot.EARRING:
                return wearFlags.HasFlag(EWearFlags.EARRINGS);
            case EquipmentSlot.COSTUME:
                return proto.IsType(EItemType.COSTUME) && proto.IsSubtype(EItemSubtype.COSTUME_BODY);
            case EquipmentSlot.HAIR:
                return proto.IsType(EItemType.COSTUME) && proto.IsSubtype(EItemSubtype.COSTUME_HAIR);
            default:
                return false;
        }
    }

    public long GetWearPosition(IItemManager itemManager, uint itemId)
    {
        var proto = itemManager.GetItem(itemId);
        if (proto is null)
        {
            return _offset + (ushort)EquipmentSlot.BODY;
        }

        var slot = proto.GetWearSlot()!;
        return (long)slot + _offset;
    }
}
