using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace Watcher
{
    public partial class Watcher : TerrariaPlugin
    {
        /// <summary>
        /// 获取从2020.1.1日到现在经过的秒数
        /// </summary>
        /// <returns></returns>
        public static long getNowTimeSecond()
        {
            DateTime centuryBegin = new DateTime(2020, 1, 1);
            long elapsedTicks = DateTime.Now.Ticks - centuryBegin.Ticks;
            return (elapsedTicks / 10000000L);
        }


        /// <summary>
        /// 获取从2020.1.1日到现在经过的嘀嗒数,60 tick == 1s
        /// </summary>
        /// <returns></returns>
        public static long getNowTimeTicks()
        {
            DateTime centuryBegin = new DateTime(2020, 1, 1);
            long elapsedTicks = DateTime.Now.Ticks - centuryBegin.Ticks;
            return (elapsedTicks / 166667L);
        }


        //备份tshock.sqlite文件
        public void BackUpTshockSql()
        {
            string tDirPath = Path.Combine(TShock.SavePath + "/Watcher/tshock_backups");
            string tFilePath = Path.Combine(tDirPath, DateTime.Now.ToString("u").Replace(":", "-") + "_tshock.sqlite");
            if (!Directory.Exists(tDirPath))
                Directory.CreateDirectory(tDirPath);
            if (File.Exists(tFilePath))
                return;
            File.Copy(TShock.SavePath + "/tshock.sqlite", tFilePath, false);

            DeleteOldFiles(tDirPath, config.tshockSql备份时常分钟);

            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [{tFilePath}] 已保存" });
        }


        /// <summary>
        /// 物品作弊检测方法
        /// </summary>
        /// <param name="e"></param>
        /// <param name="Model">2为单个玩家检查，必须要传送形参e, 1为全员检查，e传入null即可</param>
        public void ItemCheck(GetDataHandlers.PlayerSlotEventArgs e, int Model = 0)
        {
            //全员检查 model == 1
            if (Model == 1)
            {
                foreach (TSPlayer v in TShock.Players)
                {
                    if (v == null || !KickOrBanGroupAllow(v))
                    {
                        continue;
                    }

                    List<Item> items = new List<Item>();
                    items.AddRange(v.TPlayer.inventory);
                    items.AddRange(v.TPlayer.armor);
                    items.AddRange(v.TPlayer.Loadouts[0].Armor);
                    items.AddRange(v.TPlayer.Loadouts[1].Armor);
                    items.AddRange(v.TPlayer.Loadouts[2].Armor);
                    items.AddRange(v.TPlayer.dye);
                    items.AddRange(v.TPlayer.Loadouts[0].Dye);
                    items.AddRange(v.TPlayer.Loadouts[1].Dye);
                    items.AddRange(v.TPlayer.Loadouts[2].Dye);
                    items.AddRange(v.TPlayer.miscEquips);
                    items.AddRange(v.TPlayer.miscDyes);
                    items.Add(v.TPlayer.trashItem);
                    items.AddRange(v.TPlayer.bank.item);
                    items.AddRange(v.TPlayer.bank2.item);
                    items.AddRange(v.TPlayer.bank3.item);
                    items.AddRange(v.TPlayer.bank4.item);
                    //移除所有的空白物品
                    items.RemoveAll(i => i.IsAir);

                    if (isItemsAbnormal(items, out List<Item> citems, out string text))
                    {
                        Warning(v, TypesOfCheat.ItemCheat, config.物品作弊警告方式, null, e, config.物品作弊是否计入总违规作弊次数, text);

                        /*
                        int num = AddCheatingPlayers(v, TypesOfCheat.ItemCheat);
                        Console.WriteLine($" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {v.LastNetPosition / 16} 违规次数 {num}");
                        AvoidLogSize("logDirPath", logDirPath, logFilePath);
                        AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                        if (num < config.最多违规作弊次数)
                        {
                            text = FormatArrangement(text, 5, new char[] { ',' }, "\n");
                            v.Kick($"携带不属于这个时期的物品或系统设置违禁物：\n{text}\n违规次数 {num}，若有疑问请及时向管理员联系");
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                            File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                        }
                        else
                        {
                            v.Ban($"总计违规次数已达到{config.最多违规作弊次数}次！若有疑问请及时联系管理员");
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {v.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                            File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {v.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                        }
                        */
                    }
                }
            }


            //单人检查检查 model == 2
            if (Model == 2 && KickOrBanGroupAllow(e.Player))
            {
                List<Item> items = new List<Item>();
                items.AddRange(e.Player.TPlayer.inventory);
                items.AddRange(e.Player.TPlayer.armor);
                items.AddRange(e.Player.TPlayer.Loadouts[0].Armor);
                items.AddRange(e.Player.TPlayer.Loadouts[1].Armor);
                items.AddRange(e.Player.TPlayer.Loadouts[2].Armor);
                items.AddRange(e.Player.TPlayer.dye);
                items.AddRange(e.Player.TPlayer.Loadouts[0].Dye);
                items.AddRange(e.Player.TPlayer.Loadouts[1].Dye);
                items.AddRange(e.Player.TPlayer.Loadouts[2].Dye);
                items.AddRange(e.Player.TPlayer.miscEquips);
                items.AddRange(e.Player.TPlayer.miscDyes);
                items.Add(e.Player.TPlayer.trashItem);
                items.AddRange(e.Player.TPlayer.bank.item);
                items.AddRange(e.Player.TPlayer.bank2.item);
                items.AddRange(e.Player.TPlayer.bank3.item);
                items.AddRange(e.Player.TPlayer.bank4.item);
                //移除所有的空白物品
                items.RemoveAll(i => i.IsAir);

                if (isItemsAbnormal(items, out List<Item> citems, out string text))
                {
                    Warning(e.Player, TypesOfCheat.ItemCheat, config.物品作弊警告方式, null, e, config.物品作弊是否计入总违规作弊次数, text);
                    /*
                    int num = AddCheatingPlayers(e.Player, TypesOfCheat.ItemCheat);
                    Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {e.Player.LastNetPosition / 16} 违规次数 {num}");
                    AvoidLogSize("logDirPath", logDirPath, logFilePath);
                    AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                    if (num < config.最多违规作弊次数)
                    {
                        text = FormatArrangement(text, 5, new char[] { ',' }, "\n");
                        e.Player.Kick($"携带不属于这个时期的物品或系统设置违禁物：\n{text}\n违规次数 {num}，若有疑问请及时向管理员联系");
                        File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
                        File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
                    }
                    else
                    {
                        e.Player.Ban($"总计违规次数已达到{config.最多违规作弊次数}次！若有疑问请及时联系管理员");
                        File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {e.Player.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                        File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {text} 在 {e.Player.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                    }
                    */
                }
            }
        }


        /// <summary>
        /// 接受items，检查是否含有违禁物，将违禁物写入citems，文本信息写入text
        /// </summary>
        /// <param name="items">需要检查的物品集合</param>
        /// <param name="citems">违规物品集合</param>
        /// <param name="text">违规物品集合名称</param>
        /// <returns>如果含有违规物，返回真，否则返回假</returns>
        public bool isItemsAbnormal(List<Item> items, out List<Item> citems, out string text)
        {
            NPCKillsTracker NKT = Main.BestiaryTracker.Kills;

            citems = new List<Item>();
            foreach (Item i in items)
            {
                //如果是豁免物品并且不在强制检查物里，不算作弊，这个得放到最前面
                if (config.不需要被作弊检查的物品ID.Contains(i.type) && !config.必须被检查的物品_覆盖上面一条.Contains(i.type))
                {
                    continue;
                }

                //常规检查物品检查
                if (!NPC.downedMoonlord && CheatData.AfterMoonlord.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!NPC.downedAncientCultist && CheatData.AfterLunaticCultist.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!NPC.downedGolemBoss && CheatData.AfterGolemBeforeCultist.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!NPC.downedPlantBoss && CheatData.AfterPlanteraBeforeGolem.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) && CheatData.AfterMechanicsBeforePlantera.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!NPC.downedMechBossAny && CheatData.AfterAnyMechanics.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!Main.hardMode && CheatData.AfterHardBeforeOneOfThree.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!NPC.downedBoss3 && CheatData.AfterBoss3BeforeHard.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!NPC.downedQueenBee && CheatData.QueenBee.Contains(i.type))
                {
                    citems.Add(i);
                }
                if (!NPC.downedBoss2 && CheatData.AfterBoss2BeforeBoss3.Contains(i.type))
                {
                    citems.Add(i);
                }

                //单独boss的专家物品判断，因为全员npc掉落物检查不包含专家物品
                if (!NPC.downedEmpressOfLight && i.type == 4989)
                {
                    citems.Add(i);
                }
                if (!NPC.downedFishron && i.type == 3367)
                {
                    citems.Add(i);
                }
                if (!NPC.downedQueenSlime && i.type == 4987)
                {
                    citems.Add(i);
                }
                if (!NPC.downedDeerclops && i.type == 5100)
                {
                    citems.Add(i);
                }
                if (!NPC.downedBoss1 && i.type == 3097)
                {
                    citems.Add(i);
                }
                if (!NPC.downedSlimeKing && i.type == 3090)
                {
                    citems.Add(i);
                }

                /*
                //1.4.4.7*********************************************
                //dont dig up 和 天顶
                if (Main.remixWorld || Main.zenithWorld)
                {
                    //先移除 钥匙剑，冰雪弓，魔法飞刀，邪恶三叉戟，泡泡枪，这些原违规物
                    citems.RemoveAll(x => x.type == 671 || x.type == 725 || x.type == 517 || x.type == 683 || x.type == 2623);
                    if (!NPC.downedBoss3 && (i.type == 683 || i.type == 2623))//邪恶三叉戟和泡泡枪由击败骷髅王获得
                    {
                        citems.Add(i);
                    }
                    if (!Main.hardMode && (i.type == 1319 || i.type == 3069 || i.type == 5147))//雪球炮，火花，结霜魔杖由肉后宝箱怪获得
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedMechBossAny && i.type == 112)//火之花由红魔鬼掉落
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedPlantBoss && i.type == 2273)//武士刀由花后骷髅怪掉落
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedFishron && i.type == 157)//海蓝权杖由猪鲨掉落
                    {
                        citems.Add(i);
                    }


                    //环境净化物品，如净化枪，溶液，这些不该在这个世界生成
                    List<int> ls1 = new List<int> { 779, 780, 781, 782, 783, 784, 5392, 5393, 5394 };
                    if (ls1.Contains(i.type) && !citems.Exists(x => x.type == i.type))
                    {
                        citems.Add(i);
                    }
                }
                */

                #region 各种其他情况下可以制作的物品
                //奥库瑞姆的剃刀
                if (!Main.zenithWorld)
                {
                    if (i.type == 5334)
                    {
                        citems.Add(i);
                    }
                }
                //华美甜点
                if (!NPC.downedSlimeKing && !NPC.downedQueenSlime && i.type == 5131)
                {
                    citems.Add(i);
                }
                #endregion


                #region 全员npc掉落物检查
                List<DropRateInfo> drops = new List<DropRateInfo>();
                List<Item> unl1 = new List<Item>();//这种生物未击败时获得的违禁物
                List<Item> unl2 = new List<Item>();//从另一种会掉落相同物品但是已经击败过的生物身上获得的可获得物（如所有的史莱姆都会掉落史莱姆法杖）
                for (int id = -65; id < NPCID.Count; id++)
                {
                    List<IItemDropRule> rulesForNPCID = Main.ItemDropsDB.GetRulesForNPCID(id, false);
                    foreach (IItemDropRule itemDropRule in rulesForNPCID)
                    {
                        itemDropRule.ReportDroprates(drops, new DropRateInfoChainFeed(1f));
                    }
                    foreach (DropRateInfo info in drops)
                    {
                        if (NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[id]) == 0 && i.type == info.itemId && !unl1.Contains(i))
                        {
                            unl1.Add(i);
                        }
                        if (NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[id]) > 0 && i.type == info.itemId && !unl2.Contains(i))
                        {
                            unl2.Add(i);
                        }
                    }
                    drops.Clear();
                }
                unl1.RemoveAll(x => unl2.Contains(x) || CheatData.AllNPCLootExclude.Contains(x.type));//从unl1中移除所有unl2中的元素，同时需要排除一些特殊东西
                citems.AddRange(unl1);//把违禁物加入总统计违禁物中
                #endregion


                #region 以下要去除各种彩蛋种子导致的掉落物调换顺序等各种复杂情况
                //十周年移除史莱姆法杖等
                if (Main.tenthAnniversaryWorld || Main.zenithWorld)
                {
                    citems.RemoveAll(x => x.type == 1309 || x.type == 905);
                }
                //天顶种子和上挖种子中物品调换的修正
                if (Main.remixWorld || Main.zenithWorld)
                {
                    if (!NPC.downedBoss3 && (i.type == 683 || i.type == 2623))//邪恶三叉戟和泡泡枪由击败骷髅王获得
                    {
                        citems.Add(i);
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[629]) == 0) && (i.type == 1319 || i.type == 1264 || i.type == 676))//雪球炮，寒霜之花，霜印剑，肉后冰雪宝箱怪，不该出现在肉前
                    {
                        citems.Add(i);
                    }
                    if (NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[85]) == 0 && i.type == 517)//魔法飞刀，肉前保险怪
                    {
                        citems.Add(i);
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[85]) == 0) && i.type == 437 || i.type == 535 || i.type == 536 || i.type == 532 || i.type == 554)//双钩，点金石，泰坦手套，星星斗篷，十字项链，肉前保险怪，不该出现在肉前
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedMechBossAny && i.type == 112)//火之花由红魔鬼掉落
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedPlantBoss && i.type == 2273)//武士刀由花后骷髅怪掉落
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedFishron && i.type == 157)//海蓝权杖由猪鲨掉落
                    {
                        citems.Add(i);
                    }
                }
                else//如果不是这些种子，按原方法修正
                {
                    if (!NPC.downedBoss3 && (i.type == 112 || i.type == 157))//火之花和海蓝由击败骷髅王获得
                    {
                        citems.Add(i);
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[629]) == 0) && (i.type == 676 || i.type == 1264 || i.type == 725))//霜印剑，寒霜之花，冰雪弓，由肉后冰雪宝箱怪获得
                    {
                        citems.Add(i);
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[85]) == 0) && (i.type == 437 || i.type == 535 || i.type == 536 || i.type == 532 || i.type == 554 || i.type == 517))//双钩，点金石，泰坦手套，星星斗篷，十字项链，魔法飞刀，肉后宝箱怪，不该出现在肉前
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedMechBossAny && i.type == 683)//邪恶三叉戟由红魔鬼掉落
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedPlantBoss && i.type == 671)//击败世纪之花掉钥匙剑
                    {
                        citems.Add(i);
                    }
                    if (!NPC.downedFishron && i.type == 2623)//泡泡枪由猪鲨掉落
                    {
                        citems.Add(i);
                    }
                }
                //这里考虑10周年蒸汽朋克的情况
                if (!Main.tenthAnniversaryWorld && !NPC.downedMechBossAny && CheatData.Steampunker.Contains(i.type))
                {
                    citems.Add(i);
                }
                //共鸣法杖
                if (!NPC.downedPlantBoss && i.type == 5065)
                {
                    citems.Add(i);
                }
                #endregion


                //如果是强制检查物，将违规物添加进去，这个得放到最后面
                if (config.必须被检查的物品_覆盖上面一条.Contains(i.type) && !citems.Contains(i))
                {
                    citems.Add(i);
                }
            }


            //如果一个都没，未违规
            if (citems.Count == 0)
            {
                text = "";
                return false;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                int count = 0;
                foreach (Item i in citems)
                {
                    if (count == 0)
                    {
                        sb.Append($"{i.Name}[{i.type}]");
                    }
                    else
                    {
                        sb.Append($", {i.Name}[{i.type}]");
                    }
                    count++;
                }
                text = sb.ToString();
                return true;
            }
        }


        //创建相关文件夹和文件
        public void SetWatcherFile(string dirPathName, string dirPath)
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
                Console.WriteLine(e.Message);
                TShock.Log.Error(e.Message);
            }
        }


        //当日志过大时重新创建日志
        public void AvoidLogSize(string dirPathName, string dirPath, string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 1024 * 1024 * config.日志和作弊日志文件的最大MB)
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
        public void LogClean()
        {
            DeleteOldFiles(logDirPath, config.任何日志的备份时常分钟);
            DeleteOldFiles(cheatLogDirPath, config.任何日志的备份时常分钟);
        }


        /// <summary>
        /// 增加某个玩家的总计作弊次数，每调用一次，作弊次数+1
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cheatType">作弊类型 0：钓鱼作弊，1：射弹伤害作弊，2：物品作弊，3: 星璇机枪作弊</param>，
        /// <returns>返回计算后的作弊次数</returns>
        public int AddCheatingPlayers(TSPlayer e, TypesOfCheat cheatType)
        {
            int cheatTimes = 0;
            bool flag = false;
            foreach (WPlayer w in wPlayers)
            {
                if (w.uuid == e.UUID)
                {
                    flag = true;
                    w.cheatingTimes++;
                    switch (cheatType)
                    {
                        case TypesOfCheat.FishCheat:
                            w.isFishCheat = true;
                            break;
                        case TypesOfCheat.DamageCheat:
                            w.isDamageCheat = true;
                            break;
                        case TypesOfCheat.ItemCheat:
                            w.isItemCheat = true;
                            break;
                        case TypesOfCheat.VortexCheat:
                            w.isVortexCheat = true;
                            break;
                        default:
                            break;
                    }
                    cheatTimes = w.cheatingTimes;
                    if (w.cheatingTimes >= config.最多违规作弊次数)
                    {
                    }
                    //在对wPlayers做出修改后，这里对配置文件更新一下
                    WPM.SaveConfigFile();
                    break;
                }
            }
            if (!flag)
            {
                wPlayers.Add(new WPlayer(e));
                int n = AddCheatingPlayers(e, cheatType);
                //在对wPlayers做出修改后，这里对配置文件更新一下
                WPM.SaveConfigFile();
                return n;
            }
            else
            {
                return cheatTimes;
            }
        }


        /// <summary>
        /// 返回这个玩家的总作弊次数，不增加作弊次数
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public int getCheatTimes(TSPlayer e)
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


        /// <summary>
        /// kick或Ban人时对所选择的组进行区别。返回是否被踢办或处理
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool KickOrBanGroupAllow(TSPlayer v)
        {
            if (v == null)
            {
                return false;
            }
            bool flag = false;
            foreach (string str in config.需要被检测的玩家组)
            {
                if (v != null && (v.Group.Name.Equals(str, StringComparison.CurrentCultureIgnoreCase) || TShock.Groups.GetGroupByName(str).Name.Equals(v.Group.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }


        /// <summary>
        /// 给玩家TSplayer一个物品
        /// </summary>
        /// <param name="p">玩家</param>
        /// <param name="type">物品id</param>
        /// <param name="stack">数目</param>
        /// <param name="prefix">前缀</param>
        public static void GiveItem(TSPlayer p, int type, int stack, int prefix = 0)
        {
            int num = Item.NewItem(new EntitySource_DebugCommand(), (int)p.TPlayer.Center.X, (int)p.TPlayer.Center.Y, p.TPlayer.width, p.TPlayer.height, type, stack, true, prefix, true, false);
            Main.item[num].playerIndexTheItemIsReservedFor = p.Index;
            p.SendData(PacketTypes.ItemDrop, "", num, 1f, 0f, 0f, 0);
            p.SendData(PacketTypes.ItemOwner, null, num, 0f, 0f, 0f, 0);
        }


        /// <summary>
        /// 给单个玩家发送悬浮文本
        /// </summary>
        /// <param name="player"> 玩家 </param>
        /// <param name="text">文本</param>
        /// <param name="color">颜色</param>
        /// <param name="position">位置</param>
        public static void SendPlayerText(TSPlayer player, string text, Color color, Vector2 position)
        {
            player.SendData(PacketTypes.CreateCombatTextExtended, text, (int)color.packedValue, position.X, position.Y);
        }


        /// <summary>
        /// 禁止在肉前使用恶魔心饰品栏
        /// </summary>
        public void DisableHardModeAccessorySlot()
        {
            //筛选恶魔心的问题
            if (!Main.hardMode)
            {
                foreach (TSPlayer p in TShock.Players)
                {
                    if (p == null || !KickOrBanGroupAllow(p))
                    {
                        continue;
                    }
                    else if (p != null && p.IsLoggedIn && p.TPlayer.active)
                    {
                        int flag = 0;
                        if (!p.TPlayer.armor[8].IsAir)
                        {
                            Item i = p.TPlayer.armor[8];
                            GiveItem(p, i.type, i.stack, i.prefix);
                            p.TPlayer.armor[8].TurnToAir();
                            p.SendData(PacketTypes.PlayerSlot, "", p.Index, PlayerItemSlotID.Armor0 + 8);
                            flag++;
                        }
                        if (!p.TPlayer.armor[18].IsAir)
                        {
                            Item i = p.TPlayer.armor[18];
                            GiveItem(p, i.type, i.stack, i.prefix);
                            p.TPlayer.armor[18].TurnToAir();
                            p.SendData(PacketTypes.PlayerSlot, "", p.Index, PlayerItemSlotID.Armor0 + 18);
                            flag++;
                        }
                        if (!p.TPlayer.dye[8].IsAir)
                        {
                            Item i = p.TPlayer.dye[8];
                            GiveItem(p, i.type, i.stack, i.prefix);
                            p.TPlayer.dye[8].TurnToAir();
                            p.SendData(PacketTypes.PlayerSlot, "", p.Index, PlayerItemSlotID.Dye0 + 8);
                            flag++;
                        }
                        if (flag != 0)
                        {
                            SendPlayerText(p, "世界未开启困难模式，禁止使用恶魔心饰品栏", Color.Red, p.TPlayer.Center);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 给出物品组的图标文本，帮你自动排版
        /// </summary>
        /// <param name="str">物品组的图标文本</param>
        /// <param name="num">一行几个</param>
        /// <param name="block">间隔字符</param>
        /// <returns></returns>
        public string FormatArrangement(string str, int num, char[] chars, string block = "")
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                List<string> ls = str.Split(chars).ToList();
                for (int i = 0; i < ls.Count; i++)
                {
                    if ((i + 1) % (num + 1) == 0)
                    {
                        ls.Insert(i, block);
                    }
                }
                string str2 = "";
                foreach (string s in ls)
                {
                    str2 += s;
                }
                return str2;
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// 返回命中这个数字概率
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public bool getRand(double d)
        {
            if (d >= 1)
            {
                return true;
            }
            if (d <= 0)
            {
                return false;
            }
            int a = (int)(d * 100000);
            if (Main.rand.Next(100000) <= a)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 惩罚类型
        /// </summary>
        public enum TypesOfPunish
        {
            oral,       //口头惩罚
            disable,    //封住行动
            kill,       //杀掉
            kick,       //踢掉
            ban         //直接ban掉
        }

        /// <summary>
        /// 对作弊或违规玩家进行警告惩罚或封禁，写入日志和作弊记录日志
        /// </summary>
        /// <param name="p">作弊玩家</param>
        /// <param name="ctype">作弊类型</param>
        /// <param name="ptype">警告类型</param>
        /// <param name="e1">当ctype等于 FishCheat，DamageCheat，VortexCheat 时，需要设置这个值，其他情况为null</param>
        /// <param name="e2">当ctype等于 ItemCheat 时，需要设置这个值，其他情况为null</param>
        /// <param name="IncludeCheatTimes">是否计入总作弊次数，这会在到达一定值时ban掉玩家</param>
        /// <param name="text">当ctype等于 ItemCheat 时，需要设置这个值，其他情况为 ""</param>
        public void Warning(TSPlayer p, TypesOfCheat ctype, TypesOfPunish ptype, GetDataHandlers.NewProjectileEventArgs e1 = null, GetDataHandlers.PlayerSlotEventArgs e2 = null, bool IncludeCheatTimes = true, string text = "")
        {
            if (!KickOrBanGroupAllow(p))
            {
                return;
            }

            int num;
            string reason = "", str = "", reason2 = "";
            if (IncludeCheatTimes)
            {
                num = AddCheatingPlayers(p, ctype);
            }
            else
            {
                num = getCheatTimes(p);
                str = "（本次不计入总作弊次数）";
            }

            switch (ctype)
            {
                case TypesOfCheat.FishCheat:
                    reason = $"WARNING [id:{p.Account.ID}][{p.Name}] 掷出的 {p.TPlayer.HeldItem.Name} 浮标数目不正常 在 {{X:{(int)(e1.Position.X / 16)}, Y:{(int)(e1.Position.Y / 16)}}} 违规次数 {num}" + str;
                    break;
                case TypesOfCheat.DamageCheat:
                    reason = $"WARNING [id:{p.Account.ID}][{p.Name}] 伤害 {e1.Damage} 过高 生成 {Lang.GetProjectileName(e1.Type)} 在 {{X:{(int)(e1.Position.X / 16)}, Y:{(int)(e1.Position.Y / 16)}}} 违规次数 {num}" + str;
                    Main.projectile[e1.Type].Kill();
                    break;
                case TypesOfCheat.ItemCheat:
                    reason = $"WARNING [id:{p.Account.ID}][{p.Name}] 携带非正常时期物品或系统设置违禁物 {text} 在 {{X:{(int)(p.LastNetPosition.X / 16)}, Y:{(int)(p.LastNetPosition.Y / 16)}}} 违规次数 {num}" + str;
                    reason2 = $"WARNING [id:{p.Account.ID}][{p.Name}] 携带非正常时期物品或系统设置违禁物\n{FormatArrangement(text, 5, new char[] { ',' }, "\n")} 在 {{X:{(int)(p.LastNetPosition.X / 16)}, Y:{(int)(p.LastNetPosition.Y / 16)}}} 违规次数 {num}" + str;
                    break;
                case TypesOfCheat.VortexCheat:
                    reason = $"WARNING [id:{p.Account.ID}][{p.Name}] 使用星璇机枪bug 在 {{X:{(int)(e1.Position.X / 16)}, Y:{(int)(e1.Position.Y / 16)}}} 违规次数 {num}" + str;
                    break;
                default:
                    break;
            }

            if (num < config.最多违规作弊次数)
            {
                switch (ptype)
                {
                    case TypesOfPunish.oral:
                        p.SendMessage("【单独警告】" + reason, Color.Red);
                        SendPlayerText(p, "【单独警告】" + reason, Color.Red, p.LastNetPosition);
                        break;
                    case TypesOfPunish.disable:
                        TSPlayer.All.SendMessage(reason, Color.Red);
                        SendPlayerText(p, reason, Color.Red, p.LastNetPosition);
                        p.Disable();
                        break;
                    case TypesOfPunish.kill:
                        TSPlayer.All.SendMessage(reason, Color.Red);
                        SendPlayerText(p, reason, Color.Red, p.LastNetPosition);
                        p.KillPlayer();
                        break;
                    case TypesOfPunish.kick:
                        if (ctype == TypesOfCheat.ItemCheat)
                        {
                            p.Kick(reason2);
                        }
                        else
                            p.Kick(reason);

                        break;
                    case TypesOfPunish.ban:
                        p.Ban(reason);
                        Console.WriteLine(reason + " 已直接封禁！");
                        AvoidLogSize("logDirPath", logDirPath, logFilePath);
                        AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                        File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason + " 已直接封禁！" });
                        File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason + " 已直接封禁！" });
                        break;
                    default:
                        break;
                }
            }

            if (num < config.最多违规作弊次数)
            {
                Console.WriteLine(DateTime.Now.ToString("u") + " " + reason);
                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason });
                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason });
            }
            else if (ptype != TypesOfPunish.ban)
            {
                p.Ban($"总计违规次数已达到{config.最多违规作弊次数}次！若有疑问请及时联系管理员");
                Console.WriteLine(DateTime.Now.ToString("u") + " " + reason + " 已封禁");
                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason + " 已封禁" });
                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason + " 已封禁" });
            }
        }

        /// <summary>
        /// 对作弊或违规玩家进行警告惩罚或封禁，写入日志和作弊记录日志
        /// </summary>
        /// <param name="p">作弊玩家</param>
        /// <param name="ctype">作弊类型</param>
        /// <param name="ptype">警告方式类型</param>        /// <param name="e1">当ctype等于 FishCheat，DamageCheat，VortexCheat 时，需要设置这个值，其他情况为null</param>
        /// <param name="e2">当ctype等于 ItemCheat 时，需要设置这个值，其他情况为null</param>
        /// <param name="IncludeCheatTimes">是否计入总作弊次数，这会在到达一定值时ban掉玩家</param>
        /// <param name="text">当ctype等于 ItemCheat 时，需要设置这个值，其他情况为 ""</param>
        public void Warning(TSPlayer p, TypesOfCheat ctype, int ptype, GetDataHandlers.NewProjectileEventArgs e1 = null, GetDataHandlers.PlayerSlotEventArgs e2 = null, bool IncludeCheatTimes = true, string text = "")
        {
            if (ptype >= (int)TypesOfPunish.oral && ptype <= (int)TypesOfPunish.ban)
            {
                Warning(p, ctype, (TypesOfPunish)ptype, e1, e2, IncludeCheatTimes, text);
            }
            else
            {
                Warning(p, ctype, TypesOfPunish.kick, e1, e2, IncludeCheatTimes, text);
                TShock.Log.Error("WARNING: TypesOfPunish范围错误，已设置为默认 3，请检查WatcherConfig中的各种作弊类型的警告方式填写是否正确！(取值范围 0 ~ 4 整数)");
                Console.WriteLine("WARNING: TypesOfPunish范围错误，已设置为默认 3，请检查WatcherConfig中的各种作弊类型的警告方式填写是否正确！(取值范围 0 ~ 4 整数)");
                TSPlayer.All.SendErrorMessage("WARNING: TypesOfPunish范围错误，已设置为默认 3，请检查WatcherConfig中的各种作弊类型的警告方式填写是否正确！(取值范围 0 ~ 4 整数)");
            }
        }


        #region 被弃用的部分
        /*
        //召唤boss写入日志(因功能不全被弃用)
        public void SummonBoss(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.SpawnBossorInvasion)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
                {
                    BinaryReader reader = new BinaryReader(data);
                    int id = reader.ReadUInt16();
                    int type = reader.ReadUInt16();
                    TSPlayer player = TShock.Players[id];
                    if (player != null)
                    {
                        if (type > 0)
                        {
                            AvoidLogSize("logDirPath", logDirPath, logFilePath);
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{player.Account.ID}][{player.Name}] 召唤了 {Lang.GetNPCName(type)} 在 {player.LastNetPosition / 16}" });
                        }
                        else
                        {
                            AvoidLogSize("logDirPath", logDirPath, logFilePath);
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{player.Account.ID}][{player.Name}] 召唤错误 在 {player.LastNetPosition / 16}" });
                        }
                    }
                }
            }
        }


        
        /// <summary>
        /// 将文本换行排版
        /// </summary>
        /// <param name="str">文本</param>
        /// <param name="num">一行几个字符</param>
        /// <param name="block">间隔字符</param>
        /// <returns></returns>
        public string FormatArrangement(string str, int num, string block = "")
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                string str2 = "";
                for (int i = 0; i < str.Length; i++)
                {
                    str2 += str[i];
                    if (i % num == 0 && i != 0)
                    {
                        str2 += block;
                    }
                }
                return str2;
            }
            else
            {
                return "";
            }
        }





        /*
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
                    items.AddRange(v.TPlayer.Loadouts[0].Armor);
                    items.AddRange(v.TPlayer.Loadouts[1].Armor);
                    items.AddRange(v.TPlayer.Loadouts[2].Armor);
                    items.AddRange(v.TPlayer.dye);
                    items.AddRange(v.TPlayer.Loadouts[0].Dye);
                    items.AddRange(v.TPlayer.Loadouts[1].Dye);
                    items.AddRange(v.TPlayer.Loadouts[2].Dye);
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
                        if (config.不需要被作弊检查的物品ID.Contains(i.type) && !config.必须被检查的物品_覆盖上面一条.Contains(i.type))
                        {
                            continue;
                        }
                        bool isCheat = false;
                        //如果是强制检查物，直接结束下面的检查过程
                        if (config.必须被检查的物品_覆盖上面一条.Contains(i.type))
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
                            int num = AddCheatingPlayers(v, 3);
                            Console.WriteLine($" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num}");
                            AvoidLogSize("logDirPath", logDirPath, logFilePath);
                            AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                            if (num < config.最多违规作弊次数)
                            {
                                v.Kick($"携带不属于这个时期的物品或系统设置违禁物：{i.Name} [id:{i.type}]\n违规次数 {num}，若有疑问请及时向管理员联系");
                                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{v.Account.ID}][{v.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {v.LastNetPosition / 16} 违规次数 {num}" });
                            }
                            else
                            {
                                v.Ban($"总计违规次数已达到{config.最多违规作弊次数}次！若有疑问请及时联系管理员");
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
                items.AddRange(e.Player.TPlayer.Loadouts[0].Armor);
                items.AddRange(e.Player.TPlayer.Loadouts[1].Armor);
                items.AddRange(e.Player.TPlayer.Loadouts[2].Armor);
                items.AddRange(e.Player.TPlayer.dye);
                items.AddRange(e.Player.TPlayer.Loadouts[0].Dye);
                items.AddRange(e.Player.TPlayer.Loadouts[1].Dye);
                items.AddRange(e.Player.TPlayer.Loadouts[2].Dye);
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
                    if (config.不需要被作弊检查的物品ID.Contains(i.type) && !config.必须被检查的物品_覆盖上面一条.Contains(i.type))
                    {
                        continue;
                    }
                    bool isCheat = false;
                    if (config.必须被检查的物品_覆盖上面一条.Contains(i.type))
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
                        int num = AddCheatingPlayers(e.Player, 3);
                        Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num}");
                        AvoidLogSize("logDirPath", logDirPath, logFilePath);
                        AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                        if (num < config.最多违规作弊次数)
                        {
                            e.Player.Kick($"携带不属于这个时期的物品或系统设置违禁物：{i.Name} [id:{i.type}]\n违规次数 {num}，若有疑问请及时向管理员联系");
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
                            File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num}" });
                        }
                        else
                        {
                            e.Player.Ban($"总计违规次数已达到{config.最多违规作弊次数}次！若有疑问请及时联系管理员");
                            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                            File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING BAN [acc:{e.Player.Account.ID}][{e.Player.Name}] 携带不属于这个时期的物品或系统设置违禁物 {i.Name} [id:{i.type}] 在 {e.Player.LastNetPosition / 16} 违规次数 {num} 已封禁" });
                        }
                    }
                }
            }
        */
        #endregion
    }
}
