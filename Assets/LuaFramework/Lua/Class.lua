local _class={}  
 
function Class(className, super)
	local class_type={}
	class_type.ctor=false
	class_type.super=super	
	--xmf: 后面判断为：self.className == xxxClass.className 
	class_type.className = className;			
	class_type.new=function(...) 
			local obj={}
			obj.className = className;	
			do
				local create
				create = function(c,...)
					if c.super then
						create(c.super,...)
					end
					if c.ctor then
						c.ctor(obj,...)
					end
				end
 
				create(class_type,...)
			end
			setmetatable(obj,{ __index=_class[class_type] })
			return obj
		end
	local vtbl={}
	_class[class_type]=vtbl
 
 	--对类赋值，相当于对成员赋值
	setmetatable(class_type,{__newindex=
		function(t,k,v)
			vtbl[k]=v
		end
	})
 
	if super then
		setmetatable(vtbl,{__index=
			function(t,k)
				local ret=_class[super][k]
				vtbl[k]=ret
				return ret
			end
		})
	end
 
	return class_type
end