
LuaMemTracker = {}

--弱数组字典
local _map 	= {};


--跟踪变量
function LuaMemTracker.tracker( obj, objName )
	local _list;
	if _map[objName] == nil then
		_list = {};
		setmetatable(_list, {__mode = "v"});
		_map[objName] = _list;
	else
		_list = _map[objName];
	end

	table.insert(_list, obj);
	--print("记录了：", objName, obj);
end


--获取所有跟踪的变量数量
function LuaMemTracker.getAllTypeNum(  )
	local typeList = {};
	for k,list in pairs(_map) do
		local info = {};
		info.key = k;
		info.count = 0;
		for kk,obj in pairs(list) do
			if obj ~= nil then
				info.count = info.count + 1;
			end
		end
		table.insert(typeList, info);
		--print("存在：", info.key, info.count);
	end
	return typeList;
end
