using EMS.DAL.DTO;
using EMS.DB.Models;

namespace EMS.BAL.Interfaces;

public interface IRoleBAL
{
    public Task<int> AddRoleAsync(Role role);
    public Task<List<RoleDto>?> GetAllAsync(RoleFilters? filters);
    public Task<List<RoleDto>> GetRolesByDeptIdAsync(RoleFilters filter);
    public Task<List<RoleDto>> GetRolesByRoleIdAsync(RoleFilters filter);
    public Task<List<RoleDto>?> FilterRolesAsync(RoleFilters? filters);
}