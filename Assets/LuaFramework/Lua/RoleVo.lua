
RoleVo = Class("LuaBev");

function RoleVo:ctor(  )
	self.roleList = {};
end

function RoleVo:AddRole( roleData )
	table.insert(self.roleList, roleData);
end

function RoleVo:Clear(  )
	self.roleList = {};
end
