# Watcher
## 一个泰拉瑞亚tshock插件

## 功能介绍：

能够对游戏内玩家行为的详细信息进行记录，如记录每个玩家持有什么物品、丢弃什么物品、生成什么射弹，均记录在watcher文件夹内的log里，玩家作弊信息记录在cheatlog里

一定程度上检测作弊的插件，如多杆钓鱼检测，弹幕伤害过高检测（排除了冲击波和尖桩发射器），不符合游戏进度的物品检测，对作弊的玩家踢出，超过限定次数时ban掉

允许用户自定义各种日志是否写入，是否检测作弊等功能，详情配置查看config文件

## 指令

- 权限1： watcher.locknpc
- 指令1： /locknpc npc.id或boss名称或boss名称拼音缩写
- 功能1： 保护这个npc不会被玩家伤害和杀死

- 权限2： watcher.unlocknpc
- 指令2： /unlocknpc npc.id或boss名称或boss名称拼音缩写
- 功能2： 移除受保护的npc

- 权限3： watcher.listlocknpc
- 指令3： /listlocknpc
- 功能3： 列出所有受保护的npc

- 权限4： watcher.adduncheckeditem
- 指令4： /adduci item.id
- 功能4： 该插件的物品作弊检查功能不会检查这个物品

- 权限5： watcher.delunchecheditem
- 指令5： /deluci item.id
- 功能5： 移除这个物品不会被检查的效果

- 权限6： watcher.listuncheckeditem
- 指令6： /listuci
- 功能6： 列出所有避免被检查的物品
