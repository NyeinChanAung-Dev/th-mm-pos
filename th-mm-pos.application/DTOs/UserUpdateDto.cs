namespace th_mm_pos.application.DTOs;

public class UserUpdateDto
{
    public int UserId { get; set; }
    public List<int> RoleIds { get; set; } = new();
    public bool? IsActive { get; set; }
}