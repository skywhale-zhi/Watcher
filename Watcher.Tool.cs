using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using Terraria.Localization;
using System.Diagnostics;
using Terraria.ID;
using System.Data;
using TShockAPI.DB;
using System.Collections;

namespace Watcher
{
    public partial class Watcher : TerrariaPlugin
    {
        //获取从2020.1.1日到现在经过的秒数
        public static long getNowTimeSecond()
        {
            DateTime centuryBegin = new DateTime(2020, 1, 1);
            long elapsedTicks = DateTime.Now.Ticks - centuryBegin.Ticks;
            return (elapsedTicks / 10000000L);
        }

        //获取从2020.1.1日到现在经过的嘀嗒数,60 tick == 1s
        public static long getNowTimeTicks()
        {
            DateTime centuryBegin = new DateTime(2020, 1, 1);
            long elapsedTicks = DateTime.Now.Ticks - centuryBegin.Ticks;
            return (elapsedTicks / 166667L);
        }


        //备份tshock.sqlite文件
        private void BackUpTshockSql()
        {
            if ((getNowTimeSecond() % (60 * config.backupInterval_备份间隔)) != 0)
            {
                return;
            }

            string tDirPath = Path.Combine(TShock.SavePath + "/Watcher/tshock_backups");
            string tFilePath = Path.Combine(tDirPath, DateTime.Now.ToString("u").Replace(":", "-") + "_tshock.sqlite");
            if (!Directory.Exists(tDirPath))
                Directory.CreateDirectory(tDirPath);
            if (File.Exists(tFilePath))
                return;
            File.Copy(TShock.SavePath + "/tshock.sqlite", tFilePath, false);

            DeleteOldFiles(tDirPath, config.backUpTime_备份时长);

            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [{tFilePath}] 已保存" });
        }


