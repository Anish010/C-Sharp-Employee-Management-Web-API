using EMS.DAL.DTO;
using EMS.DB.Models;

namespace EMS.DAL.Interfaces;

public interface IRoleDAL
{
    public Task<int> InsertAsync(Role role);
    public Task<List<RoleDto>> RetrieveAllAsync(RoleFilters filters);
    public Task<List<RoleDto>> RetrieveByDeptIdAsync(RoleFilters filter);
    public Task<List<RoleDto>> RetrieveByRoleIdAsync(RoleFilters filter);
    public Task<List<RoleDto>?> FilterAsync(RoleFilters? filters);
    
}