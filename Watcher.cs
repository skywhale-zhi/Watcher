using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Watcher;

[ApiVersion(2, 1)]
public partial class Watcher : TerrariaPlugin
{
    public override string Author => "z枳";

    public override string Description => "更详细的日志信息和一些防作弊措施";

    public override string Name => "Watcher";

    public override Version Version => new Version(1, 3, 0, 0);


    #region 数据与字段
    /// <summary>
    /// 日志文件夹
    /// </summary>
    public readonly string logDirPath = Path.Combine(TShock.SavePath + "/Watcher/WatcherLogs");
    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string logFilePath = "";
    /// <summary>
    /// 作弊检测日志文件夹
    /// </summary>
    public readonly string cheatLogDirPath = Path.Combine(TShock.SavePath + "/Watcher/CheatLogs");
    /// <summary>
    /// 作弊检测日志
    /// </summary>
    public string cheatLogFilePath = "";
    /// <summary>
    /// 玩家的信息记录，考虑active，保留作弊和在线的玩家，不作弊且不在线的会被移除
    /// </summary>
    public static List<WPlayer> wPlayers = new List<WPlayer>();
    /// <summary>
    /// config变量
    /// </summary>
    public static Config config = new Config();
    /// <summary>
    /// 计时器 , 60 Timer == 1 秒
    /// </summary>
    public static long Timer = 0;
    /// <summary>
    /// 作弊类型的枚举
    /// </summary>
    public enum TypesOfCheat
    {
        FishCheat,      //多线钓鱼作弊
        DamageCheat,    //高额伤害作弊
        ItemCheat,      //超进度物品作弊
        DangerousProj   //危险的射弹
    }
    /// <summary>
    /// 惩罚类型
    /// </summary>
    public enum TypesOfPunish
    {
        oral,       //口头惩罚
        publicWarning, //公开警告
        disable,    //封住行动
        kill,       //杀掉
        kick,       //踢掉
    }
    //权限
    public readonly string p_admin = "watcher.admin";
    public readonly string p_check = "watcher.check";
    public readonly string p_use = "watcher.use";
    #endregion


    public Watcher(Main game) : base(game) { }

    public override void Initialize()
    {
        Timer = 0L;

        //将聊天写入日志
        ServerApi.Hooks.ServerChat.Register(this, OnChat);
        //丢弃物品写入日志
        GetDataHandlers.ItemDrop += SetDropItemLog;
        //持有物品写入日志，持有物品时，检查背包
        GetDataHandlers.PlayerSlot += PlayerSlots;
        //生成射弹写入日志并检查射弹作弊
        GetDataHandlers.NewProjectile += OnNewProjectile;
        //初始化完毕时
        ServerApi.Hooks.GamePostInitialize.Register(this, OnPostGameI);
        //击中npc时，记住伤害
        ServerApi.Hooks.NpcStrike.Register(this, OnNpcStrike);

        /*
        GetDataHandlers.ChestItemChange.Register(OnChestItemChange);
        GetDataHandlers.ChestOpen.Register(OnChestOpen);
        ServerApi.Hooks.ItemForceIntoChest.Register(this, OnItemForceChest);
        */

        //每秒服务器更新执行一次
        ServerApi.Hooks.GameUpdate.Register(this, GameRun);
        //检查登录的人是否是作弊人员
        ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
        ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
        GeneralHooks.ReloadEvent += OnReload;

        #region 指令

        Commands.ChatCommands.Add(new Command(p_use, Help, "watcher", "wat")
        {
            HelpText = "输入 /wat help 来获取该插件的帮助"
        });
        #endregion
        /*
        Commands.ChatCommands.Add(new Command("", Test, "t")
        {
            HelpText = "输入 /t"
        });*/

    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
            GetDataHandlers.ItemDrop -= SetDropItemLog;
            GetDataHandlers.NewProjectile -= OnNewProjectile;
            GetDataHandlers.PlayerSlot -= PlayerSlots;
            ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostGameI);
            ServerApi.Hooks.NpcStrike.Deregister(this, OnNpcStrike);
            ServerApi.Hooks.GameUpdate.Deregister(this, GameRun);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            GeneralHooks.ReloadEvent -= OnReload;
        }
        base.Dispose(disposing);
    }


    private void OnReload(ReloadEventArgs e)
    {
        Config config = Config.LoadConfigFile();
        //把一些不合法的配置文件输入纠正下
        if (config.全员物品检测时间间隔_单位秒 < 1)
        {
            e.Player.SendWarningMessage("全员物品检测时间间隔不能小于1，已改为默认300");
            config.全员物品检测时间间隔_单位秒 = 300;
        }
        if (config.启用伤害检测)
        {
            TShock.Config.Settings.MaxProjDamage = int.MaxValue;
            TShock.Config.Settings.MaxDamage = int.MaxValue;
            SaveTConfig();
        }
        if (config.伤害作弊警告方式 < (int)TypesOfPunish.oral || config.伤害作弊警告方式 > (int)TypesOfPunish.kick)
        {
            e.Player.SendWarningMessage("伤害作弊警告方式的范围为 0 ~ 4 整数，已纠正为默认3");
            config.伤害作弊警告方式 = 3;
        }
        if (config.物品作弊警告方式 < (int)TypesOfPunish.oral || config.物品作弊警告方式 > (int)TypesOfPunish.kick)
        {
            e.Player.SendWarningMessage("物品作弊警告方式的范围为 0 ~ 4 整数，已纠正为默认3");
            config.物品作弊警告方式 = 3;
        }
        if (config.钓鱼作弊警告方式 < (int)TypesOfPunish.oral || config.钓鱼作弊警告方式 > (int)TypesOfPunish.kick)
        {
            e.Player.SendWarningMessage("钓鱼作弊警告方式的范围为 0 ~ 4 整数，已纠正为默认3");
            config.钓鱼作弊警告方式 = 3;
        }
        if (config.全员物品检测时间间隔_单位秒 < 1)
        {
            e.Player.SendWarningMessage("全员物品检查时间间隔不能小于1，已纠正为默认320");
            config.全员物品检测时间间隔_单位秒 = 320;
        }
        wPlayers = WPlayer.LoadConfigFile();
        WPlayer.SaveConfigFile();
        Watcher.config = config;
        Watcher.config.SaveConfigFile();
    }
}