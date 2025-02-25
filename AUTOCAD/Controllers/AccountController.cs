using System.Threading.Tasks;
using AUTOCAD.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    //rejestracja
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Imie = model.Imie, Nazwisko = model.Nazwisko };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Dodanie imienia jako claim
                await _userManager.AddClaimAsync(user, new Claim("Imie", model.Imie ?? string.Empty));

                var userCount = await _userManager.Users.CountAsync(); // Pobranie liczby użytkowników

                // Sprawdzenie, czy jest to pierwszy użytkownik w systemie
                if (userCount == 1)
                {
                    // Jeśli tak, nadaj rolę "Administrator" pierwszemu użytkownikowi
                    await _userManager.AddToRoleAsync(user, "Administrator");
                }
                else
                {
                    // W przeciwnym razie, nadaj rolę "User" kolejnym użytkownikom
                    await _userManager.AddToRoleAsync(user, "User");
                }

                // Pozostała część kodu bez zmian
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

   
    //logowanie
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
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    //wylogowanie
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home"); // Przekierowanie po wylogowaniu
    }
}
