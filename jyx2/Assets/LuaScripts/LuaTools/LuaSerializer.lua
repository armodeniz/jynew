--[[
/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */
 ]]--
-- 封装一个简单的序列化工具
local luaS = {}
local function DoSerialize(save, obj)
end
function luaS.serialize(o)
    if type(o) ~= "table" then
        return
    end
    if o.__serializeFields then
        print("yes")
    elseif o.__cname == "LuaList" then
        print("LuaList")
    end
end

return luaS
