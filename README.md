# Watcher
## 一个泰拉瑞亚tshock插件

## 功能介绍：

1. 能够对游戏内玩家行为的详细信息进行记录，如记录每个玩家手里拿着什么物品、丢弃什么物品、生成什么射弹（在有人炸图或乱喷腐化溶液的时候可以快速找到它），均记录在watcher文件夹内的logs里，玩家作弊信息记录在cheatlogs里。

2. 一定程度上检测作弊的插件，如多杆钓鱼检测，弹幕伤害过高检测（排除了冲击波和尖桩发射器），不符合游戏进度的物品检测。对作弊的玩家踢出，超过限定允许的次数时ban掉，可在WatcherConfig.json里改允许次数，再控制台里/reload即可生效（下面不在赘述，均简称为 可改）。

3. 允许封禁游戏进度，其实就是允许设置某些npc（包括怪物，boss，城镇npc，小动物）不可被伤害，包括来自玩家、敌怪、岩浆、机关的伤害，可以强制玩家不能快速推进度，也可以保护城镇npc免遭怪物伤害，~~即使开了一击必杀作弊器和各种debuff也不行~~，

4. 自动备份tshock.sqlite文件（原版tshock只备份了地图，sqlite文件没有备份，而又有极低的概率损坏玩家的存档）可设置备份时常间隔。不用担心日志过多，会自动清理，可改日志的备份时常，间隔，日志大小。

5. ~~允许用户自定义各种日志是否写入，是否检测作弊等功能，详情配置查看WatcherConfig.json文件（废话）~~。

## 指令

- 权限1： watcher.locknpc
- 指令1： /locknpc npc.id或boss名称或boss名称拼音缩写
- 功能1： 保护这个npc不会被伤害和杀死，管理员可以使用`/clear npc`来清除npc
-
- 权限2： watcher.unlocknpc
- 指令2： /unlocknpc npc.id或boss名称或boss名称拼音缩写
- 功能2： 移除受保护的npc
-
- 权限3： watcher.listlocknpc
- 指令3： /listlocknpc
- 功能3： 列出所有受保护的npc
-
- 权限4： watcher.adduncheckeditem
- 指令4： /adduci item.id
- 功能4： 该插件的物品作弊检查功能不会检查这个物品
-
- 权限5： watcher.delunchecheditem
- 指令5： /deluci item.id
- 功能5： 移除这个物品不会被检查的效果
-
- 权限6： watcher.listuncheckeditem
- 指令6： /listuci
- 功能6： 列出所有避免被检查的物品

## 用法
直接将插件放置于tshock插件文件夹里即可，默认修改控制台为中文，可改。

你可以在Watcher/logs文件夹里看到玩家日常信息搜集，在watcher/cheatlogs文件夹里看到作弊信息搜集，在Watcher/tshock_backups文件夹里看到备份的sqlite文件，在WatcherConfig.json里修改你需要的各种配置信息。

输入`/locknpc 46`那么所有的兔兔均不能被任何方法杀死。输入`/locknpc kslzy`或`/locknpc 克苏鲁之眼`或`/locknpc 4`或`/locknpc ky`或`/locknpc 克眼`那么克苏鲁之眼和克苏鲁之仆均不能被玩家杀死（对于boss，一次添加会同时保护boss和大多数boss的仆从，肢体等，这里`/locknpc 4`同时保护了克眼和仆从，对于机械骷髅王还会同时保护四个钳子，不需要用户一个一个输入指令来保护）

输入`/unlocknpc 46`将解除兔兔的保护，克眼也一样，同时解除boss的仆从，肢体保护

输入`/listlocknpc`来查看哪些npc被保护

输入`/adduci 2624`来将海啸设置为不被检查的物品，因为Watcher插件有物品进度检测功能，在打败猪鲨之前获得海啸会被认定为作弊，使用此指令将该物品设置为豁免物

输入`/deluci item.id`来移除某个物品的豁免

输入`/listuci`来查看所有豁免物品

## WatcherConfig.json

一个可以详细修改功能的文件，修改后在控制台或游戏内使用`/reload`即可生效，下面对每个项进行了解释

```
{
  "enableChinese_启用中文": true,                                                    //顾名思义
  "whetherToWriteTheConversationContentInTheLog_是否把对话内容写入日志": true,       //就是把正常玩家对话和使用指令写到watcher/logs里（可能包含玩家注册密码）
  "whetherToWriteTheDiscardsIntoTheLog_是否把丢弃物写入日志": true,                  //丢弃物包括正常丢弃和用户在世界进行操作时生成的物品，如砍树掉落的木头也算进去
  "whetherToWriteTheHoldingObjectIntoTheLog_是否把手持物写入日志": true,             //顾名思义
  "whetherToWriteTheProjectilesIntoTheLog_是否把生成射弹写入日志": true,             //顾名思义
  "logAndCheatLogBackUpTime_日志和作弊记录日志的备份时常": 21600,                    //单位，分钟
  "maxMBofLogAndCheatLog_日志和作弊日志文件的最大MB": 1,                             //每个日志最大1MB，单位MB
  "ImmunityHoldItemID_拿持日志中的豁免物品ID": [                                     //当玩家手持这些物品时，不会写入日志，以防日志记录太多无效信息，这里2，3仅是例子，该插件会自动为你生成常用的豁免ID，无需手动搜集
    2,
    3
  ],
  "ImmunityDropItemsID_丢弃日志中的豁免物品ID": [                                    //同上
    0,
    2
  ],
  "DangerousProjectileID_射弹日志中需要记录的危险的射弹物ID": [                      //这里是需要记录的射弹，与上面的反过来了，通常这里是炸弹，岩浆炸弹，各种火箭等危险毁图射弹，该插件也已经准备好常用的警告射弹物ID了，这里的17，28仅是例子，无需手动搜集
    17,
    28
  ],
  "backUpTshockSql_是否备份tshockSql": true,
  "backupInterval_备份间隔": 20,                                                    //单位分钟，都是分钟
  "backUpTime_备份时长": 2880,
  "enableItemDetection_启用物品作弊检测": true,                                     //携带超进度物品会被踢
  "enableProjDamage_启用射弹伤害检测": true,                                        //射弹伤害检测，排除了灰橙冲击枪，尖桩发射器，这个功能会在tshock自带的伤害检测后运行，注意使用顺序
  "enableBobberNum_启用浮标数目检测": true,                                         //检测多竿钓鱼
  "numberOfBan_允许的违规次数": 7,                                                  //当违规次数达到7次时，强制ban掉违规玩家
  "needCheckedPlayerGroups_需要被检测的玩家组": [                                   //以下玩家组会被作弊系统检测，包括射弹，钓鱼，物品，伤害等检测
    "default",
    "vip"
  ],
  "ignoreCheckedItemsID_不需要被作弊检查的物品id": [],                              //写入此处的物品id将不会被 物品进度检测功能 算进去
  "ProtectedNPC_被保护的NPC": []                                                    //写入此处的 npc.id （注意是npc.id 不是 item.id）将不会被杀死，可以用来保护boss，防止有人偷推进度，或者保护城镇npc，或者保护松露虫等等
}
```

## 代码很烂请大佬不要在意，欢迎大家使用
