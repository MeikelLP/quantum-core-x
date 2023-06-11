using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;

namespace QuantumCore.API.Game.World
{
    public interface IPlayerEntity : IEntity
    {
        string Name { get; }
        IGameConnection Connection { get; }
        PlayerData Player { get; }
        byte Empire { get; }
        IInventory Inventory { get; }
        IEntity Target { get; set; }
        IList<Guid> Groups { get; }
        IShop Shop { get; set; }
        IQuickSlotBar QuickSlotBar { get; }
        IQuest CurrentQuest { get; set; }
        Dictionary<string, IQuest> Quests { get; }
        EAntiFlags AntiFlagClass { get; }
        EAntiFlags AntiFlagGender { get; }
        
        Task Load();
        T GetQuestInstance<T>() where T : IQuest;
        Task Respawn(bool town);
        uint CalculateAttackDamage(uint baseDamage);
        uint GetHitRate();
        ValueTask AddPoint(EPoints point, int value);
        ValueTask SetPoint(EPoints point, uint value);
        Task DropItem(ItemInstance item, byte count);
        Task Pickup(IGroundItem groundItem);
        Task DropGold(uint amount);
        ItemInstance GetItem(byte window, ushort position);
        bool IsSpaceAvailable(ItemInstance item, byte window, ushort position);
        bool IsEquippable(ItemInstance item);
        Task<bool> DestroyItem(ItemInstance item);
        Task RemoveItem(ItemInstance item);
        Task SetItem(ItemInstance item, byte window, ushort position);
        Task SendBasicData();
        Task SendPoints();
        Task SendInventory();
        Task SendItem(ItemInstance item);
        Task SendRemoveItem(byte window, ushort position);
        Task SendCharacter(IConnection connection);
        Task SendCharacterAdditional(IConnection connection);
        Task SendCharacterUpdate();
        Task SendChatMessage(string message);
        Task SendChatCommand(string message);
        Task SendChatInfo(string message);
        Task SendTarget();
        Task Show(IConnection connection);
        void Disconnect();
        string ToString();
    }
}