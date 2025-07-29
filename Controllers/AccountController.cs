using System.ComponentModel.DataAnnotations;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace E_commerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IEmailSender sendEmailService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager , IConfiguration configuration, IEmailSender sendEmailService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.sendEmailService = sendEmailService;
        }
        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("index", "home");
            } //34an lw ra7 my3rf4 ywsl ll action deh w hwa authenticated
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("index", "home");
            } //34an lw ra7 my3rf4 ywsl ll action deh w hwa authenticated

            //1. check model
            if (!ModelState.IsValid)
                return View(registerDTO);
            //2. create user by userManager
            //mapping 
            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                Address = registerDTO.Address,
                PasswordHash = registerDTO.Password,
                CreatedAt = DateTime.Now
            };
            var result = await userManager.CreateAsync(newUser , registerDTO.Password);
            //3 add role using rolemanger => sign in using signInManager
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, "Client");
                await signInManager.SignInAsync(newUser, false);
                return RedirectToAction("index","home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(registerDTO);
        }

        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User)) //check if user is authenticated user property mwgpoda fe controller
            {
                await signInManager.SignOutAsync();
            }
            return RedirectToAction("index","home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTo)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("index", "home");
            } //34an lw ra7 my3rf4 ywsl ll action deh w hwa authenticated
            if (!ModelState.IsValid) 
                return View(loginDTo);
            /*----method 1----*/
            var result = await signInManager.PasswordSignInAsync(loginDTo.Email , loginDTo.Password, loginDTo.RememberMe,false);
            if (result.Succeeded)
            {
                var currentUser= await userManager.GetUserAsync(User);
                var userRoles = await userManager.GetRolesAsync(currentUser);
                if (userRoles.Contains("admin"))
                {
                    return RedirectToAction("DashBoard", "AdminOrders");
                }
                return RedirectToAction("index", "home");
            }
            else
            {
                ViewBag.ErrorMessages = "Login attempt failed"; //bb3t data ll view
            }
            return View(loginDTo);

            /* method 2----
                var user = await userManager.FindByEmailAsync(loginDTo.Email);
                if (user == null)
                {
                    Exception ex = new Exception("user not found! check your email address");
                    ModelState.AddModelError("", ex.Message);
                    return View(loginDTo);
                }
                var checkPassword = await userManager.CheckPasswordAsync(user!, loginDTo.Password);
                if (checkPassword == true) {

                    await signInManager.SignInAsync(user, isPersistent: loginDTo.RememberMe);
                    return RedirectToAction("index", "home");
                }
                else
                {
                    Exception ex = new Exception("wrong password, please re-check your password");
                    ModelState.AddModelError("", ex.Message);
                    return View(loginDTo);
                }
            -------*/

        }

        [Authorize]  //lw user authenticated 
        public async Task<IActionResult> Profile()
        {
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("index", "home");
            }
            IList<string> userRoles = await userManager.GetRolesAsync(appUser!);
            ViewBag.UserRoles = userRoles;
            ViewBag.AppUser = appUser;
            //mapping appUser to userDTo to be used in EditForm
            ProfileDTO userDTO = new ProfileDTO()
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Address = appUser.Address,
                Email = appUser.Email ?? "", //34an Email m4 nullable 
                PhoneNumber = appUser.PhoneNumber
            };
            return View(userDTO);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDTO profileDTO)
        {
            
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("index", "home");
            }
            IList<string> userRoles = await userManager.GetRolesAsync(appUser!);
            ViewBag.UserRoles = userRoles;
            ViewBag.AppUser = appUser;
            //dah 34an ymla view bta3 profile m4 bta3 modal edit profile 
            
            //
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessages = "Please fill all required fields with valid values!";
                //34an modal bootstrap hidden by default
                return View(profileDTO);
            }

            appUser.FirstName = profileDTO.FirstName;
            appUser.LastName = profileDTO.LastName;
            appUser.UserName = profileDTO.Email; // unique identifier created by asp identity lib
            appUser.Address = profileDTO.Address;
            appUser.Email = profileDTO.Email;
            appUser.PhoneNumber = profileDTO.PhoneNumber;
            var result = await userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                ViewBag.SuccessMessages = "Profile updated successfully";
            }
            else
            {
                ViewBag.ErrorMessages = "Unable to update profile: "+ result.Errors.First().Description;
            }
            ViewBag.AppUser = appUser;
            //dah 34an ymla view bta3 profile m4 bta3 modal edit profile 

            return View(profileDTO);
        }

        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        //update password in profile
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Password(PasswordDTO passwordDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(passwordDTO);
            }
            
            var appUser = await userManager.GetUserAsync(User);
            if(appUser == null)
            {
                return RedirectToAction("index", "home");
            }

            var result = await userManager.ChangePasswordAsync(appUser, passwordDTO.CurrentPassword , passwordDTO.NewPassword);
            if (result.Succeeded)
            {
                ViewBag.SuccessMessages = "Password updated successfully";
                return View();
            }
            else
            {
                ViewBag.ErrorMessages = "Unable to update profile: " + result.Errors.First().Description;
                return View(passwordDTO);
            }
        }


        //begin forget password codes
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword([Required] [EmailAddress]  string email)
            //ezay t constrain data mn 8er model bdef attributes 3la paramter
        {
            //1-> check that user is not signedIn
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction(actionName:"index" , controllerName: "home");
            }

            //2-> check model state
                // w hnb3thoh ll view fel viewBag
            ViewBag.Email = email;

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState["email"]?.Errors.First().ErrorMessage ?? "Invalid Email address" ; //imp
                return View();
            }

            //3-> get user with the email & check if user exists -> eb3t ll view error fel viewBag
            var user= await userManager.FindByEmailAsync(email);
            if(user == null)
            {
                ViewBag.Errors = "User not found, please Recheck the email you entered.";
                return View();
            }

            //4-> lw kloh tmam generate token w bghz url ely hb3toh ll email
            var token= await userManager.GeneratePasswordResetTokenAsync(user);
            string resetUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id,token=token }, 
                protocol: Request.Scheme, host: Request.Host.Value) ?? "";  //imp

            /*sending email*/
            string emailSubject = "password reset";
            string recieverEMail = user.Email ?? "";
            string htmlContent = @$"
                  <div style=""font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;"">
                    <div style=""max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; border: 1px solid #ddd;"">
                      <h2 style=""color: #5DADE2;"">Reset Your Password</h2>
                      <p style=""font-size: 16px; color: #333;"">
                        Hi there,<br /><br />
                        We received a request to reset your password. Click the button below to reset it:
                      </p>
                      <div style=""text-align: center; margin: 30px 0;"">
                        <a href='{resetUrl}' 
                           style=""background-color: #5DADE2; color: white; padding: 12px 25px; text-decoration: none; font-weight: bold; border-radius: 5px;"">
                          Reset Password
                        </a>
                      </div>
                      <p style=""font-size: 14px; color: #666;"">
                        If you didn’t request this, you can safely ignore this email.
                      </p>
                    </div>
                  </div>";
            try
            {
                await sendEmailService.SendEmailAsync(recieverEMail, emailSubject, htmlContent);
                ViewBag.SuccessMessages = "Please check your email account and click on password reset link";
                return View();
            }catch(Exception ex)
            {
                ViewBag.Errors = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid password reset token");
            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.Error = "Invalid Token";
                return View(model);
            }

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        public IActionResult AccessDenied()
        {
            return RedirectToAction("index", "home");
        }

    }
}
