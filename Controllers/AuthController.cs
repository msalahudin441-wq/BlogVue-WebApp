using BlogVue.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogVue.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;


        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Check for validation
            if (ModelState.IsValid)
            {
                // Create Identity user object
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                // user create
                var result = await _userManager.CreateAsync(user, model.Password);
                //if result succeeded
                if (result.Succeeded) {
                    //if user not exists then create
                    if (!await _roleManager.RoleExistsAsync("User")) {

                        await _roleManager.CreateAsync(new IdentityRole("User"));

                    }
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return RedirectToAction("Index", "Post");

                }


            }
            return View(model);

        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user =await _userManager.FindByEmailAsync(model.Email);
                if (user == null) {

                    ModelState.AddModelError("", "Email or password incorrect");
                    return View(model);
                
                }
                var SignInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (!SignInResult.Succeeded)
                {
                    ModelState.AddModelError("", "Email or Password incorrect");
                    return View(model);
                }

                return RedirectToAction("Index", "Post");
            }
            return View(model);
            
        }
        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Post");

        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}