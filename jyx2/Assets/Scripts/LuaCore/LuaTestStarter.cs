using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

using Jyx2;
using Jyx2.MOD.ModV2;

[CreateAssetMenu(menuName = "金庸重制版/LUA测试器", fileName = "LuaTestStarter")]
public class LuaTestStarter : ScriptableObject
{
#if UNITY_EDITOR
    [LabelText("指定测试ModId")] public string modId = "";
    [LabelText("要测试的Lua文件")] public List<TextAsset> luaFiles;
    [Button("测试Lua代码")]
    public async void luaTest()
    {
        if (modId != "")
            await DoModInit();
        LuaManager.Clear();
        LuaManager.Init("require 'main'");
        var luaEnv = LuaManager.GetLuaEnv();
        foreach (var file in luaFiles)
        {
            Debug.Log($"开始测试{file.name}");
            luaEnv.DoString(Encoding.UTF8.GetBytes(file.text),file.name);
        }
    }

    private async UniTask DoModInit()
    {
        var editorModLoader = new GameModEditorLoader();
        Debug.Log("当前场景所属Mod："+ modId);
        foreach (var mod in await editorModLoader.LoadMods())
        {
            if (mod.Id == modId)
            {
                await Jyx2.ResourceManagement.ResLoader.LaunchMod(mod);
                break;
            }
        }
    }
#endif
}
