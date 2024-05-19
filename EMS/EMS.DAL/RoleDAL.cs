using EMS.DAL.DTO;
using EMS.DAL.Interfaces;
using EMS.DAL.Mapper;
using EMS.DB.Context;
using EMS.DB.Mapper;
using EMS.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.DAL;

public class RoleDAL : IRoleDAL
{
    private readonly EMSContext _context;
    private readonly IRoleMapper _roleMapper;
    private readonly IEmployeeMapper _employeeMapper;

    public RoleDAL(EMSContext context)
    {
        _context = context;
        _roleMapper = new RoleMapper();
        _employeeMapper = new EmployeeMapper();
    }

    public async Task<int> InsertAsync(Role role)
    {
        _context.Role.Add(role);
        await _context.SaveChangesAsync();

        return role.Id;
    }

    public async Task<List<RoleDto>> RetrieveAllAsync(RoleFilters? filters)
    {
        var roles = await _context.Role
                .Skip((filters!.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Include(r => r.Department)
                .ToListAsync();
        return _roleMapper.ToRoleDto(roles);
    }

    public async Task<List<RoleDto>> RetrieveByDeptIdAsync(RoleFilters filters)
    {
        var roles = await _context.Role
                .Include(r => r.Department)
                .Where(r => r.DepartmentId == filters.DepartmentId)
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

        return _roleMapper.ToRoleDto(roles);
    }

    public async Task<List<RoleDto>> RetrieveByRoleIdAsync(RoleFilters filters)
    {
        var roles = await _context.Role.Include(r => r.Department).Where(r => r.Id == filters.RoleId).ToListAsync();
        return _roleMapper.ToRoleDto(roles);
    }

    public async Task<List<RoleDto>?> FilterAsync(RoleFilters? filters)
    {
        if (filters == null)
        {
            return null;
        }

        var roles = _context.Role.Include(r => r.Department).AsQueryable();

        if (filters.Departments != null && filters.Departments.Count != 0)
        {
            roles = roles.Where(e => e.DepartmentId.HasValue && filters.Departments.Contains(e.DepartmentId.Value));
        }

        // Apply pagination
        roles = roles
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize);

        var result = await roles.ToListAsync();
        return _roleMapper.ToRoleDto(result);
    }
}
