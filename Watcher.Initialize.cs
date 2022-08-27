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
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using OTAPI;

namespace Watcher
{
    public partial class Watcher : TerrariaPlugin
    {
        //对话
        private void OnChat(ServerChatEventArgs args)
        {
            if (args == null || Main.player[args.Who] == null || !Main.player[args.Who].active)
            {
                return;
            }
            if (config.whetherToWriteTheConversationContentInTheLog_是否把对话内容写入日志)
            {
                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                if (TShock.Players[args.Who].Account != null)
                    File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{TShock.Players[args.Who].Account.ID}][{Main.player[args.Who].name}]: \"{args.Text}\" in {Main.player[args.Who].position / 16}" });
                else
                    File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:?][{Main.player[args.Who].name}]: \"{args.Text}\" in {Main.player[args.Who].position / 16}" });
            }
        }


        //扔掉某项东西
        private void SetDropItemLog(object sender, GetDataHandlers.ItemDropEventArgs e)
        {
            if (!config.whetherToWriteTheDiscardsIntoTheLog_是否把丢弃物写入日志 || CheatData.ImmunityDropItems.Contains(e.Type))
                return;

            //TShock.Log.Write("[{0}]丢弃{1}个{2}在{3}", e.Player.Name, e.Stacks, Lang.GetItemNameValue((int)e.Type), e.Player.LastNetPosition/16), TraceLevel.Info);
            //Console.WriteLine(DateTime.Now.ToString("u") + " [{0}] 丢弃 {1} 个 {2} 在 {3}", e.Player.Name, e.Stacks, Lang.GetItemNameValue((int)e.Type), e.Player.LastNetPosition / 16));
            AvoidLogSize("logDirPath", logDirPath, logFilePath);
            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 丢弃 {e.Stacks} 个 {Lang.GetItemNameValue((int)e.Type)} 在 {e.Player.LastNetPosition / 16}" });
        }


        //手持某样东西
        private void SetItemLog(object sender, GetDataHandlers.PlayerSlotEventArgs e)
        {
            if (!config.whetherToWriteTheHoldingObjectIntoTheLog_是否把手持物写入日志)
                return;

            if (e.Slot == 58 && e.Stack != 0 && !CheatData.ImmunityHoldItems.Contains(e.Type))
            {
                //TShock.Log.Write("[{0}]拿了{1}个{2}在{3}", e.Player.Name, e.Stack, Lang.GetItemNameValue((int)e.Type), e.Player.LastNetPosition/16), TraceLevel.Info);
                //Console.WriteLine(DateTime.Now.ToString("u") + " [{0}] 拿持 {1} 个 {2} 在 {3}", e.Player.Name, e.Stack, Lang.GetItemNameValue((int)e.Type), e.Player.LastNetPosition / 16));
                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 拿持 {e.Stack} 个 {Lang.GetItemNameValue((int)e.Type)} 在 {e.Player.LastNetPosition / 16}" });
            }
        }


        //生成某些射弹
        private void SetProjLog(object sender, GetDataHandlers.NewProjectileEventArgs e)
        {
            if (!config.whetherToWriteTheProjectilesIntoTheLog_是否把生成射弹写入日志)
                return;

            if (CheatData.DangerousProjectile.Contains(e.Type))
            {
                //Console.WriteLine(DateTime.Now.ToString("u") + " [{0}] 生成 {1} 在 {2}", e.Player.Name, Lang.GetProjectileName(e.Type), e.Position / 16));
                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 生成 {Lang.GetProjectileName(e.Type)} 在 {e.Position / 16}" });
            }
        }

        /*
        //召唤boss写入日志
        private void SummonBoss(GetDataEventArgs args)
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
        */

        /*
        //放置物写入日志
        private void PlaceTiles(object sender, GetDataHandlers.PlaceObjectEventArgs e)
        {
            e.Player.SendErrorMessage(DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 放置了 {TileObject.} 在 {new Vector2(e.X, e.Y)}");
            AvoidLogSize("logDirPath", logDirPath, logFilePath);
            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 放置了 {Lang.GetMapObjectName(e.Type)} 在 {new Vector2(e.X, e.Y)}" });
        }
        */

        //游戏运行
        private void GameRun(EventArgs args)
        {
            LogClean();
            if (config.backUpTshockSql_是否备份tshockSql)
                BackUpTshockSql();
            if (config.enableItemDetection_启用物品作弊检测 && Main.timeForVisualEffects != 0)
                ItemCheatingCheck(args, null, null, 1);
        }


        //射弹作弊检查检查
        private void ProjCheatingCheck(object sender, GetDataHandlers.NewProjectileEventArgs e)
        {
            #region 射弹伤害检测
            if (config.enableProjDamage_启用射弹伤害检测 && KickOrBanGroupAllow(e.Player))
            {
                //判断伤害溢出的情况，排除灰橙冲击枪、尖桩发射器、暗影武士长枪
                bool isExclude = e.Type == 876 || e.Type == 323 || e.Type == 878;
                //如果伤害 > 3000并且不在可排除的范围内，警告踢掉
                if (e.Damage >= 3000 && !isExclude)
                {
                    int num = CheakingPlayers(e.Player, "damage");
                    if (num < config.numberOfBan_允许的违规次数)
                    {
                        e.Player.Kick($"不正常的伤害，违规次数 {num}，达到{config.numberOfBan_允许的违规次数}次直接封禁\n若有疑问请及时向管理员联系");
                    }
                    else
                    {
                        e.Player.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员", true);
                    }
                    Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 伤害 {e.Damage} 过高 生成 {Lang.GetProjectileName(e.Type)} 在 {e.Position / 16} 违规次数 {num}");
                    AvoidLogSize("logDirPath", logDirPath, logFilePath);
                    AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                    File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 伤害 {e.Damage} 过高 生成 {Lang.GetProjectileName(e.Type)} 在 {e.Position / 16} 违规次数 {num}" });
                    File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 伤害 {e.Damage} 过高 生成 {Lang.GetProjectileName(e.Type)} 在 {e.Position / 16} 违规次数 {num}" });
                    //及时把违规射弹杀掉。
                    Main.projectile[e.Type].Kill();
                }
            }
            #endregion


            #region 钓鱼检测
            if (config.enableBobberNum_启用浮标数目检测 && KickOrBanGroupAllow(e.Player))
            {
                int bobber1 = 0;     //木质 proj: 360   item: 2289
                int bobber2 = 0;     //强化 proj: 361   item: 2291
                int bobber3 = 0;     //腐化 proj: 363   item: 2293
                int bobber4 = 0;     //血腥 proj: 381   item: 2421
                int bobber5 = 0;     //血月 proj: 760   item: 4325
                int bobber6 = 0;     //甲虫 proj: 775   item: 4442
                int bobber7 = 0;     //玻璃 proj: 362   item: 2292
                int bobber8 = 0;     //机械 proj: 365   item: 2295
                int bobber9 = 0;     //坐鸭 proj: 366   item: 2296
                int bobber10 = 0;    //热线 proj: 382   item: 2422
                int bobber11 = 0;    //黄金 proj: 364   item: 2294

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].owner == e.Owner && Main.player[Main.projectile[i].owner].name == e.Player.Name)
                    {
                        if (Main.projectile[i].type == 360)
                            bobber1++;
                        if (Main.projectile[i].type == 361)
                            bobber2++;
                        if (Main.projectile[i].type == 363)
                            bobber3++;
                        if (Main.projectile[i].type == 381)
                            bobber4++;
                        if (Main.projectile[i].type == 760)
                            bobber5++;
                        if (Main.projectile[i].type == 775)
                            bobber6++;
                        if (Main.projectile[i].type == 362)
                            bobber7++;
                        if (Main.projectile[i].type == 365)
                            bobber8++;
                        if (Main.projectile[i].type == 366)
                            bobber9++;
                        if (Main.projectile[i].type == 382)
                            bobber10++;
                        if (Main.projectile[i].type == 364)
                            bobber11++;

                        if (bobber1 + bobber2 + bobber3 + bobber4 + bobber5 + bobber6 + bobber7 + bobber8 + bobber9 + bobber10 + bobber11 >= 2)
                        {
                            int target = 0;

                            int num = CheakingPlayers(e.Player, "fishing");
                            if (num < config.numberOfBan_允许的违规次数)
                            {
                                if (bobber1 > 1)
                                {
                                    target = 2289;
                                    e.Player.Kick($"木质钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber2 > 1)
                                {
                                    target = 2291;
                                    e.Player.Kick($"强化钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber3 > 1)
                                {
                                    target = 2293;
                                    e.Player.Kick($"腐化钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber4 > 1)
                                {
                                    target = 2421;
                                    e.Player.Kick($"血腥钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber5 > 1)
                                {
                                    target = 4325;
                                    e.Player.Kick($"血月钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber6 > 1)
                                {
                                    target = 4442;
                                    e.Player.Kick($"甲虫钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber7 > 1)
                                {
                                    target = 2292;
                                    e.Player.Kick($"玻璃钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber8 > 1)
                                {
                                    target = 2295;
                                    e.Player.Kick($"机械钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber9 > 1)
                                {
                                    target = 2296;
                                    e.Player.Kick($"坐鸭钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber10 > 1)
                                {
                                    target = 2422;
                                    e.Player.Kick($"热线钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }
                                else if (bobber11 > 1)
                                {
                                    target = 2294;
                                    e.Player.Kick($"黄金钓竿浮标数目异常，违规次数{num}，如有错误，请及时向管理员反馈");
                                }

                                Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 掷出的 {Lang.GetItemName(target)} 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}");
                                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                                AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 掷出的 {Lang.GetItemName(target)} 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 掷出的 {Lang.GetItemName(target)} 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                                break;
                            }
                            else
                            {
                                e.Player.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员", true);
                                Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}");
                                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                                AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                                break;
                            }
                        }
                    }
                }
            }
            #endregion
        }


        //物品进度检查
        private void ItemCheatingCheck(object sender, GetDataHandlers.PlayerSlotEventArgs e)
        {
            if (!config.enableItemDetection_启用物品作弊检测)
            {
                return;
            }
            TSPlayer player = e.Player;
            bool flag = false;
            for (int i = 0; i < OnlineCheakingPlayers.Count; i++)
            {
                if (e.Player.IsLoggedIn && e.Stack == 1 && OnlineCheakingPlayers[i][0] == player.Name && OnlineCheakingPlayers[i][2].Contains("item"))
                {
                    if (Math.Abs(Main.timeForVisualEffects - int.Parse(OnlineCheakingPlayers[i][3])) > 60 * 12)
                    {
                        ItemCheatingCheck(null, sender, e, 2);
                    }
                    flag = true;
                    break;
                }
            }
            // 正常玩家1/4概率触发，减少服务器负担
            if (!flag && e.Player.IsLoggedIn && e.Stack == 1 && Main.rand.Next(1, 4) == 2)
            {
                ItemCheatingCheck(null, sender, e, 2);
            }
        }


        //在玩家进入时运行
        private void OnServerjoin(JoinEventArgs args)
        {
            //像作弊嫌疑玩家发送消息
            if (args == null || TShock.Players[args.Who] == null)
                return;

            TSPlayer player = TShock.Players[args.Who];
            for (int i = 0; i < OnlineCheakingPlayers.Count; i++)
            {
                if (OnlineCheakingPlayers[i][0] == player.Name && OnlineCheakingPlayers[i][2].Contains("item"))
                {
                    OnlineCheakingPlayers[i][3] = Main.timeForVisualEffects.ToString();
                    player.SendErrorMessage($"请在 10 秒内尽快清理身上违规物品");
                }
                if (OnlineCheakingPlayers[i][0] == player.Name)
                {
                    player.SendErrorMessage($"您已违规{OnlineCheakingPlayers[i][1]}次，最多{config.numberOfBan_允许的违规次数}次开始封禁");
                }
            }
            //向watcherlog写入信息
            int count = 0;
            string sname = "";
            foreach (Player v in Main.player)
            {
                if (v.active && v != null)
                {
                    count++;
                    sname += "[" + v.name + "]";
                }
            }
            //因为刚进去的人不会算到遍历得到的人物组中，所以要手动加
            count++;
            sname += "[" + player.Name + "]";
            AvoidLogSize("logDirPath", logDirPath, logFilePath);
            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" INFORMATION [{player.Name}] 进入游戏，当前在线玩家{count}人：{sname}" });
        }


        //玩家离开时发送消息
        private void OnServerLeave(LeaveEventArgs args)
        {
            //向watcherlog写入消息
            if (args == null || TShock.Players[args.Who] == null)
                return;

            TSPlayer player = TShock.Players[args.Who];
            int count = 0;
            string sname = "";
            foreach (Player v in Main.player)
            {
                if (v.active && v != null)
                {
                    count++;
                    sname += "[" + v.name + "]";
                }
            }
            count--;
            sname = sname.Replace("[" + player.Name + "]", "");
            AvoidLogSize("logDirPath", logDirPath, logFilePath);
            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" INFORMATION [{player.Name}] 已离开，当前在线玩家{count}人：{sname}" });
        }

        
        //击中npc时触发，向攻击保护动物的玩家发送消息,对保护动物进行回血保护
        private HookResult OnStrike(NPC npc, ref double cancelResult, ref int Damage, ref float knockBack, ref int hitDirection, ref bool crit, ref bool noEffect, ref bool fromNet, Entity entity)
        {
            if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(npc.netID))
            {
                npc.HealEffect(Damage);
                Damage = 0;
                knockBack = 0;
                npc.life = npc.lifeMax;
                if (Main.rand.Next(1, 8) == 4 && entity is Player)
                {
                    Player player = entity as Player;
                    TShock.Players[player.whoAmI].SendInfoMessage($"{npc.FullName} 被系统保护，若有疑问请联系管理员");
                }
            }
            return HookResult.Continue;
        }


        //生成生物时触发
        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            NPC npc = Main.npc[args.NpcId];
            if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(npc.netID))
                return;
            if (!npc.boss)
                return;
            TSPlayer.All.SendInfoMessage($"该Boss:{npc.FullName} 被管理员设为禁止伤害，请尽快放弃攻击，不要白费力气 (¦3[▓▓] ");
        }


        //锁住该boss或生物
        private void LockNpc(CommandArgs args)
        {
            if (!args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /locknpc 【NPC id/Boss 汉字名称/Boss 拼音缩写】 来封禁玩家对该boss或npc攻击\nEnter /lock [NPC id] To block the player from attacking the boss or NPC");
                return;
            }
            string text = args.Parameters[0];
            if (args.Parameters.Count > 1)
            {
                text = string.Join(" ", args.Parameters);
            }

            int num = -999;
            string bossName = "";
            //常规boss
            if (text == "slmw" || text == "史莱姆王" || text == "sw" || text == "史王" || int.TryParse(text, out num) && num == NPCID.KingSlime)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.KingSlime))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.KingSlime);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SlimeSpiked))//尖刺史莱姆
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.SlimeSpiked);
                }
                bossName = Lang.GetNPCNameValue(NPCID.KingSlime);
            }
            else if (text == "kslzy" || text == "克苏鲁之眼" || text == "ky" || text == "克眼" || int.TryParse(text, out num) && num == NPCID.EyeofCthulhu)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EyeofCthulhu))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.EyeofCthulhu);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.ServantofCthulhu))//克苏鲁之仆
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.ServantofCthulhu);
                }
                bossName = Lang.GetNPCNameValue(NPCID.EyeofCthulhu);
            }
            else if (text == "sjtsz" || text == "世界吞噬者" || text == "rc" || text == "蠕虫" || int.TryParse(text, out num) && (num == NPCID.EaterofWorldsHead || num == NPCID.EaterofWorldsBody || num == NPCID.EaterofWorldsTail))
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EaterofWorldsHead))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.EaterofWorldsHead);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EaterofWorldsBody))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.EaterofWorldsBody);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EaterofWorldsTail))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.EaterofWorldsTail);
                }
                bossName = "世界吞噬者";
            }
            else if (text == "kslzn" || text == "克苏鲁之脑" || text == "kn" || text == "克脑" || int.TryParse(text, out num) && (num == NPCID.BrainofCthulhu || num == NPCID.Creeper))
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.BrainofCthulhu))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.BrainofCthulhu);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Creeper))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Creeper);
                }
                bossName = Lang.GetNPCNameValue(NPCID.BrainofCthulhu);
            }
            else if (text == "fh" || text == "蜂后" || int.TryParse(text, out num) && num == NPCID.QueenBee)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenBee))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.QueenBee);
                }
                bossName = Lang.GetNPCNameValue(NPCID.QueenBee);
            }
            else if (text == "klw" || text == "骷髅王" || int.TryParse(text, out num) && num == NPCID.SkeletronHead)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SkeletronHead))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.SkeletronHead);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SkeletronHand))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.SkeletronHand);
                }
                bossName = "骷髅王";
            }
            else if (text == "jl" || text == "巨鹿" || text == "ljg" || text == "鹿角怪" || int.TryParse(text, out num) && num == NPCID.Deerclops)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Deerclops))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Deerclops);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Deerclops);
            }
            else if (text == "xrq" || text == "血肉墙" || text == "rs" || text == "肉山" || int.TryParse(text, out num) && num == NPCID.WallofFlesh)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.WallofFlesh))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.WallofFlesh);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.WallofFleshEye))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.WallofFleshEye);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheHungry))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.TheHungry);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheHungryII))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.TheHungryII);
                }
                bossName = Lang.GetNPCNameValue(NPCID.WallofFlesh);
            }
            else if (text == "slmhh" || text == "史莱姆皇后" || text == "sh" || text == "史后" || int.TryParse(text, out num) && num == NPCID.QueenSlimeBoss)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeBoss))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.QueenSlimeBoss);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeMinionBlue))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.QueenSlimeMinionBlue);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeMinionPink))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.QueenSlimeMinionPink);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeMinionPurple))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.QueenSlimeMinionPurple);
                }
                bossName = Lang.GetNPCNameValue(NPCID.QueenSlimeBoss);
            }
            else if (text == "125" || text == "126" || text == "szmy" || text == "双子魔眼")
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(125))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(125);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(126))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(126);
                }
                bossName = "双子魔眼";
            }
            else if (text == "hmz" || text == "毁灭者" || text == "jxrc" || text == "机械蠕虫" || int.TryParse(text, out num) && (num == NPCID.TheDestroyer || num == NPCID.TheDestroyerBody || num == NPCID.TheDestroyerTail))
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheDestroyer))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.TheDestroyer);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheDestroyerBody))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.TheDestroyerBody);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheDestroyerTail))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.TheDestroyerTail);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Probe))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Probe);
                }
                bossName = "毁灭者";
            }
            else if (text == "jxklw" || text == "机械骷髅王" || int.TryParse(text, out num) && (num == NPCID.SkeletronPrime || num == NPCID.PrimeCannon || num == NPCID.PrimeSaw || num == NPCID.PrimeVice || num == NPCID.PrimeLaser))
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(127))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(127);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(128))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(128);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(129))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(129);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(130))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(130);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(131))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(131);
                }
                bossName = "机械骷髅王";
            }
            else if (text == "sjzh" || text == "世纪之花" || text == "世花" || int.TryParse(text, out num) && num == NPCID.Plantera)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Plantera))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Plantera);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.PlanterasTentacle))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.PlanterasTentacle);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Plantera);
            }
            else if (text == "sjr" || text == "石巨人" || int.TryParse(text, out num) && num == NPCID.Golem)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Golem))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Golem);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.GolemFistLeft))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.GolemFistLeft);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.GolemFistRight))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.GolemFistRight);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.GolemHead))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.GolemHead);
                }
                bossName = "石巨人";
            }
            else if (text == "zlygj" || text == "猪龙鱼公爵" || text == "zs" || text == "猪鲨" || int.TryParse(text, out num) && num == NPCID.DukeFishron)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DukeFishron))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.DukeFishron);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Sharkron))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Sharkron);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Sharkron2))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Sharkron2);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DukeFishron);
            }
            else if (text == "636" || text == "gznh" || text == "光之女皇" || text == "gn" || text == "光女")
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.HallowBoss))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.HallowBoss);
                }
                bossName = Lang.GetNPCNameValue(NPCID.HallowBoss);
            }
            else if (text == "xjt" || text == "邪教徒" || text == "byxjt" || text == "拜月邪教徒" || int.TryParse(text, out num) && num == NPCID.CultistBoss)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.CultistBoss))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.CultistBoss);
                }
                bossName = Lang.GetNPCNameValue(NPCID.CultistBoss);
            }
            else if (text == "yqlz" || text == "月球领主" || text == "yllz" || text == "月亮领主" || text == "yz" || text == "月总" || int.TryParse(text, out num) && (num == NPCID.MoonLordHead || num == NPCID.MoonLordHand || num == NPCID.MoonLordCore || num == NPCID.MoonLordLeechBlob))
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordHead))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.MoonLordHead);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordHand))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.MoonLordHand);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordCore))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.MoonLordCore);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordLeechBlob))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.MoonLordLeechBlob);
                }
                bossName = "月球领主";
            }
            //四柱
            else if (text == "ryz" || text == "日耀柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerSolar)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerSolar))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.LunarTowerSolar);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerSolar);
            }
            else if (text == "xxz" || text == "星璇柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerVortex)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerVortex))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.LunarTowerVortex);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerVortex);
            }
            else if (text == "xcz" || text == "星尘柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerStardust)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerStardust))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.LunarTowerStardust);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerStardust);
            }
            else if (text == "xyz" || text == "星云柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerNebula)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerNebula))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.LunarTowerNebula);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerNebula);
            }
            //天国军团boss
            else if (text == "hafs" || text == "黑暗法师" || text == "hamfs" || text == "黑暗魔法师" || int.TryParse(text, out num) && (num == NPCID.DD2DarkMageT1 || num == NPCID.DD2DarkMageT3))
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2DarkMageT1))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.DD2DarkMageT1);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2DarkMageT3))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.DD2DarkMageT3);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DD2DarkMageT1);
            }
            else if (text == "srm" || text == "食人魔" || int.TryParse(text, out num) && (num == NPCID.DD2OgreT2 || num == NPCID.DD2OgreT3))
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2OgreT2))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.DD2OgreT2);
                }
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2OgreT3))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.DD2OgreT3);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DD2OgreT2);
            }
            else if (text == "szyl" || text == "双足翼龙" || text == "betsy" || text == "贝西塔" || int.TryParse(text, out num) && num == NPCID.DD2Betsy)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2Betsy))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.DD2Betsy);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DD2Betsy);
            }
            //南瓜月，霜月
            else if (text == "am" || text == "哀木" || int.TryParse(text, out num) && num == NPCID.MourningWood)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MourningWood))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.MourningWood);
                }
                bossName = Lang.GetNPCNameValue(NPCID.MourningWood);
            }
            else if (text == "ngw" || text == "南瓜王" || int.TryParse(text, out num) && num == NPCID.Pumpking)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Pumpking))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Pumpking);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Pumpking);
            }
            else if (text == "cljjg" || text == "常绿尖叫怪" || text == "sds" || text == "圣诞树" || int.TryParse(text, out num) && num == NPCID.Everscream)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Everscream))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.Everscream);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Everscream);
            }
            else if (text == "sdtk" || text == "圣诞坦克" || text == "sdlr" || text == "圣诞老人" || int.TryParse(text, out num) && num == NPCID.SantaNK1)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SantaNK1))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.SantaNK1);
                }
                bossName = Lang.GetNPCNameValue(NPCID.SantaNK1);
            }
            else if (text == "bxnh" || text == "冰雪女皇" || text == "bxnw" || text == "冰雪女王" || int.TryParse(text, out num) && num == NPCID.IceQueen)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.IceQueen))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(NPCID.IceQueen);
                }
                bossName = Lang.GetNPCNameValue(NPCID.IceQueen);
            }
            //非boss
            else if (int.TryParse(text, out num) && num >= NpcIDMin && num <= NpcIDMax)
            {
                if (!config.BossAndMonsterProgress_Boss和怪物封禁.Contains(num))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Add(num);
                }
                bossName = Lang.GetNPCNameValue(num);
            }
            //判断添加成功
            if (bossName != "")
            {
                args.Player.SendMessage($"封禁Boss或生物: {bossName} 添加成功！", Color.Green);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                args.Player.SendErrorMessage("添加失败！该Boss或生物不存在，请检查字符拼写是否正确");
            }
        }


        //解锁boss或生物
        private void UnlockNpc(CommandArgs args)
        {
            if (!args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /unlocknpc 【NPC id/Boss 汉字名称/Boss 拼音缩写】 来解除玩家对该boss或npc的攻击\nEnter /unlocknpc [NPC id] To remove the player's attack on the boss or NPC");
                return;
            }
            string text = args.Parameters[0];
            if (args.Parameters.Count > 1)
            {
                text = string.Join(" ", args.Parameters);
            }

            int num = -999;
            string bossName = "";
            //常规boss
            if (text == "slmw" || text == "史莱姆王" || text == "sw" || text == "史王" || int.TryParse(text, out num) && num == NPCID.KingSlime)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.KingSlime) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SlimeSpiked))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.KingSlime);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.SlimeSpiked);
                    bossName = Lang.GetNPCNameValue(NPCID.KingSlime);
                }
            }
            else if (text == "kslzy" || text == "克苏鲁之眼" || text == "ky" || text == "克眼" || int.TryParse(text, out num) && num == NPCID.EyeofCthulhu)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EyeofCthulhu) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.ServantofCthulhu))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.EyeofCthulhu);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.ServantofCthulhu);
                    bossName = Lang.GetNPCNameValue(NPCID.EyeofCthulhu);
                }
            }
            else if (text == "sjtsz" || text == "世界吞噬者" || text == "rc" || text == "蠕虫" || int.TryParse(text, out num) && (num == NPCID.EaterofWorldsHead || num == NPCID.EaterofWorldsBody || num == NPCID.EaterofWorldsTail))
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EaterofWorldsHead) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EaterofWorldsBody) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.EaterofWorldsTail))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.EaterofWorldsHead);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.EaterofWorldsBody);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.EaterofWorldsTail);
                    bossName = "世界吞噬者";
                }
            }
            else if (text == "kslzn" || text == "克苏鲁之脑" || text == "kn" || text == "克脑" || int.TryParse(text, out num) && (num == NPCID.BrainofCthulhu || num == NPCID.Creeper))
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.BrainofCthulhu) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Creeper))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.BrainofCthulhu);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Creeper);
                    bossName = Lang.GetNPCNameValue(NPCID.BrainofCthulhu);
                }
            }
            else if (text == "fh" || text == "蜂后" || int.TryParse(text, out num) && num == NPCID.QueenBee)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenBee))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.QueenBee);
                    bossName = Lang.GetNPCNameValue(NPCID.QueenBee);
                }
            }
            else if (text == "klw" || text == "骷髅王" || int.TryParse(text, out num) && num == NPCID.SkeletronHead)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SkeletronHead) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SkeletronHand))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.SkeletronHead);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.SkeletronHand);
                    bossName = "骷髅王";
                }
            }
            else if (text == "jl" || text == "巨鹿" || text == "ljg" || text == "鹿角怪" || int.TryParse(text, out num) && num == NPCID.Deerclops)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Deerclops))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Deerclops);
                    bossName = Lang.GetNPCNameValue(NPCID.Deerclops);
                }
            }
            else if (text == "xrq" || text == "血肉墙" || text == "rs" || text == "肉山" || int.TryParse(text, out num) && num == NPCID.WallofFlesh)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.WallofFlesh) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.WallofFleshEye) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheHungry) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheHungryII))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.WallofFlesh);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.WallofFleshEye);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.TheHungry);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.TheHungryII);
                    bossName = Lang.GetNPCNameValue(NPCID.WallofFlesh);
                }
            }
            else if (text == "slmhh" || text == "史莱姆皇后" || text == "sh" || text == "史后" || int.TryParse(text, out num) && num == NPCID.QueenSlimeBoss)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeBoss) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeMinionBlue) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeMinionPink) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.QueenSlimeMinionPurple))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.QueenSlimeBoss);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.QueenSlimeMinionBlue);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.QueenSlimeMinionPink);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.QueenSlimeMinionPurple);
                    bossName = Lang.GetNPCNameValue(NPCID.QueenSlimeBoss);
                }
            }
            else if (text == "125" || text == "126" || text == "szmy" || text == "双子魔眼")
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(125) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(126))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(125);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(126);
                    bossName = "双子魔眼";
                }
            }
            else if (text == "hmz" || text == "毁灭者" || text == "jxrc" || text == "机械蠕虫" || int.TryParse(text, out num) && (num == NPCID.TheDestroyer || num == NPCID.TheDestroyerBody || num == NPCID.TheDestroyerTail))
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheDestroyer) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheDestroyerBody) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Probe) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.TheDestroyerTail))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.TheDestroyer);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.TheDestroyerBody);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.TheDestroyerTail);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Probe);
                    bossName = "毁灭者";
                }
            }
            else if (text == "jxklw" || text == "机械骷髅王" || int.TryParse(text, out num) && (num == NPCID.SkeletronPrime || num == NPCID.PrimeCannon || num == NPCID.PrimeSaw || num == NPCID.PrimeVice || num == NPCID.PrimeLaser))
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(127) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(128) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(131) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(130) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(129))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(127);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(128);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(129);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(130);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(131);
                    bossName = "机械骷髅王";
                }
            }
            else if (text == "sjzh" || text == "世纪之花" || text == "世花" || int.TryParse(text, out num) && num == NPCID.Plantera)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Plantera) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.PlanterasTentacle))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Plantera);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.PlanterasTentacle);
                    bossName = Lang.GetNPCNameValue(NPCID.Plantera);
                }
            }
            else if (text == "sjr" || text == "石巨人" || int.TryParse(text, out num) && num == NPCID.Golem)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Golem) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.GolemFistLeft) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.GolemFistRight) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.GolemHead))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Golem);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.GolemFistLeft);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.GolemFistRight);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.GolemHead);
                    bossName = "石巨人";
                }
            }
            else if (text == "zlygj" || text == "猪龙鱼公爵" || text == "zs" || text == "猪鲨" || int.TryParse(text, out num) && num == NPCID.DukeFishron)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DukeFishron) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Sharkron) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Sharkron2))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.DukeFishron);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Sharkron);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Sharkron2);
                    bossName = Lang.GetNPCNameValue(NPCID.DukeFishron);
                }
            }
            else if (text == "636" || text == "gznh" || text == "光之女皇" || text == "gn" || text == "光女")
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.HallowBoss))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.HallowBoss);
                    bossName = Lang.GetNPCNameValue(NPCID.HallowBoss);
                }
            }
            else if (text == "xjt" || text == "邪教徒" || text == "byxit" || text == "拜月邪教徒" || int.TryParse(text, out num) && num == NPCID.CultistBoss)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.CultistBoss))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.CultistBoss);
                    bossName = Lang.GetNPCNameValue(NPCID.CultistBoss);
                }
            }
            else if (text == "yqlz" || text == "月球领主" || text == "yllz" || text == "月亮领主" || text == "yz" || text == "月总" || int.TryParse(text, out num) && num == NPCID.MoonLordHead)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordHead) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordHand) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordCore) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MoonLordLeechBlob))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.MoonLordHead);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.MoonLordHand);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.MoonLordCore);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.MoonLordLeechBlob);
                    bossName = "月球领主";
                }
            }
            //四柱
            else if (text == "ryz" || text == "日耀柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerSolar)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerSolar))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.LunarTowerSolar);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerSolar);
                }
            }
            else if (text == "xxz" || text == "星璇柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerVortex)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerVortex))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.LunarTowerVortex);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerVortex);
                }
            }
            else if (text == "xcz" || text == "星尘柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerStardust)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerStardust))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.LunarTowerStardust);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerStardust);
                }
            }
            else if (text == "xyz" || text == "星云柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerNebula)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.LunarTowerNebula))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.LunarTowerNebula);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerNebula);
                }
            }
            //天国军团boss
            else if (text == "hafs" || text == "黑暗法师" || text == "hamfs" || text == "黑暗魔法师" || int.TryParse(text, out num) && (num == NPCID.DD2DarkMageT1 || num == NPCID.DD2DarkMageT3))
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2DarkMageT1) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2DarkMageT3))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.DD2DarkMageT1);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.DD2DarkMageT3);
                    bossName = Lang.GetNPCNameValue(NPCID.DD2DarkMageT1);
                }
            }
            else if (text == "srm" || text == "食人魔" || int.TryParse(text, out num) && (num == NPCID.DD2OgreT2 || num == NPCID.DD2OgreT3))
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2OgreT2) || config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2OgreT3))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.DD2OgreT2);
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.DD2OgreT3);
                    bossName = Lang.GetNPCNameValue(NPCID.DD2OgreT2);
                }
            }
            else if (text == "szyl" || text == "双足翼龙" || text == "betsy" || text == "贝西塔" || int.TryParse(text, out num) && num == NPCID.DD2Betsy)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.DD2Betsy))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.DD2Betsy);
                    bossName = Lang.GetNPCNameValue(NPCID.DD2Betsy);
                }
            }
            //南瓜月，霜月
            else if (text == "am" || text == "哀木" || int.TryParse(text, out num) && num == NPCID.MourningWood)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.MourningWood))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.MourningWood);
                    bossName = Lang.GetNPCNameValue(NPCID.MourningWood);
                }
            }
            else if (text == "ngw" || text == "南瓜王" || int.TryParse(text, out num) && num == NPCID.Pumpking)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Pumpking))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Pumpking);
                    bossName = Lang.GetNPCNameValue(NPCID.Pumpking);
                }
            }
            else if (text == "cljjg" || text == "常绿尖叫怪" || text == "sds" || text == "圣诞树" || int.TryParse(text, out num) && num == NPCID.Everscream)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.Everscream))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.Everscream);
                    bossName = Lang.GetNPCNameValue(NPCID.Everscream);
                }
            }
            else if (text == "sdtk" || text == "圣诞坦克" || text == "sdlr" || text == "圣诞老人" || int.TryParse(text, out num) && num == NPCID.SantaNK1)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.SantaNK1))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.SantaNK1);
                    bossName = Lang.GetNPCNameValue(NPCID.SantaNK1);
                }
            }
            else if (text == "bxnh" || text == "冰雪女皇" || text == "bxnw" || text == "冰雪女王" || int.TryParse(text, out num) && num == NPCID.IceQueen)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(NPCID.IceQueen))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(NPCID.IceQueen);
                    bossName = Lang.GetNPCNameValue(NPCID.IceQueen);
                }
            }
            //非boss
            else if (int.TryParse(text, out num) && num >= NpcIDMin && num <= NpcIDMax)
            {
                if (config.BossAndMonsterProgress_Boss和怪物封禁.Contains(num))
                {
                    config.BossAndMonsterProgress_Boss和怪物封禁.Remove(num);
                    bossName = Lang.GetNPCNameValue(num);
                }
            }
            //判断删除成功
            if (bossName != "")
            {
                args.Player.SendMessage($"封禁Boss或生物: {bossName} 删除成功！", Color.Green);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                args.Player.SendErrorMessage("删除失败！该Boss或生物不存在或未被保护，请检查字符拼写是否正确");
            }

        }


        //查看被封禁的boss或生物
        private void ListLockNpc(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /listlocknpc   来查看所有禁止被攻击的生物\nEnter /listlocknpc   To view all creatures prohibited from being attacked");
                return;
            }

            string str = "";
            int count = 0;
            foreach (int v in config.BossAndMonsterProgress_Boss和怪物封禁)
            {
                str += Lang.GetNPCNameValue(v) + "  ";
                count++;
                if (count % 15 == 0)
                {
                    str += "\n";
                }
            }
            if (str != "")
            {
                args.Player.SendInfoMessage("所有被保护的NPC:\n" + str);
            }
            else
            {
                args.Player.SendInfoMessage("所有被保护的NPC:\n" + "无");
            }
        }


        //添加不被检查的物品
        private void AddUnCheckedItem(CommandArgs args)
        {
            if (!args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /adduci 【item:id】 来添加不被系统检查的物品\nEnter /adduci [item:id] To add items that are not checked by the system");
                return;
            }
            string text = args.Parameters[0];
            if (args.Parameters.Count > 1)
            {
                text = string.Join(" ", args.Parameters);
            }
            int itemID;
            if (!int.TryParse(text, out itemID) || itemID < ItemIDMin || itemID > ItemIDMax)
            {
                args.Player.SendErrorMessage("输入不合理");
                return;
            }
            if (config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Contains(itemID))
            {
                args.Player.SendInfoMessage($"物品：[i:{itemID}] 已存在");
            }
            else
            {
                config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Add(itemID);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"物品：[i:{itemID}] 已添加", Color.Green);
            }
        }


        //删除不被检查的物品
        private void DelUnCheckedItem(CommandArgs args)
        {
            if (!args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /deluci 【item:id】 来删除不被系统检查的物品\nEnter /deluci [item:id] To delete items that are not checked by the system");
                return;
            }
            string text = args.Parameters[0];
            if (args.Parameters.Count > 1)
            {
                text = string.Join(" ", args.Parameters);
            }
            int itemID;
            if (!int.TryParse(text, out itemID) || itemID < ItemIDMin || itemID > ItemIDMax)
            {
                args.Player.SendErrorMessage("输入不合理");
                return;
            }
            if (config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Contains(itemID))
            {
                config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Remove(itemID);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"物品：[i:{itemID}] 已删除", Color.Green);
            }
            else
            {
                args.Player.SendInfoMessage($"物品：[i:{itemID}] 不存在");
            }

        }


        //列出不被删除的物品
        private void ListUnCheckedItem(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /listuci 来查看所有不被系统检查的物品\nEnter /listuci  To view all items that are not checked by the system");
                return;
            }

            string str = "";
            int count = 0;
            foreach (int v in config.ignoreCheckedItemsID_不需要被作弊检查的物品id)
            {
                str += $"[i:{v}] ";
                count++;
                if (count % 40 == 0)
                {
                    str += "\n";
                }
            }
            if (str != "")
            {
                args.Player.SendInfoMessage($"所有不被检查的物品：\n" + str);
            }
            else
            {
                args.Player.SendInfoMessage($"所有不被检查的物品：\n" + "无");
            }
        }


        //重新加载config等文件
        private void OnReload(ReloadEventArgs e)
        {
            Config.SetConfigFile();
            config = Config.ReadConfigFile();
            CheatData.SetCheatData();
            if (config.enableChinese_启用中文)
            {
                LanguageManager.Instance.SetLanguage("zh-Hans");
            }
            else
            {
                LanguageManager.Instance.SetLanguage("default");
            }
            if (!config.enableItemDetection_启用物品作弊检测)
            {
                OnlineCheakingPlayers.Clear();
            }
        }
    }
}
