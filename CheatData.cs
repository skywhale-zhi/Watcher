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
    public class CheatData
    {
        public static HashSet<int> AfterBoss1BeforeBoss2;

        public static HashSet<int> AfterBoss2BeforeBoss3;

        public static HashSet<int> AfterBoss3BeforeHard;

        public static HashSet<int> AfterHardBeforeOneOfThree;
        
        public static HashSet<int> AfterMechanicsBeforePlantera;

        public static HashSet<int> AfterPlanteraBeforeGolem;

        public static HashSet<int> AfterGolemBeforeCultist;

        public static HashSet<int> AfterLunaticCultist;

        //单体boss或事件
        public static HashSet<int> DarkMage;//黑魔法师

        public static HashSet<int> Ogre;//食人魔

        public static HashSet<int> Betsy;//双足翼龙

        public static HashSet<int> FlyingDutchman;//飞行荷兰人

        public static HashSet<int> Halloween;//南瓜月

        public static HashSet<int> Christmas;//冰霜月

        public static HashSet<int> QueenBee;

        public static HashSet<int> Deerclops;

        public static HashSet<int> SlimeQueen;

        public static HashSet<int> DukeFishron;

        public static HashSet<int> EmpressofLight;

        //单独物品
        public static HashSet<int> RareItems;

        //拿持日志中不需要记录的豁免物品
        public static HashSet<int> ImmunityHoldItems;

        //丢弃日志中不需要记录的豁免物品
        public static HashSet<int> ImmunityDropItems;

        //射弹日志中需要记录的危险的射弹物
        public static HashSet<int> DangerousProjectile;

        //蒸汽朋克。为了区别彩蛋图的原因
        public static HashSet<int> Steampunker;

        public CheatData()
        {
            SetCheatData();
        }

        public static void SetCheatData()
        {
            Config config;
            Config.SetConfigFile();
            config = Config.ReadConfigFile();

            //骷髅王后，肉山前
            AfterBoss3BeforeHard = new HashSet<int>
            {
                1363,4993,3323,3245,1281,1273,1313,4801,4927,164,157,113,163,156,155,273,329,397,3317
            };

            //邪教徒后，包括月总物品
            AfterLunaticCultist = new HashSet<int>
            {//武器
                1553,2774,2776,2779,2781,2784,2786,3063,3065,3464,3466,3473,3474,3475,3476,3522,3523,3524,3525,3531,3540,3541,3542,3543,3569,3570,3571,3930,4956,
             //盔甲
                2760,2761,2762,2757,2758,2759,2763,2764,3381,3382,3383,
             //放置物
                3536,3537,3538,3539,4318,4951,3467,3595,3357,3460,3549,3573,3574,3575,3576,3461,
             //材料
                3456,3459,3457,3458,
             //饰品
                3468,3469,3470,3471,4954,1131,
             //弹药药水宠物坐骑染料召唤物圣物，杂物
                2768,3567,3568,3544,3332,3577,4810,4809,4469,3526,3527,3528,3529,3530,3601,4937,4938
            };

            //困难模式后，一王前
            AfterHardBeforeOneOfThree = new HashSet<int>
            {//武器
                426,434,435,436,481,482,483,484,496,514,517,518,519,533,534,672,676,682,723,725,726,776,777,778,905,991,992,993,1185,1187,1188,1192,1194,1195,1199,1201,1202,1222,1223,1224,
                1244,1264,1265,1306,1308,1336,2270,2331,2366,2551,2584,2750,3006,3007,3008,3013,3014,3029,3051,3052,3053,3209,3210,3211,3269,3779,3787,3778,4269,4270,4317,4348,
             //盔甲
                371,372,373,374,375,376,377,378,379,380,400,401,402,403,404,684,685,686,754,755,1205,1206,1207,1208,1209,1210,1211,1212,1213,1214,1215,1216,1217,1218,1219,2370,2371,2372,4761,
             //放置物，矿锭
                364,365,366,381,382,391,415,416,487,502,523,524,525,1104,1105,1106,1184,1191,1198,1220,1221,1591,1593,3064,3884,3885,3979,3980,3981,3982,3983,3984,3985,3986,3987,4054,4406,4408,
             //饰品
                485,489,490,491,492,493,532,535,536,554,761,785,822,860,862,885,888,889,890,892,897,901,902,903,904,1162,1165,1247,1253,1321,1612,1613,2494,2998,3015,3016,3991,3992,
                4001,4002,4006,
             //宠物坐骑
                1171,1312,2429,3260,3771,
             //杂物
                499,500,2422,3324,3335,501,507,508,520,521,522,527,528,531,575,1328,1332,1347,1348,1432,1519,1533,1534,1535,1536,1537,2161,2607,3031,3091,3092,3783,4714,
             //弹药
                515,516,545,546,1334,1335,1351,1352,3009,3010,3011,3103,3104
            };

            //任意机械boss后，世纪花前
            AfterMechanicsBeforePlantera = new HashSet<int>
            {//武器
                368,494,495,533,550,561,578,579,674,675,756,787,990,1226,1227,1228,1229,1230,1231,1232,1233,1234,1262,2188,2535,3819,3823,3825,3830,3833,3835,3836,3852,3854,4678,
                4790,
             //盔甲
                551,552,553,558,559,1001,1002,1003,1004,1005,1006,3800,3801,3802,3803,3804,3805,3806,3807,3808,3809,3810,3811,3812,4873,4896,4897,4898,4899,4900,4901,
             //饰品
                749,821,935,936,1343,2220,
             //杂物
                547,548,549,947,1179,1235,1291,1366,1367,1368,1369,1518,1521,1611,3325,3326,3327,3354,3355,3356,
                3615,3865,3868,4372,4931,4932,4933,
             //宠物坐骑
                425,3353,3856,4803,4804,4805
            };
            
            //猪龙鱼公爵
            DukeFishron = new HashSet<int>
            {
                2589,3330,3367,2588,2623,2611,2622,2621,2624,2609,4808,4936
            };

            //光之女皇
            EmpressofLight = new HashSet<int>
            {
                4952,4923,4914,4953,4823,4778,4715,5075,5005,4784,4783,4949,4782,4989,4811
            };

            ImmunityHoldItems = config.ImmunityHoldItemID_拿持日志中的豁免物品ID;

            ImmunityDropItems = config.ImmunityDropItemsID_丢弃日志中的豁免物品ID;

            DangerousProjectile = config.DangerousProjectileID_射弹日志中需要记录的危险的射弹物ID;

            //蒸汽朋克有关的所有物品
            Steampunker = new HashSet<int>
            {
                748,779,780,781,782,784,839,840,841,995,1263,1344,1742,2193,2203,3602,3603,3604,3605,3606,3607,3608,3609,3610,3618,3663,4142,4472,948,
            //齿轮做的东西
                1302,2627,1708,2649,2655,1712,2638,2845,1718,2253,1722,2256,2125,2250,2241,2024,2036,2096,2130,2412,4114,3726,3727,3728,3729
            };
        }
    }
}
