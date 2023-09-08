using Newtonsoft.Json;
using Terraria;
using TShockAPI;

namespace Watcher;

public class Config
{
    public static readonly string configPath = Path.Combine(TShock.SavePath + "/Watcher", "WatcherConfig.json");

    public static Config LoadConfigFile()
    {
        if (!Directory.Exists(TShock.SavePath + "/Watcher"))
        {
            Directory.CreateDirectory(TShock.SavePath + "/Watcher");
        }
        if (!File.Exists(configPath))
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(new Config(0), Formatting.Indented));
        }
        Config c;
        try
        {
            c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            var temp = c.不需要被作弊检查的物品ID.Keys.ToArray();
            foreach (int i in temp)
            {
                c.不需要被作弊检查的物品ID[i] = Lang.GetItemNameValue(i);
            }
            temp = c.必须被检查的物品_覆盖上面一条.Keys.ToArray();
            foreach (int i in temp)
            {
                c.必须被检查的物品_覆盖上面一条[i] = Lang.GetItemNameValue(i);
            }
            temp = c.拿持日志中的豁免物品ID.Keys.ToArray();
            foreach (int i in temp)
            {
                c.拿持日志中的豁免物品ID[i] = Lang.GetItemNameValue(i);
            }
            temp = c.丢弃日志中的豁免物品ID.Keys.ToArray();
            foreach (int i in temp)
            {
                c.丢弃日志中的豁免物品ID[i] = Lang.GetItemNameValue(i);
            }
            temp = c.射弹日志中需要记录的危险的射弹物ID.Keys.ToArray();
            foreach (int i in temp)
            {
                c.射弹日志中需要记录的危险的射弹物ID[i] = Lang.GetProjectileName(i).Value;
            }
        }
        catch
        {
            TSPlayer.All.SendWarningMessage("WatcherConfig.json 反序列化失败，可能有填写错误或配置文件需要更新，已对配置文件作了更新处理");
            Console.WriteLine("WatcherConfig.json 反序列化失败，可能有填写错误或配置文件需要更新，已对配置文件作了更新处理");
            Watcher.config.SaveConfigFile();
            c = Watcher.config;
        }
        return c;
    }

    public void SaveConfigFile()
    {
        if (!Directory.Exists(TShock.SavePath + "/Watcher"))
        {
            Directory.CreateDirectory(TShock.SavePath + "/Watcher");
        }
        File.WriteAllText(configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }


    public Config()
    {
        最多违规作弊次数 = 10;
        检测哪些玩家组 = new HashSet<string>();


        启用物品作弊检测 = true;
        单人物品检测概率 = 0.2;
        全员物品检测时间间隔_单位秒 = 320;
        不需要被作弊检查的物品ID = new Dictionary<int, string>();
        必须被检查的物品_覆盖上面一条 = new Dictionary<int, string>();
        物品作弊警告方式 = 3;
        物品作弊是否算违规 = true;
        检测到违禁物时是否清空 = false;


        启用伤害检测 = true;
        射弹最大伤害 = 5000;
        其他最大伤害 = 5000;
        伤害作弊警告方式 = 3;
        伤害作弊是否算违规 = true;


        启用浮标数目检测 = true;
        最大浮标数目 = 1;
        钓鱼作弊警告方式 = 3;
        钓鱼作弊是否算违规 = true;


        危险射弹广播警告 = true;
        生成危险射弹的频率_次每秒 = 1;
        生成危险射弹警告方式 = 1;


        是否禁用肉前恶魔心饰品栏 = true;


        是否把对话内容写入日志 = true;
        是否把丢弃物写入日志 = true;
        是否把手持物写入日志 = true;
        是否把生成射弹写入日志 = true;


        Watcher日志的备份时长_单位分钟 = 21600;
        //设置这个的目的是如果都写入tshock自带的日志的话，体积过大很容易乱码，我不知道为什么，干脆自己弄个别的日志限制大小了
        Watcher日志的最大体积_MB = 10;
        拿持日志中的豁免物品ID = new Dictionary<int, string>();
        丢弃日志中的豁免物品ID = new Dictionary<int, string>();
        射弹日志中需要记录的危险的射弹物ID = new Dictionary<int, string>();
    }

    public Config(short n)
    {
        SetConfig();
    }

    private void SetConfig()
    {
        最多违规作弊次数 = 10;
        检测哪些玩家组 = new HashSet<string> { "default", "vip" };


        启用物品作弊检测 = true;
        单人物品检测概率 = 0.2;
        全员物品检测时间间隔_单位秒 = 320;
        不需要被作弊检查的物品ID = new Dictionary<int, string>();
        必须被检查的物品_覆盖上面一条 = new Dictionary<int, string>();
        物品作弊警告方式 = 3;
        物品作弊是否算违规 = true;
        检测到违禁物时是否清空 = false;


        启用伤害检测 = true;
        射弹最大伤害 = 5000;
        其他最大伤害 = 5000;
        伤害作弊警告方式 = 3;
        伤害作弊是否算违规 = true;


        启用浮标数目检测 = true;
        最大浮标数目 = 1;
        钓鱼作弊警告方式 = 3;
        钓鱼作弊是否算违规 = true;


        危险射弹广播警告 = true;
        生成危险射弹的频率_次每秒 = 1;
        生成危险射弹警告方式 = 1;




        是否禁用肉前恶魔心饰品栏 = true;


        是否把对话内容写入日志 = true;
        是否把丢弃物写入日志 = true;
        是否把手持物写入日志 = true;
        是否把生成射弹写入日志 = true;


        Watcher日志的备份时长_单位分钟 = 21600;
        Watcher日志的最大体积_MB = 10;

        var temp = new HashSet<int> {
            2, 3, 8, 9, 27, 40, 42, 71, 72, 93, 94, 97, 129, 168, 607, 965, 2119, 2274, 2260, 2261, 2262, 2263, 2264, 2271, 3905, 3908, 4547, 4548, 4564, 4565, 4580, 4962
        };
        foreach (var v in temp)
        {
            拿持日志中的豁免物品ID.TryAdd(v, Lang.GetItemNameValue(v));
        }
        temp = new HashSet<int>{
            0, 2, 3, 23, 58, 71, 72, 73, 169, 172, 176, 184, 409, 593, 664, 1734, 1735, 1867, 1868
        };
        foreach (var v in temp)
        {
            丢弃日志中的豁免物品ID.TryAdd(v, Lang.GetItemNameValue(v));
        }
        temp = new HashSet<int> {
            10, 11, 17, 28, 29, 37, 42, 65, 68, 69, 70, 80, 99, 108, 136,
            137, 138, 142, 143, 144, 145, 146, 147, 148, 149,
            158, 159, 160, 161, 164, 281, 339, 341, 354, 463, 470, 516,
            519, 621, 637, 715, 716, 717, 718, 727, 773, 780, 781,
            782, 783, 784, 785, 786, 787, 788, 789, 790, 791, 792,
            796, 797, 798, 799, 800, 801, 804, 805, 806, 807, 809,
            810, 863, 868, 869, 903, 904, 905, 906, 910, 911, 1015, 1016, 1017
        };
        foreach (var v in temp)
        {
            射弹日志中需要记录的危险的射弹物ID.TryAdd(v, Lang.GetProjectileName(v).Value);
        }
    }


    public int 最多违规作弊次数;
    public HashSet<string> 检测哪些玩家组 = new HashSet<string>();


    public bool 启用物品作弊检测;
    public double 单人物品检测概率;
    public int 全员物品检测时间间隔_单位秒;
    public Dictionary<int, string> 不需要被作弊检查的物品ID = new Dictionary<int, string>();
    public Dictionary<int, string> 必须被检查的物品_覆盖上面一条 = new Dictionary<int, string>();
    [JsonProperty("物品作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出")]
    public int 物品作弊警告方式;
    public bool 物品作弊是否算违规;
    public bool 检测到违禁物时是否清空;


    public bool 启用伤害检测;
    public int 射弹最大伤害;
    public int 其他最大伤害;
    [JsonProperty("伤害作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出")]
    public int 伤害作弊警告方式;
    public bool 伤害作弊是否算违规;


    public bool 启用浮标数目检测;
    public int 最大浮标数目;
    [JsonProperty("钓鱼作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出")]
    public int 钓鱼作弊警告方式;
    public bool 钓鱼作弊是否算违规;


    public bool 危险射弹广播警告;
    public int 生成危险射弹的频率_次每秒;
    [JsonProperty("生成危险射弹警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出")]
    public int 生成危险射弹警告方式;


    public bool 是否禁用肉前恶魔心饰品栏;


    public bool 是否把对话内容写入日志;
    public bool 是否把丢弃物写入日志;
    public bool 是否把手持物写入日志;
    public bool 是否把生成射弹写入日志;


    public int Watcher日志的备份时长_单位分钟;
    public int Watcher日志的最大体积_MB;


    public Dictionary<int, string> 拿持日志中的豁免物品ID = new Dictionary<int, string>();
    public Dictionary<int, string> 丢弃日志中的豁免物品ID = new Dictionary<int, string>();
    public Dictionary<int, string> 射弹日志中需要记录的危险的射弹物ID = new Dictionary<int, string>();
}
