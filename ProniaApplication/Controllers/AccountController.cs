using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaApplication.Areas.Admin.Controllers;
using ProniaApplication.Models;
using ProniaApplication.Utilities.Enums;
using ProniaApplication.ViewModels;

namespace ProniaApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signinManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<AppUser> userManager, 
            SignInManager<AppUser> signinManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _roleManager = roleManager;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser user = new AppUser()
            {
                Name = userVM.Name,
                Surname = userVM.Surname,
                Email = userVM.Email,
                UserName = userVM.UserName,
            };

            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            await _userManager.AddToRoleAsync(user, UserRole.Member.ToString());

            await _signinManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signinManager.SignOutAsync();
            return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM userVM, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userVM.UserOrEmail || u.Email == userVM.UserOrEmail);

            
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
                return View();
            }

            var result = await _signinManager.PasswordSignInAsync(user, userVM.Password, userVM.IsPersistent, false);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked, please try later!");
                return View();
            }
            if (!result.Succeeded) 
            {
                ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
                return View();
            }


            if(returnUrl is null)
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(returnUrl);


            //AppUser user = await _userManager.FindByNameAsync(userVM.UserOrEmail);

            //if (user is null)
            //{
            //    user = await _userManager.FindByEmailAsync(userVM.UserOrEmail);
            //    if (user is null)
            //    {
            //        ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
            //        return View();
            //    }
            //}
        }

        public async Task<IActionResult> CreateRoles()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if(!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = role.ToString() });

                }
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
