using Newtonsoft.Json;
using TShockAPI;

namespace Watcher
{
    /// <summary>
    /// 插件内需要统计的玩家数据，只统计active = true，和违规用户
    /// </summary>
    public class WPlayer
    {
        public string name;
        public string uuid;
        public int 钓鱼作弊次数;//是否钓鱼作弊    type 1
        public int 伤害作弊次数;//是否伤害作弊 type 2
        public int 物品作弊次数;//是否违禁物作弊 type 3
        public int 总作弊次数 { get { return 钓鱼作弊次数 + 伤害作弊次数 + 物品作弊次数; } }//总计作弊次数
        public int 本次进服游玩时间;//计时器
        // 0代表可以检测，检测位的目的是为了防止短时间内检测多次导致封号
        public int 物品检测位;
        public int 伤害检测位;
        public float 危险物检测位;



        public WPlayer(MPlayer mPlayer)
        {
            name = mPlayer.名称;
            uuid = mPlayer.uuid;
            钓鱼作弊次数 = mPlayer.钓鱼作弊次数;
            伤害作弊次数 = mPlayer.伤害作弊次数;
            物品作弊次数 = mPlayer.物品作弊次数;
            本次进服游玩时间 = 0;
            物品检测位 = 0;
            伤害检测位 = 0;
            危险物检测位 = 0;
        }

        public WPlayer(string Name, string UUID, int fish, int dam, int item)
        {
            name = Name;
            uuid = UUID;
            钓鱼作弊次数 = fish;
            伤害作弊次数 = dam;
            物品作弊次数 = item;
            本次进服游玩时间 = 0;
            物品检测位 = 0;
            伤害检测位 = 0;
            危险物检测位 = 0;
        }

        /// <summary>
        /// 用于写入配置文件中的类，主要给用户看的，免得计入一些别的数据进去
        /// </summary>
        public class MPlayer
        {
            public string 名称;
            public string uuid;
            public int 钓鱼作弊次数;//是否钓鱼作弊    type 1
            public int 伤害作弊次数;//是否伤害作弊 type 2
            public int 物品作弊次数;//是否违禁物作弊 type 3
            public int 总作弊次数;//总计作弊次数

            /// <summary>
            /// 必要的，否则反序列化失败
            /// </summary>
            public MPlayer()
            {
                名称 = "";
                uuid = "";
                钓鱼作弊次数 = 0;
                伤害作弊次数 = 0;
                物品作弊次数 = 0;
                总作弊次数 = 0;
            }

            public MPlayer(WPlayer wplayer)
            {
                名称 = wplayer.name;
                uuid = wplayer.uuid;
                钓鱼作弊次数 = wplayer.钓鱼作弊次数;
                伤害作弊次数 = wplayer.伤害作弊次数;
                物品作弊次数 = wplayer.物品作弊次数;
                总作弊次数 = 钓鱼作弊次数 + 伤害作弊次数 + 物品作弊次数;
            }

            public MPlayer(string Name, string UUID, int fish, int dam, int item)
            {
                名称 = Name;
                uuid = UUID;
                钓鱼作弊次数 = fish;
                伤害作弊次数 = dam;
                物品作弊次数 = item;
                总作弊次数 = fish + dam + item;
            }
        }



        public static string configPath = Path.Combine(TShock.SavePath + "/Watcher", "CheatPlayers.json");


        /// <summary>
        /// 从文本文件中获取wPlayers违规人员信息
        /// </summary>
        /// <returns>返回一个wPlayers</returns>
        public static List<WPlayer> LoadConfigFile()
        {
            if (!Directory.Exists(TShock.SavePath))
            {
                Directory.CreateDirectory(TShock.SavePath);
            }
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new List<MPlayer>(), Formatting.Indented));
            }
            List<WPlayer> ls1 = new List<WPlayer>();
            List<MPlayer> ls2;
            try
            {
                ls2 = JsonConvert.DeserializeObject<List<MPlayer>>(File.ReadAllText(configPath));
                foreach (var v in ls2)
                {
                    ls1.Add(new WPlayer(v));
                }
                return ls1;
            }
            catch
            {
                TSPlayer.All.SendWarningMessage("CheatPlayers.json 反序列化失败，可能有填写错误或配置文件需要更新，已对配置文件作了更新处理");
                SaveConfigFile();
                ls1 = Watcher.wPlayers;
                return ls1;
            }
        }

        /// <summary>
        /// 将内存中wPlayers违规人员信息写入文本文件
        /// </summary>
        public static void SaveConfigFile()
        {
            List<MPlayer> mPlayers = new List<MPlayer>();
            foreach (var v in Watcher.wPlayers)
            {
                if (v.总作弊次数 > 0)
                    mPlayers.Add(new MPlayer(v));
            }
            File.WriteAllText(configPath, JsonConvert.SerializeObject(mPlayers, Formatting.Indented));
        }
    }
}
