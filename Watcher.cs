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
using Rests;
using OTAPI;

namespace Watcher
{
    [ApiVersion(2, 1)]
    public partial class Watcher : TerrariaPlugin
    {
        /// <summary>
        /// Gets the author(s) of this plugin
        /// </summary>
        public override string Author => "z枳";

        /// <summary>
        /// Gets the description of this plugin.
        /// A short, one lined description that tells people what your plugin does.
        /// </summary>
        public override string Description => "详细监视日志和一些防违禁物措施";

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name => "Watcher";

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version => new Version(1, 0, 0, 1);

        #region 数据与字段
        //日志文件夹
        public string logDirPath = Path.Combine(TShock.SavePath + "/Watcher/logs");
        //日志文件路径
        public string logFilePath;
        //作弊检测日志文件夹
        public string cheatLogDirPath = Path.Combine(TShock.SavePath + "/Watcher/cheatLogs");
        //作弊检测日志
        public string cheatLogFilePath;
        //config文件路径
        public string configPath = Path.Combine(TShock.SavePath + "/Watcher", "WatcherConfig.json");
        //item的id上下限
        public const int ItemIDMax = Main.maxItemTypes, ItemIDMin = 1;
        //npc的id上下限
        public const int NpcIDMax = Main.maxNPCTypes, NpcIDMin = -65;
        //作弊玩家信息记录
        public List<WPlayer> wPlayers = new List<WPlayer>();
        //config变量
        public Config config;
        #endregion


        public Watcher(Main game) : base(game)
        {
        }


        public override void Initialize()
        {
            SetWatcherFile("logDirPath", logDirPath);
            SetWatcherFile("cheatLogDirPath", cheatLogDirPath);
            config = Config.LoadConfigFile();
            CheatData.SetCheatData();

            if (config.enableChinese_启用中文)
                LanguageManager.Instance.SetLanguage("zh-Hans");
            else
                LanguageManager.Instance.SetLanguage("default");

            //将聊天写入日志
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            //丢弃物品写入日志
            GetDataHandlers.ItemDrop += SetDropItemLog;
            //持有物品写入日志
            GetDataHandlers.PlayerSlot += SetItemLog;
            //生成射弹写入日志
            GetDataHandlers.NewProjectile += SetProjLog;
            //每秒服务器更新执行一次
            ServerApi.Hooks.GameUpdate.Register(this, GameRun);
            //射弹作弊检查
            GetDataHandlers.NewProjectile += ProjCheatingCheck;
            //持有物品时，检查背包
            GetDataHandlers.PlayerSlot += ItemCheatingCheck;
            //检查登录的人是否是作弊人员
            ServerApi.Hooks.ServerJoin.Register(this, OnServerjoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            //击中npc时触发，向攻击保护动物的玩家发送消息,对保护动物进行回血保护
            ServerApi.Hooks.NpcStrike.Register(this, OnStrike);
            
            //Hooks.NPC.Strike += OnStrike;
            //这里当boss生成是发送警告此为保护动物的消息
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            

            GeneralHooks.ReloadEvent += OnReload;

            #region 指令

            Commands.ChatCommands.Add(new Command("watcher.clearcheatdata", ClearCheatData, "clearcd", "CLEARCD")
            {
                HelpText = "输入 /clearcd 【玩家名称】 来清理该玩家的作弊数据\nEnter /clearcd [player name] To clear the player's cheating data"
            }); 
            Commands.ChatCommands.Add(new Command("watcher.clearcheatdata", ClearCheatDataAll, "clearcdall", "CLEARCDALL")
            {
                HelpText = "输入 /clearcdall 来清理所有玩家的作弊数据\nEnter /clearcd [player name] To clear the cheating data of all players"
            });


            Commands.ChatCommands.Add(new Command("watcher.locknpc", LockNpc, "locknpc", "LOCKNPC", "Locknpc")
            {
                HelpText = "输入 /locknpc 【NPC id/Boss 汉字名称/Boss 拼音缩写】 来封禁玩家对该boss或npc攻击\nEnter /lock [NPC id] To block the player from attacking the boss or NPC"
            });
            Commands.ChatCommands.Add(new Command("watcher.unlocknpc", UnlockNpc, "unlocknpc", "UNLOCKNPC", "Unlocknpc")
            {
                HelpText = "输入 /unlocknpc 【NPC id/Boss 汉字名称/Boss 拼音缩写】 来解除玩家对该boss或npc的攻击\nEnter /unlocknpc [NPC id] To remove the player's attack on the boss or NPC"
            });
            Commands.ChatCommands.Add(new Command("watcher.listlocknpc", ListLockNpc, "listlocknpc", "LISTLOCKNPC", "Listlocknpc")
            {
                HelpText = "输入 /listlocknpc   来查看所有禁止被攻击的生物\nEnter /listlocknpc   To view all creatures prohibited from being attacked"
            });


            Commands.ChatCommands.Add(new Command("watcher.adduncheckeditem", AddUnCheckedItem, "adduci", "ADDUCI")
            {
                HelpText = "输入 /adduci 【item:id】 来添加不被系统检查的物品\nEnter /adduci [item:id] To add items that are not checked by the system"
            });
            Commands.ChatCommands.Add(new Command("watcher.deluncheckeditem", DelUnCheckedItem, "deluci", "DELUCI")
            {
                HelpText = "输入 /deluci 【item:id】 来删除不被系统检查的物品\nEnter /deluci [item:id] To delete items that are not checked by the system"
            });
            Commands.ChatCommands.Add(new Command("watcher.listuncheckeditem", ListUnCheckedItem, "listuci", "LISTUCI")
            {
                HelpText = "输入 /listuci 来查看所有不被系统检查的物品\nEnter /listuci  To view all items that are not checked by the system"
            });


            Commands.ChatCommands.Add(new Command("watcher.addmustcheckeditem", AddMustCheckedItem, "addmci", "ADDMCI")
            {
                HelpText = "输入 /addmci 【item:id】 来添加必定被系统检查的物品（强制检查豁免物）\nEnter /addmci [item:id] To add items that must be checked by the system (will cover unchecked items)"
            });
            Commands.ChatCommands.Add(new Command("watcher.delmustcheckeditem", DelMustCheckedItem, "delmci", "DELMCI")
            {
                HelpText = "输入 /delmci 【item:id】 来删除必定被系统检查的物品（强制检查豁免物）\nEnter /delmci [item:id] To delete items that must be checked by the system (will cover unchecked items)"
            });
            Commands.ChatCommands.Add(new Command("watcher.listmustcheckeditem", ListMustCheckedItem, "listmci", "LISTMCI")
            {
                HelpText = "输入 /listmci 来查看所有被强制检查检查的物品\nEnter /listmci  To view all items subject to mandatory inspection"
            });

            #endregion

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                GetDataHandlers.ItemDrop -= SetDropItemLog;
                GetDataHandlers.PlayerSlot -= SetItemLog;
                GetDataHandlers.NewProjectile -= SetProjLog;

                ServerApi.Hooks.GameUpdate.Deregister(this, GameRun);
                GetDataHandlers.NewProjectile -= ProjCheatingCheck;
                GetDataHandlers.PlayerSlot -= ItemCheatingCheck;
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerjoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                //Hooks.Npc.Strike -= OnStrike;

                ServerApi.Hooks.NpcStrike.Deregister(this, OnStrike);

                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);


                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
    }
}