local test = CS.Jyx2.LuaToCsBridge.TestMethod
local awaiter = test():GetAwaiter()
if awaiter ~= nil then
    print(awaiter.IsCompleted)
end

print("test end")
