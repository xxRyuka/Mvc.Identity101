using Microsoft.AspNetCore.Identity;
using Mvc.Identity101.Data.Entites;

namespace Mvc.Identity101.CustomValidations;

public class CustomUserValidation : IUserValidator<AppUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
    {
        var errList = new List<IdentityError>();
        bool isStartsWithDigit = int.TryParse(user.UserName[0].ToString(), out _);

        if (isStartsWithDigit)
        {
            errList.Add(new IdentityError(){Code = "UserNameStartsWithDigit" , Description = "User name starts with digit"});
        }

        if (errList.Any())
        {
            return Task.FromResult(IdentityResult.Failed(errList.ToArray()));
        }
        return Task.FromResult(IdentityResult.Success);
    }
}