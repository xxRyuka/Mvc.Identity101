using Mvc.Identity101.Services.Abstract;
using Mvc.Identity101.Services.Data;

namespace Mvc.Identity101.Services.Concrete;

public class ProfileImageService : IProfileImageService
{
    public async Task<string> SaveImageAsync( string userId ,IFormFile file, ImageType ImgType)
    {
        
        
        // simdi eger bir profil fotosu var ise imageTypei Profil fotosu ise ayri bir kontrol yapacağiz buda su oalcak
        /// eski fotoyu sileceğiz ondan sonra ekleyeceğiz egerki imageType.Profile fotosu ise 

        if (ImgType == ImageType.ProfilePhoto)
        {
            try
            {
                var UploadPath = Path.Combine("wwwroot", "uploads",$"{ImgType}",userId ); //her id için yeni klasor ?
                if (Directory.Exists(UploadPath))
                {
                    Directory.Delete(UploadPath+"/", true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        try
        {

        
        // userId yi controllerde check edebiliriz aslında buraya signin manager eklemeyi pek mantıklı bulmadım acıkcası 
        // burda sadece bir dosya pathi dondureceğim
        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            return  "error";
        }
        
        var extension =  Path.GetExtension(file.FileName).ToLowerInvariant();
        var AllowedExtensions = new [] { ".jpg", ".jpeg", ".png","gif" };
        if (!AllowedExtensions.Contains(extension))
        {
            throw  new Exception("Invalid image extension");
        }

        var UploadPath = Path.Combine("wwwroot", "uploads",$"{ImgType}",userId ); //her id için yeni klasor ?
        if (!Directory.Exists(UploadPath))
            Directory.CreateDirectory(UploadPath);
        var guidName = Guid.NewGuid().ToString() + extension;
        
        var filePath = Path.Combine(UploadPath, guidName);

        using (var fileStream = new  FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        
        //vtye kaydedecğeimiz path 
        var RelativePath = Path.Combine("uploads",$"{ImgType}",userId,guidName ).Replace("\\", "/");
        return "/"+  RelativePath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}