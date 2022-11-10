using System;
using Terraria;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;
using static Watcher.Watcher;

namespace Watcher
{
    public class Config
    {
        static string configPath = Path.Combine(TShock.SavePath + "/Watcher", "WatcherConfig.json");
        public static Config LoadConfigFile()
        {
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new Config(true,
                    true, 20, 2880,
                    true, 0.5, 300, new HashSet<int> { }, new HashSet<int> { }, 3, true,
                    true, 3000, 3, true,
                    true, 1, 3, true,
                    true, 3, true,
                    true,
                    10, new List<string> { "default", "vip" },
                    new List<int> { },
                    true, true, true, true, 21600, 5,
                    new HashSet<int> { 2, 3, 8, 9, 27, 40, 42, 71, 72, 93, 94, 97, 129, 168, 607, 965, 2119, 2274, 2260, 2261, 2262, 2263, 2264, 2271, 3905, 3908, 4547, 4548, 4564, 4565, 4580, 4962 },
                    new HashSet<int> { 0, 2, 3, 23, 58, 71, 72, 73, 169, 172, 176, 184, 409, 593, 664, 1734, 1735, 1867, 1868 },
                    new HashSet<int> {
                        10, 11, 17, 28, 29, 37, 42, 65, 68, 69, 70, 80, 99, 108, 133, 134, 135, 136,
                        137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149,
                        158, 159, 160, 161, 164, 281, 338, 339, 340, 341, 354, 370, 371, 463, 470, 516,
                        519, 621, 637, 715, 716, 717, 718, 727, 773, 776, 777, 778, 779, 780, 781,
                        782, 783, 784, 785, 786, 787, 788, 789, 790, 791, 792, 793, 794, 795,
                        796, 797, 798, 799, 800, 801, 803, 804, 805, 806, 807, 808, 809,
                        810, 862, 863, 868, 869, 903, 904, 905, 906, 910, 911,1015,1016,1017}
                    ), Formatting.Indented));
            }

            Config c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            return c;
        }

        public Config(bool 启用中文, bool 是否备份tshockSql, int 备份间隔, int 备份时长,
            bool 启用物品作弊检测, double 物品检测频率, int 全员物品检测间隔时间秒, HashSet<int> 不需要被作弊检查的物品id, HashSet<int> 必须被检查的物品_覆盖上面一条, int 物品作弊警告方式, bool 物品作弊是否计入总违规作弊次数,
            bool 启用射弹伤害检测, int 射弹最大伤害, int 伤害作弊警告方式, bool 伤害作弊是否计入总违规作弊次数,
            bool 启用浮标数目检测, int 最大浮标数目, int 钓鱼作弊警告方式, bool 钓鱼作弊是否计入总违规作弊次数,
            bool 是否启用pe版星璇机枪bug检测, int 星璇机枪作弊警告方式, bool 星璇作弊是否计入总违规作弊次数,
            bool 是否禁用肉前恶魔心饰品栏,
            int 允许的违规次数, List<string> 需要被检测的玩家组, List<int> 被保护的NPC,
            bool 是否把对话内容写入日志, bool 是否把丢弃物写入日志, bool 是否把手持物写入日志, bool 是否把生成射弹写入日志, int 任何日志的备份时常分钟, int 日志和作弊日志文件的最大MB, HashSet<int> 拿持日志中的豁免物品ID, HashSet<int> 丢弃日志中的豁免物品ID, HashSet<int> 射弹日志中需要记录的危险的射弹物ID)

        {
            this.启用中文 = 启用中文;

            this.是否备份tshockSql = 是否备份tshockSql;
            this.tshockSql备份间隔分钟 = 备份间隔;
            this.tshockSql备份时常分钟 = 备份时长;

            this.启用物品作弊检测 = 启用物品作弊检测;
            this.单人物品检测概率 = 物品检测频率;
            this.全员物品检测间隔时间秒 = 全员物品检测间隔时间秒;
            this.不需要被作弊检查的物品ID = 不需要被作弊检查的物品id;
            this.必须被检查的物品_覆盖上面一条 = 必须被检查的物品_覆盖上面一条;
            this.物品作弊警告方式 = 物品作弊警告方式;
            this.物品作弊是否计入总违规作弊次数 = 物品作弊是否计入总违规作弊次数;

            this.启用射弹伤害检测 = 启用射弹伤害检测;
            this.射弹最大伤害 = 射弹最大伤害;
            this.伤害作弊警告方式 = 伤害作弊警告方式;
            this.伤害作弊是否计入总违规作弊次数 = 伤害作弊是否计入总违规作弊次数;

            this.启用浮标数目检测 = 启用浮标数目检测;
            this.最大浮标数目 = 最大浮标数目;
            this.钓鱼作弊警告方式 = 钓鱼作弊警告方式;
            this.钓鱼作弊是否计入总违规作弊次数 = 钓鱼作弊是否计入总违规作弊次数;

            this.是否启用pe版星璇机枪bug检测 = 是否启用pe版星璇机枪bug检测;
            this.星璇机枪作弊警告方式 = 星璇机枪作弊警告方式;
            this.星璇作弊是否计入总违规作弊次数 = 星璇作弊是否计入总违规作弊次数;

            this.是否禁用肉前恶魔心饰品栏 = 是否禁用肉前恶魔心饰品栏;

            this.最多违规作弊次数 = 允许的违规次数;
            this.需要被检测的玩家组 = 需要被检测的玩家组;
            this.被保护的NPC = 被保护的NPC;

            this.是否把对话内容写入日志 = 是否把对话内容写入日志;
            this.是否把丢弃物写入日志 = 是否把丢弃物写入日志;
            this.是否把手持物写入日志 = 是否把手持物写入日志;
            this.是否把生成射弹写入日志 = 是否把生成射弹写入日志;

            this.任何日志的备份时常分钟 = 任何日志的备份时常分钟;
            this.日志和作弊日志文件的最大MB = 日志和作弊日志文件的最大MB;

            this.拿持日志中的豁免物品ID = 拿持日志中的豁免物品ID;
            this.丢弃日志中的豁免物品ID = 丢弃日志中的豁免物品ID;
            this.射弹日志中需要记录的危险的射弹物ID = 射弹日志中需要记录的危险的射弹物ID;
        }

        public bool 启用中文;

        public bool 是否备份tshockSql;
        public int tshockSql备份间隔分钟;
        public int tshockSql备份时常分钟;

        public bool 启用物品作弊检测;
        public double 单人物品检测概率;
        public int 全员物品检测间隔时间秒;
        public HashSet<int> 不需要被作弊检查的物品ID;
        public HashSet<int> 必须被检查的物品_覆盖上面一条;
        public int 物品作弊警告方式;
        public bool 物品作弊是否计入总违规作弊次数;

        public bool 启用射弹伤害检测;
        public int 射弹最大伤害;
        public int 伤害作弊警告方式;
        public bool 伤害作弊是否计入总违规作弊次数;

        public bool 启用浮标数目检测;
        public int 最大浮标数目;
        public int 钓鱼作弊警告方式;
        public bool 钓鱼作弊是否计入总违规作弊次数;

        public bool 是否启用pe版星璇机枪bug检测;
        public int 星璇机枪作弊警告方式;
        public bool 星璇作弊是否计入总违规作弊次数;

        public bool 是否禁用肉前恶魔心饰品栏;

        public int 最多违规作弊次数;
        public List<string> 需要被检测的玩家组;
        public List<int> 被保护的NPC;

        public bool 是否把对话内容写入日志;
        public bool 是否把丢弃物写入日志;
        public bool 是否把手持物写入日志;
        public bool 是否把生成射弹写入日志;
        public int 任何日志的备份时常分钟;
        public int 日志和作弊日志文件的最大MB;

        public HashSet<int> 拿持日志中的豁免物品ID;
        public HashSet<int> 丢弃日志中的豁免物品ID;
        public HashSet<int> 射弹日志中需要记录的危险的射弹物ID;
    }
}
