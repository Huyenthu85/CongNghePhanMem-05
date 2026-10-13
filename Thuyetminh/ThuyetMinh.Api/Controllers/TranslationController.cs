using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;

        public TranslationController(TranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var translations = _translationService.GetAllTranslations();
            return Ok(translations);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var translation = _translationService.GetTranslationById(id);
            if (translation == null)
            {
                return NotFound();
            }
            return Ok(translation);
        }

        [HttpGet("search")]
        public IActionResult GetTranslation(
            long shopSubmissionId,
            int languageId)
        {
            var translation = _translationService.GetTranslation(
                shopSubmissionId,
                languageId);
            if (translation == null)
            {
                return NotFound();
            }
            return Ok(translation);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(
            long shopSubmissionId,
            int languageId,
            string translatedText)
        {
            var translation = _translationService.CreateTranslation(
                shopSubmissionId,
                languageId,
                translatedText);
            return Ok(translation);
        }
    }
}
