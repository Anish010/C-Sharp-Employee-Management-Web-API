using EMS.DAL.DTO;
using EMS.DAL.Interfaces;
using EMS.DB.Models;

namespace EMS.DAL.Mapper;

public class RoleMapper : IRoleMapper
{
    public List<RoleDto> ToRoleDto(IEnumerable<Role> roles)
    {
        return roles.Select(ToRoleDto).ToList()!;
    }

    public RoleDto? ToRoleDto(Role role)
    {
        if(role == null)
        {
            return null;
        }
        return new RoleDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            DepartmentId = role.DepartmentId,
            DepartmentName = role.Department!.Name
        };
    }
}
