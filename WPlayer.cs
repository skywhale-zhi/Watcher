using TShockAPI;

namespace Watcher
{
    public class WPlayer
    {
        //通过uuid强制共享作弊数据，别想换号躲系统判定
        public TSPlayer tsplayer;//玩家
        public string name;
        public string uuid;
        public bool isFishCheat;//是否钓鱼作弊    type 1
        public bool isProjDamageCheat;//是否射弹伤害作弊 type 2
        public bool isItemCheat;//是否违禁物作弊 type 3
        public int cheatingTimes;//总计作弊次数
        public double Timer = 0;//计时器

        public WPlayer(TSPlayer ts)
        {
            tsplayer = ts;
            name = ts.Name;
            uuid = ts.UUID;
            isFishCheat = false;
            isProjDamageCheat = false;
            isItemCheat = false;
            cheatingTimes = 0;
        }
    }
}
