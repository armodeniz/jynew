/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using XLua;
using UnityEngine;

namespace Jyx2
{
    /// <summary>
    /// 让Lua侧可以await一个UniTask异步任务
    /// </summary>
    [LuaCallCSharp]
    public static class UniTaskLuaAwaiter
    {
        public async static void LuaAwaiter(this UniTask ut, Action callback)
        {
            await ut;
            callback();
        }

        public async static void LuaAwaiter(this UniTask<bool> ut, Action<bool> callback)
        {
            bool rst = await ut;
            callback(rst);
        }

        public async static void LuaAwaiter(this YieldAwaitable ut, Action callback)
        {
            await ut;
            callback();
        }

    }
}
