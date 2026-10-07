using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : ControllerBase
    {
        private readonly LanguageService _languageService;

        public LanguageController()
        {
            _languageService = new LanguageService();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var languages = _languageService.GetAllLanguages();
            return Ok(languages);
        }

        [HttpGet("{code}")]
        public IActionResult GetByCode(string code)
        {
            var language = _languageService.GetLanguageByCode(code);

            if (language == null)
            {
                return NotFound();
            }

            return Ok(language);
        }
    }
}