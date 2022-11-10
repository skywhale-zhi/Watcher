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
        public bool isDamageCheat;//是否伤害作弊 type 2
        public bool isItemCheat;//是否违禁物作弊 type 3
        public bool isVortexCheat;//是否星璇作弊 type 4
        public int cheatingTimes;//总计作弊次数
        public double Timer = 0;//计时器

        public WPlayer(TSPlayer ts)
        {
            tsplayer = ts;
            name = ts.Name;
            uuid = ts.UUID;
            isFishCheat = false;
            isDamageCheat = false;
            isItemCheat = false;
            isVortexCheat = false;
            cheatingTimes = 0;
        }
        public WPlayer(string Name, string UUID, bool isf, bool isd, bool isi, bool isv, int ct)
        {
            tsplayer = null;
            name = Name;
            uuid = UUID;
            isFishCheat = isf;
            isDamageCheat = isd;
            isItemCheat = isi;
            isVortexCheat = isv;
            cheatingTimes = ct;
        }
    }
}
