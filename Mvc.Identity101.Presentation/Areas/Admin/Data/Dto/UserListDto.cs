namespace Mvc.Identity101.Areas.Admin.Data.Dto;

public record UserListDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
}