﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanelCRUD.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Register()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Register(MemberRegisterViewModel memberVM)
		{
			if(!ModelState.IsValid) return View();
			AppUser member = await _userManager.FindByNameAsync(memberVM.Username);
			if(member != null)
			{
				ModelState.AddModelError("Username","Username has taken!");
			}
			member= await _userManager.FindByEmailAsync(memberVM.Email);
			if (member != null)
			{
				ModelState.AddModelError("Email", "Email has taken");
			}
			member = new AppUser
			{
				FullName = memberVM.Fullname,
				Email = memberVM.Email,
				UserName = memberVM.Username
			};
			var result = await _userManager.CreateAsync(member, memberVM.Password);
			if (!result.Succeeded)
			{
				foreach(var err in result.Errors)
				{
					ModelState.AddModelError("", err.Description);
					return View();
				}
			}
			var roleResult = await _userManager.AddToRoleAsync(member, "Member");
			if (!roleResult.Succeeded)
			{
				foreach (var err in roleResult.Errors)
				{
					ModelState.AddModelError("", err.Description);
					return View();
				}
			}
			await _signInManager.SignInAsync(member, isPersistent: false);
			return View("index","home");
		}
		public async Task<IActionResult> Login()
		{
			return View();
		}
	}
}
