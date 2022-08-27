using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;

namespace Watcher
{
    public class Config
    {
        static string configPath = Path.Combine(TShock.SavePath + "/Watcher", "WatcherConfig.json");
        public static void SetConfigFile()
        {
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new Config(true, true, true, true, true, 21600, 1,
                    new HashSet<int> { 2, 3, 8, 9, 27, 40, 42, 71, 72, 93, 94, 97, 129, 168, 607, 965, 2119, 2274, 2260, 2261, 2262, 2263, 2264, 2271, 3905, 3908, 4547, 4548, 4564, 4565, 4580, 4962 },
                    new HashSet<int> { 0, 2, 3, 23, 58, 71, 72, 73, 169, 172, 176, 184, 409, 593, 664, 1734, 1735, 1867, 1868 },
                    new HashSet<int> {
                        17, 28, 29, 37, 42, 65, 68, 69, 70, 99, 108, 133, 134, 135, 136,
                        137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149,
                        158, 159, 160, 161, 164, 281, 338, 339, 340, 341, 354, 463, 470, 516,
                        519, 621, 637, 715, 716, 717, 718, 727, 773, 776, 777, 778, 780, 781,
                        782, 784, 785, 786, 787, 788, 789, 790, 791, 792, 793, 794, 795,
                        796, 797, 798, 799, 800, 801, 803, 804, 805, 806, 807, 808, 809,
                        810, 903, 904, 905, 906, 910, 911},
                    true, 20, 2880, true, true, true, 7, new string[] { "default", "vip" },
                    new HashSet<int> {},
                    new List<int> {}
                    ), Formatting.Indented));
            }
        }

        public static Config ReadConfigFile()
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        }

        public Config(bool b0, bool b1, bool b2, bool b3, bool b4, int num1, int num2, HashSet<int> hs1, HashSet<int> hs2, HashSet<int> hs3, bool b5, int num6, int num7, bool b8, bool b9, bool b10, int num8, string[] str1, HashSet<int> hs4, List<int> l1)
        {
            enableChinese_启用中文 = b0;
            whetherToWriteTheConversationContentInTheLog_是否把对话内容写入日志 = b1;
            whetherToWriteTheDiscardsIntoTheLog_是否把丢弃物写入日志 = b2;
            whetherToWriteTheHoldingObjectIntoTheLog_是否把手持物写入日志 = b3;
            whetherToWriteTheProjectilesIntoTheLog_是否把生成射弹写入日志 = b4;

            logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常 = num1;
            maxMBofLogAndCheatLog_日志和作弊日志文件的最大MB = num2;

            ImmunityHoldItemID_拿持日志中的豁免物品ID = hs1;
            ImmunityDropItemsID_丢弃日志中的豁免物品ID = hs2;
            DangerousProjectileID_射弹日志中需要记录的危险的射弹物ID = hs3;

            backUpTshockSql_是否备份tshockSql = b5;
            backupInterval_备份间隔 = num6;
            backUpTime_备份时长 = num7;

            enableItemDetection_启用物品作弊检测 = b8;
            enableProjDamage_启用射弹伤害检测 = b9;
            enableBobberNum_启用浮标数目检测 = b10;

            numberOfBan_允许的违规次数 = num8;
            needCheckedPlayerGroups_需要被检测的玩家组 = str1;
            ignoreCheckedItemsID_不需要被作弊检查的物品id = hs4;
            ProtectedNPC_被保护的NPC = l1;
        }

        public bool enableChinese_启用中文;
        public bool whetherToWriteTheConversationContentInTheLog_是否把对话内容写入日志;
        public bool whetherToWriteTheDiscardsIntoTheLog_是否把丢弃物写入日志;
        public bool whetherToWriteTheHoldingObjectIntoTheLog_是否把手持物写入日志;
        public bool whetherToWriteTheProjectilesIntoTheLog_是否把生成射弹写入日志;
        public int logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常;
        public int maxMBofLogAndCheatLog_日志和作弊日志文件的最大MB;

        public HashSet<int> ImmunityHoldItemID_拿持日志中的豁免物品ID;
        public HashSet<int> ImmunityDropItemsID_丢弃日志中的豁免物品ID;
        public HashSet<int> DangerousProjectileID_射弹日志中需要记录的危险的射弹物ID;

        public bool backUpTshockSql_是否备份tshockSql;
        public int backupInterval_备份间隔;
        public int backUpTime_备份时长;

        public bool enableItemDetection_启用物品作弊检测;
        public bool enableProjDamage_启用射弹伤害检测;
        public bool enableBobberNum_启用浮标数目检测;
        public int numberOfBan_允许的违规次数;
        public string[] needCheckedPlayerGroups_需要被检测的玩家组;
        public HashSet<int> ignoreCheckedItemsID_不需要被作弊检查的物品id;
        public List<int> ProtectedNPC_被保护的NPC;
    }
}
