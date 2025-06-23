using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Identity101.Data;
using Mvc.Identity101.Data.Dto;
using Mvc.Identity101.Data.Entites;
using Mvc.Identity101.Services.Abstract;
using Mvc.Identity101.Services.Data;

namespace Mvc.Identity101.Controllers;

[Authorize]
public class MemberController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IProfileImageService _profileImageService;
    private readonly AppDbContext _context;

    public MemberController(SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager, IProfileImageService profileImageService, AppDbContext context)
    {
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userManager = userManager;
        _profileImageService = profileImageService;
        _context = context;
    }

    public async Task SignOut()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<IActionResult> Index()
    {
        // var user = await _userManager.GetUserAsync(User);
        
        // fotoyla işlem yapılacaği için böyle yapabiliriz
        var user = await _context.Users
            .Include(u => u.Gallery)
            .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

        if (user == null)
        {
            return RedirectToAction("AccessDenied");
        }

        UserDto dto = new UserDto()
        {
            Email = user.Email,
            UserName = user.UserName,
            Id = user.Id,
            Phone = user.PhoneNumber,
            imgPath = string.IsNullOrEmpty(user.imgPath)
                ? "/img/default.jpg"
                : user.imgPath,
            
        };

        foreach (var photo in user.Gallery)
        {
            dto.Photos.Add(photo);
        }

        return View(dto);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);

            var checkPassword = await _userManager.CheckPasswordAsync(user, request.OldPassword);

            if (checkPassword)
            {
                var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return View();
                    }
                }

                await _userManager
                    .UpdateSecurityStampAsync(user); // sifre gibi kritik bir veri değiştirdiğimiz için güncelliyoruz
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, request.NewPassword, true, false);
                TempData["Message"] = "Your password has been changed successfully";
                return View();
            }

            ModelState.AddModelError(string.Empty, "The current password is incorrect");
        }

        return View();
    }


    //simdilik sadece footrafi güncelleyelim diğer alanlar kolay zaten 
    [HttpGet]
    public async Task<IActionResult> ChangePhoto()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePhoto(TestProfileImgDto request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("AccessDenied");
        }

        var result = request.img;
        var extension = Path.GetExtension(result.FileName);
        var fileName = Guid.NewGuid() + extension;
        var pathFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profileImg");
        if (!Directory.Exists(pathFolder))
        {
            Directory.CreateDirectory(pathFolder);
        }

        var path = Path.Combine(pathFolder, fileName);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await result.CopyToAsync(stream);
        }


        user.imgPath = "/img/profileImg/" + fileName;
        await _userManager.UpdateAsync(user);

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> TestProfileImg()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TestProfileImg(TestProfileImgDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        var path = await _profileImageService.SaveImageAsync(user.Id, dto.img, ImageType.GalleryPhoto);

        var photo = new UserPhoto
        {
            imgPath = path,
            UserId = user.Id,
            User = user,
            Description = dto.description,
        };
        await _context.UserPhotos.AddAsync(photo);
        user.Gallery.Add(photo);
        _context.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }


    public async Task<IActionResult> ImgDetail(int id)
    {
        // var user = await _userManager.GetUserAsync(User);
        var img = await _context.UserPhotos.FindAsync(id);
        if (img == null)
        {
            return NotFound();    
        }

        var detail = new ImgDetailDto()
        {
            Description = img.Description,
            ImgPath = img.imgPath,
            UploadDate = img.UploadDate
        };
        
        return View(detail);
    }
}