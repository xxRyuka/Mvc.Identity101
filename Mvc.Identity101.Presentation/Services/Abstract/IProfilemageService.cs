using Mvc.Identity101.Data.Dto;

namespace Mvc.Identity101.Services.Abstract;

public interface IProfilemageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder); // bir path dondureceğiz sonucunda 
    Task<string> SaveImageAsync(IFormFile file, string folder, string fileName);
}