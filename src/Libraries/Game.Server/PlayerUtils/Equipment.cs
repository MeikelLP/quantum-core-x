using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.PlayerUtils
{
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
            switch ((EquipmentSlots)(position - _offset))
            {
                case EquipmentSlots.Body:
                    Body = item;
                    return true;
                case EquipmentSlots.Head:
                    Head = item;
                    return true;
                case EquipmentSlots.Shoes:
                    Shoes = item;
                    return true;
                case EquipmentSlots.Bracelet:
                    Bracelet = item;
                    return true;
                case EquipmentSlots.Weapon:
                    Weapon = item;
                    return true;
                case EquipmentSlots.Necklace:
                    Necklace = item;
                    return true;
                case EquipmentSlots.Earring:
                    Earrings = item;
                    return true;
                case EquipmentSlots.Costume:
                    Costume = item;
                    return true;
                case EquipmentSlots.Hair:
                    Hair = item;
                    return true;
            }

            return false;
        }

        public ItemInstance? GetItem(EquipmentSlots slot)
        {
            switch (slot)
            {
                case EquipmentSlots.Body:
                    return Body;
                case EquipmentSlots.Head:
                    return Head;
                case EquipmentSlots.Shoes:
                    return Shoes;
                case EquipmentSlots.Bracelet:
                    return Bracelet;
                case EquipmentSlots.Weapon:
                    return Weapon;
                case EquipmentSlots.Necklace:
                    return Necklace;
                case EquipmentSlots.Earring:
                    return Earrings;
                case EquipmentSlots.Costume:
                    return Costume;
                case EquipmentSlots.Hair:
                    return Hair;
            }

            return null;
        }

        public ItemInstance? GetItem(ushort position)
        {
            return GetItem((EquipmentSlots)(position - _offset));
        }

        public bool RemoveItem(ItemInstance item)
        {
            switch ((EquipmentSlots)(item.Position - _offset))
            {
                case EquipmentSlots.Body:
                    Body = null;
                    return true;
                case EquipmentSlots.Head:
                    Head = null;
                    return true;
                case EquipmentSlots.Shoes:
                    Shoes = null;
                    return true;
                case EquipmentSlots.Bracelet:
                    Bracelet = null;
                    return true;
                case EquipmentSlots.Weapon:
                    Weapon = null;
                    return true;
                case EquipmentSlots.Necklace:
                    Necklace = null;
                    return true;
                case EquipmentSlots.Earring:
                    Earrings = null;
                    return true;
                case EquipmentSlots.Costume:
                    Costume = null;
                    return true;
                case EquipmentSlots.Hair:
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

            switch ((EquipmentSlots)(position - _offset))
            {
                case EquipmentSlots.Body:
                    return wearFlags.HasFlag(EWearFlags.Body);
                case EquipmentSlots.Head:
                    return wearFlags.HasFlag(EWearFlags.Head);
                case EquipmentSlots.Shoes:
                    return wearFlags.HasFlag(EWearFlags.Shoes);
                case EquipmentSlots.Bracelet:
                    return wearFlags.HasFlag(EWearFlags.Bracelet);
                case EquipmentSlots.Weapon:
                    return wearFlags.HasFlag(EWearFlags.Weapon);
                case EquipmentSlots.Necklace:
                    return wearFlags.HasFlag(EWearFlags.Necklace);
                case EquipmentSlots.Earring:
                    return wearFlags.HasFlag(EWearFlags.Earrings);
                case EquipmentSlots.Costume:
                    return proto.IsType(EItemType.Costume) && proto.IsSubtype(EItemSubtype.CostumeBody);
                case EquipmentSlots.Hair:
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
                return _offset + (ushort)EquipmentSlots.Body;
            }

            var slot = proto.GetWearSlot()!;
            return (long)slot + _offset;
        }
    }
}
