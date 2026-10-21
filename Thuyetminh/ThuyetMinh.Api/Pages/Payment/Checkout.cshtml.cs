using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
namespace ThuyetMinh.Api.Pages.Payment
{
    public class CheckoutModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        public CheckoutModel(IWebHostEnvironment environment) => _environment = environment;
        public long AmountVnd { get; } = 20000;
        public long GrantedSeconds { get; } = 900;
        public bool ShowDemoControls => _environment.IsDevelopment()
            && User.IsInRole("Admin")
            && ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext);
        public void OnGet() { }
    }
}
