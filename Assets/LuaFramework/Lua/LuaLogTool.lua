
LuaLogTool = {}

--输出配置
LuaLogTool.isOutFunction = false;   --输出函数信息
LuaLogTool.isOutNumber = false;     --输出数字元素
LuaLogTool.isOutBool = false;       --输出布尔元素
LuaLogTool.isOutUnity = false;      --输出Unity信息
LuaLogTool.isTableID = false;       --输出本表信息

LuaLogTool._index = LuaLogTool
local PrintedCatch = nil            --记录已经打印过的表命 防止递归遍历
local _localMap;
local tokens;
local snapList;

--第一次采样
function LuaLogTool.dump1()
    --print("执行:LuaLogTool.dump1...");
    LuaLogTool.dumptree(_G,"_G")
    LuaLogTool.saveListLog("lua1.log", snapList);
    snapList = nil;
end

--二次采样
function LuaLogTool.dump2()
    LuaLogTool.dumptree(_G,"_G");    
    LuaLogTool.saveListLog("lua2.log", snapList);
    snapList = nil;
end

function LuaLogTool.clearSnap(  )
	snapList = nil;
end

--日志落地
function LuaLogTool.saveListLog( fileName, list )
	--使用table.concat 快速连接	
	local str;
	tokens = {};
	--排序
	table.sort(list,function(a,b) 
		--return a.count > b.count 
		return a.udNum > b.udNum;
	end);
	for i,v in ipairs(list) do
		if v.udNum > 0 then			
			local udStr = "";
			for key,num in pairs(v.udMap) do
				udStr = udStr..key..":"..num..", ";
			end
			str = v.nullNum.."\t"..v.address.."\t"..v.count.."\t"
            ..v.udNum.."\t"..udStr.."\t"..v.key.."\t"..v.path;
		else
			str = v.nullNum.."\t"..v.address.."\t"..v.count.."\t"
            ..v.udNum.."\t".."\t"..v.key.."\t"..v.path;
		end
		table.insert(tokens, str);
	end
	local result = table.concat(tokens, "\n");
	tokens = nil;
	--print("得到字符：", #result);
	local path = UnityEngine.Application.dataPath .. "/../"..fileName;
    local file = io.open(path, "w+");
    if file == nil then
        error("File Is Not Exist!")
    end
    
    file:write(result)
    io.close(file)
end


function LuaLogTool.dumptree(obj,ObjName, width)
    -- 递归打印函数
    local dump_obj;
    local end_flag = {};
    
    --检查表是否需要分析
    local function isOkTable( obj )
        if type(obj) == "table" and obj["__newindex"] == nil
                and obj["_listener_for_children"] == nil
                and obj["__tostring"] == nil then
            return true;
        else
            return false;
        end
    end

    local function make_indent(layer, is_end)
        local subIndent = string.rep("  ", width)
        local indent = "";
        end_flag[layer] = is_end;
        local subIndent = string.rep("  ", width)
        for index = 1, layer - 1 do
            if end_flag[index] then
                indent = indent.." "..subIndent
            else
                indent = indent.."|"..subIndent
            end
        end

        if is_end then
            return indent.."└"..string.rep("─", width).." "
        else
            return indent.."├"..string.rep("─", width).." "
        end
    end

    local function make_quote(str)
        str = string.gsub(str, "[%c\\\"]", {
            ["\t"] = "\\t",
            ["\r"] = "\\r",
            ["\n"] = "\\n",
            ["\""] = "\\\"",
            ["\\"] = "\\\\",
        })
        return "\""..str.."\""
    end

    local function dump_key(key)
        if type(key) == "number" then
            return key .. ":"
        elseif type(key) == "string" then
            return "\"".. key.. "\": "
        elseif type(key) == "userdata" then
            return "userdata:"
        elseif type(key) == "table" then
            return "table"
        elseif type(key) == "thread" then
            return "thread"
        end

        return "["..type(key).."]"..key..":"
    end

    local function dump_val(val,parentObjName,layer)
        if type(val) == "table" then
            dump_obj(val,parentObjName,layer)
            return "";
        elseif type(val) == "string" then
            return make_quote(val)
        else
            return tostring(val)
        end
    end

    --创建采集结构
    local function createSnap(address, key, count, udNum, path, pLayer)
    	local _table = {};
    	_table.address = address;
    	_table.key = dump_key(key);
    	_table.count = count;
    	_table.udNum = udNum;
        _table.nullNum = 0;
    	_table.path = path;
        _table.layer = pLayer;
    	_table.udMap = {};
    	_table.addUserData = function ( obj, udName )
    		if type(udName) ~= "string" then
    			return
    		elseif obj.udMap[udName] == nil then
    			obj.udMap[udName] = 1;
    		else
    			obj.udMap[udName] = obj.udMap[udName] + 1;
    		end
    	end
    	setmetatable(_table, {
            __newindex = function ( t,k,v )
                error("table is ready only!");
            end
            });    	
    	return _table;
    end

    --统计元素数量
    local function count_elements(obj)
        local count = 0
        for k, v in pairs(obj) do
            if v ~= nil then
                count = count + 1
            end
        end
        return count
    end

    --分析函数里的local表
    local function parse_func( toTable, func, parentObjName, layer)
        -- body
        local index = 1;
        local lines = make_indent(layer , false);
        local isNull = true;
        while true do
            local name, value = debug.getupvalue(func, index);
            if name == nil then
                break;
            end
            
            if isOkTable(value) and _localMap[value] == nil then                
                local path = parentObjName..name;             
                local address = tostring(value);
                local count = count_elements(value);
            	local snap = createSnap(address, name, count, 0, path, layer);
            	_localMap[value] = snap;
            	table.insert(snapList, snap);
                dump_obj(value,path ,layer + 1) ;
                isNull = false;
            end
            
            index = index + 1;
        end
        --去掉最后一个换行
        return isNull;
    end

    --分析一个表
    dump_obj = function(obj,parentName,layer)
        if type(obj) ~= "table" then
            return count_elements(obj)
        end

        if PrintedCatch[obj] ~= nil then
            return parentName
        else
            PrintedCatch[obj] = true
        end

        layer = layer + 1        
        local max_count = count_elements(obj)
        local cur_count = 1

        for k, v in pairs(obj) do
            local key_name = dump_key(k)

            if k == "PrintedCatch" then
                --table.insert(tokens, make_indent(layer, true) .. "table is filter")
                --logWarn(k)            
            elseif LuaLogTool.isOutUnity == false 
                and (string.find(key_name, 'UnityEngine') ~= nil
                    or string.find(key_name, 'System') ~= nil
                    or string.find(key_name, 'Config') ~= nil
                    or string.find(key_name, 'LuaFramework') ~= nil) then
                --unity基类不输出
            elseif string.find(key_name, 'LuaLogTool') ~= nil then
                --自身不输出
           
            elseif type(v) == "table" then
                --子表
                --print("v,k, layer", v,k, layer,key_name);
                if isOkTable(v) and _localMap[v] == nil then  
                    local address = tostring(v);
                    local path ;
                    if type(k) == "table" then
                       path = parentName.." -> ".. "["..address.."]";                    
                    else
                        path = parentName.." -> ".. key_name;  
                    end
                    local count = count_elements(v);
                	local snap = createSnap(address, k, count, 0, path, layer);
                	--print("parentName" , parentName, snap.udNum );
                	_localMap[v] = snap;
                	table.insert(snapList, snap);
                    dump_val(v,path, layer);
                elseif isOkTable(v) and _localMap[v] ~= nil
                    and _localMap[v].layer >= layer then
                    --如果是全局表，优先显示此路径
                    --print("重新处理路径")
                    _localMap[v].path = parentName.." > ".. key_name;  
                end
            elseif type(v) == "function" then
                if not LuaLogTool.isOutFunction then
                    --忽略函数输出
                    --print("忽略函数输出", k);                    
                end
                --分析函数下的local容器  
                local path = parentName.."-> "..key_name;  
                local isNull = parse_func(tokens,v,parentName, layer);
                if isNull == false then
                    --table.insert(tokens, strParse )
                end
            elseif type(v) == "userdata" then
                --print("存在UserData.....",count_elements(obj) )
                -- table.insert(tokens, make_indent(layer, cur_count == max_count)
                --     .. key_name .."c# ".. tostring(v));
                local snap = _localMap[obj];
                if snap ~= nil then
                	snap.udNum = snap.udNum + 1;
                    local usName = tostring(v);
                	snap:addUserData(usName);
                    if usName == "null" then
                        snap.nullNum = snap.nullNum + 1;
                    end
                end
            elseif type(k) == "userdata" then
                --键值为c#Objet
                local snap = _localMap[obj];
                if snap ~= nil then
                	snap.udNum = snap.udNum + 1;
                    local usName = tostring(k);
                	snap:addUserData(usName);
                    if usName == "null" then
                        snap.nullNum = snap.nullNum + 1;
                    end
                end
            elseif type(v) == "number" and not LuaLogTool.isOutNumber then
                --不输出数据类型    
                
            elseif type(v) == "boolean" and not LuaLogTool.isOutBool then
                --不输出布尔类型                  
            else
                -- table.insert(tokens, make_indent(layer, cur_count == max_count)
                --     .. key_name .. dump_val(v,k, layer))
            end

            cur_count = cur_count + 1
        end

        -- 处理空table
        if max_count == 0 then
            --table.insert(tokens, make_indent(layer, true) .. "{ }")
        end
    end

    if type(obj) ~= "table" then
        return "the params you input is "..type(obj)..
            ", not a table, the value is --> "..tostring(obj)
    end

    width = width or 2
    PrintedCatch = {};
    _localMap = {};
    snapList = {}; 
    collectgarbage("collect");
    local mmCount = collectgarbage("count");
    print("lua内存:", (mmCount / 1024 ) .. "M");
    
    dump_obj(obj,ObjName,0)
    --local str = table.concat(tokens, "\n");
    --print("得到的表长度为：" , #tokens);
    print("收集表长度：", #snapList);
    PrintedCatch = nil;
    _localMap = nil;
end
