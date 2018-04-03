
--保存类类型的虚表
local _class = {}



function BaseClass(...)
	args = {...}
	--printTable(args)
	if 0 < #args then
		local supers = {}
		for i=1,#args do
			--print(i)
			supers[i] = {args[i], 1, 0}
		end

		--printTable(supers)
		return BaseClassEx(unpack(supers))
	else
		return BaseClassEx()
	end
	

	
end


--{type, 0} 无构造函数 {type, 1, 0} 子类构造参数 {type, 2, "myargs"} zi
function BaseClassEx(...)
	-- 生成一个类类型
	supers = {...}

	--printTable (super)
	local class_type = {}
	-- 在创建对象的时候自动调用
	class_type.__init = false
	class_type.__delete = false 
	--printTable(supers)
	
	if 0 < #supers then

		class_type.derive = true
		class_type.supers = {}
		class_type.spr_smt = {}
		class_type.spr_arg = {}
		for i=1,#supers do
			--supers[i] = {args[1], 1, 0}
			class_type.supers[i] = supers[i][1]
			class_type.spr_smt[i] = supers[i][2]
			class_type.spr_arg[i] = supers[i][3]
			--print(supers[i][1])
		end
		class_type.super = supers[1][1]


	else
		class_type.derive = false
	end


	class_type.New = function(...)
		--print("class_type.New = function(...)")
		-- 生成一个类对象
		local obj = {}
		obj._class_type = class_type

		-- 在初始化之前注册基类方法
		setmetatable(obj, { __index = _class[class_type] })

		-- 调用初始化方法
		do
			local create 
			create = function(c, ...)
				if c.derive then
				--if c.supers then
					--local __base = rawget(c, "__base")
					for i=1,#c.supers do
						--print(#c.supers)
						--print(c.__base)
						if 0 == c.spr_smt[i] then
							create(c.supers[i], nil)
						elseif 1 == c.spr_smt[i] then
							create(c.supers[i], ...)
						elseif 2 == c.spr_smt[i] then
							create(c.supers[i], c.spr_arg[i])
						end	

					end
				end

				if c.__init then
					c.__init(obj, ...)
				end
			end

			create(class_type, ...)
		end

				-- 注册一个delete方法
		local  delete
		delete = function(obj, c)
			if c.__delete then
				c.__delete(obj)
			end
			if c.derive then
				for i=1,#c.supers do
					delete(obj, c.supers[i])
				end
			end
		end
		obj.DeleteMe = function(obj)
			delete(obj, obj._class_type)
		end

		return obj
	end

	local vtbl = {}
	_class[class_type] = vtbl
 


	setmetatable(class_type, {__newindex =
		function(t,k,v) 
			vtbl[k] = v
		end
		, 
		__index = vtbl, --For call parent method
	})
 

	if class_type.derive then


	--if supers then
	--if 0 ~= #supers then
		--print("have super".. #super)

		setmetatable(vtbl, {__index =
			function(t,k)


				if class_type.derive then
				--if class_type.supers then

					for i=1,#class_type.supers do

						if _class[class_type.supers[i]][k] then

							return _class[class_type.supers[i]][k]
						end
					end
				end
				--local ret = _class[super][k]
				--do not do accept, make hot update work right!
				--vtbl[k] = ret
				--return ret
				return nil
			end
		})
	end
 
	return class_type
end

--[[

local b1 = b1 or BaseClass()
function b1:test1()
	print("i am b1")
end
function b1:__init()
	print("b1 init")
end
function b1:__delete()
	print("b1 del")
end


local b2 = b2 or BaseClass()
function b2:test2()
	print("i am b2")
end
function b2:__init(name)
	print("b2 init" .. name)
end
function b2:__delete()
	print("b2 del")
end




--local  b3 = BaseClassEx({b1,1,0}, {b2,2,"carry5"})
local  b3 = BaseClass(b1, b2)

function b3:test3()
	print("i am b3")
end
function b3:__init(name)
	print("b3 init")
end
function b3:__delete()
	print("b3 del")
end


local b4 = BaseClass(b3)


function b4:__init(name)
	print("b4 init")
end
function b4:__delete()
	print("b4 del")
end


a = b4.New("myname")
a:test2()
]]

--a:test2()
--a:test6()
--local  c = BaseClass(b1)

--a:test1()
--a:test2()
--a:test3()
--a:DeleteMe()
--a:test2()
--dump(_class)