        //物品作弊检测方法
        private void ItemCheatingCheck(EventArgs args, object sender, GetDataHandlers.PlayerSlotEventArgs e, int Model = 0)
        {
            //Model == 1 , 5分钟在线全员检测一次
            if (Model == 1 && getNowTimeSecond() % 300 == 0) //大致5分钟
            {
                foreach (TSPlayer v in TShock.Players)
                {
                    if (!KickOrBanGroupAllow(v))
                    {
                        continue;
                    }

                    List<Item> items = new List<Item>();
                    items.AddRange(v.TPlayer.inventory);
                    items.AddRange(v.TPlayer.armor);
                    items.AddRange(v.TPlayer.dye);
                    items.AddRange(v.TPlayer.miscEquips);
                    items.AddRange(v.TPlayer.miscDyes);
                    items.Add(v.TPlayer.trashItem);
                    items.AddRange(v.TPlayer.bank.item);
                    items.AddRange(v.TPlayer.bank2.item);
                    items.AddRange(v.TPlayer.bank3.item);
                    items.AddRange(v.TPlayer.bank4.item);

                    items.RemoveAll(i => i.IsAir);

                    foreach (Item i in items)
                    {
                        //如果是豁免物品并且不在强制检查物里，不算作弊
                        if (config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Contains(i.type) && !config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Contains(i.type))
                        {
                            continue;
                        }
                        bool isCheat = false;
                        //如果是强制检查物，直接结束下面的检查过程
                        if (config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else
                        {
                            if (!NPC.downedAncientCultist && CheatData.AfterLunaticCultist.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!Main.hardMode && CheatData.AfterHardBeforeOneOfThree.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!NPC.downedFishron && CheatData.DukeFishron.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!NPC.downedEmpressOfLight && CheatData.EmpressofLight.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!NPC.downedBoss3 && CheatData.AfterBoss3BeforeHard.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!NPC.downedMechBossAny && CheatData.AfterAnyMechanics.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) && CheatData.AfterMechanicsBeforePlantera.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!NPC.downedPlantBoss && CheatData.AfterPlanteraBeforeGolem.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            else if (!NPC.downedGolemBoss && CheatData.AfterGolemBeforeCultist.Contains(i.type))
                            {
                                isCheat = true;
                            }
                            //这里考虑10周年蒸汽朋克的情况
                            else if (!NPC.downedMechBossAny && (!Main.tenthAnniversaryWorld && (CheatData.AfterMechanicsBeforePlantera.Contains(i.type) || CheatData.Steampunker.Contains(i.type)) || Main.tenthAnniversaryWorld && CheatData.AfterMechanicsBeforePlantera.Contains(i.type)))
                            {
                                isCheat = true;
                            }
                            //高射速子弹和肉后，三王后，十周年的关系(怎么这么乱)
                            else if (!Main.hardMode && Main.tenthAnniversaryWorld && i.type == 1302 || !NPC.downedMechBossAny && i.type == 1302)
                            {
                                isCheat = true;
                            }
                        }
                        if (isCheat)
                        {
                            int num = CheatingPlayers(v, 3);
                            Console.WriteLine($" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num}");
                            AvoidLogSize("logDirPath", logDirPath, logFilePath);
                            AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                            if (num < config.numberOfBan_允许的违规次数)
                            {
                                v.Kick($"携带不属于这个时期的物品或系统设置违禁物：{i.Name} [id:{i.type}]\n违规次数 {num}，若有疑问请及时向管理员联系");
                                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                            }
                            else
                            {
                                v.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员");
                                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                            }
                        }
                    }
                }
            }

            //Model == 2 , 单人触发检测(每次拿取时触发)
            if (Model == 2)
            {
                if (!KickOrBanGroupAllow(e.Player))
                {
                    return;
                }
                List<Item> items = new List<Item>();
                items.AddRange(e.Player.TPlayer.inventory);
                items.AddRange(e.Player.TPlayer.armor);
                items.AddRange(e.Player.TPlayer.dye);
                items.AddRange(e.Player.TPlayer.miscEquips);
                items.AddRange(e.Player.TPlayer.miscDyes);
                items.Add(e.Player.TPlayer.trashItem);
                items.AddRange(e.Player.TPlayer.bank.item);
                items.AddRange(e.Player.TPlayer.bank2.item);
                items.AddRange(e.Player.TPlayer.bank3.item);
                items.AddRange(e.Player.TPlayer.bank4.item);

                items.RemoveAll(i => i.IsAir);

                foreach (Item i in items)
                {
                    //如果是豁免物品并且不属于强制检查物，不算作弊
                    if (config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Contains(i.type) && !config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Contains(i.type))
                    {
                        continue;
                    }
                    bool isCheat = false;
                    if (config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Contains(i.type))
                    {
                        isCheat = true;
                    }
                    else
                    {
                        if (!NPC.downedAncientCultist && CheatData.AfterLunaticCultist.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!Main.hardMode && CheatData.AfterHardBeforeOneOfThree.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!NPC.downedFishron && CheatData.DukeFishron.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!NPC.downedEmpressOfLight && CheatData.EmpressofLight.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!NPC.downedBoss3 && CheatData.AfterBoss3BeforeHard.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!NPC.downedMechBossAny && CheatData.AfterAnyMechanics.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) && CheatData.AfterMechanicsBeforePlantera.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!NPC.downedPlantBoss && CheatData.AfterPlanteraBeforeGolem.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        else if (!NPC.downedGolemBoss && CheatData.AfterGolemBeforeCultist.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        //这里考虑10周年蒸汽朋克的情况
                        else if (!NPC.downedMechBossAny && (!Main.tenthAnniversaryWorld && (CheatData.AfterMechanicsBeforePlantera.Contains(i.type) || CheatData.Steampunker.Contains(i.type)) || Main.tenthAnniversaryWorld && CheatData.AfterMechanicsBeforePlantera.Contains(i.type)))
                        {
                            isCheat = true;
                        }
                        //高射速子弹和肉后，三王后，十周年的关系(怎么这么乱)
                        else if (!Main.hardMode && Main.tenthAnniversaryWorld && i.type == 1302 || !NPC.downedMechBossAny && i.type == 1302)
                        {
                            isCheat = true;
                        }
                    }
                    if (isCheat)
                    {
                        int num = CheatingPlayers(e.Player, 3);
                        Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num}");
                        AvoidLogSize("logDirPath", logDirPath, logFilePath);
                        AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                        if (num < config.numberOfBan_允许的违规次数)
                        {
                            e.Player.Kick($"携带不属于这个时期的物品或系统设置违禁物：{i.Name} [id:{i.type}]\n违规次数 {num}，若有疑问请及时向管理员联系");
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
                            File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
                        }
                        else
                        {
                            e.Player.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员");
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                            File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                        }
                    }
                }
            }
        }


        //创建相关文件夹和文件
        private void SetWatcherFile(string dirPathName, string dirPath)
        {
            string filePath = "";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            if (dirPathName == "logDirPath")
            {
                filePath = DateTime.Now.ToString("s") + ".log";
                filePath = filePath.Replace(":", "-");
                logFilePath = Path.Combine(dirPath, filePath);
                filePath = logFilePath;
            }
            else if (dirPathName == "cheatLogDirPath")
            {
                filePath = DateTime.Now.ToString("s") + "_Cheat.log";
                filePath = filePath.Replace(":", "-");
                cheatLogFilePath = Path.Combine(dirPath, filePath);
                filePath = cheatLogFilePath;
            }

            if (!File.Exists(filePath))
            {
                File.CreateText(filePath).Close();
            }
        }


        //从dir文件夹里删除超过min时常的旧文件
        private static void DeleteOldFiles(string dir, int min)
        {
            try
            {
                if (!Directory.Exists(dir) || min < 1)
                    return;
                var now = DateTime.Now;
                foreach (var f in Directory.GetFileSystemEntries(dir).Where(f => File.Exists(f)))
                {
                    var t = File.GetLastWriteTime(f);
                    var elapsedTicks = now.Ticks - t.Ticks;
                    var elapsedTime = new TimeSpan(elapsedTicks);
                    if (elapsedTime.TotalMinutes > min)
                    {
                        File.Delete(f);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                TShock.Log.Error(e.Message);
            }
        }


        //当日志过大时重新创建日志
        private void AvoidLogSize(string dirPathName, string dirPath, string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 1024 * 1024 * config.maxMBofLogAndCheatLog_日志和作弊日志文件的最大MB)
                {
                    SetWatcherFile(dirPathName, dirPath);
                }
            }
            else
            {
                File.Create(filePath).Close();
            }
        }


        //定期日志清理，不要让日志过多
        private void LogClean()
        {
            if (getNowTimeSecond() % 7200 == 0)   //两小时清理一次
            {
                DeleteOldFiles(logDirPath, config.logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常);
                DeleteOldFiles(cheatLogDirPath, config.logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常);
            }
        }


        //作弊人员的信息记录
        /// <summary>
        /// 计算总计作弊次数，每调用一次，作弊次数+1
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cheatType">作弊类型 1：钓鱼作弊，2：射弹伤害作弊，3：物品作弊，4: 星璇机枪作弊</param>，
        /// <returns>返回计算后的作弊次数</returns>
        private int CheatingPlayers(TSPlayer e, int cheatType)
        {
            int cheatTimes = 0;
            foreach (WPlayer w in wPlayers)
            {
                if (w.uuid == e.UUID)
                {
                    w.cheatingTimes++;
                    switch (cheatType)
                    {
                        case 1:
                            w.isFishCheat = true;
                            break;
                        case 2:
                            w.isProjDamageCheat = true;
                            break;
                        case 3:
                            w.isItemCheat = true;
                            break;
                        default:
                            break;
                    }
                    cheatTimes = w.cheatingTimes;
                    if (w.cheatingTimes >= config.numberOfBan_允许的违规次数)
                    {
                    }
                    break;
                }
            }
            return cheatTimes;
        }

        /// <summary>
        /// 返回这个玩家的作弊次数
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private int getCheatTimes(TSPlayer e)
        {
            foreach (WPlayer w in wPlayers)
            {
                if (w.uuid == e.UUID)
                {
                    return w.cheatingTimes;
                }
            }
            return 0;
        }


        //kick或Ban人时对所选择的组进行区别。返回是否被踢办或处理
        private bool KickOrBanGroupAllow(TSPlayer v)
        {
            bool flag = false;
            foreach (string str in config.needCheckedPlayerGroups_需要被检测的玩家组)
            {
                if (v != null && v.Group == TShock.Groups.GetGroupByName(str))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
