using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

using Jyx2;
using Jyx2.MOD.ModV2;

[CreateAssetMenu(menuName = "金庸重制版/ES测试器", fileName = "ESTestStarter")]
public class ESTestStarter : ScriptableObject
{
#if UNITY_EDITOR
    [LabelText("指定测试ModId")] public string modId = "";
    [LabelText("要测试的Lua文件")] public List<TextAsset> luaFiles;
    [Button("测试")]
    public async void esTest()
    {
        var settings = new ES3Settings(ES3.EncryptionType.AES, "Meow");
        //var strtmp = ES3.LoadRawString("E:/Github/jynew/jyx2/Assets/Mods/jytest/Temp/SaveObjectPlayerTeam.save", settings);
        foreach (var key in ES3.GetKeys("E:/Github/jynew/jyx2/Assets/Mods/jytest/Temp/SaveObjectPlayerTeam.save", settings))
        {
            Debug.Log(key);
        }
        //Debug.Log(strtmp);
    }

#endif
}
