using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scrabble.Web.Server.Pages.Auth
{

    public class LoginModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string returnUrl="/")
        {
            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
            return Content("");
        }

    }
}
