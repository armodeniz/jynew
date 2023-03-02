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
using Jyx2.InputCore;

using ch.sycoforge.Decal;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Jyx2.Battle
{
    public class BattleboxLuaHelper : MonoBehaviour//,IJyx2_InputContext
    {
#region Settings
        [LabelText("战斗格子类型")]
        public int m_BlockType = 1;
        [LabelText("障碍物检测半径")]
        public float m_DetechRadius = 0.5f;
        [LabelText("格子贴图放缩")]
        public float m_BlockTexMultiplier = 1.0f;
        [SerializeField]
        [HideInInspector]
        public string m_BattleboxSerializedData;
#endregion

        private Collider[] _colliders;

#if UNITY_EDITOR
        private async void DoLuaInit()
        {
            Awake();
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
                        await Jyx2.ResourceManagement.ResLoader.LaunchMod(mod);
                        break;
                    }
                }
            }
        }
        [PropertySpace]
        [Button("显示战斗格子")]
        public async void ShowBattleBlocks()
        {
            Debug.Log("xszdgz");
            DoLuaInit();

            LuaManager.Clear();
            LuaManager.Init("require 'main'");
            LuaMonoBehaviour battleBox = gameObject.GetComponent(typeof(LuaMonoBehaviour)) as LuaMonoBehaviour;
            await battleBox.LuaInit();

            foreach (var block in _battleBlocks)
            {
                DestroyImmediate(block.gameObject);
            }

            _battleBlocks.Clear();
            battleBox.DoString("InitDataset()");
            battleBox.DoString("InitBattleBlocks()");
            //battleBox.DoString("Start()");
            battleBox.OnDestroy();
        }
        [PropertySpace]
        [Button("清除战斗格子")]
        public void ClearBattleBlocks()
        {
            foreach (var block in _battleBlocks)
            {
                DestroyImmediate(block.gameObject);
            }
            _battleBlocks.Clear();
        }
        [PropertySpace]
        [Button("重新生成战斗格子")]
        public async void RegenerateBattleBlocks()
        {
            DoLuaInit();
            LuaManager.Clear();
            LuaManager.Init("require 'main'");
            LuaMonoBehaviour battleBox = gameObject.GetComponent(typeof(LuaMonoBehaviour)) as LuaMonoBehaviour;
            await battleBox.LuaInit();
            battleBox.DoString("CreateDataset()");
            battleBox.OnDestroy();
        }
