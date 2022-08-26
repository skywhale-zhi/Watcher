# Watcher
## 一个泰拉瑞亚tshock插件

## 功能介绍：

1. 能够对游戏内玩家行为的详细信息进行记录，如记录每个玩家手里拿着什么物品、丢弃什么物品、生成什么射弹（在有人炸图或乱喷腐化溶液的时候可以快速找到它），均记录在watcher文件夹内的logs里，玩家作弊信息记录在cheatlogs里

2. 一定程度上检测作弊的插件，如多杆钓鱼检测，弹幕伤害过高检测（排除了冲击波和尖桩发射器），不符合游戏进度的物品检测。对作弊的玩家踢出，超过限定允许的次数时ban掉，可在WatcherConfig.json里改允许次数，再控制台里/reload即可生效（下面不在赘述，均简称为 可改）

3. 允许封禁游戏进度，其实就是允许设置某些npc（boss）为不可伤害，强制玩家不能快速推进度，~~即使开了一击必杀作弊器也不行~~，

4. 自动备份tshock.sqlite文件（原版tshock只备份了地图，sqlite文件没有备份，而又有极低的概率损坏某些玩家的存档）可设置备份时常间隔。不用担心日志过多，会自动清理，可改 备份时常，间隔，日志要求大小

5. ~~允许用户自定义各种日志是否写入，是否检测作弊等功能，详情配置查看WatcherConfig.json文件（废话）~~

## 指令

- 权限1： watcher.locknpc
- 指令1： /locknpc npc.id或boss名称或boss名称拼音缩写
- 功能1： 保护这个npc不会被玩家伤害和杀死
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
直接将插件放置于tshock插件文件夹里即可，默认修改控制台为中文，可改
你可以在watcher/logs文件夹里看到玩家日常信息搜集，在watcher/cheatlogs文件夹里看到作弊信息搜集，在watcher/tshock_backups文件夹里看到备份的sqlite文件，在WatcherConfig.json里修改你需要的各种配置信息
