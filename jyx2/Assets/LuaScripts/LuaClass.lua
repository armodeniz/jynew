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
-- 封装创建class的方法
local function class(className, super) -- 类名className 父类super(可为空)
    -- 构建类
    local clazz = { __cname = className, super = super}
    if super then
        -- 将父类设置为子类的元表，让子类访问父类成员
        setmetatable(clazz, { __index = super })
    end
    clazz.__index = clazz
    -- new方法用来创建对象
    function clazz:new(o)
        o = o or {}
        self.__index = self
        setmetatable(o, self)
        -- 可以自行设置ctor方法进行类的初始化
        if o.ctor then
            o:ctor()
        end
        return o
    end
    return clazz
end

return class
