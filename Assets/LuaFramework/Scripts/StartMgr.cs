using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

public class StartMgr : LuaClient {

    public List<RoleClass> roleList = new List<RoleClass>();
    public GameObject rolePrefab;

    private LuaState _luaState;
    private LuaTable _luaRoleMgr;

    // Use this for initialization
    void Start() {
        DontDestroyOnLoad(this.gameObject);
        _luaState = LuaClient.GetMainState();
        _luaRoleMgr = _luaState.GetTable("RoleMgr");
        _luaState.translator.LogGC = true;
        if (_luaRoleMgr == null)
        {
            Debugger.LogError("找不到RoleMgr");
            return;
        }
        CreateGameObject();
    }

    // Update is called once per frame
    void Update() {

    }

    /// <summary>
    /// 创建一个GameObject
    /// </summary>
    public void CreateGameObject()
    {
        if (rolePrefab == null)
            return;

        GameObject createObj = GameObject.Instantiate<GameObject>(rolePrefab);
        RoleClass r1 = createObj.AddComponent<RoleClass>();
        roleList.Add(r1);
        //位置排序
        if (roleList.Count > 1)
        {
            float sx = -(float)roleList.Count / 2 + 0.5f;
            Vector3 p1 = Vector3.zero;
            for (var i = 0; i < roleList.Count; i++)
            {
                p1.x = sx + (i);
                roleList[i].transform.position = p1;
            }
        }
        //增加到lua容器
        var luaFun = _luaRoleMgr.GetLuaFunction("AddRoleClass");
        if (luaFun == null)
        {
            Debugger.LogError("找不到AddRoleClass");
            return;
        }
        luaFun.Call(createObj);
    }

    /// <summary>
    /// 删除所有创建的goj
    /// </summary>
    public void DelAllGameObject()
    {
        if (roleList.Count <= 0)
            return;

        for (var i = 0; i < roleList.Count; i++)
        {
            GameObject.Destroy(roleList[i].gameObject);            
        }
        roleList.Clear();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    /// <summary>
    /// 清理c#的容器
    /// </summary>
    public void ClearCsharpRef()
    {
        roleList.Clear();
        Debugger.Log("c#->roleList.Clear");
        System.GC.Collect();
    }

    /// <summary>
    /// 清理Lua的容器引用
    /// </summary>
    public void ClearLuaRef()
    {
        //Debug.Log("ClearLuaRef...");        

        var luaFun = _luaRoleMgr.GetLuaFunction("ClearList");
        if (luaFun == null)
        {
            Debugger.LogError("找不到ClearList");
            return;
        }
        luaFun.Call();
    }


    public void CallLuaRef()
    {
        Debug.Log("ClearLuaRef...");
        _luaState.translator.ClearLuaRef();

        var luaFun = _luaRoleMgr.GetLuaFunction("ClearLuaRef");
        if (luaFun == null)
        {
            Debugger.LogError("找不到ClearLuaRef");
            return;
        }
        luaFun.Call();
    }

    public void CallLuaGC()
    {
        var luaFun = _luaRoleMgr.GetLuaFunction("TryGc");
        if (luaFun == null)
        {
            Debugger.LogError("找不到TryGc");
            return;
        }
        luaFun.Call();
    }

    //必须加这一句手机打包才可访问正常路径
    protected override LuaFileUtils InitLoader()
    {
        return new LuaResLoader();
    }
}