#endif

        void OnDestroy()
        {
            //InputContextManager.Instance.RemoveInputContext(this);
        }

        public static BattleboxLuaHelper Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<BattleboxLuaHelper>();
                return _instance;
            }
        }
        private static BattleboxLuaHelper _instance;

        private GameObject blockObj;
        void Awake()
        {
            InitCollider();
            blockObj = Resources.Load<GameObject>("BattleboxBlock");
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
        public bool JudgeCoord(float topX, float topY, float topZ, float height, out float posX, out float posY, out float posZ)
        {
            var top = new Vector3(topX, topY, topZ);
            var pos = new Vector3(0f, 0f, 0f);

            posX = posY = posZ = 0f;

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
                        posX = pos.x; posY = pos.y; posZ = pos.z;
                        return true;
                    }
                }
            }
            return false;
        }

        //存储逻辑数据
        private List<BattleBlockData> _battleBlocks = new List<BattleBlockData>();

        //mouseover显示攻击范围的格子
        private List<BattleBlockData> _rangeLayerBlocks = new List<BattleBlockData>();
        private GameObject _parent;
        public const float BATTLEBLOCK_DECAL_ALPHA = 0.4f;
        private Vector3 _blockScale = BattleboxDataset.BlockLength * new Vector3(1, 1, 1);
        private GameObject FindOrCreateBlocksParent()
        {
            if (_parent == null)
                _parent = new GameObject("block_parent");
            return _parent;
        }
        //绘制战斗格子，默认不显示
        public GameObject DrawBattleBlock(float posX, float posY, float posZ, bool initRangeBlocks = false)
        {
            var parent = FindOrCreateBlocksParent();

            var pos = new Vector3(posX, posY, posZ);

            var obj = EasyDecal.Project(blockObj, pos, Quaternion.identity);
            obj.Quality = 2;
            obj.Distance = 0.05f;
            if (initRangeBlocks)
            {
                obj.Distance = 0.07f;
            }

            obj.transform.SetParent(parent.transform, false);
            obj.transform.localScale = m_BlockTexMultiplier * _blockScale;

            var bbd = new BattleBlockData();
            bbd.WorldPos = pos;
            bbd.gameObject = obj.gameObject;

            if (initRangeBlocks)
            {
                _rangeLayerBlocks.Add(bbd);
                obj.DecalRenderer.material.SetColor("_TintColor", new Color(0, 0, 1, BATTLEBLOCK_DECAL_ALPHA));
            }
            else
            {
                _battleBlocks.Add(bbd);    
            }
            return obj.gameObject;
        }

        public Action<float,float> ShowBlockAtPoint {get;set;}
        public void OnMouseOverBattleBlock()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //待调整为格子才可以移动
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, 1 << LayerMask.NameToLayer("Ground")))
            {
                if (ShowBlockAtPoint != null)
                    ShowBlockAtPoint(hitInfo.point.x, hitInfo.point.z);
            }

        }
        void Update()
        {
            OnMouseOverBattleBlock();
        }
        /*
        #region 输入操作
        private int[] xPositions = new int[0];
        private int xMiddlePos = -1;
        private int[] yPositions = new int[0];
        private int yMiddlePos;
        private int CurPosX;
        private int CurPosY;

        public bool AnalogMoved = false;

        private int m_InputEnableFrame = int.MaxValue;
        public bool CanUpdate => m_InputEnableFrame <= Time.frameCount;


        public void OnUpdate()
        {
            if (xPositions.Length == 0 || yPositions.Length == 0)
                return;
            if(Jyx2_Input.GetNegativeButtonDown(Jyx2ActionConst.MoveHorizontal))
            {
                TryMoveHorizontal(CurPosX + 1);
            }
            else if(Jyx2_Input.GetButtonDown(Jyx2ActionConst.MoveHorizontal))
            {
                TryMoveHorizontal(CurPosX - 1);
            }

            if (Jyx2_Input.GetNegativeButtonDown(Jyx2ActionConst.MoveVertical))
            {
                TryMoveVertical(CurPosY + 1);
            }
            else if (Jyx2_Input.GetButtonDown(Jyx2ActionConst.MoveVertical))
            {
                TryMoveVertical(CurPosY - 1);
            }


            if (Jyx2_Input.GetButtonDown(Jyx2ActionConst.UIConfirm))
            {
                if (AnalogMoved && !IsMoveSelectAndBlocked(CurPosX, CurPosY))
                {
                    var selectedBlock =  _currentBattlebox.GetBlockData(CurPosX, CurPosY);
                    if (selectedBlock != null && !selectedBlock.Inaccessible)
                    {
                        OnBlockConfirmed?.Invoke(selectedBlock);
                    }
                }
            }

            if (IsCancelBoxSelection())
            {
                TryCancelBoxSelection();
            }

        }

        private void TryMoveHorizontal(int newPosX)
        {
            if (newPosX < xPositions.First() || newPosX > xPositions.Last())
                return;
            if (TrySelectNewBlock(newPosX, CurPosY))
                CurPosX = newPosX;
        }

        private void TryMoveVertical(int newPosY)
        {
            if (newPosY < yPositions.First() || newPosY > yPositions.Last())
                return;
            if (TrySelectNewBlock(CurPosX, newPosY))
                CurPosY = newPosY;
        }

        public bool IsMoveSelectAndBlocked(int x, int y)
        {
            return m_CurrentType == BattleBlockType.MoveZone && IsRoleStandingInBlock(x, y);
        }

        public bool IsRoleStandingInBlock(int x, int y)
        {
            var battleModel = BattleManager.Instance.GetModel();
            if (battleModel == null)
                return false;
            return battleModel.BlockHasRole(x, y);
        }

        private bool IsCancelBoxSelection()
        {
            if (Jyx2_Input.GetButtonDown(Jyx2ActionConst.UIClose))
                return true;
            if (Input.GetMouseButtonDown(1) && !Application.isMobilePlatform)
                return true;
            return false;
        }

        public void TryCancelBoxSelection()
        {
            var ui = Jyx2_UIManager.Instance.GetUI<BattleActionUIPanel>();
            if (ui != null)
                ui.OnCancelBoxSelection();
        }


        private bool TrySelectNewBlock(int newX, int newY)
        {
            var newSelectedBlock =  _currentBattlebox.GetBlockData(newX, newY);
            if (newSelectedBlock != null && newSelectedBlock.IsActive)
            {
                if (_selectedBlock != null)
                {
                    _selectedBlock.gameObject.GetComponent<EasyDecal>().DecalRenderer.material.SetColor("_TintColor", _oldColor);
                }

                _selectedBlock = newSelectedBlock;
                _oldColor = newSelectedBlock.gameObject.GetComponent<EasyDecal>().DecalRenderer.material.GetColor("_TintColor");
                Color hiliteColor = newSelectedBlock.Inaccessible ?				
                    new Color(0.4f, 0.4f, 0.4f, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA) : //gray color for inaccessible blocks
                    new Color(1, 0, 1, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA);
                _selectedBlock.gameObject.GetComponent<EasyDecal>().DecalRenderer.material.SetColor("_TintColor", hiliteColor);

                AnalogMoved = true;

                if (OnBlockSelectMoved != null)
                    OnBlockSelectMoved(newSelectedBlock);

                return true;
            }

            return false;
        }

        public event Action<BattleBlockData> OnBlockSelectMoved;
        public event Action<BattleBlockData> OnBlockConfirmed;

        private void initXPos()
        {
            xPositions = this._currentBattlebox
                .GetBattleBlocks()
                .Where(b => b.IsActive)
                .Select(b => b.BattlePos.X)
                .OrderBy(p => p)
                .Distinct()
                .ToArray();
        }

        private void initYPos()
        {
            yPositions = this._currentBattlebox
                .GetBattleBlocks()
                .Where(b => b.IsActive)
                .Select(b => b.BattlePos.Y)
                .OrderBy(p => p)
                .Distinct()
                .ToArray();
        }

        bool rangeMode = false;
        private BattleBlockData _selectedBlock;
        private Color _oldColor;

        public void ShowBlocks(RoleInstance role, IEnumerable<BattleBlockVector> list, BattleBlockType type = BattleBlockType.MoveZone,
                bool selectMiddlePos = false)
        {
            if (!GeneralPreJudge()) return;
            HideAllBlocks();
            m_CurrentType = type;
            if (type == BattleBlockType.MoveZone)
            {
                _currentBattlebox.SetAllBlockColor(new Color(1, 1, 1, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA));
                _selectedBlock = null;
            }
            else if (type == BattleBlockType.AttackZone)
            {
                _currentBattlebox.SetAllBlockColor(new Color(1, 0, 0, BattleboxManager.BATTLEBLOCK_DECAL_ALPHA));
                _selectedBlock = null;
            }

            foreach (var vector in list)
            {
                var block = _currentBattlebox.GetBlockData(vector.X, vector.Y);
                if (block != null && block.BoxBlock.IsValid)
                {
                    if (vector.Inaccessible)
                    {
                        _currentBattlebox.SetBlockInaccessible(block);
                    }

                    block.Inaccessible = vector.Inaccessible;
                    block.Show();
                }
            }

            xMiddlePos = role.Pos.X;
            yMiddlePos = role.Pos.Y;
            initShownPositions();

            if (selectMiddlePos)
            {
                TrySelectNewBlock(CurPosX, CurPosY);
            }
            RegisterInput();
            rangeMode = false;
        }

        private void initShownPositions()
        {
            initXPos();
            CurPosX = xMiddlePos;

            initYPos();
            CurPosY = yMiddlePos;
        }

        public void ShowRangeBlocks(IEnumerable<BattleBlockVector> list)
        {
            //todo: debug skillCast that has range instead of just one point
            if (!GeneralPreJudge()) return;
            _currentBattlebox.HideAllRangeBlocks();

            foreach (var vector in list)
            {
                var block = _currentBattlebox.GetRangelockData(vector.X, vector.Y);
                if (block != null && block.BoxBlock.IsValid) 
                    block.Show();
            }

            //initShownPositions();
            rangeMode = true;
        }

        public void HideAllBlocks(bool hideRangeBlock = false)
        {
            if (!GeneralPreJudge()) return;

            _currentBattlebox.HideAllBlocks();
            if (hideRangeBlock)
            {
                _currentBattlebox.HideAllRangeBlocks();
            }
            _selectedBlock = null;
            UnRegisterInput();
        }



        public void ShowAllBlocks()
        {
            if (!GeneralPreJudge()) return;

            _currentBattlebox.ShowAllValidBlocks();
        }

        public void ShowMoveZone(Vector3 center, int range = -1)
        {
            if (!GeneralPreJudge()) return;
            _currentBattlebox.HideAllBlocks();

            if (range == -1)
            {
                range = m_MoveZoneDrawRange;
            }

            _currentBattlebox.ShowBlocksCenterDist(center, range);
        }
        #endregion
        */

        // End of Class
    }
}
