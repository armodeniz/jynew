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
-- 封装一个简单的List类型
local class = require("LuaClass")

local LuaList = class("LuaList")

LuaList.ctor = function(instance, ...)
    instance.Count = 0
    local args = table.pack(...)
    for i = 1, args.n do
        if args[i] ~= nil then
            instance:Add(args[i])
        end
    end
end

function LuaList:Add(value)
    if value == nil then
        return
    end
    local count = self.Count + 1
    self[count] = value
    self.Count = count
end

function LuaList:Clear()
    for i,_ in ipairs(self) do
        self[i] = nil
    end
    self.Count = 0
end

return LuaList
