namespace EMS.DAL.DTO;

public class RoleFilters
{
    public List<int>? Departments { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public int? RoleId { get; set; } = null;
    public int? DepartmentId { get; set; } = null;
}
