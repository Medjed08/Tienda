using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asgard_Store.Models;
using Asgard_Store.ViewModels;

namespace Asgard_Store.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = $"{model.Nombre} {model.Apellido}",
                    Email = model.Email,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    PhoneNumber = model.Telefono,
                    Direccion = model.Direccion,
                    Departamento = model.Departamento,
                    CodigoPostal = model.CodigoPostal,
                    FechaRegistro = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Buscar usuario por email
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // Intentar login con el UserName del usuario encontrado
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName,  // Usar el UserName (Nombre Apellido)
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Tu cuenta ha sido bloqueada temporalmente por múltiples intentos fallidos.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                }
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}