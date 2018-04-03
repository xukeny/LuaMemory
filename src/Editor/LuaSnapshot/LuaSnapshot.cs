/*
** 作者： xmf
** QQ: 16342021
** 创始时间：2017-11-11
** 描述： 获取lua两次内存的样本，找出增长的lua表，需要基于tolua框架使用。参考了unity的MemoryProfiler
** Copyright (c) Leniu
*/
using UnityEngine;
using UnityEditor;
using System;
using LuaInterface;
using System.IO;
using System.Collections.Generic;

public class LuaSnapshot : EditorWindow
{

    [MenuItem("Tools/输出Lua内存日志")]
    static void Create()
    {
        _win = EditorWindow.GetWindow<LuaSnapshot>();
    }
    static LuaSnapshot _win;
    

    //================成员方法=================//
    private bool _firstSnap = false;
    private bool _secondSnap = false;
    private bool _sortAdd = true;
    private Color _defColor ;
    private Color _defBgColor ;
    private GUIContent _tips;
    private List<SnapShotVo> _list;
    private int addTblNum = 0;
    private int removeTblNum = 0;
    //定义垂直滑动条的值  
    private Vector2 _verticalValue;

    private void OnDestroy()
    {
        //Debug.Log("OnDestroy...");
    }

    private void Awake()
    {
        _defColor = GUI.color;
        _defBgColor = GUI.backgroundColor;
    }

    private GUIContent GetTips()
    {
        if(_tips == null)
            _tips = new GUIContent();

        return _tips;
    }

    private LuaSnapshot GetWin() {
        if (_win == null)
            _win = EditorWindow.GetWindow<LuaSnapshot>();

        return _win;
    }

