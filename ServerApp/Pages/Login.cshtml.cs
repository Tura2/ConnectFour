using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerApp.Models;
using ServerApp.Data;

namespace ServerApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ConnectFourContext _context;

        public LoginModel(ConnectFourContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int PlayerId { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var player = _context.Players.FirstOrDefault(p => p.Id == PlayerId);
            if (player == null)
            {
                ErrorMessage = "Player not found. Please register first.";
                return Page();
            }

            // TODO: אפשר לשמור את השחקן ב-Session או להמשיך למשחק
            return RedirectToPage("Welcome", new { id = PlayerId });
        }
    }
}
