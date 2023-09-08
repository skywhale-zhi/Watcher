using Terraria;
using Terraria.ID;

namespace Watcher;

internal static class Utils
{
    internal static void ClearPlayerItems(IEnumerable<int> itemIDs, Item[] clearList, int startSlot, int playerIndex)
    {
        for (int i = 0; i < clearList.Length; i++)
        {
            ref var checkItem = ref clearList[i];
            if(checkItem.IsAir)
            {
                continue;
            }
            if (itemIDs.Contains(checkItem.type))
            {
                checkItem.TurnToAir();
                NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, playerIndex, startSlot + i);
            }
        }
    }
    internal static void ClearPlayerItem(IEnumerable<int> itemIDs, Item checkItem, int slot, int playerIndex)
    {
        if (!checkItem.IsAir)
        {
            if (itemIDs.Contains(checkItem.type))
            {
                checkItem.TurnToAir();
                NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, playerIndex, slot);
            }
        }
    }
}