    private void OnGUI()
    {
        //第一排
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear Log", GUILayout.Width(100)))
        {
            LuaClearSnap();
        }

        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("Take LuaMemory 1"))
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Take LuaMemory 1", "Snapshot...", 0.0f);
            try
            {
                LuaSnapshotPrint(1);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        EditorGUI.BeginDisabledGroup(!_firstSnap);
        if (GUILayout.Button("Take LuaMemory 2"))
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Take LuaMemory 2", "Snapshot...", 0.0f);
            try
            {
                LuaSnapshotPrint(2);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUI.BeginDisabledGroup(!_firstSnap || !_secondSnap);
        GUI.backgroundColor = Color.green;
        GUI.color = Color.white;
        if (GUILayout.Button("Compare Snapshot"))
        {
            ReadLogAndShow();
        }
        EditorGUI.EndDisabledGroup();
        //状态统计
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;    //这是设置背景填充的
        bb.alignment = TextAnchor.MiddleCenter;
        bb.normal.textColor = Color.grey;
        string stateStr = "";
        if (_list != null && _list.Count > 0)
        {
            stateStr = string.Format("- {0} 表 , + {1} 表", removeTblNum, addTblNum);
        }
        GUILayout.Label(stateStr, bb, GUILayout.Width(150), GUILayout.Height(25));
        GUILayout.EndHorizontal();
        GUI.backgroundColor = _defBgColor;
        GUI.color = _defColor;
        
        //第二排
        GUILayout.BeginHorizontal();
        GUILayout.Space(15);
        GUILayout.Label("Path", EditorStyles.boldLabel, GUILayout.Width(GetWin().position.width - 900));
        GUILayout.Label("tableID", EditorStyles.boldLabel, GUILayout.Width(200));
        var clickChilds = GUILayout.Button("userdata childs", EditorStyles.boldLabel, GUILayout.Width(350));
        var clickCount = GUILayout.Button("Count", EditorStyles.boldLabel, GUILayout.Width(100));
        var clickAddRemove = GUILayout.Button("Add/Remove", EditorStyles.boldLabel, GUILayout.Width(100));
        var clickUseCS = GUILayout.Button("use C#", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.EndHorizontal();
        if (clickChilds)
        {
            if (_list != null && _list.Count > 0)
            {
                _list.Sort(sortForNull);
            }
        }
        else if (clickCount)
        {
            if (_list != null && _list.Count > 0)
            {
                _list.Sort(sortForCount);                
            }
        }
        else if (clickAddRemove)
        {
            if (_sortAdd)
            {
                _sortAdd = false;
                _list.Sort(sortForAddCount);
            }
            else
            {
                _sortAdd = true;
                _list.Sort(sortForRemoveCount);
            }
        }
        else if (clickUseCS)
        {
            if (_list != null && _list.Count > 0)
            {
                _list.Sort(sortForUseCS);
            }
        }
        //第三排
        /**/
        if (_list != null && _list.Count > 0)
        {
            var pos = GetWin().position;
            var drawHeight = pos.height - 45;       //能绘制的高度
            var length = _list.Count;
            if (length > 500)
                length = 500;

            _verticalValue = GUI.BeginScrollView(new Rect(0, 45, pos.width, drawHeight),
                            _verticalValue, new Rect(0, 45, pos.width - 20, length * 34));          

            for (var i = 0; i < length; i++)
            {
                GUILayout.BeginHorizontal("box", GUILayout.Height(30));
                GUILayout.TextField(_list[i].path, GUILayout.Width(pos.width - 900));
                //复制粘贴
                if (GUILayout.Button(_list[i].tAddress, EditorStyles.label, GUILayout.Width(200)))
                {
                    Debug.Log("地址： " + _list[i].tAddress);
                }
                if (_list[i].udMap != null)
                {
                    if (_list[i].udMap.Length < 255)
                        GUILayout.TextField(_list[i].udMap, GUILayout.Width(350));
                    else
                        GUILayout.TextField(_list[i].udMap.Substring(0,254), GUILayout.Width(350));
                }
                else
                {
                    GUILayout.Label("", EditorStyles.label, GUILayout.Width(350));
                }
                GUILayout.Label(_list[i].count.ToString(), EditorStyles.label, GUILayout.Width(100));
                if (_list[i].addCount >= 0)
                {
                    GUILayout.Label("+ " + _list[i].addCount, EditorStyles.label, GUILayout.Width(100));
                }
                else
                {
                    GUI.color = Color.gray;
                    GUILayout.Label( _list[i].addCount.ToString(), EditorStyles.label, GUILayout.Width(100));
                    GUI.color = _defColor;
                }
                GUILayout.Label(_list[i].udNum.ToString(), EditorStyles.label, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            GUI.EndScrollView();
        }
           
        
    }

    /// <summary>
    /// 执行lua
    /// <param name="type"></param>
    void LuaSnapshotPrint(byte type)
    {
        //LuaManager mgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
        //mgr.CallFunction("LuaLogTool.dump");
        LuaState luaState = LuaClient.GetMainState();
        if (luaState == null)
        {
            Debug.Log("需要先运行游戏.....");
            GetTips().text = "需要先运行游戏";
            GetWin().ShowNotification(GetTips());
            return;
        }
        /**/
        var table = luaState.GetTable("LuaLogTool");
        if (table == null)
        {
            Debug.LogError("不存在lua表");
            return;
        }

        LuaFunction luaFun;
        if (type == 1)
        {
            luaFun = table.GetLuaFunction("dump1");
        }
        else
        {
            luaFun = table.GetLuaFunction("dump2");
        }
        if (luaFun == null)
        {
            Debug.LogError("不存在lua函数");
            return;
        }

        luaFun.Call();
        if (type == 1)
            _firstSnap = true;
        else
            _secondSnap = true;

        GetTips().text = "日志：" + type + " 采集成功！";
        GetWin().ShowNotification(GetTips());
    }

    void LuaClearSnap()
    {
        _firstSnap = false;
        _secondSnap = false;        
        var path1 = Application.dataPath + "/../lua1.log";
        var path2 = Application.dataPath + "/../lua2.log";
        if (File.Exists(path1))
        {
            File.Delete(path1);
        }
        if (File.Exists(path2))
        {
            File.Delete(path2);
        }
        _list = null;
        GetTips().text = "删除日志文件成功！";
        GetWin().ShowNotification(GetTips());
    }

    void ReadLogAndShow()
    {
        var path1 = Application.dataPath + "/../lua1.log";
        var path2 = Application.dataPath + "/../lua2.log";
        if (!File.Exists(path1))
        {
            GetTips().text = "日志文件1不存在，请检查";
            GetWin().ShowNotification(GetTips());
            return;
        }
        else if (!File.Exists(path2))
        {
            GetTips().text = "日志文件2不存在，请检查";
            GetWin().ShowNotification(GetTips());
            return;
        }

        string[] logLine1 = File.ReadAllLines(path1);
        string[] logLine2 = File.ReadAllLines(path2);
        _list = new List<SnapShotVo>();
        addTblNum = 0;
        removeTblNum = 0;
        var tmpList = new List<SnapShotVo>();
        Dictionary<string, SnapShotVo> map1 = new Dictionary<string, SnapShotVo>();
        Dictionary<string, SnapShotVo> map2 = new Dictionary<string, SnapShotVo>();
        SnapShotVo sVo;
        SnapShotVo old;
        for (var i = 0; i < logLine1.Length; i++)
        {
            sVo = ParseLine(logLine1[i]);
            map1.Add(sVo.tAddress, sVo);
            tmpList.Add(sVo);
        }
        for (var i = 0; i < logLine2.Length; i++)
        {
            sVo = ParseLine(logLine2[i]);
            _list.Add(sVo);
            map2.Add(sVo.tAddress, sVo);
            //如果从日志1能找到表地址，算出增加的元素，否则算全量
            if (map1.TryGetValue(sVo.tAddress, out old))
            {
                sVo.addCount = sVo.count - old.count;
                sVo.addUdNum = sVo.udNum - old.udNum;
            }
            else
            {
                sVo.addCount = sVo.count;
                sVo.addUdNum = sVo.udNum;
                addTblNum++;
            }
        }
        //统计出已经删除的表
        for (var i = 0; i < tmpList.Count; i++)
        {
            sVo = tmpList[i];
            if (map2.ContainsKey(sVo.tAddress) == false)
            {
                sVo.addCount = -sVo.count;
                sVo.count = 0;
                sVo.udNum = 0;
                sVo.udMap = null;
                sVo.nullNum = 0;
                _list.Add(sVo);
                removeTblNum++;
            }
        }
        _list.Sort(sortForUseCS);
        Debug.Log("需要显示的数量：" + _list.Count);
    }

    SnapShotVo ParseLine(string line)
    {
        string[] objstr = line.Split('\t');
        if (objstr.Length < 7)
        {
            Debug.Log("line:" + line);
            Debug.LogError("日志文件格式有误");
            return null;
        }
        var sVo = new SnapShotVo();
        sVo.nullNum = Int32.Parse(objstr[0]);
        sVo.tAddress = objstr[1];
        sVo.count = Int32.Parse(objstr[2]);
        sVo.udNum = Int32.Parse(objstr[3]);
        if (objstr[4].Length > 0)
        {
            sVo.udMap = objstr[4];
        }
        sVo.tKey = objstr[5];
        sVo.path = objstr[6];
        return sVo;
    }

    int sortForAddCount(SnapShotVo a, SnapShotVo b)
    {
        if (a != b && a != null && b != null)
        {
            if (a.addCount < b.addCount) return 1;
            else if (a.addCount > b.addCount) return -1;
            return a.tAddress.GetHashCode() > b.tAddress.GetHashCode() ? 1 : -1;
        }
        return 0;
    }

    int sortForRemoveCount(SnapShotVo a, SnapShotVo b)
    {
        if (a != b && a != null && b != null)
        {
            if (a.addCount < b.addCount) return -1;
            else if (a.addCount > b.addCount) return 1;
            return a.tAddress.GetHashCode() > b.tAddress.GetHashCode() ? 1 : -1;
        }
        return 0;
    }

    int sortForCount(SnapShotVo a, SnapShotVo b)
    {
        if (a != b && a != null && b != null)
        {
            if (a.count < b.count) return 1;
            else if (a.count > b.count) return -1;
            return 0;
        }
        return 0;
    }

    int sortForNull(SnapShotVo a, SnapShotVo b)
    {
        if (a != b && a != null && b != null)
        {
            if (a.nullNum < b.nullNum) return 1;
            else if (a.nullNum > b.nullNum) return -1;
            //return a.tAddress.GetHashCode() > b.tAddress.GetHashCode() ? 1 : -1;
            return sortForUseCS(a, b);
        }
        return 0;
    }

    int sortForUseCS(SnapShotVo a, SnapShotVo b)
    {
        if (a != b && a != null && b != null)
        {
            if (a.udNum < b.udNum) return 1;
            else if (a.udNum > b.udNum) return -1;
            return a.tAddress.GetHashCode() > b.tAddress.GetHashCode() ? 1 : -1;
        }
        return 0;
    }
}

class SnapShotVo
{
    public string path;
    public string tAddress;
    public string tKey;
    public string udMap;
    public int count;           // 元素数量
    public int udNum;           // userData引用数量
    public int addCount;        // 对比后增加的元素数量，负数为减少的数量
    public int addUdNum;        // 增加的userData引用数量
    public int nullNum;         // c#对象已经销毁,lua仍然持有的数量
}
