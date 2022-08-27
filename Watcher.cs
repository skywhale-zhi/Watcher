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
        public override Version Version => new Version(1, 0, 0, 0);

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
        public const int ItemIDMax = 5124, ItemIDMin = 1;
        //npc的id上下限
        public const int NpcIDMax = 669, NpcIDMin = -65;
        //作弊人员信息记录
        //[0]：玩家名称  [1]：违规次数  [2]：违规类型  [3]：计时器。违规类型：“damage”，“fishing”，“item” 分别为 伤害溢出，多杆钓鱼，不合理物品 三种作弊
        public List<string[]> OnlineCheakingPlayers = new List<string[]>();
        //config变量
        public Config config;
        #endregion


        /// <summary>
        /// Initializes a new instance of the TestPlugin class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        ///初始化TestPlugin类的新实例。
        ///这是设置插件顺序和性能的地方，来自其他构造函数逻辑
        /// </summary>
        public Watcher(Main game) : base(game)
        {
        }


        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        ///处理插件初始化。
        ///在服务器启动和插件加载时触发。
        ///您可以在此处注册挂钩、执行加载过程等。
        /// </summary>
        public override void Initialize()
        {
            SetWatcherFile("logDirPath", logDirPath);
            SetWatcherFile("cheatLogDirPath", cheatLogDirPath);
            Config.SetConfigFile();
            config = Config.ReadConfigFile();
            CheatData.SetCheatData();

            if (config.enableChinese_启用中文)
                LanguageManager.Instance.SetLanguage("zh-Hans");
            else
                LanguageManager.Instance.SetLanguage("default");

            //将聊天写入日志
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
            //丢弃物品写入日志
            GetDataHandlers.ItemDrop.Register(SetDropItemLog);
            //持有物品写入日志
            GetDataHandlers.PlayerSlot.Register(SetItemLog);
            //生成射弹写入日志
            GetDataHandlers.NewProjectile.Register(SetProjLog);
            //召唤boss写入日志
            //ServerApi.Hooks.NetGetData.Register(this, SummonBoss);
            //放置物写入日志
            //GetDataHandlers.PlaceObject.Register(PlaceTiles);
            //每秒服务器更新执行一次
            ServerApi.Hooks.GameUpdate.Register(this, GameRun);
            //射弹作弊检查
            GetDataHandlers.NewProjectile.Register(ProjCheatingCheck);
            //持有物品时，检查背包
            GetDataHandlers.PlayerSlot.Register(ItemCheatingCheck);
            //检查登录的人是否是作弊人员
            ServerApi.Hooks.ServerJoin.Register(this, OnServerjoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            //击中npc时触发，向攻击保护动物的玩家发送消息,对保护动物进行回血保护
            Hooks.Npc.Strike += OnStrike;
            //这里当boss生成是发送警告此为保护动物的消息
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);


            GeneralHooks.ReloadEvent += OnReload;

            #region 指令
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
            #endregion

            //ServerApi.Hooks.NpcKilled.Register(this, new HookHandler<NpcKilledEventArgs>(this.OnNpcKilled));
            //ServerApi.Hooks.ServerLeave.Register(this, new HookHandler<LeaveEventArgs>(this.OnServerLeave));
        }


        /// <summary>
        /// Handles plugin disposal logic.
        /// *Supposed* to fire when the server shuts down.
        /// You should deregister hooks and free all resources here.
        ///处理插件处理逻辑。
        ///*Supposed**应该*在服务器关闭时触发。
        ///您应该取消注册挂钩并释放此处的所有资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
                //GetDataHandlers.ItemDrop.UnRegister(SetDropItemLog);
                //GetDataHandlers.PlayerSlot.UnRegister(SetItemLog);
                //GetDataHandlers.NewProjectile.UnRegister(SetProjLog);
                ServerApi.Hooks.GameUpdate.Deregister(this, GameRun);
                //GetDataHandlers.NewProjectile.UnRegister(ProjCheatingCheck);
                //GetDataHandlers.PlayerSlot.UnRegister(ItemCheatingCheck);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerjoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                //ServerApi.Hooks.NetGetData.Deregister(this, SummonBoss);
                Hooks.Npc.Strike -= OnStrike;
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);


                GeneralHooks.ReloadEvent -= OnReload;

                //GetDataHandlers.PlayerSlot.UnRegister(TestItem);
                //ServerApi.Hooks.NpcKilled.Deregister(this, new HookHandler<NpcKilledEventArgs>(this.OnNpcKilled));
                //ServerApi.Hooks.ServerLeave.Deregister(this, new HookHandler<LeaveEventArgs>(this.OnServerLeave));
            }
            base.Dispose(disposing);
        }
    }
}