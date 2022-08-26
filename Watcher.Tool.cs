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
        //备份tshock.sqlite文件
        public void BackUpTshockSql()
        {
            if ((int)(Main.timeForVisualEffects % (3600 * config.backupInterval_备份间隔)) != 0)
                return;

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
        public void ItemCheatingCheck(EventArgs args, object sender, GetDataHandlers.PlayerSlotEventArgs e, int Model = 0)
        {
            //Model = 1, 15分钟全员检测一次
            if (Model == 1 && (int)Main.timeForVisualEffects % (60 * 60 * 5) == 0) //大致15分钟 = 54000
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
                        //如果是豁免物品，不算作弊
                        if (config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Contains(i.type))
                        {
                            continue;
                        }
                        bool isCheat = false;
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
                        else if(!NPC.downedMechBossAny && CheatData.AfterMechanicsBeforePlantera.Contains(i.type))
                        {
                            isCheat = true;
                        }
                        //这里考虑10周年蒸汽朋克的情况
                        else if (!NPC.downedMechBossAny && (!Main.tenthAnniversaryWorld && (CheatData.AfterMechanicsBeforePlantera.Contains(i.type) || CheatData.Steampunker.Contains(i.type)) || Main.tenthAnniversaryWorld && CheatData.AfterMechanicsBeforePlantera.Contains(i.type)))
                        {
                            isCheat = true;
                        }

                        if (isCheat)
                        {
                            int num = CheakingPlayers(v, "item");
                            if (num < config.numberOfBan_允许的违规次数)
                            {
                                v.Kick($"携带不属于这个时期的物品：{i.Name} 违规次数 {num}，若有疑问请及时向管理员联系");
                            }
                            else
                            {
                                v.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员", true);
                            }
                            Console.WriteLine($" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品 {i.Name} 在 {v.LastNetPosition / 16} 违规次数 {num}");
                            AvoidLogSize("logDirPath", logDirPath, logFilePath);
                            AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品 {i.Name} 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                            File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品 {i.Name} 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                        }
                    }

                }
            }
            //单人触发检测
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
                    //如果是豁免物品，不算作弊
                    if (config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Contains(i.type))
                    {
                        continue;
                    }
                    bool isCheat = false;
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
                    //这里考虑10周年蒸汽朋克的情况
                    else if (!NPC.downedMechBossAny && (!Main.tenthAnniversaryWorld && (CheatData.AfterMechanicsBeforePlantera.Contains(i.type) || CheatData.Steampunker.Contains(i.type)) || Main.tenthAnniversaryWorld && CheatData.AfterMechanicsBeforePlantera.Contains(i.type)))
                    {
                        isCheat = true;
                    }

                    if (isCheat)
                    {
                        int num = CheakingPlayers(e.Player, "item");
                        if (num < config.numberOfBan_允许的违规次数)
                        {
                            e.Player.Kick($"携带不属于这个时期的物品：{i.Name} 违规次数 {num}，若有疑问请及时向管理员联系");
                        }
                        else
                        {
                            e.Player.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员", true);
                        }
                        Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品 {i.Name} 在 {e.Player.LastNetPosition / 16} 违规次数 {num}");
                        AvoidLogSize("logDirPath", logDirPath, logFilePath);
                        AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                        File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品 {i.Name} 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
                        File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品 {i.Name} 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
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
            //自动删除15d前的旧日志，每次启动游戏时
            //DeleteOldFiles(dirPath, config.logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常);
        }


        //从dir文件夹里删除超过min时常的旧文件
        public static void DeleteOldFiles(string dir, int min)
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
                Console.WriteLine(e.ToString());
            }
        }


        //当日志过大时重新创建日志
        public void AvoidLogSize(string dirPathName, string dirPath, string filePath)
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
            if (Main.timeForVisualEffects == 100 || Main.timeForVisualEffects % (3600 * 60 * 5) == 0)
            {
                DeleteOldFiles(logDirPath, config.logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常);
                DeleteOldFiles(cheatLogDirPath, config.logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常);
            }
        }


        //作弊人员的信息记录
        private int CheakingPlayers(TSPlayer e, string cheakType)
        {
            int cheakTimes = 0;
            if (OnlineCheakingPlayers.Any())
            {
                for (int i = 0; i < OnlineCheakingPlayers.Count; i++)
                {
                    if (OnlineCheakingPlayers[i][0] == e.Name)
                    {
                        OnlineCheakingPlayers[i][1] = (int.Parse(OnlineCheakingPlayers[i][1]) + 1).ToString();
                        if (!OnlineCheakingPlayers[i][2].Contains(cheakType))
                        {
                            OnlineCheakingPlayers[i][2] += " " + cheakType;
                        }
                        cheakTimes = int.Parse(OnlineCheakingPlayers[i][1]);
                        if (cheakTimes >= config.numberOfBan_允许的违规次数)
                        {
                            OnlineCheakingPlayers[i][1] = "0";
                        }
                        break;
                    }
                }
            }

            if (cheakTimes == 0)
            {
                OnlineCheakingPlayers.Add(new string[] { e.Name, "1", cheakType, "" });
                cheakTimes = 1;
            }
            //Console.WriteLine($"玩家{e.Player.Name}已作弊{cheakTimes}次");
            return cheakTimes;
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
