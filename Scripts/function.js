var REL_Id_Names;
function getUsernameById(gridId, userId) {
    var data = $(gridId).data("kendoGrid").dataItems();
    if (!REL_Id_Names) {
        REL_Id_Names = {};
        $.each(data, function (k, v) {
            REL_Id_Names[v.UserId] = v.UserName;
        })
    }
    return REL_Id_Names[userId];
}

var REL_Id_Names_Cible;
function getUsernameCibleById(gridId, userId) {
    var data = $(gridId).data("kendoGrid").dataItems();
    if (!REL_Id_Names_Cible) {
        REL_Id_Names_Cible = {};
        $.each(data, function (k, v) {
            REL_Id_Names_Cible[v.UserCibleId] = v.UserCibleName;
        })
    }
    return REL_Id_Names_Cible[userId];
}

var REL_Id_Role_Names;
function getRoleById(gridId, roleId) {
    var data = $(gridId).data("kendoGrid").dataItems();
    if (!REL_Id_Role_Names) {
        REL_Id_Role_Names = {};
        $.each(data, function (k, v) {
            REL_Id_Role_Names[v.RoleId] = v.RoleName;
        })
    }
    return REL_Id_Role_Names[roleId];
}