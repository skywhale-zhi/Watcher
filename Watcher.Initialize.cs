using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace Watcher
{
    public partial class Watcher : TerrariaPlugin
    {
        /// <summary>
        /// 对话写入日志
        /// </summary>
        /// <param name="args"></param>
        private void OnChat(ServerChatEventArgs args)
        {
            Player player = Main.player[args.Who];
            TSPlayer tSPlayer = TShock.Players[args.Who];
            if (args == null || tSPlayer == null || !tSPlayer.Active || !config.是否把对话内容写入日志)
            {
                return;
            }
            string text = args.Text;
            if (args.Text.StartsWith("/register ", StringComparison.OrdinalIgnoreCase) || args.Text.StartsWith(".register ", StringComparison.OrdinalIgnoreCase))
            {
                text = "/register 【密码不该在日志显示出来】";
            }
            if (args.Text.StartsWith("/login", StringComparison.OrdinalIgnoreCase) || args.Text.StartsWith(".login", StringComparison.OrdinalIgnoreCase))
            {
                text = "/login 【密码不该在日志显示出来】";
            }

            if (TShock.Players[args.Who].Account != null)
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{tSPlayer.Account.ID}][{tSPlayer.Name}]: \"{text}\" 在 {player.position / 16}" });
            else
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:?][{tSPlayer.Name}]: \"{text}\" 在 {player.position / 16}" });
        }


        /// <summary>
        /// 扔掉某项东西写入日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDropItemLog(object? sender, GetDataHandlers.ItemDropEventArgs e)
        {
            if (!config.是否把丢弃物写入日志 || CheatData.ImmunityDropItems.Contains(e.Type))
                return;

            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 丢弃 {e.Stacks} 个 {Lang.GetItemNameValue(e.Type)} 在 {e.Player.LastNetPosition / 16}" });
        }


        /// <summary>
        /// 在游戏初始化完成后执行
        /// </summary>
        /// <param name="args"></param>
        private void OnPostGameI(EventArgs args)
        {
            //创建好Watcher文件夹。watcher日志文件夹-日志。作弊文件夹-作弊日志
            SetWatcherFile();
            //watcer配置文件的加载
            config = Config.LoadConfigFile();
            if (config.启用伤害检测)
            {
                TShock.Config.Settings.MaxProjDamage = int.MaxValue;
                TShock.Config.Settings.MaxDamage = int.MaxValue;
                SaveTConfig();
            }
            //作弊玩家数据的加载
            wPlayers = WPlayer.LoadConfigFile();
            //每次启动时清理下旧日志
            DeleteOldFiles(logDirPath, config.Watcher日志的备份时长_单位分钟);
            DeleteOldFiles(cheatLogDirPath, config.Watcher日志的备份时长_单位分钟);
        }


        /// <summary>
        /// 游戏运行
        /// </summary>
        /// <param name="args"></param>
        private void GameRun(EventArgs args)
        {
            /*
            foreach (var v in TShock.Players)
            {
                if (v != null && v.IsLoggedIn && v.Group.Name == "superadmin")
                {
                    if (v.TPlayer.controlUseItem)
                    {
                        v.SendInfoMessage($"{v.TPlayer.itemAnimation}, {v.TPlayer.itemAnimationMax}, {v.TPlayer.itemTime}, {v.TPlayer.itemTimeMax}");

                    }
                }
            }
            */
            Timer++;
            //每秒执行一次，而不是每帧都执行，降低服务器压力
            if (Timer % 60L == 0L)
            {
                //把wPlayer里没同步的加进去
                foreach (var p in TShock.Players)
                {
                    if (p != null && p.Active)
                    {
                        WPlayer? wp = wPlayers.Find(x => x.uuid == p.UUID);
                        if (wp != null)
                        {
                            wp.本次进服游玩时间++;
                            wp.物品检测位 = 0;
                            wp.危险物检测位 = 0;
                            wp.伤害检测位 = 0;
                        }
                        else
                            wPlayers.Add(new WPlayer(p.Name, p.UUID, 0, 0, 0));
                    }
                }
                for (int i = 0; i < wPlayers.Count; i++)
                {
                    bool ex = false;
                    foreach (var v in TShock.Players)
                    {
                        if (wPlayers[i].总作弊次数 > 0 || v != null && v.Active && wPlayers[i].uuid == v.UUID)
                        {
                            ex = true;
                            break;
                        }
                    }
                    if (!ex)
                    {
                        wPlayers.RemoveAt(i);
                        i--;
                    }
                }
                //全员检查
                if (config.启用物品作弊检测 && Timer % (60L * config.全员物品检测时间间隔_单位秒) == 0L)//每几秒全员检查一次
                {
                    ItemOfPlayerCheck(null, out string mess);
                    wPlayers.ForEach(x =>
                    {
                        x.物品检测位++;
                    });
                }

                //避免日志过大
                if (Timer % 300L == 0L)
                {
                    AvoidLogSize(logDirPath, logFilePath);
                    AvoidLogSize(cheatLogDirPath, cheatLogFilePath);
                }
            }

            //禁止恶魔心饰品栏的问题
            if (config.是否禁用肉前恶魔心饰品栏 && Timer % 5L == 0L && !Main.hardMode)
            {
                foreach (TSPlayer p in TShock.Players)
                {
                    if (p == null || !p.IsLoggedIn || !NeedBeChecked(p))
                        continue;

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


        /// <summary>
        /// 射弹写入日志，射弹作弊检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewProjectile(object? sender, GetDataHandlers.NewProjectileEventArgs e)
        {
            if (config.是否把生成射弹写入日志 && CheatData.DangerousProjectile.Contains(e.Type))
            {
                File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 生成 {Lang.GetProjectileName(e.Type)} 在 {e.Position / 16}" });
            }
            //本来想在这里检测每秒扔出危险炸弹物，但是大多数炸弹物的扔出频率太低，大概也就1秒1次到1秒2次的频率，感觉检测的意义不大
            //我还没有吧喜庆弹射器2算进去
            if (config.危险射弹广播警告 && CheatData.DangerousProjectile.Contains(e.Type))
            {
                WPlayer? wp = wPlayers.Find(x => x.uuid == e.Player.UUID);
                if (wp != null)
                {
                    switch (e.Type)
                    {
                        //貌似炸弹都挺慢的
                        case 28:
                        case 29:
                        case 911:
                            wp.危险物检测位 += 0.55f;
                            break;
                            //环境溶液一秒喷6发，除以6平衡下
                        case 145:
                        case 146:
                        case 147:
                        case 148:
                        case 149:
                        case 1015:
                        case 1016:
                        case 1017:
                            wp.危险物检测位 += 0.16f;
                            break;
                        case 80:
                        case 158:
                        case 159:
                        case 160:
                        case 161:
                        case 281:
                        case 868:
                        case 869:
                            break;
                        default:
                            wp.危险物检测位 += 1f;
                            break;
                    }
                    if (wp.危险物检测位 > config.生成危险射弹的频率_次每秒)
                    {
                        Warning(e.Player, TypesOfCheat.DangerousProj, (TypesOfPunish)config.生成危险射弹警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出, false, Lang.GetProjectileName(e.Type).Value, 0, e.Type, e.Position);
                    }
                }
            }

            #region 射弹伤害检测
            //这个检测的目的是因为tshock自带的检测目前没排除永夜，泰拉刀 词缀和搭配泰坦手套导致的高额伤害bug，这些武器会由于bug生成伤害高达几万的怪异数字，但是实际上并不会在游戏里造成这样的伤害不过依然会被tshock捕捉
            //如果为了避免误判就要提高 tshock 的 config 内 MaxProjDamage 值，但是需要提高很多，就失去了检测的意义，所以这里进行了额外判断，能自动排除这些武器，但是需要将 tshock 的 MaxProjDamage 设置的非常大，只有这样改才能防止调用，因为tshock没有给出关掉这个检测的配置
            if (config.启用伤害检测 && NeedBeChecked(e.Player))
            {
                //判断伤害溢出的情况，排除灰橙冲击枪、尖桩发射器，永夜，真永夜，断钢剑，真断钢剑，泰拉刀，南瓜剑
                bool isExclude = e.Type == 876 || e.Type == 323 || e.Player.TPlayer.HeldItem.type == 273 || e.Player.TPlayer.HeldItem.type == 675 || e.Player.TPlayer.HeldItem.type == 368 || e.Player.TPlayer.HeldItem.type == 674 || e.Player.TPlayer.HeldItem.type == 757 || e.Player.TPlayer.HeldItem.type == 1826;
                //如果伤害 > config 内的 并且不在可排除的范围内，警告踢掉
                if (e.Damage > config.射弹最大伤害 && !isExclude)
                {
                    var wp = wPlayers.Find(x => x.uuid == e.Player.UUID);
                    if (wp != null && wp.伤害检测位 == 0)
                    {
                        wp.伤害检测位++;
                        Warning(e.Player, TypesOfCheat.DamageCheat, (TypesOfPunish)config.伤害作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出, config.伤害作弊是否算违规, Lang.GetProjectileName(e.Type).Value, e.Damage, e.Type, e.Position);
                    }
                }
            }
            #endregion


            #region 钓鱼检测
            if (config.启用浮标数目检测 && NeedBeChecked(e.Player))
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
                foreach (var v in CheatData.浮标射弹)
                {
                    if (Main.player[e.Player.Index].ownedProjectileCounts[v] > config.最大浮标数目)
                    {
                        target = v;
                    }
                }
                if (target != 0)
                {
                    Warning(e.Player, TypesOfCheat.FishCheat, (TypesOfPunish)config.钓鱼作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出, config.钓鱼作弊是否算违规, Lang.GetProjectileName(target).Value, 0, target, e.Position);
                }
            }
            #endregion
        }


        /// <summary>
        /// 手持物写入日志和物品进度检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerSlots(object? sender, GetDataHandlers.PlayerSlotEventArgs e)
        {
            //手持物写入日志
            if (config.是否把手持物写入日志)
            {
                if (e.Slot == 58 && e.Stack != 0 && !CheatData.ImmunityHoldItems.Contains(e.Type))
                {
                    File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" [acc:{e.Player.Account.ID}][{e.Player.Name}] 拿持 {e.Stack} 个 {Lang.GetItemNameValue(e.Type)} 在 {e.Player.LastNetPosition / 16}" });
                }
            }

            //物品作弊检测
            if (!config.启用物品作弊检测 || !(e.Type == 0 && e.Stack == 0))
            {
                return;
            }
            foreach (WPlayer w in wPlayers)
            {
                if (w.uuid == e.Player.UUID)
                {
                    //作弊玩家
                    if (w.物品作弊次数 > 0 && w.本次进服游玩时间 > 10 && w.物品检测位 == 0 && getRand(config.单人物品检测概率 * 2))
                    {
                        ItemOfPlayerCheck(e.Player, out string mess);
                        w.物品检测位++;
                    }
                    //非作弊玩家
                    if (w.物品作弊次数 == 0 && w.本次进服游玩时间 > 10 && w.物品检测位 == 0 && getRand(config.单人物品检测概率))
                    {
                        ItemOfPlayerCheck(e.Player, out string mess);
                        w.物品检测位++;
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// 玩家进入
        /// </summary>
        /// <param name="args"></param>
        private void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            TSPlayer tsplayer = TShock.Players[args.Who];
            if (tsplayer == null || !tsplayer.Active)
                return;

            //这个玩家存在在wPlayer里?
            bool ex = false;
            //如果进入游戏的玩家uuid有记录即：重进玩家，换号玩家。则同步下name,acc,tsplayer信息
            for (int i = 0; i < wPlayers.Count; i++)
            {
                if (wPlayers[i].uuid == tsplayer.UUID && wPlayers[i].总作弊次数 > 0)
                {
                    ex = true;
                    wPlayers[i].name = tsplayer.Name;
                    wPlayers[i].本次进服游玩时间 = 0;
                    TShock.Players[args.Who].SendErrorMessage($"您已违规 {wPlayers[i].总作弊次数} 次，最多 {config.最多违规作弊次数} 次开始封禁");
                    if (wPlayers[i].物品作弊次数 > 0 && !config.检测到违禁物时是否清空)
                        tsplayer.SendInfoMessage("请在 10 秒内清理身上的违禁物品");
                }
                else if (wPlayers[i].uuid == tsplayer.UUID)
                {
                    if (wPlayers[i].name != tsplayer.Name)
                        wPlayers[i].name = tsplayer.Name;
                    ex = true;
                }
            }
            if (!ex)
            {
                wPlayers.Add(new WPlayer(tsplayer.Name, tsplayer.UUID, 0, 0, 0));
            }
            WPlayer.SaveConfigFile();

            //向watcherlog写入玩家进入信息
            int count = 0;
            string str = "";
            foreach (TSPlayer v in TShock.Players)
            {
                if (v != null)
                {
                    count++;
                    str += $"[{v.Name}], ";
                }
            }
            str = str.TrimEnd(' ', ',');
            str = str.TrimEnd(' ', ',');
            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" INFORMATION [{tsplayer.Name}] 进入游戏，当前在线玩家{count}人：{(count > 0 ? str : "无")}" });
        }


        /// <summary>
        /// 玩家离开时发送消息
        /// </summary>
        /// <param name="args"></param>
        private void OnServerLeave(LeaveEventArgs args)
        {
            //向watcherlog写入消息
            if (args == null || TShock.Players[args.Who] == null)
                return;
            //退出的人
            TSPlayer player = TShock.Players[args.Who];
            int count = 0;
            string str = "";
            foreach (TSPlayer v in TShock.Players)
            {
                if (v != null)
                {
                    count++;
                    str += $"[{v.Name}], ";
                }
            }
            str.Replace("[" + player.Name + "]", "");
            count--;
            while (str.EndsWith(' ') || str.EndsWith(','))
            {
                str = str.TrimEnd(' ', ',');
            }
            File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("u") + $" INFORMATION [{player.Name}] 已离开，当前在线玩家{count}人：{(count > 0 ? str : "无")}" });
            WPlayer.SaveConfigFile();
        }


        /// <summary>
        /// 检查伤害溢出
        /// </summary>
        /// <param name="args"></param>
        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            if (!config.启用伤害检测)
                return;
            //灰冲击枪，橙冲击枪，尖桩发射器和吸血鬼
            bool isExclude = args.Player.HeldItem.type == 4347 || args.Player.HeldItem.type == 4348 || args.Player.HeldItem.type == 1835 && (args.Npc.netID == 158 || args.Npc.netID == 159);
            if (args.Damage > config.其他最大伤害 && !isExclude)
            {
                var wp = wPlayers.Find(x => x.uuid == TShock.Players[args.Player.whoAmI].UUID);
                if (wp != null && wp.伤害检测位 == 0)
                {
                    wp.伤害检测位++;
                    Warning(TShock.Players[args.Player.whoAmI], TypesOfCheat.DamageCheat, (TypesOfPunish)config.伤害作弊警告方式_0口头私聊_1广播警告_2广播并网住_3广播并杀死_4广播并踢出, config.伤害作弊是否算违规, "", args.Damage, 0, args.Player.position);
                }
            }
        }


        #region 指令

        /// <summary>
        /// 帮助指令
        /// </summary>
        /// <param name="args"></param>
        private void Help(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage("输入 /wat help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    args.Player.SendInfoMessage(
                        "输入 /wat me    来查看自己的违规状态\n" +
                        "输入 /wat clear [name]    来清理该玩家的作弊数据，可以填多个\n" +
                        "输入 /wat clearall    来清理所有玩家的作弊数据\n" +
                        "输入 /wat adduci [ID/name]    来添加不被系统检查的物品，可以填多个\n" +
                        "输入 /wat deluci [ID/name]    来删除不被系统检查的物品，可以填多个\n" +
                        "输入 /wat listuci    来查看所有不被系统检查的物品\n" +
                        "输入 /wat addmci [ID/name]    来添加必定被系统检查的物品(强制检查豁免物)，可以填多个\n" +
                        "输入 /wat delmci [ID/name]    来删除必定被系统检查的物品(强制检查豁免物)，可以填多个\n" +
                        "输入 /wat listmci    来查看所有被强制检查检查的物品\n" +
                        "输入 /wat check [name]    来强制检查某个玩家的物品是否合理，只能填一个\n" +
                        "输入 /wat checkall    来强制检查所有玩家的物品是否合理\n" +
                        "输入 /wat checki [ID/name]    来检查这个物品在当前进度是否合理，可以填多个");
                }
                else if (args.Parameters[0].Equals("me", StringComparison.OrdinalIgnoreCase))
                {
                    string str = "";
                    foreach (WPlayer me in wPlayers)
                    {
                        if (me.uuid == args.Player.UUID && me.总作弊次数 > 0)
                        {
                            str = "您已违规！\n钓鱼作弊次数：" + me.钓鱼作弊次数 +
                                "\n伤害作弊次数：" + me.伤害作弊次数 +
                                "\n物品作弊次数：" + me.物品作弊次数 +
                                "\n总计作弊次数：" + me.总作弊次数 +
                                "\n距封禁还剩：" + (config.最多违规作弊次数 - me.总作弊次数) + " 次";
                            break;
                        }
                    }
                    if (str == "")
                    {
                        args.Player.SendMessage("您没有违规记录", new Color(0, 255, 0));
                    }
                }
                else if (args.Parameters[0].Equals("clearall", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission(p_admin))
                    {
                        args.Player.SendErrorMessage($"权限不足：[{p_admin}]"); return;
                    }
                    wPlayers.Clear();
                    WPlayer.SaveConfigFile();
                    if (args.Player.IsLoggedIn)
                    {
                        TSPlayer.All.SendMessage("所有玩家的作弊数据已清除", Color.Lime);
                    }
                    else
                    {
                        args.Player.SendInfoMessage("所有玩家的作弊数据已清除");
                        TSPlayer.All.SendMessage("所有玩家的作弊数据已清除", Color.Lime);
                    }
                }
                else if (args.Parameters[0].Equals("listuci", StringComparison.OrdinalIgnoreCase))
                {
                    string str = "";
                    int count = 0;
                    if (args.Player.IsLoggedIn)
                    {
                        foreach (int v in config.不需要被作弊检查的物品ID.Keys.ToArray())
                        {
                            str += $"[i:{v}] ";
                            count++;
                            if (count == 30)
                            {
                                str += "\n";
                                count = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (int v in config.不需要被作弊检查的物品ID.Keys.ToArray())
                        {
                            str += $"[{Lang.GetItemNameValue(v)}:{v}] ";
                            count++;
                            if (count == 10)
                            {
                                str += "\n";
                                count = 0;
                            }
                        }
                    }
                    while (str.EndsWith(' ') || str.EndsWith('\n'))
                    {
                        str = str.TrimEnd(' ', '\n');
                    }
                    if (str != "")
                        args.Player.SendInfoMessage("所有不被检查的物品：\n" + str);
                    else
                        args.Player.SendInfoMessage("所有不被检查的物品：无");
                }
                else if (args.Parameters[0].Equals("listmci", StringComparison.OrdinalIgnoreCase))
                {
                    string str = "";
                    int count = 0;
                    if (args.Player.IsLoggedIn)
                    {
                        foreach (int v in config.必须被检查的物品_覆盖上面一条.Keys.ToArray())
                        {
                            str += $"[i:{v}] ";
                            count++;
                            if (count == 30)
                            {
                                str += "\n";
                                count = 0;
                            }
                        }
                    }
                    else
                    {
                        foreach (int v in config.必须被检查的物品_覆盖上面一条.Keys.ToArray())
                        {
                            str += $"[{Lang.GetItemNameValue(v)}:{v}] ";
                            count++;
                            if (count == 10)
                            {
                                str += "\n";
                                count = 0;
                            }
                        }
                    }
                    while (str.EndsWith(' ') || str.EndsWith('\n'))
                    {
                        str = str.TrimEnd(' ', '\n');
                    }
                    if (str != "")
                        args.Player.SendInfoMessage("所有必定被检查的物品：\n" + str);
                    else
                        args.Player.SendInfoMessage("所有必定被检查的物品：无");
                }
                else if (args.Parameters[0].Equals("checkall", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission(p_admin) && !args.Player.HasPermission(p_check))
                    {
                        args.Player.SendErrorMessage($"权限不足：[{p_check}]"); return;
                    }
                    if (ItemOfPlayerCheck(null, out string mess))
                        args.Player.SendInfoMessage(mess);
                    else
                        args.Player.SendSuccessMessage("目前未发现有玩家持有不合理物品");
                }
                else
                    args.Player.SendInfoMessage("输入 /wat help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count >= 2)
            {
                if (args.Parameters[0].Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission(p_admin))
                    {
                        args.Player.SendErrorMessage($"权限不足：[{p_admin}]"); return;
                    }
                    List<string> list = args.Parameters.ToList();
                    list.RemoveAt(0);
                    list.RemoveAll(x => string.IsNullOrWhiteSpace(x));
                    foreach (var v in list)
                    {
                        bool find = false;
                        for (int i = 0; i < wPlayers.Count; i++)
                        {
                            if (wPlayers[i].name == v)
                            {
                                find = true;
                                args.Player.SendSuccessMessage("玩家：[" + v + "] 的所有作弊信息已清除");
                                foreach (var p in TShock.Players)
                                {
                                    if (p != null && p.IsLoggedIn && p.UUID == wPlayers[i].uuid)
                                        p.SendInfoMessage("您的所有作弊信息已清除");
                                }
                                wPlayers.RemoveAt(i);
                                break;
                            }
                        }
                        if (!find)
                        {
                            args.Player.SendInfoMessage("玩家：[" + v + "] 的未作弊");
                        }
                    }
                }
                else if (args.Parameters[0].Equals("adduci", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("deluci", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("addmci", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("delmci", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("checki", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters[0].Equals("checki", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!args.Player.HasPermission(p_admin) && !args.Player.HasPermission(p_check))
                        {
                            args.Player.SendErrorMessage($"权限不足：[{p_check}]"); return;
                        }
                    }
                    else
                    {
                        if (!args.Player.HasPermission(p_admin))
                        {
                            args.Player.SendErrorMessage($"权限不足：[{p_admin}]"); return;
                        }
                    }
                    List<int> items = new List<int>();
                    for (int i = 1; i < args.Parameters.Count; i++)
                    {
                        if (int.TryParse(args.Parameters[i], out int temp))
                        {
                            if (temp > 0 && temp < ItemID.Count)
                                items.Add(temp);
                            else
                                args.Player.SendInfoMessage($"数字 {temp} 不是合理的物品ID");
                        }
                        else
                        {
                            List<Item> items2 = new List<Item>();
                            for (int j = 1; j < ItemID.Count; j++)
                            {
                                var itemtemp = ContentSamples.ItemsByType[j];
                                if (itemtemp.Name.Contains(args.Parameters[i], StringComparison.OrdinalIgnoreCase))
                                {
                                    items2.Add(itemtemp);
                                }
                            }
                            if (items2.Count > 1)
                            {
                                string messc = $"搜索到叫[{args.Parameters[i]}]物品有多个，你想查找的是？\n";
                                int countc = 0;
                                foreach (var c in items2)
                                {
                                    countc++;
                                    messc += c.Name + ":" + c.netID + "，";
                                    if (countc == 10)
                                    {
                                        messc += "\n"; countc = 0;
                                    }
                                }
                                while (messc.EndsWith('\n') || messc.EndsWith('，'))
                                    messc = messc.TrimEnd('，', '\n');
                                args.Player.SendMessage(messc, Color.Orange);
                            }
                            else if (items2.Count == 1)
                            {
                                items.Add(items2[0].netID);
                            }
                            else
                                args.Player.SendInfoMessage($"物品 [{args.Parameters[i]}] 不存在");
                        }
                    }
                    //讨论
                    if (items.Count > 0)
                    {
                        if (args.Parameters[0].Equals("checki", StringComparison.OrdinalIgnoreCase))
                        {
                            string mess;
                            List<Item> temp = new List<Item>();
                            foreach (var item in items)
                            {
                                temp.Add(ContentSamples.ItemsByType[item]);
                            }
                            if (!ReasonableItem(temp, out Dictionary<Item, string> abitem1) | !ReasonableItemOfRecipe(temp, out Dictionary<Item, string> abItem2))
                            {
                                mess = "含有违规物品：\n";
                                foreach (var t1 in abitem1)
                                {
                                    mess += t1.Value + "\n";
                                }
                                foreach (var t2 in abItem2)
                                {
                                    mess += t2.Value + "\n";
                                }
                                mess = mess.Trim('\n');
                                args.Player.SendInfoMessage(mess);
                                return;
                            }
                            else
                            {
                                mess = "该物品不违规：";
                                foreach (var i in temp)
                                {
                                    mess += i.Name + "，";
                                }
                                mess = mess.TrimEnd('，');
                                args.Player.SendInfoMessage(mess);
                                return;
                            }
                        }

                        string str = "物品：";
                        var keys1 = config.不需要被作弊检查的物品ID.Keys.ToList();
                        var keys2 = config.必须被检查的物品_覆盖上面一条.Keys.ToList();
                        foreach (var v in items)
                        {
                            if (args.Parameters[0].Equals("adduci", StringComparison.OrdinalIgnoreCase))
                            {
                                if (config.不需要被作弊检查的物品ID.TryAdd(v, Lang.GetItemNameValue(v)))
                                {
                                    str += Lang.GetItemNameValue(v) + "，";
                                }
                            }
                            if (args.Parameters[0].Equals("deluci", StringComparison.OrdinalIgnoreCase))
                            {
                                if (keys1.Remove(v))
                                {
                                    str += Lang.GetItemNameValue(v) + "，";
                                }
                            }
                            if (args.Parameters[0].Equals("addmci", StringComparison.OrdinalIgnoreCase))
                            {
                                if (config.必须被检查的物品_覆盖上面一条.TryAdd(v, Lang.GetItemNameValue(v)))
                                {
                                    str += Lang.GetItemNameValue(v) + "，";
                                }
                            }
                            if (args.Parameters[0].Equals("delmci", StringComparison.OrdinalIgnoreCase))
                            {
                                if (keys2.Remove(v))
                                {
                                    str += Lang.GetItemNameValue(v) + "，";
                                }
                            }
                        }
                        if (str == "物品：")
                        {
                            if (args.Parameters[0].Equals("adduci", StringComparison.OrdinalIgnoreCase))
                                args.Player.SendInfoMessage("添加豁免物失败，请检查是否已添加过");
                            if (args.Parameters[0].Equals("deluci", StringComparison.OrdinalIgnoreCase))
                                args.Player.SendInfoMessage("删除豁免物失败，请检查是否存在");
                            if (args.Parameters[0].Equals("addmci", StringComparison.OrdinalIgnoreCase))
                                args.Player.SendInfoMessage("添加违禁物失败，请检查是否已添加过");
                        }
                        else
                        {
                            str = str.TrimEnd('，');
                            if (args.Parameters[0].Equals("adduci", StringComparison.OrdinalIgnoreCase))
                                str += " 添加豁免物成功";
                            if (args.Parameters[0].Equals("deluci", StringComparison.OrdinalIgnoreCase))
                            {
                                str += " 删除豁免物成功";
                                config.不需要被作弊检查的物品ID.Clear();
                                foreach (var v in keys1)
                                {
                                    config.不需要被作弊检查的物品ID.TryAdd(v, Lang.GetItemNameValue(v));
                                }
                            }
                            if (args.Parameters[0].Equals("addmci", StringComparison.OrdinalIgnoreCase))
                                str += " 添加违禁物成功";
                            if (args.Parameters[0].Equals("delmci", StringComparison.OrdinalIgnoreCase))
                            {
                                str += " 删除违禁物成功";
                                config.必须被检查的物品_覆盖上面一条.Clear();
                                foreach (var v in keys2)
                                {
                                    config.必须被检查的物品_覆盖上面一条.TryAdd(v, Lang.GetItemNameValue(v));
                                }
                            }
                            config.SaveConfigFile();
                            args.Player.SendSuccessMessage(str);
                        }
                    }
                    else
                    {
                        if (args.Parameters[0].Equals("adduci", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("addmci", StringComparison.OrdinalIgnoreCase))
                            args.Player.SendInfoMessage("添加失败，你输入的ID或名称均无效");
                        if (args.Parameters[0].Equals("deluci", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("delmci", StringComparison.OrdinalIgnoreCase))
                            args.Player.SendInfoMessage("删除失败，你输入的ID或名称均无效");
                        if (args.Parameters[0].Equals("checki", StringComparison.OrdinalIgnoreCase))
                            args.Player.SendInfoMessage("检查失败，你输入的ID或名称均无效");
                    }
                }
                else if (args.Parameters[0].Equals("check", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission(p_admin) && !args.Player.HasPermission(p_check))
                    {
                        args.Player.SendErrorMessage($"权限不足：[{p_check}]"); return;
                    }
                    List<TSPlayer> players = new List<TSPlayer>();
                    if (int.TryParse(args.Parameters[1], out int index) && TShock.Players[index] != null && TShock.Players[index].Active)
                    {
                        players.Add(TShock.Players[index]);
                    }
                    if (players.Count == 0)
                        foreach (var p in TShock.Players)
                        {
                            if (p != null && p.Active && p.Name == args.Parameters[1])
                            {
                                players.Add(p);
                            }
                        }
                    if (players.Count == 0)
                        foreach (var p in TShock.Players)
                        {
                            if (p != null && p.Active && p.Name.Contains(args.Parameters[1], StringComparison.OrdinalIgnoreCase))
                            {
                                players.Add(p);
                            }
                        }
                    if (players.Count == 0)
                        args.Player.SendInfoMessage("此玩家不存在");
                    else
                    {
                        if (players.Count > 1)
                            args.Player.SendInfoMessage("查找到该玩家不唯一");
                        foreach (var v in players)
                        {
                            if (ItemOfPlayerCheck(v, out string mess))
                            {
                                args.Player.SendInfoMessage(mess);
                            }
                            else
                            {
                                args.Player.SendSuccessMessage($"玩家 [{v.Name}] 未持有不合理物品");
                            }
                        }
                    }
                }
                else
                    args.Player.SendInfoMessage("输入 /wat help 来获取该插件的帮助");
            }
        }
        #endregion
    }
}
