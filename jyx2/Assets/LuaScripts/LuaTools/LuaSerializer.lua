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
local template = "return %s"
local rstTable
local opened
local function DoSerialize(obj)
    local t = type(obj)
    if t == "number" or t == "string" or t == "boolean" or t == "nil" then
        table.insert(rstTable, string.format("%q", obj))
    elseif t == "table" then
        local savableFields = obj.__savableFields
        if not savableFields then
            return
        end
        if opened[obj] then
            return
        end
        opened[obj] = true
        local exlusiveFields = obj.__exlusiveFields or {}
        table.insert(rstTable, "{")
        for k,_ in pairs(savableFields) do
            if not exlusiveFields[k] then
                table.insert(rstTable, string.format("[%q]=", k))
                --print(k)
                DoSerialize(obj[k])
                table.insert(rstTable, ",")
            end
        end
        table.insert(rstTable, "}")
    else
        error("cannot serialize a ".. t)
    end
end
function luaS:serialize()
    if self.__savableFields then
        rstTable = {}
        opened = {}
        print("start serial")
        DoSerialize(self)
        local rstString = string.format(template, table.concat(rstTable))
        rstTable = nil
        return rstString
    end
    return ""
end

function luaS:deserialize(savedData)
    local tmpTable
    if type(savedData) == "string" then
        tmpTable = load(savedData)()

    elseif type(savedData) == "table" then
        tmpTable = savedData
    else
        return nil
    end

    if self.new and tmpTable.__cname == nil then
        self:new(tmpTable)
    end

    local savableFields = tmpTable.__savableFields
    if not savableFields then
        return tmpTable
    end
    local exlusiveFields = tmpTable.__exlusiveFields or {}
    for i,v in pairs(tmpTable) do
        if not exlusiveFields[i] and type(savableFields[i]) == "table" and savableFields[i].deserialize then
            savableFields[i]:deserialize(v)
        end
    end

    return tmpTable
end

return luaS
