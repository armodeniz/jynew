--[[
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 ]]--
-- 本脚本为Lua侧游戏战斗模块初始化
-- 载入工具模块
local util = require("xlua.util")
local await = jy_utils.await

local battle = {}

battle.SkillCoverType = {
    POINT = 0,
    LINE = 1,
    CROSS = 2,
    RECT = 3,
    RHOMBUS = 4,
    INVALID = -1
}

battle.DamageCaculator = jy_utils.prequire("Jyx2Battle/DamageCaculator")
battle.RangeLogic = jy_utils.prequire("Jyx2Battle/RangeLogic")
battle.AIManager = jy_utils.prequire("Jyx2Battle/AIManager")
battle.Manager = jy_utils.prequire("Jyx2Battle/BattleMgr")

local function LoadBattle (battle, callback, co_callback)
    local UniTask = CS.Cysharp.Threading.Tasks.UniTask
    local LevelMaster = CS.LevelMaster
    print("battle start")
    local isWin = false

    -- 记录当前地图和位置
    local currentMap = LevelMaster.GetCurrentGameMap()
    local pos = LevelMaster.Instance:GetPlayerPosition()
    local rotate = LevelMaster.Instance:GetPlayerOrientation()

    local ret = await(CS.Jyx2.LevelLoader.LoadGameMap

    local levelLoadPara = LevelMaster.LevelLoadPara()
    levelLoadPara.loadType = LevelMaster.LevelLoadPara.LevelLoadType.ReturnFromBattle
    levelLoadPara.Pos = pos
    levelLoadPara.Rotate = rotate

    CS.LevelLoader.LoadGameMap
    print("battle start suc")
    if co_callback then
        isWin = (ret == CS.Jyx2.BattleResult.Win)
        LevelMaster.IsInBattle = false
        CS.AudioManager.PlayMusic(formalMusic)
        co_callback(isWin)
    end
end

battle.LoadBattleAsync = util.coroutine_call(LoadBattle)
battle.LoadBattle = util.async_to_sync(LoadBattle)

return battle
