using Microsoft.Xna.Framework;
using System.Data;
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
        /// 保存tshock的config.json
        /// </summary>
        public static void SaveTConfig()
        {
            TShock.Config.Write(Path.Combine(TShock.SavePath, "config.json"));
        }

        /*已经没必要了
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
        */

        /// <summary>
        /// 检测这个玩家的物品是否正常，当 TSplayer == null 时检查全部在场玩家
        /// </summary>
        public bool ItemOfPlayerCheck(TSPlayer? player, out string mess)
        {
            //该玩家是否物品作弊
            bool flag = false;
            //这个玩家作弊的原因
            mess = "";
            //全员检查
            if (player == null)
            {
                foreach (TSPlayer v in TShock.Players)
                {
                    if (v == null || !NeedBeChecked(v))
                        continue;

                    //作弊玩家在刚进服的10秒内不要检查他，让他有时间扔东西
                    WPlayer? wp = wPlayers.Find(x => v.UUID == x.uuid);
                    if (wp != null && wp.物品作弊次数 > 0 && wp.本次进服游玩时间 < 10)
                        continue;

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

                    if (!ReasonableItem(items, out Dictionary<Item, string> abItem1) | !ReasonableItemOfRecipe(items, out Dictionary<Item, string> abItem2))
                    {
                        flag = true;
                        List<string> temp = new List<string>();
                        foreach (var a in abItem1)
                        {
                            temp.Add(a.Value);
                        }
                        foreach (var a in abItem2)
                        {
                            temp.Add(a.Value);
                        }
                        mess += Warning(v, TypesOfCheat.ItemCheat, (TypesOfPunish)config.物品作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出, config.物品作弊是否算违规, "", 0, 0, Vector2.Zero, temp) + '\n';
                        if (config.检测到违禁物时是否清空 && v != null && v.IsLoggedIn)
                        {
                            List<Item> list = new List<Item>();
                            list.AddRange(abItem1.Keys.ToArray());
                            list.AddRange(abItem2.Keys.ToArray());
                            ClearPlayersItem(list.ToArray(), v);
                            if (config.物品作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出 != 0)
                                TSPlayer.All.SendSuccessMessage($"玩家 [ {v.Name} ] 的违规物品已全部清理");
                            else
                                v.SendInfoMessage("您的违规物品已全部清理");
                        }
                    }
                }
                while (mess.EndsWith("\n"))
                {
                    mess = mess.TrimEnd('\n');
                }
            }
            else if (NeedBeChecked(player))
            {
                List<Item> items = new List<Item>();
                items.AddRange(player.TPlayer.inventory);
                items.AddRange(player.TPlayer.armor);
                items.AddRange(player.TPlayer.Loadouts[0].Armor);
                items.AddRange(player.TPlayer.Loadouts[1].Armor);
                items.AddRange(player.TPlayer.Loadouts[2].Armor);
                items.AddRange(player.TPlayer.dye);
                items.AddRange(player.TPlayer.Loadouts[0].Dye);
                items.AddRange(player.TPlayer.Loadouts[1].Dye);
                items.AddRange(player.TPlayer.Loadouts[2].Dye);
                items.AddRange(player.TPlayer.miscEquips);
                items.AddRange(player.TPlayer.miscDyes);
                items.Add(player.TPlayer.trashItem);
                items.AddRange(player.TPlayer.bank.item);
                items.AddRange(player.TPlayer.bank2.item);
                items.AddRange(player.TPlayer.bank3.item);
                items.AddRange(player.TPlayer.bank4.item);
                //移除所有的空白物品
                items.RemoveAll(i => i.IsAir);

                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                if (!ReasonableItem(items, out Dictionary<Item, string> abItem1) | !ReasonableItemOfRecipe(items, out Dictionary<Item, string> abItem2))
                {
                    List<string> temp = new List<string>();
                    foreach (var a in abItem1)
                    {
                        temp.Add(a.Value);
                    }
                    foreach (var a in abItem2)
                    {
                        temp.Add(a.Value);
                    }
                    mess += Warning(player, TypesOfCheat.ItemCheat, (TypesOfPunish)config.物品作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出, config.物品作弊是否算违规, "", 0, 0, Vector2.Zero, temp);
                    flag = true;
                    if (config.检测到违禁物时是否清空 && player != null && player.IsLoggedIn)
                    {
                        List<Item> list = new List<Item>();
                        list.AddRange(abItem1.Keys.ToArray());
                        list.AddRange(abItem2.Keys.ToArray());
                        ClearPlayersItem(list.ToArray(), player);
                        if (config.物品作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出 != 0)
                            TSPlayer.All.SendSuccessMessage($"玩家 [ {player.Name} ] 的违规物品已全部清理");
                        else
                            player.SendInfoMessage("您的违规物品已全部清理");
                    }
                }
                //sw.Stop();
                //TSPlayer.All.SendMessage("耗时: " + sw.Elapsed, Color.Aqua);
            }
            return flag;
        }


        /// <summary>
        /// 接受items，检查是否含有违禁物，将违禁物写入citems，文本信息写入text
        /// </summary>
        /// <param name="items">需要检查的物品集合</param
        /// <param name="abItem"><违规物，违规原因></param>
        /// <returns>如果含有违规物，返回真，否则返回假</returns>
        public bool ReasonableItem(List<Item> items, out Dictionary<Item, string> abItem)
        {
            NPCKillsTracker NKT = Main.BestiaryTracker.Kills;
            abItem = new Dictionary<Item, string>();

            foreach (Item i in items)
            {
                //如果是豁免物品并且不在强制检查物里，不算作弊，这个得放到最前面
                if (config.不需要被作弊检查的物品ID.Keys.Contains(i.type) && !config.必须被检查的物品_覆盖上面一条.Keys.Contains(i.type))
                {
                    continue;
                }

                //常规检查非生物掉落物和非制作物品
                if (!NPC.downedMoonlord && CheatData.月亮领主后.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedTowerVortex && i.type == 3456 && (!NPC.downedTowerStardust || !NPC.downedTowerSolar || !NPC.downedTowerNebula))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedTowerNebula && i.type == 3457 && (!NPC.downedTowerSolar || !NPC.downedTowerStardust || !NPC.downedTowerVortex))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedTowerSolar && i.type == 3458 && (!NPC.downedTowerStardust || !NPC.downedTowerVortex || !NPC.downedTowerNebula))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedTowerStardust && i.type == 3459 && (!NPC.downedTowerSolar || !NPC.downedTowerNebula || !NPC.downedTowerVortex))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedGolemBoss && CheatData.石巨人后_邪教徒前.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedPlantBoss && CheatData.世纪之花后_石巨人前.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) && CheatData.所有机械Boss后_世花前.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedMechBossAny && CheatData.任意一个机械Boss后.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!Main.hardMode && CheatData.肉山后_任意一个机械Boss前.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedBoss3 && CheatData.骷髅王后_肉山前.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if (!NPC.downedBoss2 && CheatData.邪恶Boss后_骷髅王前.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }

                //单独boss的专家物品判断，因为全员npc掉落物检查不包含专家物品
                if (!NPC.downedEmpressOfLight && i.type == 4989)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                else if (!NPC.downedFishron && i.type == 3367)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                else if (!NPC.downedQueenSlime && i.type == 4987)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                else if (!NPC.downedDeerclops && i.type == 5100)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                else if (!NPC.downedQueenBee && i.type == 3333)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                else if (!NPC.downedBoss1 && i.type == 3097)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                else if (!NPC.downedSlimeKing && i.type == 3090)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }


                #region 各种其他情况下可以制作的物品
                //奥库瑞姆的剃刀
                if (!Main.zenithWorld)
                {
                    if (i.type == 5334)
                    {
                        abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                    }
                }
                //华美甜点
                if (!NPC.downedSlimeKing && !NPC.downedQueenSlime && i.type == 5131)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                #endregion


                #region 全部 NPC 掉落物检查
                List<DropRateInfo> drops = new List<DropRateInfo>();
                //<物品，掉落者ID>
                List<KeyValuePair<Item, int>> und1 = new List<KeyValuePair<Item, int>>();//这种生物未击败时获得的违禁物
                List<KeyValuePair<Item, int>> und2 = new List<KeyValuePair<Item, int>>();//从另一种会掉落相同物品但是已经击败过的生物身上获得的可获得物（如所有的史莱姆都会掉落史莱姆法杖）

                for (int id = -65; id < NPCID.Count; id++)
                {
                    List<IItemDropRule> rulesForNPCID = Main.ItemDropsDB.GetRulesForNPCID(id, false);
                    foreach (IItemDropRule itemDropRule in rulesForNPCID)
                    {
                        itemDropRule.ReportDroprates(drops, new DropRateInfoChainFeed(1f));
                    }
                    foreach (DropRateInfo info in drops)
                    {
                        if (NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[id]) == 0 && i.type == info.itemId && !und1.Exists(x => x.Key.type == i.type))
                        {
                            und1.Add(new KeyValuePair<Item, int>(i, id));
                        }
                        if (NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[id]) > 0 && i.type == info.itemId && !und2.Exists(x => x.Key.type == i.type))
                        {
                            und2.Add(new KeyValuePair<Item, int>(i, id));
                        }
                    }
                    drops.Clear();
                }
                und1.RemoveAll(x => und2.Exists(y => y.Key.type == x.Key.type) || CheatData.AllNPCLootExclude.Contains(x.Key.type));
                foreach (var t in und1)
                {
                    if (!t.Key.IsAir)
                    {
                        abItem.TryAdd(t.Key, $"掉落物错误根据图鉴: {t.Key.Name}[{t.Key.netID}] < {Lang.GetNPCNameValue(t.Value)}[{t.Value}]");
                    }
                }
                //unl1.RemoveAll(x => unl2.Contains(x) || CheatData.AllNPCLootExclude.Contains(x.type));//从unl1中移除所有unl2中的元素，同时需要排除一些特殊东西
                //citems.AddRange(unl1);//把违禁物加入总统计违禁物中
                #endregion


                #region 以下要去除各种彩蛋种子导致的掉落物调换顺序等各种复杂情况（泰拉越更新统计越头疼>_<）
                //十周年移除史莱姆法杖等
                if (Main.tenthAnniversaryWorld || Main.zenithWorld)
                {
                    var temp = abItem.Where(x => x.Key.netID == 1309 || x.Key.netID == 905).ToArray();
                    foreach (var v in temp)
                    {
                        abItem.Remove(v.Key);
                    }
                }
                //如果未打败任何非史后的肉后boss,开发者套的判断
                if (!NPC.downedMechBossAny && !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedEmpressOfLight && !NPC.downedMoonlord && CheatData.开发者时装.Contains(i.type))
                {
                    abItem.TryAdd(i, $"开发者时装错误: {i.Name}[{i.type}]");
                }
                //天顶种子和上挖种子中物品调换的修正
                if (Main.remixWorld || Main.zenithWorld)
                {
                    if (!NPC.downedBoss3 && (i.type == 683 || i.type == 2623))//邪恶三叉戟和泡泡枪由击败骷髅王获得
                    {
                        abItem.TryAdd(i, $"颠倒世界的时期错误: {i.Name}[{i.type}]");
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[629]) == 0) && (i.type == 1319 || i.type == 1264 || i.type == 676))//雪球炮，寒霜之花，霜印剑，肉后冰雪宝箱怪，不该出现在肉前
                    {
                        abItem.TryAdd(i, $"颠倒世界的时期错误: {i.Name}[{i.type}]");
                    }
                    if (NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[85]) == 0 && i.type == 517)//魔法飞刀，肉前保险怪
                    {
                        abItem.TryAdd(i, $"颠倒世界的时期错误: {i.Name}[{i.type}]");
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[85]) == 0) && (i.type == 437 || i.type == 535 || i.type == 536 || i.type == 532 || i.type == 554))//双钩，点金石，泰坦手套，星星斗篷，十字项链，肉前保险怪，不该出现在肉前
                    {
                        abItem.TryAdd(i, $"颠倒世界的时期错误: {i.Name}[{i.type}]");
                    }
                    if (!NPC.downedMechBossAny && i.type == 112)//火之花由红魔鬼掉落
                    {
                        abItem.TryAdd(i, $"颠倒世界的时期错误: {i.Name}[{i.type}]");
                    }
                    if (!NPC.downedPlantBoss && i.type == 2273)//武士刀由花后骷髅怪掉落
                    {
                        abItem.TryAdd(i, $"颠倒世界的时期错误: {i.Name}[{i.type}]");
                    }
                    if (!NPC.downedFishron && i.type == 157)//海蓝权杖由猪鲨掉落
                    {
                        abItem.TryAdd(i, $"颠倒世界的时期错误: {i.Name}[{i.type}]");
                    }
                }
                else//如果不是这些种子，按原方法修正
                {
                    if (!NPC.downedBoss3 && (i.type == 112 || i.type == 157))//火之花和海蓝由击败骷髅王获得
                    {
                        abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[629]) == 0) && (i.type == 676 || i.type == 1264 || i.type == 725))//霜印剑，寒霜之花，冰雪弓，由肉后冰雪宝箱怪获得
                    {
                        abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                    }
                    if ((!Main.hardMode || NKT.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[85]) == 0) && (i.type == 437 || i.type == 535 || i.type == 536 || i.type == 532 || i.type == 554 || i.type == 517))//双钩，点金石，泰坦手套，星星斗篷，十字项链，魔法飞刀，肉后宝箱怪，不该出现在肉前
                    {
                        abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                    }
                    if (!NPC.downedMechBossAny && i.type == 683)//邪恶三叉戟由红魔鬼掉落
                    {
                        abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                    }
                    if (!NPC.downedPlantBoss && i.type == 671)//击败世纪之花掉钥匙剑
                    {
                        abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                    }
                    if (!NPC.downedFishron && i.type == 2623)//泡泡枪由猪鲨掉落
                    {
                        abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                    }
                }

                //这里考虑10周年蒸汽朋克的情况，如果非十周年，且没有的打败任意机械boss
                if ((!Main.tenthAnniversaryWorld && !NPC.downedMechBossAny) && CheatData.Steampunker1.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }//如果是10周年，且没有打败机械boss
                if (Main.tenthAnniversaryWorld && !Main.hardMode && CheatData.Steampunker2.Contains(i.type))
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }

                //共鸣法杖
                if (!Main.tenthAnniversaryWorld && !Main.zenithWorld && !NPC.downedPlantBoss && i.type == 5065)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                if ((Main.tenthAnniversaryWorld || Main.zenithWorld) && !Main.hardMode && i.type == 5065)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                //彩虹砖
                if (!Main.tenthAnniversaryWorld && !Main.hardMode && i.type == 662)
                {
                    abItem.TryAdd(i, $"时期错误: {i.Name}[{i.type}]");
                }
                #endregion


                #region 染料类
                if (!Main.hardMode && CheatData.Dye.Contains(i.type))
                {
                    abItem.TryAdd(i, $"非困难模式染料: {i.Name}[{i.type}]");
                }
                if (!NPC.downedMechBossAny && (i.type == 2883 || i.type == 2869 || i.type == 2870 || i.type == 2873))
                {
                    abItem.TryAdd(i, $"非困难模式染料: {i.Name}[{i.type}]");
                }
                if (!NPC.downedPlantBoss && (i.type == 2878 || i.type == 2879 || i.type == 2884 || i.type == 2885))
                {
                    abItem.TryAdd(i, $"非困难模式染料: {i.Name}[{i.type}]");
                }
                if (!NPC.downedMartians && (i.type == 2864 || i.type == 3556))
                {
                    abItem.TryAdd(i, $"非困难模式染料: {i.Name}[{i.type}]");
                }
                #endregion


                #region 特殊情况
                //世界吞噬怪的鳞甲，因为吞噬怪可以在未被打败的情况下掉落鳞甲，所以生物掉落检测会出错
                if (i.type == 86)
                {
                    for (int s = 0; s < 200; s++)
                    {
                        if (Main.npc[s].active && (Main.npc[s].type == 13 || Main.npc[s].type == 14 || Main.npc[s].type == 15))
                        {
                            var temp = abItem.Where(x => x.Key.netID == 86).ToArray();
                            foreach (var v in temp)
                            {
                                abItem.Remove(v.Key);
                            }
                            break;
                        }
                    }
                }
                #endregion


                //如果是强制检查物，将违规物添加进去，这个得放到最后面
                if (config.必须被检查的物品_覆盖上面一条.Keys.Contains(i.type) && abItem.Where(x => x.Key.type == i.type).ToArray().Length == 0)
                {
                    abItem.TryAdd(i, $"违禁物: {i.Name}[{i.type}]");
                }
            }

            //如果一个都没，合理
            if (abItem.Count == 0)
                return true;
            else
                return false;
        }


        /// <summary>
        /// 物品作弊检查，仅检查物品的配方，但不会检查配方的配方(开销太大)
        /// </summary>
        /// <param name="items"> 需要检查的物品 </param>
        /// <param name="abItems"> 返回<违规物品，该物品违规的原因> </param>
        /// <returns></returns>
        public bool ReasonableItemOfRecipe(List<Item> items, out Dictionary<Item, string> abItems)
        {
            bool reasonable = true;
            Dictionary<Item, string> weigui = new Dictionary<Item, string>();//配方里违规物品的集合（统计所有配方）
            abItems = new Dictionary<Item, string>();
            foreach (Item i in items)
            {
                if (config.不需要被作弊检查的物品ID.Keys.Contains(i.type) || CheatData.AllRecipeExclude.Contains(i.type))
                    continue;

                bool makeSucce = false;//这个物品的配方没问题？
                bool hasRecipe = false;//这个物品有配方？
                //遍历所有配方
                foreach (Recipe recipe in Main.recipe)
                {
                    //这个配方的最终产物就是你要查的物品
                    if (recipe.createItem.type == i.type)
                    {
                        hasRecipe = true;//这个物品确实有配方
                        //这个配方的材料们
                        List<Item> list = recipe.requiredItem.ToList<Item>();
                        //把材料们的空材料和不需要被检查的材料移除
                        //list.RemoveAll(x => x.type == 0 || CheatData.AllRecipeExcludeMaterial.Contains(x.type));
                        list.RemoveAll(x => x.type == 0 || x.type == 23);
                        //验证材料们是否合理
                        if (!ReasonableItem(list, out Dictionary<Item, string> temp))
                        {
                            makeSucce = false;//材料们不合理
                            foreach (var v in temp)
                            {
                                weigui.TryAdd(v.Key, v.Value);
                            }
                        }
                        else
                            makeSucce = true; //材料们合理
                        //只要材料们都合理则说明这个配方可做，这个物品合理，就没必要验证该物品的其他配方了
                        if (makeSucce)
                            break;
                    }
                }
                //只有该物品有配方且所有配方都有问题才算制作失败。无配方代表这个物品判断失败，暂定为合理，等后续其他判断
                if (hasRecipe && !makeSucce)
                {
                    reasonable = false;
                    //移除weigui里同ID的东西
                    /*
                    Dictionary<int, Item> keyValuePairs = new Dictionary<int, Item>();
                    Dictionary<Item, string> keyValuePairs2 = new Dictionary<Item, string>();
                    foreach (var v in weigui.Keys.ToArray())
                    {
                        keyValuePairs.TryAdd(v.netID, v);
                    }
                    foreach (var v in keyValuePairs.Values.ToArray())
                    {
                        if (weigui.TryGetValue(v, out string value))
                        {
                            keyValuePairs2.Add(v, value);
                        }
                    }
                    */
                    //weigui = keyValuePairs2;
                    string str = $"配方错误: {i.Name}[{i.type}]->(";
                    foreach (var v in weigui)
                    {
                        str += v.Value + "，";
                    }
                    str = str.Trim('，');
                    str += ')';
                    abItems.TryAdd(i, str);
                    weigui.Clear();
                }
            }
            return reasonable;
        }


        /// <summary>
        /// 创建好Watcher文件夹。watcher日志文件夹-日志。作弊文件夹-作弊日志
        /// </summary>
        public void SetWatcherFile(string DirPath = "")
        {
            string filePath;
            if (DirPath == logDirPath || DirPath == "")
            {
                if (!Directory.Exists(logDirPath))
                {
                    Directory.CreateDirectory(logDirPath);
                }
                filePath = DateTime.Now.ToString("s");
                filePath = filePath.Replace(":", "-");
                logFilePath = Path.Combine(logDirPath, filePath + "_Watcher.log");
                if (!File.Exists(logFilePath))
                {
                    File.CreateText(logFilePath).Close();
                }
            }
            if (DirPath == cheatLogDirPath || DirPath == "")
            {
                if (!Directory.Exists(cheatLogDirPath))
                {
                    Directory.CreateDirectory(cheatLogDirPath);
                }
                filePath = DateTime.Now.ToString("s");
                filePath = filePath.Replace(":", "-");
                cheatLogFilePath = Path.Combine(cheatLogDirPath, filePath + "_Cheat.log");
                if (!File.Exists(cheatLogFilePath))
                {
                    File.CreateText(cheatLogFilePath).Close();
                }
            }
        }


        /// <summary>
        /// 从dir文件夹里删除距今超过min时常创建的旧文件
        /// </summary>
        /// <param name="dir">文件夹</param>
        /// <param name="min">分钟</param>
        public static void DeleteOldFiles(string dir, int min)
        {
            try
            {
                if (!Directory.Exists(dir) || min < 1)
                    return;
                DateTime now = DateTime.Now;
                foreach (var f in Directory.GetFileSystemEntries(dir).Where(f => File.Exists(f)))
                {
                    DateTime ft = File.GetCreationTime(f);
                    if (ft.AddMinutes(min) < now)
                    {
                        File.Delete(f);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Watcher.DeleteOldFiles:" + e.Message);
                TShock.Log.Error("Watcher.DeleteOldFiles:" + e.Message);
            }
        }


        /// <summary>
        /// 当日志过大时重新创建日志
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="filePath"></param>
        public void AvoidLogSize(string dirPath, string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 1024 * 1024 * config.Watcher日志的最大体积_MB)
                {
                    SetWatcherFile(dirPath);
                }
            }
            else
            {
                SetWatcherFile(dirPath);
            }
        }


        /// <summary>
        /// 增加某个玩家的总计作弊次数，每调用一次，作弊次数+1
        /// </summary>
        /// <param name="e"></param>
        /// <param name="cheatType">作弊类型 0：钓鱼作弊，1：射弹伤害作弊，2：物品作弊，3: 星璇机枪作弊</param>，
        /// <returns>返回计算后的总作弊次数</returns>
        public int AddCheatingPlayers(TSPlayer e, TypesOfCheat cheatType)
        {
            int cheatTimes = 0;
            bool flag = false;
            foreach (WPlayer w in wPlayers)
            {
                //如果是惯犯
                if (w.uuid == e.UUID)
                {
                    flag = true;
                    switch (cheatType)
                    {
                        case TypesOfCheat.FishCheat:
                            w.钓鱼作弊次数++;
                            break;
                        case TypesOfCheat.DamageCheat:
                            w.伤害作弊次数++;
                            break;
                        case TypesOfCheat.ItemCheat:
                            w.物品作弊次数++;
                            break;
                        default:
                            break;
                    }
                    cheatTimes = w.总作弊次数;
                    WPlayer.SaveConfigFile();
                    break;
                }
            }
            //初犯
            if (!flag)
            {
                wPlayers.Add(new WPlayer(e.Name, e.UUID, 0, 0, 0));
                cheatTimes = AddCheatingPlayers(e, cheatType);
                //在对wPlayers做出修改后，这里对配置文件更新一下
                //WPM.SaveConfigFile();
                return cheatTimes;
            }
            else
            {
                return cheatTimes;
            }
        }


        /// <summary>
        /// 返回是否该玩家在被检查的范围内
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool NeedBeChecked(TSPlayer v)
        {
            if (v == null || !v.Active || v.Index == -1)
            {
                return false;
            }
            bool flag = false;
            foreach (string str in config.检测哪些玩家组)
            {
                if (v.Group.Name.Equals(str, StringComparison.OrdinalIgnoreCase))
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
        /// 返回 参数 / 1 的概率
        /// 直接用泰拉的随机数生成器弄的
        /// </summary>
        /// <param name="d">分子</param>
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
            int a = (int)(d * 1000000);
            if (Main.rand.Next(0, 1000000) <= a)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 对作弊或违规玩家进行警告惩罚或封禁，(默认作弊次数+1)，写入w日志和作弊记录日志和tshock日志
        /// </summary>
        /// <param name="p">作弊玩家</param>
        /// <param name="ctype">作弊类型</param>
        /// <param name="ptype">警告类型</param>
        /// <param name="projName">当ctype等于 FishCheat，DamageCheat 时，需要设置这个值</param>
        /// <param name="projDamage">当ctype等于 FishCheat，DamageCheat 时，需要设置这个值</param>
        /// <param name="projNetID">当ctype等于 FishCheat，DamageCheat 时，需要设置这个值</param>
        /// <param name="projPos">当ctype等于 FishCheat，DamageCheat 时，需要设置这个值</param>
        /// <param name="abItemMessage">当ctype等于 ItemCheat 时，需要设置这个值，其他情况为 ""</param>
        /// <param name="IncludeCheatTimes">是否计入总作弊次数，这会在到达一定值时ban掉玩家</param>
        /// <returns> 返回作弊的具体信息 </returns>
        public string Warning(TSPlayer p, TypesOfCheat ctype, TypesOfPunish ptype, bool IncludeCheatTimes, string projName = "", int projDamage = 0, int projNetID = 0, Vector2 projPos = default, List<string> abItemMessage = default)
        {
            if (!NeedBeChecked(p))
            {
                return "";
            }
            //总作弊次数
            int num = 0;
            //原因
            StringBuilder reason = new StringBuilder();
            //是否计入违规次数的说明
            string str = "";
            if (IncludeCheatTimes)
            {
                num = AddCheatingPlayers(p, ctype);
            }
            else
            {
                foreach (var v in wPlayers)
                {
                    if (v.uuid == p.UUID)
                    {
                        num = v.总作弊次数; break;
                    }
                }
                str = "（非违规）";
            }

            switch (ctype)
            {
                case TypesOfCheat.FishCheat:
                    reason.Append($"[{p.Account.ID}][{p.Name}] 手持 {p.TPlayer.HeldItem.Name} 掷出的 {projName} 浮标数目不正常 在 {{X:{(int)(projPos.X / 16)}, Y:{(int)(projPos.Y / 16)}}} 违规次数 {num}{str}");
                    break;
                case TypesOfCheat.DamageCheat:
                    if (projNetID != 0)//射弹作弊
                        reason.Append($"[{p.Account.ID}][{p.Name}] 伤害 {projDamage} 过高 生成 {projName}[{projNetID}] 在 {{X:{(int)(projPos.X / 16)}, Y:{(int)(projPos.Y / 16)}}} 违规次数 {num}{str}");
                    else//非射弹作弊
                        reason.Append($"[{p.Account.ID}][{p.Name}] 伤害 {projDamage} 过高 手持 {p.TPlayer.HeldItem.Name}[{p.TPlayer.HeldItem.netID}] 在 {{X:{(int)(projPos.X / 16)}, Y:{(int)(projPos.Y / 16)}}} 违规次数 {num}{str}");
                    break;
                case TypesOfCheat.ItemCheat:
                    reason.AppendLine($"[{p.Account.ID}][{p.Name}] 物品异常");
                    foreach (var v in abItemMessage)
                    {
                        reason.AppendLine(v);
                    }
                    reason.Append($"在 {{X:{(int)(p.LastNetPosition.X / 16)}, Y:{(int)(p.LastNetPosition.Y / 16)}}} 违规次数 {num}{str}");
                    break;
                case TypesOfCheat.DangerousProj:
                    reason.Append($"[{p.Account.ID}][{p.Name}] 生成 {projName}[{projNetID}] 过于频繁在 {{X:{(int)(projPos.X / 16)}, Y:{(int)(projPos.Y / 16)}}}");
                    break;
                default:
                    break;
            }

            if (num < config.最多违规作弊次数)
            {
                switch (ptype)
                {
                    case TypesOfPunish.oral:
                        p.SendMessage("【私聊警告】" + reason, Color.Orange);
                        SendPlayerText(p, "【私聊警告】" + reason, Color.Red, p.LastNetPosition);
                        break;
                    case TypesOfPunish.publicWarning:
                        TSPlayer.All.SendMessage("【警告】" + reason, Color.Orange);
                        break;
                    case TypesOfPunish.disable:
                        TSPlayer.All.SendMessage("【警告】" + reason, Color.OrangeRed);
                        SendPlayerText(p, "【警告】" + reason, Color.Red, p.LastNetPosition);
                        p.SetBuff(149, 360);
                        p.SetBuff(156, 360);
                        p.SetBuff(47, 150);
                        p.SetBuff(23, 150);
                        break;
                    case TypesOfPunish.kill:
                        TSPlayer.All.SendMessage("【警告】" + reason, Color.OrangeRed);
                        SendPlayerText(p, "【警告】" + reason, Color.Red, p.LastNetPosition);
                        p.KillPlayer();
                        break;
                    case TypesOfPunish.kick:
                        TSPlayer.All.SendMessage("【警告】" + reason, Color.Red);
                        p.Kick("【警告】" + reason, true);
                        break;
                    default:
                        break;
                }
            }

            if (num < config.最多违规作弊次数)
            {
                Console.WriteLine(DateTime.Now.ToString("u") + " " + reason);
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason });
                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason });
                TShock.Log.Warn(DateTime.Now.ToString("u") + " " + reason);
            }
            else
            {
                p.Ban($"总计违规次数已达到 {config.最多违规作弊次数} 次！若有疑问请及时联系管理员", "作弊检测 by Watcher");
                Console.WriteLine(DateTime.Now.ToString("u") + " " + reason + " 已封禁");
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason + " 已封禁" });
                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + " " + reason + " 已封禁" });
                TShock.Log.Warn(DateTime.Now.ToString("u") + " " + reason + " 已封禁");
            }
            return reason.ToString();
        }


        /// <summary>
        /// 清空玩家库存中 item.type 符合 items 的物品，(顺便清理所有 buff )只比较id不比较前缀，数量等，id 就是 type
        /// 我觉得有更好的写法，但我不会，我感觉我这样写也没什么问题，就是代码多点
        /// </summary>
        /// <param name="items"> 需要清空的物品 </param>
        /// <param name="tSPlayer"> 需要被清理的玩家 </param>
        public void ClearPlayersItem(Item[] items, TSPlayer tSPlayer)
        {
            if (tSPlayer == null || !tSPlayer.IsLoggedIn)
            {
                return;
            }
            for (int i = 0; i < tSPlayer.TPlayer.inventory.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.inventory[i].IsAir && tSPlayer.TPlayer.inventory[i].type == item.type)
                    {
                        tSPlayer.TPlayer.inventory[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Inventory0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.armor.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.armor[i].IsAir && tSPlayer.TPlayer.armor[i].type == item.type)
                    {
                        tSPlayer.TPlayer.armor[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Armor0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.Loadouts[0].Armor.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.Loadouts[0].Armor[i].IsAir && tSPlayer.TPlayer.Loadouts[0].Armor[i].type == item.type)
                    {
                        tSPlayer.TPlayer.Loadouts[0].Armor[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Loadout1_Armor_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.Loadouts[1].Armor.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.Loadouts[1].Armor[i].IsAir && tSPlayer.TPlayer.Loadouts[1].Armor[i].type == item.type)
                    {
                        tSPlayer.TPlayer.Loadouts[1].Armor[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Loadout2_Armor_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.Loadouts[2].Armor.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.Loadouts[2].Armor[i].IsAir && tSPlayer.TPlayer.Loadouts[2].Armor[i].type == item.type)
                    {
                        tSPlayer.TPlayer.Loadouts[2].Armor[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Loadout3_Armor_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.dye.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.dye[i].IsAir && tSPlayer.TPlayer.dye[i].type == item.type)
                    {
                        tSPlayer.TPlayer.dye[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Dye0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.Loadouts[0].Dye.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.Loadouts[0].Dye[i].IsAir && tSPlayer.TPlayer.Loadouts[0].Dye[i].type == item.type)
                    {
                        tSPlayer.TPlayer.Loadouts[0].Dye[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Loadout1_Dye_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.Loadouts[1].Dye.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.Loadouts[1].Dye[i].IsAir && tSPlayer.TPlayer.Loadouts[1].Dye[i].type == item.type)
                    {
                        tSPlayer.TPlayer.Loadouts[1].Dye[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Loadout2_Dye_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.Loadouts[2].Dye.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.Loadouts[2].Dye[i].IsAir && tSPlayer.TPlayer.Loadouts[2].Dye[i].type == item.type)
                    {
                        tSPlayer.TPlayer.Loadouts[2].Dye[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Loadout3_Dye_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.miscEquips.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.miscEquips[i].IsAir && tSPlayer.TPlayer.miscEquips[i].type == item.type)
                    {
                        tSPlayer.TPlayer.miscEquips[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Misc0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.miscDyes.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.miscDyes[i].IsAir && tSPlayer.TPlayer.miscDyes[i].type == item.type)
                    {
                        tSPlayer.TPlayer.miscDyes[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.MiscDye0 + i);
                    }
                }
            }
            foreach (Item item in items)
            {
                if (!tSPlayer.TPlayer.trashItem.IsAir && tSPlayer.TPlayer.trashItem.type == item.type)
                {
                    tSPlayer.TPlayer.trashItem.TurnToAir();
                    tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.TrashItem);
                }
            }
            foreach (Item item in items)
            {
                if (!tSPlayer.TPlayer.inventory[tSPlayer.TPlayer.selectedItem].IsAir && tSPlayer.TPlayer.inventory[tSPlayer.TPlayer.selectedItem].type == item.type)
                {
                    tSPlayer.TPlayer.inventory[tSPlayer.TPlayer.selectedItem].TurnToAir();
                    tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.InventoryMouseItem);
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.bank.item.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.bank.item[i].IsAir && tSPlayer.TPlayer.bank.item[i].type == item.type)
                    {
                        tSPlayer.TPlayer.bank.item[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Bank1_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.bank2.item.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.bank2.item[i].IsAir && tSPlayer.TPlayer.bank2.item[i].type == item.type)
                    {
                        tSPlayer.TPlayer.bank2.item[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Bank2_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.bank3.item.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.bank3.item[i].IsAir && tSPlayer.TPlayer.bank3.item[i].type == item.type)
                    {
                        tSPlayer.TPlayer.bank3.item[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Bank3_0 + i);
                    }
                }
            }
            for (int i = 0; i < tSPlayer.TPlayer.bank4.item.Length; i++)
            {
                foreach (Item item in items)
                {
                    if (!tSPlayer.TPlayer.bank4.item[i].IsAir && tSPlayer.TPlayer.bank4.item[i].type == item.type)
                    {
                        tSPlayer.TPlayer.bank4.item[i].TurnToAir();
                        tSPlayer.SendData(PacketTypes.PlayerSlot, "", tSPlayer.Index, PlayerItemSlotID.Bank4_0 + i);
                    }
                }
            }
            for (int i = 0; i < 22; i++)
            {
                tSPlayer.TPlayer.buffType[i] = 0;
            }
            tSPlayer.SendData(PacketTypes.PlayerBuff, "", tSPlayer.Index, 0f, 0f, 0f, 0);
        }
    }
}
