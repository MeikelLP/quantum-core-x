using System;
using QuantumCore.Database;

namespace QuantumCore.Game.PlayerUtils
{
    public enum EquipmentSlots
    {
        Body,
        Head,
        Shoes,
        Bracelet,
        Weapon,
        Necklace,
        Earring,
        Costume = 19,
        Hair = 20
    }
    
    public class Equipment
    {
        public Guid Owner { get; }
        public Item Body { get; private set; }
        public Item Head { get; private set; }
        public Item Shoes { get; private set; }
        public Item Bracelet { get; private set; }
        public Item Weapon { get; private set; }
        public Item Necklace { get; private set; }
        public Item Earrings { get; private set; }
        public Item Costume { get; private set; }
        public Item Hair { get; private set; }

        private long _offset;

        public Equipment(Guid owner, long offset)
        {
            Owner = owner;
            _offset = offset;
        }

        public bool SetItem(Item item)
        {
            return SetItem(item, (ushort) item.Position);
        }

        public bool SetItem(Item item, ushort position)
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

        public Item GetItem(ushort position)
        {
            switch ((EquipmentSlots) (position - _offset))
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

        public bool RemoveItem(Item item)
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
    }
}