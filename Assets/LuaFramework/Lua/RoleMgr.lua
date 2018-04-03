require("Class");
require("RoleVo");
require("BaseClass");

RoleMgr = {};

local _list = {};
local _list2 = {};
local _logTxt;
local _roleVo = RoleVo.new();
local A1 = Class("AA");
local a2 = A1.new();
A1.roles = {};
local B1 = BaseClass();
B1.roles = {};
local b1 = B1.New();

_logTxt = UnityEngine.GameObject.Find("logOutText"):GetComponent("Text");
print("找到：_logTxt", _logTxt);

local function _init(  )
	-- body
end

--增加一个RoleClass对象到容器里
function RoleMgr.AddRoleClass( roleClass )
	_init();
	local aa = {};
	local bb = aa;
	--a2.roles = {};
	--a3.roles = {};
	print("A1.roles = ", A1.roles, a2.roles, b1.roles);
	print("bb = ", tostring(bb));
	table.insert(_list, roleClass);
	--_list[roleClass] = #_list + 1;
	_roleVo:AddRole(roleClass);
	local a1 = A1.new();
	--table.insert(_list, a1);
	-- local b1 = B1.new();
	-- a1.role = roleClass
	-- b1.role = roleClass
	table.insert(a2.roles, roleClass);
	table.insert(b1.roles, roleClass);
	--LuaMemTracker.tracker(roleClass, "roleClass");

	--table.insert(_roleVo.roleList, roleClass);
	--table.insert(_list2, roleClass);
	print("AddRoleClass ok!!");
	_logTxt.text = "AddRoleClass ok!!";
	for i=1,10 do
		--_list2[#_list2] = i;
		table.insert(_list2, i);
	end
end

function RoleMgr.TryGc(  )
	for i,v in ipairs(_list) do
		print("Destroy..",i,v);
		UnityEngine.GameObject.Destroy(v);
	end
end

--清理容器
function RoleMgr.ClearList(  )
	_list = {};
	_list2 = {};
	A1.roles = {};
	a2.roles = A1.roles;
	B1.roles = {};
	b1.roles = B1.roles;
	_roleVo:Clear();
	print("ClearList ok!!")
	_logTxt.text = "ClearList ok!!";
	collectgarbage("collect");
end

--清理引用
function RoleMgr.ClearLuaRef(  )
	collectgarbage("collect");
	print("ClearLuaRef ok!!")
	_logTxt.text = "ClearLuaRef ok!!";
end