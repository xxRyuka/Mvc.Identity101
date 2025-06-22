using System.ComponentModel.DataAnnotations;

namespace Mvc.Identity101.Data.Dto;

public class TestProfileImgDto
{

    
    // public string Id { get; set; }
    // [DataType()]
    [Required]
    public IFormFile? img { get; set; }
}