using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using SitefinityWebApp.Mvc.Models;
using SitefinityWebApp.Extentions;
using SitefinityWebApp.Extentions.OneLogin;
using Telerik.Sitefinity.Security;

// ReSharper disable Mvc.ViewNotResolved

namespace SitefinityWebApp.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "Login", Title = "Login", SectionName = "Account")]
    public class LoginController : Controller
    {
        
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            // TODO: Clear the existing external cookie to ensure a clean login process

            var model = new LoginViewModel();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel loginModel)
        {
            OidcProvider oidcProvider = new OidcProvider();

            // TODO: provide proper success and error page urls through widget properties 
            var successRedirectUrl = "http://localhost:60876/";
            var errorRedirectUrl = "http://localhost:60876/";
            
            // Get access token
            var token = oidcProvider.LoginUser(loginModel.Username, loginModel.Password);

            if (!string.IsNullOrEmpty(token.AccessToken))
            {
                // Get user information
                var oidcUser = oidcProvider.GetUserInfo(token.AccessToken);

                if (oidcUser != null)
                {
                    UserService service = new UserService();
                    var sitefinityUser = service.Authenticate(oidcUser);

                    if (sitefinityUser != null)
                    {
                        var reason = SecurityManager.SkipAuthenticationAndLogin(sitefinityUser.ProviderName,
                            sitefinityUser.UserName, true, successRedirectUrl, errorRedirectUrl);
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Login failed");
            return View(loginModel);
        }
    }
}