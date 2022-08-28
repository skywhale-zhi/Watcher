using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using OTAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Watcher
{
    public partial class Watcher : TerrariaPlugin
    {
        //对话
        private void OnChat(ServerChatEventArgs args)
        {
            if (config.whetherToWriteTheConversationContentInTheLog_是否把对话内容写入日志)
            {
                if (args == null || Main.player[args.Who] == null || !Main.player[args.Who].active)
                {
                    return;
                }
                string text = args.Text;
                if (args.Text.Length >= 10 && (args.Text.Substring(0, 10) == "/register " || args.Text.Substring(0, 10) == ".register " || args.Text.Substring(0, 10) == "/REGISTER " || args.Text.Substring(0, 10) == ".REGISTER "))
                {
                    text = "/register 【密码不该在日志显示出来】";
                }
                if (args.Text.Length >= 6 && (args.Text.Substring(0, 6) == "/login" || args.Text.Substring(0, 6) == ".login" || args.Text.Substring(0, 6) == "/LOGIN" || args.Text.Substring(0, 6) == ".LOGIN"))
                {
                    text = "/login 【密码不该在日志显示出来】";
                }

                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                if (TShock.Players[args.Who].Account != null)
                    File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{TShock.Players[args.Who].Account.ID}][{Main.player[args.Who].name}]: \"{text}\" in {Main.player[args.Who].position / 16}" });
                else
                    File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:?][{Main.player[args.Who].name}]: \"{text}\" in {Main.player[args.Who].position / 16}" });
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
        


        //游戏运行
        private void GameRun(EventArgs args)
        {
            LogClean();
            if (config.backUpTshockSql_是否备份tshockSql)
                BackUpTshockSql();
            if (config.enableItemDetection_启用物品作弊检测)
                ItemCheatingCheck(args, null, null, 1);
        }


        //射弹作弊检查检查
        private void ProjCheatingCheck(object sender, GetDataHandlers.NewProjectileEventArgs e)
        {
            #region 射弹伤害检测
            if (config.enableProjDamage_启用射弹伤害检测 && KickOrBanGroupAllow(e.Player))
            {
                //判断伤害溢出的情况，排除灰橙冲击枪、尖桩发射器
                bool isExclude = e.Type == 876 || e.Type == 323;
                //如果伤害 > 3000并且不在可排除的范围内，警告踢掉
                if (e.Damage >= 3000 && !isExclude)
                {
                    int num = CheatingPlayers(e.Player, 2);
                    if (num < config.numberOfBan_允许的违规次数)
                    {
                        e.Player.Kick($"不正常的伤害，违规次数 {num}，达到{config.numberOfBan_允许的违规次数}次直接封禁\n若有疑问请及时向管理员联系");
                    }
                    else
                    {
                        e.Player.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员");
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
                //木质 proj: 360   item: 2289
                //强化 proj: 361   item: 2291
                //腐化 proj: 363   item: 2293
                //血腥 proj: 381   item: 2421
                //血月 proj: 760   item: 4325
                //甲虫 proj: 775   item: 4442
                //玻璃 proj: 362   item: 2292
                //机械 proj: 365   item: 2295
                //坐鸭 proj: 366   item: 2296
                //热线 proj: 382   item: 2422
                //黄金 proj: 364   item: 2294

                int target = 0;
                if (Main.player[e.Player.Index].ownedProjectileCounts[360] > 1)
                {
                    target = 2289;
                    e.Player.Kick($"木质钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[361] > 1)
                {
                    target = 2291;
                    e.Player.Kick($"强化钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[363] > 1)
                {
                    target = 2293;
                    e.Player.Kick($"腐化钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[381] > 1)
                {
                    target = 2421;
                    e.Player.Kick($"血腥钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[760] > 1)
                {
                    target = 4325;
                    e.Player.Kick($"血月钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[775] > 1)
                {
                    target = 4442;
                    e.Player.Kick($"甲虫钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[362] > 1)
                {
                    target = 2292;
                    e.Player.Kick($"玻璃钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[365] > 1)
                {
                    target = 2295;
                    e.Player.Kick($"机械钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[366] > 1)
                {
                    target = 2296;
                    e.Player.Kick($"坐鸭钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[382] > 1)
                {
                    target = 2422;
                    e.Player.Kick($"热线钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }
                else if (Main.player[e.Owner].ownedProjectileCounts[364] > 1)
                {
                    target = 2294;
                    e.Player.Kick($"黄金钓竿浮标数目异常，违规次数{getCheatTimes(e.Player) + 1}，如有错误，请及时向管理员反馈");
                }

                //作弊次数
                int num;
                if (target != 0)
                {
                    num = CheatingPlayers(e.Player, 1);
                    if (num < config.numberOfBan_允许的违规次数)
                    {
                        Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 掷出的 {Lang.GetItemName(target)} 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}");
                        AvoidLogSize("logDirPath", logDirPath, logFilePath);
                        AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                        File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 掷出的 {Lang.GetItemName(target)} 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                        File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 掷出的 {Lang.GetItemName(target)} 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                    }
                    else
                    {
                        e.Player.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员");
                        Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}");
                        AvoidLogSize("logDirPath", logDirPath, logFilePath);
                        AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                        File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                        File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 浮标数目不正常 在 {e.Position / 16} 违规次数 {num}" });
                    }
                }

            }
            #endregion


            #region 星璇机枪检测
            if(Main.player[e.Player.Index].ownedProjectileCounts[615] > 0 && (Main.player[e.Player.Index].HeldItem == null || Main.player[e.Player.Index].HeldItem.type != 3475))
            {
                int num = CheatingPlayers(e.Player, 4);
                if (num < config.numberOfBan_允许的违规次数)
                {
                    e.Player.Kick($"检测到星璇机枪攻速bug，违规次数 {num}，达到{config.numberOfBan_允许的违规次数}次直接封禁\n若有疑问请及时向管理员联系");
                }
                else
                {
                    e.Player.Ban($"总计违规次数已达到{config.numberOfBan_允许的违规次数}次！若有疑问请及时联系管理员");
                }
                Console.WriteLine($" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 触发星璇机枪bug，手持 {Main.player[e.Player.Index].HeldItem.Name} 在 {e.Position / 16} 违规次数 {num}");
                AvoidLogSize("logDirPath", logDirPath, logFilePath);
                AvoidLogSize("cheatLogDirPath", cheatLogDirPath, cheatLogFilePath);
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 触发星璇机枪bug，手持 {Main.player[e.Player.Index].HeldItem.Name} 在 {e.Position / 16} 违规次数 {num}" });
                File.AppendAllLines(cheatLogFilePath, new string[] { DateTime.Now.ToString("u") + $" WARNING [acc:{e.Player.Account.ID}][{e.Player.Name}] 触发星璇机枪bug，手持 {Main.player[e.Player.Index].HeldItem.Name} 在 {e.Position / 16} 违规次数 {num}" });
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
            foreach (WPlayer w in wPlayers)
            {
                //作弊玩家100%概率触发检查
                if (player.IsLoggedIn && e.Stack == 1 && w.uuid == player.UUID && w.isItemCheat)
                {
                    //12秒刚进入游戏保护
                    if (Math.Abs(getNowTimeSecond() - w.Timer) > 60 * 12)
                    {
                        ItemCheatingCheck(null, sender, e, 2);
                    }
                    flag = true;
                    break;
                }
            }
            // 正常玩家1/3概率触发，减少服务器负担
            if (!flag && player.IsLoggedIn && e.Stack == 1 && Main.rand.Next(3) == 0)
            {
                ItemCheatingCheck(null, sender, e, 2);
            }
        }


        //在玩家进入时运行
        private void OnServerjoin(JoinEventArgs args)
        {
            if (args == null || TShock.Players[args.Who] == null)
                return;

            bool flag = false;
            for (int i = 0; i < wPlayers.Count; i++)//如果进入游戏的玩家uuid有记录即：重进玩家，换号玩家。则同步下name,acc,tsplayer信息
            {
                if (wPlayers[i].uuid == TShock.Players[args.Who].UUID)
                {
                    wPlayers[i].tsplayer = TShock.Players[args.Who];
                    wPlayers[i].name = TShock.Players[args.Who].Name;
                    flag = true;
                }
            }
            if (!flag)//如果是新玩家(以uuid为基准)
            {
                wPlayers.Add(new WPlayer(TShock.Players[args.Who]));
            }

            //给每次进来的人发送警告信息
            TSPlayer player = TShock.Players[args.Who];
            foreach (WPlayer w in wPlayers)
            {
                if (w.name == player.Name && w.isItemCheat)
                {
                    w.Timer = getNowTimeSecond();
                    player.SendErrorMessage($"请在 10 秒内尽快清理身上违规物品");
                }
                if (w.uuid == player.UUID && w.cheatingTimes > 0)
                {
                    player.SendErrorMessage($"您已违规{w.cheatingTimes}次，最多{config.numberOfBan_允许的违规次数}次开始封禁");
                }
            }


            //向watcherlog写入玩家进入信息
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
            //因为刚进去的人不会算到Main.player遍历得到的人物组中，所以要手动加Tshock.players里的信息
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
            if (config.ProtectedNPC_被保护的NPC.Contains(npc.netID))
            {
                if (Damage < 9090)
                {
                    npc.HealEffect(Damage);
                }
                else
                {
                    npc.HealEffect(114514);
                }
                Damage = 0;
                npc.life = npc.lifeMax;
                if (Main.rand.Next(8) == 0 && entity is Player)
                {
                    Player player = entity as Player;
                    TShock.Players[player.whoAmI].SendInfoMessage($"{npc.FullName} 被系统保护，若有疑问请联系管理员");
                }
            }
            return HookResult.Continue;
        }

        //(测试版)击中npc时触发，向攻击保护动物的玩家发送消息,对保护动物进行回血保护
        private void OnStrike(NpcStrikeEventArgs args)
        {
            if (config.ProtectedNPC_被保护的NPC.Contains(args.Npc.type))
            {
                if (args.Damage < 9090)
                {
                    args.Npc.HealEffect(args.Damage);
                }
                else
                {
                    args.Npc.HealEffect(114514);
                }
                args.Damage = 0;
                args.Npc.life = args.Npc.lifeMax;
                if (Main.rand.Next(8) == 0)
                {
                    TShock.Players[args.Player.whoAmI].SendInfoMessage($"{args.Npc.FullName} 被系统保护，若有疑问请联系管理员");
                }
            }
        }




        //生成生物时触发
        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            NPC npc = Main.npc[args.NpcId];
            if (!config.ProtectedNPC_被保护的NPC.Contains(npc.netID))
                return;
            if (!npc.boss)
                return;
            TSPlayer.All.SendInfoMessage($"该Boss:{npc.FullName} 被管理员设为禁止伤害，请尽快放弃攻击，不要白费力气 (¦3[▓▓] ");
        }


        //清理玩家作弊数据
        private void ClearCheatData(CommandArgs args)
        {
            if (!args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /clearcd 【玩家名称】 来清理该玩家的作弊数据\nEnter /clearcd [player name] To clear the player's cheating data");
                return;
            }
            string text = args.Parameters[0];
            if (args.Parameters.Count > 1)
            {
                text = string.Join(" ", args.Parameters);
            }

            List<TSPlayer> players = TSPlayer.FindByNameOrID(text);

            if (players.Any())
            {
                bool ischeat = false;
                for (int i = 0; i < wPlayers.Count; i++)
                {
                    if (wPlayers[i].uuid == players[0].UUID)
                    {
                        wPlayers[i] = new WPlayer(players[0]);
                        ischeat = true;
                    }
                }
                if (ischeat)
                    args.Player.SendMessage($"玩家[{players[0].Name}]的作弊数据清理完成！", Color.LightGreen);
                else
                    args.Player.SendInfoMessage($"玩家[{players[0].Name}]未作弊，无需清理");
            }
            else
            {
                args.Player.SendInfoMessage($"未找到该玩家，请检查是否输入错误");
            }
        }


        //清理所有玩家作弊数据
        private void ClearCheatDataAll(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /clearcdall 来清理所有玩家的作弊数据\nEnter /clearcd [player name] To clear the cheating data of all players");
                return;
            }
            for (int i = 0; i < wPlayers.Count; i++)
            {
                wPlayers[i] = new WPlayer(wPlayers[i].tsplayer);
            }
            args.Player.SendMessage($"所有玩家的作弊数据清理成功！", Color.LightGreen);
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
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.KingSlime))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.KingSlime);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.SlimeSpiked))//尖刺史莱姆
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.SlimeSpiked);
                }
                bossName = Lang.GetNPCNameValue(NPCID.KingSlime);
            }
            else if (text == "kslzy" || text == "克苏鲁之眼" || text == "ky" || text == "克眼" || int.TryParse(text, out num) && num == NPCID.EyeofCthulhu)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.EyeofCthulhu))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.EyeofCthulhu);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.ServantofCthulhu))//克苏鲁之仆
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.ServantofCthulhu);
                }
                bossName = Lang.GetNPCNameValue(NPCID.EyeofCthulhu);
            }
            else if (text == "sjtsz" || text == "世界吞噬者" || text == "rc" || text == "蠕虫" || int.TryParse(text, out num) && (num == NPCID.EaterofWorldsHead || num == NPCID.EaterofWorldsBody || num == NPCID.EaterofWorldsTail))
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.EaterofWorldsHead))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.EaterofWorldsHead);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.EaterofWorldsBody))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.EaterofWorldsBody);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.EaterofWorldsTail))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.EaterofWorldsTail);
                }
                bossName = "世界吞噬者";
            }
            else if (text == "kslzn" || text == "克苏鲁之脑" || text == "kn" || text == "克脑" || int.TryParse(text, out num) && (num == NPCID.BrainofCthulhu || num == NPCID.Creeper))
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.BrainofCthulhu))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.BrainofCthulhu);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Creeper))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Creeper);
                }
                bossName = Lang.GetNPCNameValue(NPCID.BrainofCthulhu);
            }
            else if (text == "fh" || text == "蜂后" || int.TryParse(text, out num) && num == NPCID.QueenBee)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenBee))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.QueenBee);
                }
                bossName = Lang.GetNPCNameValue(NPCID.QueenBee);
            }
            else if (text == "klw" || text == "骷髅王" || int.TryParse(text, out num) && num == NPCID.SkeletronHead)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.SkeletronHead))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.SkeletronHead);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.SkeletronHand))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.SkeletronHand);
                }
                bossName = "骷髅王";
            }
            else if (text == "jl" || text == "巨鹿" || text == "ljg" || text == "鹿角怪" || int.TryParse(text, out num) && num == NPCID.Deerclops)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Deerclops))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Deerclops);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Deerclops);
            }
            else if (text == "xrq" || text == "血肉墙" || text == "rs" || text == "肉山" || int.TryParse(text, out num) && num == NPCID.WallofFlesh)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.WallofFlesh))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.WallofFlesh);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.WallofFleshEye))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.WallofFleshEye);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheHungry))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.TheHungry);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheHungryII))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.TheHungryII);
                }
                bossName = Lang.GetNPCNameValue(NPCID.WallofFlesh);
            }
            else if (text == "slmhh" || text == "史莱姆皇后" || text == "sh" || text == "史后" || int.TryParse(text, out num) && num == NPCID.QueenSlimeBoss)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeBoss))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.QueenSlimeBoss);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeMinionBlue))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.QueenSlimeMinionBlue);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeMinionPink))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.QueenSlimeMinionPink);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeMinionPurple))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.QueenSlimeMinionPurple);
                }
                bossName = Lang.GetNPCNameValue(NPCID.QueenSlimeBoss);
            }
            else if (text == "125" || text == "126" || text == "szmy" || text == "双子魔眼")
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(125))
                {
                    config.ProtectedNPC_被保护的NPC.Add(125);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(126))
                {
                    config.ProtectedNPC_被保护的NPC.Add(126);
                }
                bossName = "双子魔眼";
            }
            else if (text == "hmz" || text == "毁灭者" || text == "jxrc" || text == "机械蠕虫" || int.TryParse(text, out num) && (num == NPCID.TheDestroyer || num == NPCID.TheDestroyerBody || num == NPCID.TheDestroyerTail))
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheDestroyer))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.TheDestroyer);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheDestroyerBody))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.TheDestroyerBody);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheDestroyerTail))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.TheDestroyerTail);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Probe))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Probe);
                }
                bossName = "毁灭者";
            }
            else if (text == "jxklw" || text == "机械骷髅王" || int.TryParse(text, out num) && (num == NPCID.SkeletronPrime || num == NPCID.PrimeCannon || num == NPCID.PrimeSaw || num == NPCID.PrimeVice || num == NPCID.PrimeLaser))
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(127))
                {
                    config.ProtectedNPC_被保护的NPC.Add(127);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(128))
                {
                    config.ProtectedNPC_被保护的NPC.Add(128);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(129))
                {
                    config.ProtectedNPC_被保护的NPC.Add(129);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(130))
                {
                    config.ProtectedNPC_被保护的NPC.Add(130);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(131))
                {
                    config.ProtectedNPC_被保护的NPC.Add(131);
                }
                bossName = "机械骷髅王";
            }
            else if (text == "sjzh" || text == "世纪之花" || text == "世花" || int.TryParse(text, out num) && num == NPCID.Plantera)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Plantera))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Plantera);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.PlanterasTentacle))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.PlanterasTentacle);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Plantera);
            }
            else if (text == "sjr" || text == "石巨人" || int.TryParse(text, out num) && num == NPCID.Golem)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Golem))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Golem);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.GolemFistLeft))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.GolemFistLeft);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.GolemFistRight))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.GolemFistRight);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.GolemHead))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.GolemHead);
                }
                bossName = "石巨人";
            }
            else if (text == "zlygj" || text == "猪龙鱼公爵" || text == "zs" || text == "猪鲨" || int.TryParse(text, out num) && num == NPCID.DukeFishron)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.DukeFishron))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.DukeFishron);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Sharkron))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Sharkron);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Sharkron2))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Sharkron2);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DukeFishron);
            }
            else if (text == "636" || text == "gznh" || text == "光之女皇" || text == "gn" || text == "光女")
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.HallowBoss))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.HallowBoss);
                }
                bossName = Lang.GetNPCNameValue(NPCID.HallowBoss);
            }
            else if (text == "xjt" || text == "邪教徒" || text == "byxjt" || text == "拜月邪教徒" || int.TryParse(text, out num) && num == NPCID.CultistBoss)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.CultistBoss))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.CultistBoss);
                }
                bossName = Lang.GetNPCNameValue(NPCID.CultistBoss);
            }
            else if (text == "yqlz" || text == "月球领主" || text == "yllz" || text == "月亮领主" || text == "yz" || text == "月总" || int.TryParse(text, out num) && (num == NPCID.MoonLordHead || num == NPCID.MoonLordHand || num == NPCID.MoonLordCore || num == NPCID.MoonLordLeechBlob))
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordHead))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.MoonLordHead);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordHand))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.MoonLordHand);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordCore))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.MoonLordCore);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordLeechBlob))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.MoonLordLeechBlob);
                }
                bossName = "月球领主";
            }
            //四柱
            else if (text == "ryz" || text == "日耀柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerSolar)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerSolar))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.LunarTowerSolar);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerSolar);
            }
            else if (text == "xxz" || text == "星璇柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerVortex)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerVortex))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.LunarTowerVortex);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerVortex);
            }
            else if (text == "xcz" || text == "星尘柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerStardust)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerStardust))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.LunarTowerStardust);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerStardust);
            }
            else if (text == "xyz" || text == "星云柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerNebula)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerNebula))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.LunarTowerNebula);
                }
                bossName = Lang.GetNPCNameValue(NPCID.LunarTowerNebula);
            }
            //天国军团boss
            else if (text == "hafs" || text == "黑暗法师" || text == "hamfs" || text == "黑暗魔法师" || int.TryParse(text, out num) && (num == NPCID.DD2DarkMageT1 || num == NPCID.DD2DarkMageT3))
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2DarkMageT1))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.DD2DarkMageT1);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2DarkMageT3))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.DD2DarkMageT3);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DD2DarkMageT1);
            }
            else if (text == "srm" || text == "食人魔" || int.TryParse(text, out num) && (num == NPCID.DD2OgreT2 || num == NPCID.DD2OgreT3))
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2OgreT2))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.DD2OgreT2);
                }
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2OgreT3))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.DD2OgreT3);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DD2OgreT2);
            }
            else if (text == "szyl" || text == "双足翼龙" || text == "betsy" || text == "贝西塔" || int.TryParse(text, out num) && num == NPCID.DD2Betsy)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2Betsy))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.DD2Betsy);
                }
                bossName = Lang.GetNPCNameValue(NPCID.DD2Betsy);
            }
            //南瓜月，霜月
            else if (text == "am" || text == "哀木" || int.TryParse(text, out num) && num == NPCID.MourningWood)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.MourningWood))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.MourningWood);
                }
                bossName = Lang.GetNPCNameValue(NPCID.MourningWood);
            }
            else if (text == "ngw" || text == "南瓜王" || int.TryParse(text, out num) && num == NPCID.Pumpking)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Pumpking))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Pumpking);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Pumpking);
            }
            else if (text == "cljjg" || text == "常绿尖叫怪" || text == "sds" || text == "圣诞树" || int.TryParse(text, out num) && num == NPCID.Everscream)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.Everscream))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.Everscream);
                }
                bossName = Lang.GetNPCNameValue(NPCID.Everscream);
            }
            else if (text == "sdtk" || text == "圣诞坦克" || text == "sdlr" || text == "圣诞老人" || int.TryParse(text, out num) && num == NPCID.SantaNK1)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.SantaNK1))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.SantaNK1);
                }
                bossName = Lang.GetNPCNameValue(NPCID.SantaNK1);
            }
            else if (text == "bxnh" || text == "冰雪女皇" || text == "bxnw" || text == "冰雪女王" || int.TryParse(text, out num) && num == NPCID.IceQueen)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(NPCID.IceQueen))
                {
                    config.ProtectedNPC_被保护的NPC.Add(NPCID.IceQueen);
                }
                bossName = Lang.GetNPCNameValue(NPCID.IceQueen);
            }
            //非boss
            else if (int.TryParse(text, out num) && num >= NpcIDMin && num <= NpcIDMax)
            {
                if (!config.ProtectedNPC_被保护的NPC.Contains(num))
                {
                    config.ProtectedNPC_被保护的NPC.Add(num);
                }
                bossName = Lang.GetNPCNameValue(num);
            }
            //判断添加成功
            if (bossName != "")
            {
                args.Player.SendMessage($"封禁Boss或生物: {bossName} 添加成功！", Color.LightGreen);
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
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.KingSlime) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.SlimeSpiked))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.KingSlime);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.SlimeSpiked);
                    bossName = Lang.GetNPCNameValue(NPCID.KingSlime);
                }
            }
            else if (text == "kslzy" || text == "克苏鲁之眼" || text == "ky" || text == "克眼" || int.TryParse(text, out num) && num == NPCID.EyeofCthulhu)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.EyeofCthulhu) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.ServantofCthulhu))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.EyeofCthulhu);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.ServantofCthulhu);
                    bossName = Lang.GetNPCNameValue(NPCID.EyeofCthulhu);
                }
            }
            else if (text == "sjtsz" || text == "世界吞噬者" || text == "rc" || text == "蠕虫" || int.TryParse(text, out num) && (num == NPCID.EaterofWorldsHead || num == NPCID.EaterofWorldsBody || num == NPCID.EaterofWorldsTail))
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.EaterofWorldsHead) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.EaterofWorldsBody) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.EaterofWorldsTail))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.EaterofWorldsHead);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.EaterofWorldsBody);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.EaterofWorldsTail);
                    bossName = "世界吞噬者";
                }
            }
            else if (text == "kslzn" || text == "克苏鲁之脑" || text == "kn" || text == "克脑" || int.TryParse(text, out num) && (num == NPCID.BrainofCthulhu || num == NPCID.Creeper))
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.BrainofCthulhu) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.Creeper))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.BrainofCthulhu);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Creeper);
                    bossName = Lang.GetNPCNameValue(NPCID.BrainofCthulhu);
                }
            }
            else if (text == "fh" || text == "蜂后" || int.TryParse(text, out num) && num == NPCID.QueenBee)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenBee))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.QueenBee);
                    bossName = Lang.GetNPCNameValue(NPCID.QueenBee);
                }
            }
            else if (text == "klw" || text == "骷髅王" || int.TryParse(text, out num) && num == NPCID.SkeletronHead)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.SkeletronHead) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.SkeletronHand))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.SkeletronHead);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.SkeletronHand);
                    bossName = "骷髅王";
                }
            }
            else if (text == "jl" || text == "巨鹿" || text == "ljg" || text == "鹿角怪" || int.TryParse(text, out num) && num == NPCID.Deerclops)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.Deerclops))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Deerclops);
                    bossName = Lang.GetNPCNameValue(NPCID.Deerclops);
                }
            }
            else if (text == "xrq" || text == "血肉墙" || text == "rs" || text == "肉山" || int.TryParse(text, out num) && num == NPCID.WallofFlesh)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.WallofFlesh) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.WallofFleshEye) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheHungry) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheHungryII))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.WallofFlesh);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.WallofFleshEye);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.TheHungry);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.TheHungryII);
                    bossName = Lang.GetNPCNameValue(NPCID.WallofFlesh);
                }
            }
            else if (text == "slmhh" || text == "史莱姆皇后" || text == "sh" || text == "史后" || int.TryParse(text, out num) && num == NPCID.QueenSlimeBoss)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeBoss) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeMinionBlue) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeMinionPink) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.QueenSlimeMinionPurple))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.QueenSlimeBoss);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.QueenSlimeMinionBlue);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.QueenSlimeMinionPink);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.QueenSlimeMinionPurple);
                    bossName = Lang.GetNPCNameValue(NPCID.QueenSlimeBoss);
                }
            }
            else if (text == "125" || text == "126" || text == "szmy" || text == "双子魔眼")
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(125) || config.ProtectedNPC_被保护的NPC.Contains(126))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(125);
                    config.ProtectedNPC_被保护的NPC.Remove(126);
                    bossName = "双子魔眼";
                }
            }
            else if (text == "hmz" || text == "毁灭者" || text == "jxrc" || text == "机械蠕虫" || int.TryParse(text, out num) && (num == NPCID.TheDestroyer || num == NPCID.TheDestroyerBody || num == NPCID.TheDestroyerTail))
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheDestroyer) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheDestroyerBody) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.Probe) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.TheDestroyerTail))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.TheDestroyer);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.TheDestroyerBody);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.TheDestroyerTail);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Probe);
                    bossName = "毁灭者";
                }
            }
            else if (text == "jxklw" || text == "机械骷髅王" || int.TryParse(text, out num) && (num == NPCID.SkeletronPrime || num == NPCID.PrimeCannon || num == NPCID.PrimeSaw || num == NPCID.PrimeVice || num == NPCID.PrimeLaser))
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(127) || config.ProtectedNPC_被保护的NPC.Contains(128) || config.ProtectedNPC_被保护的NPC.Contains(131) || config.ProtectedNPC_被保护的NPC.Contains(130) || config.ProtectedNPC_被保护的NPC.Contains(129))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(127);
                    config.ProtectedNPC_被保护的NPC.Remove(128);
                    config.ProtectedNPC_被保护的NPC.Remove(129);
                    config.ProtectedNPC_被保护的NPC.Remove(130);
                    config.ProtectedNPC_被保护的NPC.Remove(131);
                    bossName = "机械骷髅王";
                }
            }
            else if (text == "sjzh" || text == "世纪之花" || text == "世花" || int.TryParse(text, out num) && num == NPCID.Plantera)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.Plantera) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.PlanterasTentacle))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Plantera);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.PlanterasTentacle);
                    bossName = Lang.GetNPCNameValue(NPCID.Plantera);
                }
            }
            else if (text == "sjr" || text == "石巨人" || int.TryParse(text, out num) && num == NPCID.Golem)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.Golem) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.GolemFistLeft) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.GolemFistRight) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.GolemHead))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Golem);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.GolemFistLeft);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.GolemFistRight);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.GolemHead);
                    bossName = "石巨人";
                }
            }
            else if (text == "zlygj" || text == "猪龙鱼公爵" || text == "zs" || text == "猪鲨" || int.TryParse(text, out num) && num == NPCID.DukeFishron)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.DukeFishron) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.Sharkron) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.Sharkron2))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.DukeFishron);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Sharkron);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Sharkron2);
                    bossName = Lang.GetNPCNameValue(NPCID.DukeFishron);
                }
            }
            else if (text == "636" || text == "gznh" || text == "光之女皇" || text == "gn" || text == "光女")
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.HallowBoss))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.HallowBoss);
                    bossName = Lang.GetNPCNameValue(NPCID.HallowBoss);
                }
            }
            else if (text == "xjt" || text == "邪教徒" || text == "byxit" || text == "拜月邪教徒" || int.TryParse(text, out num) && num == NPCID.CultistBoss)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.CultistBoss))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.CultistBoss);
                    bossName = Lang.GetNPCNameValue(NPCID.CultistBoss);
                }
            }
            else if (text == "yqlz" || text == "月球领主" || text == "yllz" || text == "月亮领主" || text == "yz" || text == "月总" || int.TryParse(text, out num) && num == NPCID.MoonLordHead)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordHead) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordHand) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordCore) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.MoonLordLeechBlob))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.MoonLordHead);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.MoonLordHand);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.MoonLordCore);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.MoonLordLeechBlob);
                    bossName = "月球领主";
                }
            }
            //四柱
            else if (text == "ryz" || text == "日耀柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerSolar)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerSolar))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.LunarTowerSolar);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerSolar);
                }
            }
            else if (text == "xxz" || text == "星璇柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerVortex)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerVortex))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.LunarTowerVortex);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerVortex);
                }
            }
            else if (text == "xcz" || text == "星尘柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerStardust)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerStardust))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.LunarTowerStardust);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerStardust);
                }
            }
            else if (text == "xyz" || text == "星云柱" || int.TryParse(text, out num) && num == NPCID.LunarTowerNebula)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.LunarTowerNebula))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.LunarTowerNebula);
                    bossName = Lang.GetNPCNameValue(NPCID.LunarTowerNebula);
                }
            }
            //天国军团boss
            else if (text == "hafs" || text == "黑暗法师" || text == "hamfs" || text == "黑暗魔法师" || int.TryParse(text, out num) && (num == NPCID.DD2DarkMageT1 || num == NPCID.DD2DarkMageT3))
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2DarkMageT1) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2DarkMageT3))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.DD2DarkMageT1);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.DD2DarkMageT3);
                    bossName = Lang.GetNPCNameValue(NPCID.DD2DarkMageT1);
                }
            }
            else if (text == "srm" || text == "食人魔" || int.TryParse(text, out num) && (num == NPCID.DD2OgreT2 || num == NPCID.DD2OgreT3))
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2OgreT2) || config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2OgreT3))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.DD2OgreT2);
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.DD2OgreT3);
                    bossName = Lang.GetNPCNameValue(NPCID.DD2OgreT2);
                }
            }
            else if (text == "szyl" || text == "双足翼龙" || text == "betsy" || text == "贝西塔" || int.TryParse(text, out num) && num == NPCID.DD2Betsy)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.DD2Betsy))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.DD2Betsy);
                    bossName = Lang.GetNPCNameValue(NPCID.DD2Betsy);
                }
            }
            //南瓜月，霜月
            else if (text == "am" || text == "哀木" || int.TryParse(text, out num) && num == NPCID.MourningWood)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.MourningWood))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.MourningWood);
                    bossName = Lang.GetNPCNameValue(NPCID.MourningWood);
                }
            }
            else if (text == "ngw" || text == "南瓜王" || int.TryParse(text, out num) && num == NPCID.Pumpking)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.Pumpking))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Pumpking);
                    bossName = Lang.GetNPCNameValue(NPCID.Pumpking);
                }
            }
            else if (text == "cljjg" || text == "常绿尖叫怪" || text == "sds" || text == "圣诞树" || int.TryParse(text, out num) && num == NPCID.Everscream)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.Everscream))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.Everscream);
                    bossName = Lang.GetNPCNameValue(NPCID.Everscream);
                }
            }
            else if (text == "sdtk" || text == "圣诞坦克" || text == "sdlr" || text == "圣诞老人" || int.TryParse(text, out num) && num == NPCID.SantaNK1)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.SantaNK1))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.SantaNK1);
                    bossName = Lang.GetNPCNameValue(NPCID.SantaNK1);
                }
            }
            else if (text == "bxnh" || text == "冰雪女皇" || text == "bxnw" || text == "冰雪女王" || int.TryParse(text, out num) && num == NPCID.IceQueen)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(NPCID.IceQueen))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(NPCID.IceQueen);
                    bossName = Lang.GetNPCNameValue(NPCID.IceQueen);
                }
            }
            //非boss
            else if (int.TryParse(text, out num) && num >= NpcIDMin && num <= NpcIDMax)
            {
                if (config.ProtectedNPC_被保护的NPC.Contains(num))
                {
                    config.ProtectedNPC_被保护的NPC.Remove(num);
                    bossName = Lang.GetNPCNameValue(num);
                }
            }
            //判断删除成功
            if (bossName != "")
            {
                args.Player.SendMessage($"封禁Boss或生物: {bossName} 删除成功！", Color.LightGreen);
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
            foreach (int v in config.ProtectedNPC_被保护的NPC)
            {
                str += Lang.GetNPCNameValue(v) + $"[type:{v}]  ";
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
                args.Player.SendErrorMessage($"物品：[i:{itemID}] 已添加过");
            }
            else
            {
                config.ignoreCheckedItemsID_不需要被作弊检查的物品id.Add(itemID);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"物品：[i:{itemID}] 已添加", Color.LightGreen);
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
                args.Player.SendMessage($"物品：[i:{itemID}] 已删除", Color.LightGreen);
            }
            else
            {
                args.Player.SendErrorMessage($"物品：[i:{itemID}] 不存在");
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


        //添加强制检查物
        private void AddMustCheckedItem(CommandArgs args)
        {
            if (!args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /addmci 【item:id】 来添加必定被系统检查的物品（强制检查豁免物）\nEnter /addmci [item:id] To add items that must be checked by the system (will cover unchecked items)");
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
            if (config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Contains(itemID))
            {
                args.Player.SendErrorMessage($"物品：[i:{itemID}] 已添加过");
            }
            else
            {
                config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Add(itemID);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"物品：[i:{itemID}] 已添加", Color.LightGreen);
            }
        }


        //删除强制检查物
        private void DelMustCheckedItem(CommandArgs args)
        {
            if (!args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /delmci 【item:id】 来删除必定被系统检查的物品（强制检查豁免物）\nEnter /delmci [item:id] To delete items that must be checked by the system (will cover unchecked items)");
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
            if (config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Contains(itemID))
            {
                config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条.Remove(itemID);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                args.Player.SendMessage($"物品：[i:{itemID}] 已删除", Color.LightGreen);
            }
            else
            {
                args.Player.SendErrorMessage($"物品：[i:{itemID}] 不存在");
            }
        }


        //列出强制检查物
        private void ListMustCheckedItem(CommandArgs args)
        {
            if (args.Parameters.Any())
            {
                args.Player.SendInfoMessage("输入 /listmci 来查看所有被强制检查检查的物品\nEnter /listmci  To view all items subject to mandatory inspection");
                return;
            }

            string str = "";
            int count = 0;
            foreach (int v in config.MustBeCheckedItemsID_必须被检查的物品_覆盖上面一条)
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
            config = Config.LoadConfigFile();
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
                for (int i = 0; i < wPlayers.Count; i++)
                {
                    wPlayers[i] = new WPlayer(wPlayers[i].tsplayer);
                }
            }
        }
    }
}
