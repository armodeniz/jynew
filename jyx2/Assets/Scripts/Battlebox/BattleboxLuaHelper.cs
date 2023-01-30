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
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using XLua;
using Sirenix.OdinInspector;
using Jyx2.MOD.ModV2;

namespace Jyx2
{
    public class BattleboxLuaHelper : MonoBehaviour
    {
        [LabelText("障碍物检测半径")]
        public float m_DetechRadius = 0.5f;
        [LabelText("方块贴图放缩")]
        public float m_BlockTexMultiplier = 1.0f;
        [SerializeField]
        [HideInInspector]
        public string m_BattleboxSerializedData;

        private Collider[] _colliders;

#if UNITY_EDITOR
        [PropertySpace]
        [Button("显示战斗格子")]
        public void ShowBattleBlocks()
        {
            Debug.Log("xszdgz");
        }
        [PropertySpace]
        [Button("重新生成战斗格子")]
        public async void RegenerateBattleBlocks()
        {
            Awake();
            Debug.Log(m_BattleboxSerializedData);
            var editorModLoader = new GameModEditorLoader();
            var path = SceneManager.GetActiveScene().path;
            Debug.Log("当前调试场景："+path);
            var editorModId = "";
            if (path.Contains("Assets/Mods/"))
            {
                editorModId = path.Split('/')[2];
                Debug.Log("当前场景所属Mod："+ editorModId);
                foreach (var mod in await editorModLoader.LoadMods())
                {
                    if (mod.Id == editorModId)
                    {
                        Jyx2.ResourceManagement.ResLoader.LaunchMod(mod);
                        break;
                    }
                }
            }
            LuaManager.Clear();
            LuaManager.Init("require 'main'");
            LuaMonoBehaviour battleBox = gameObject.GetComponent(typeof(LuaMonoBehaviour)) as LuaMonoBehaviour;
            battleBox.LuaInit();
            battleBox.DoString("CreateDataset()");
            battleBox.OnDestroy();
        }
#endif

        void Awake()
        {
            m_BattleboxSerializedData = "aaabbbccc";
            InitCollider();
        }

        private void InitCollider()
        {
            _colliders = GetComponentsInChildren<Collider>();
            foreach (var col in _colliders)
            {
                var mesh = col.GetComponent<MeshCollider>();
                if (mesh != null) mesh.convex = true;
            }
        }

        public bool ColliderContain(Vector3 pos)
        {
            foreach (var mCollider in _colliders)
            {
                var temp = mCollider.ClosestPoint(pos);
                if (Vector3.Distance(pos, temp) < 1e-6) return true;
            }

            return false;
        }
        //1.一定要打到Ground
        //2.将交点信息和法线信息拿到
        /// <summary>
        /// 判断
        /// </summary>
        public bool JudgeCoord(float topX, float topY, float topZ, float height, out float posX, out float posY, out float posZ, out float normalX, out float normalY, out float normalZ)
        {
            var top = new Vector3(topX, topY, topZ);
            var pos = new Vector3(0f, 0f, 0f);
            var normal = new Vector3(0f, 0f, 0f);

            posX = posY = posZ = 0f;
            normalX = normalY = normalZ = 0f;

            var ray = new Ray(top, Vector3.down);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, height, 1 << LayerMask.NameToLayer("Ground")))
            {
                if (!ColliderContain(hitInfo.point)) return false;

                //寻找最近的导航网格边缘，排除过于“拥挤”的点
                NavMeshHit hit;
                if (NavMesh.FindClosestEdge(hitInfo.point, out hit, NavMesh.AllAreas))
                {
                    if (hit.distance >= m_DetechRadius)
                    {
                        pos = hitInfo.point;
                        normal = hitInfo.normal;
                        posX = pos.x; posY = pos.y; posZ = pos.z;
                        normalX = normal.x; normalY = normal.y; normalZ = normal.z;
                        return true;
                    }
                }
            }
            return false;
        }
        // End of Class
    }
}
