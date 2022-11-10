using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TShockAPI;

namespace Watcher
{
    public class WPM
    {
        public static string configPath = Path.Combine(TShock.SavePath + "/Watcher", "CheatPlayers.json");

        /// <summary>
        /// 从文本文件中获取wPlayers违规人员信息
        /// </summary>
        /// <returns>返回一个wPlayers</returns>
        public static List<WPlayer> LoadConfigFile()
        {
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new WPlayerCollect(), Formatting.Indented));
            }

            List<wplayer> ls = JsonConvert.DeserializeObject<WPlayerCollect>(File.ReadAllText(configPath)).Message;
            List<WPlayer> ls2 = new List<WPlayer>();
            foreach (wplayer w in ls)
            {
                ls2.Add(w.TurnToWPlayer());
            }
            return ls2;
        }

        /// <summary>
        /// 将内存中wPlayers违规人员信息写入文本文件
        /// </summary>
        public static void SaveConfigFile()
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(new WPlayerCollect(0), Formatting.Indented));
        }
    }

    public class WPlayerCollect
    {
        public List<wplayer> Message = new List<wplayer>();

        public WPlayerCollect()
        {
            Message = new List<wplayer>();
        }

        //这个构造方法用于写入配置文件的构造，参数无意义只是为了区分无参构造函数而已
        public WPlayerCollect(int x)
        {
            foreach (WPlayer W in Watcher.wPlayers)
            {
                Message.Add(new wplayer(W));
            }
        }
    }

    public class wplayer
    {
        public wplayer()
        {}

        public wplayer(string name, string uuid, bool isf, bool isd, bool isi, bool isv, int ct)
        {
            名称 = name;
            UUID = uuid;
            是否钓鱼作弊 = isf;
            是否伤害作弊 = isd;
            是否物品作弊 = isi;
            是否星璇作弊 = isv;
            总违规次数 = ct;
        }
        public wplayer(WPlayer W)
        {
            名称 = W.name;
            UUID = W.uuid;
            是否钓鱼作弊 = W.isFishCheat;
            是否伤害作弊 = W.isDamageCheat;
            是否物品作弊 = W.isItemCheat;
            是否星璇作弊 = W.isVortexCheat;
            总违规次数 = W.cheatingTimes;
        }

        public string 名称;
        public string UUID;
        public bool 是否钓鱼作弊;
        public bool 是否伤害作弊;
        public bool 是否物品作弊;
        public bool 是否星璇作弊;
        public int 总违规次数;

        public WPlayer TurnToWPlayer()
        {
            WPlayer w = new WPlayer(名称, UUID, 是否钓鱼作弊, 是否伤害作弊, 是否物品作弊, 是否星璇作弊, 总违规次数);
            return w;
        }
    }
}
