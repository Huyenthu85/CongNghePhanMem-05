using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ThuyetMinh.Api.Pages.Payment
{
    public class CheckoutModel : PageModel
    {
        public long AmountVnd { get; } = 20000;
        public long GrantedSeconds { get; } = 900;
        public void OnGet()
        {
        }
    }
}