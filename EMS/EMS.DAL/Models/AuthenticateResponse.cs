namespace EMS.DAL.Models;

public class AuthenticateResponse
{
    public bool IsAuthenticated { get; set; } = false;
    public int? Id { get; set; } = null;
    public string? FirstName { get; set; }= null;
    public string? LastName { get; set; }= null;
    public string? Email { get; set; }= null;
    public string? UID { get; set; }= null;
    public string? Password {get; set; } = null;
}