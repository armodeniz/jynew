using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jyx2.Battle
{
    /// <summary>
    /// 提供Lua侧BattleBlock的接口，效率大大提高
    /// </summary>
    [XLua.LuaCallCSharp]
    public static class BattleboxBlockHelper
    {
        /// <summary>
        /// 拓展GameObject修改Layer的接口，提高效率
        /// </summary>
        public static void SetLayer(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform sub in go.transform)
            {
                //Debug.Log(subgo.gameObject.name);
                sub.gameObject.layer = layer;
            }
        }
    }
}
