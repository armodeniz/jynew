/*
 * 金庸群侠传3D重制版
 * https://github.com/jynew/jynew
 *
 * 这是本开源项目文件头，所有代码均使用MIT协议。
 * 但游戏内资源和第三方插件、dll等请仔细阅读LICENSE相关授权协议文档。
 *
 * 金庸老先生千古！
 */

using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using XLua;
using Jyx2.ResourceManagement;

namespace Jyx2
{
    /// <summary>
    /// 用于让Lua快速访问游戏对象
    /// </summary>
    [System.Serializable]
    public class Injection
    {
        public string name;
        public GameObject value;
    }

    /// <summary>
    /// 用于给Lua传递参数
    /// </summary>
    [System.Serializable]
    public class LuaParam
    {
        public string pname;
        public string pvalue;
    }

    /// <summary>
    /// 提供Lua侧的MonoBehaviour
    /// </summary>
    [LuaCallCSharp]
    public class LuaMonoBehaviour : MonoBehaviour
    {
        // 绑定Lua脚本文件，但不利于Mod修改
        //public TextAsset luaScript;
        // 绑定Lua脚本路径，可以在Mod中覆盖
        public string luaFilePath;
        public LuaParam[] luaParams;
        // 为Lua提供快速访问其他游戏对象的接口
        public Injection[] injections;

        internal static LuaEnv luaEnv => LuaManager.GetLuaEnv();
        internal static float lastGCTime = 0;
        internal const float GCInterval = 1;//1 second 

        private Action luaStart;
        private Action luaUpdate;
        private Action luaOnDestroy;

        private LuaTable scriptEnv;

        // 由于LuaManager的初始化问题，暂时不能提供Lua侧的Awake方法
        void Awake()
        {
        }

        // 初始化，只会运行一次
        public async UniTask LuaInit()
        {
            Debug.Log(luaFilePath);
            // 检查是否填写了lua文件路径
            if (String.IsNullOrWhiteSpace(luaFilePath))
                return;
            var luaFile = await ResLoader.LoadAsset<TextAsset>($"Assets/LuaScripts/{luaFilePath}.lua");
            // 检查lua文件是否存在
            if (luaFile == null)
                return;
            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突。在独立的环境中，使用元表来访问Lua全局变量
            scriptEnv = luaEnv.NewTable();

            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", this);
            foreach (var injection in injections)
            {
                scriptEnv.Set(injection.name, injection.value);
            }

            string luaParamStr = "";
            foreach (var lp in luaParams)
            {
                luaParamStr = string.Format("{0}{1} = {2}\n", luaParamStr, lp.pname, lp.pvalue);
            }

            Encoding utf8 = Encoding.UTF8;
            string luaFileStr = luaParamStr + utf8.GetString(utf8.GetBytes(luaFile.text));
            luaEnv.DoString(luaFileStr, luaFilePath, scriptEnv);
            //luaEnv.DoString(luaScript.text, "LuaTestScript", scriptEnv);

            scriptEnv.Get("Start", out luaStart);
            scriptEnv.Get("Update", out luaUpdate);
            scriptEnv.Get("OnDestroy", out luaOnDestroy);
        }

        // 一个外部c#访问独立环境的简单接口
        public void DoString(string luaText)
        {
            luaEnv.DoString(luaText, luaFilePath, scriptEnv);
        }

        async void Start()
        {
            await LuaInit();

            if (luaStart != null)
            {
                luaStart();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (luaUpdate != null)
            {
                luaUpdate();
            }
            if (Time.time - lastGCTime > GCInterval)
            {
                luaEnv.Tick();
                lastGCTime = Time.time;
            }
        }

        public void OnDestroy()
        {
            if (luaOnDestroy != null)
            {
                luaOnDestroy();
            }
            luaOnDestroy = null;
            luaUpdate = null;
            luaStart = null;
            scriptEnv?.Dispose();
            injections = null;
        }
    }
}
