using OTAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Watcher
{
    [ApiVersion(2, 1)]
    public partial class Watcher : TerrariaPlugin
    {
        public override string Author => "z枳";

        public override string Description => "详细监视日志和一些防作弊措施";

        public override string Name => "Watcher";

        public override Version Version => new Version(2, 0, 0, 0);

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
        public readonly int ItemIDMax = ItemID.Count, ItemIDMin = 1;
        //npc的id上下限
        public readonly int NpcIDMax = NPCID.Count, NpcIDMin = -65;
        //作弊玩家信息记录
        public static List<WPlayer> wPlayers = new List<WPlayer>();
        //config变量
        public Config config;
        /// <summary>
        /// 作弊类型的枚举
        /// </summary>
        public enum TypesOfCheat
        {
            FishCheat,      //多线钓鱼作弊
            DamageCheat,    //高额伤害作弊
            ItemCheat,      //超进度物品作弊
            VortexCheat     //星璇机枪作弊
        }
        #endregion

        public Watcher(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            SetWatcherFile("logDirPath", logDirPath);
            SetWatcherFile("cheatLogDirPath", cheatLogDirPath);
            config = Config.LoadConfigFile();
            wPlayers = WPM.LoadConfigFile();
            CheatData.SetCheatData();

            if (config.启用中文)
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
           
            //这里当boss生成是发送警告此为保护动物的消息
            GeneralHooks.ReloadEvent += OnReload;


            #region 指令

            Commands.ChatCommands.Add(new Command("", Help, "watcher", "wat")
            {
                HelpText = "输入 /watcher(或 wat) help 来获取该插件的帮助"
            });


            Commands.ChatCommands.Add(new Command("watcher.clearcheatdata", ClearCheatData, "clearcd", "clcd")
            {
                HelpText = "输入 /clearcd(或clcd) 【玩家名称】 来清理该玩家的作弊数据"
            });
            Commands.ChatCommands.Add(new Command("watcher.clearcheatdata", ClearCheatDataAll, "clearcda", "clall")
            {
                HelpText = "输入 /clearcdall(或clall) 来清理所有玩家的作弊数据"
            });


            Commands.ChatCommands.Add(new Command("watcher.locknpc", LockNpc, "locknpc")
            {
                HelpText = "输入 /locknpc 【NPC id/Boss 汉字名称/Boss 拼音缩写】 来封禁玩家对该boss或npc攻击"
            });
            Commands.ChatCommands.Add(new Command("watcher.unlocknpc", UnlockNpc, "unlocknpc")
            {
                HelpText = "输入 /unlocknpc 【NPC id/Boss 汉字名称/Boss 拼音缩写】 来解除玩家对该boss或npc的攻击"
            });
            Commands.ChatCommands.Add(new Command("watcher.listlocknpc", ListLockNpc, "listlocknpc")
            {
                HelpText = "输入 /listlocknpc   来查看所有禁止被攻击的生物"
            });


            Commands.ChatCommands.Add(new Command("watcher.adduncheckeditem", AddUnCheckedItem, "adduci")
            {
                HelpText = "输入 /adduci 【item:id】 来添加不被系统检查的物品"
            });
            Commands.ChatCommands.Add(new Command("watcher.deluncheckeditem", DelUnCheckedItem, "deluci")
            {
                HelpText = "输入 /deluci 【item:id】 来删除不被系统检查的物品"
            });
            Commands.ChatCommands.Add(new Command("watcher.listuncheckeditem", ListUnCheckedItem, "listuci")
            {
                HelpText = "输入 /listuci 来查看所有不被系统检查的物品"
            });


            Commands.ChatCommands.Add(new Command("watcher.addmustcheckeditem", AddMustCheckedItem, "addmci")
            {
                HelpText = "输入 /addmci 【item:id】 来添加必定被系统检查的物品（强制检查豁免物）"
            });
            Commands.ChatCommands.Add(new Command("watcher.delmustcheckeditem", DelMustCheckedItem, "delmci")
            {
                HelpText = "输入 /delmci 【item:id】 来删除必定被系统检查的物品（强制检查豁免物）"
            });
            Commands.ChatCommands.Add(new Command("watcher.listmustcheckeditem", ListMustCheckedItem, "listmci")
            {
                HelpText = "输入 /listmci 来查看所有被强制检查检查的物品"
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

                ServerApi.Hooks.NpcStrike.Deregister(this, OnStrike);
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);

                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
    }
}