using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;

        public TranslationController()
        {
            _translationService = new TranslationService();
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
            int poiId,
            int versionId,
            int languageId)
        {
            var translation = _translationService.GetTranslation(
                poiId,
                versionId,
                languageId);

            if (translation == null)
            {
                return NotFound();
            }

            return Ok(translation);
        }

        [HttpPost]
        public IActionResult Create(
            int poiId,
            int versionId,
            int languageId,
            string translatedText)
        {
            var translation = _translationService.CreateTranslation(
                poiId,
                versionId,
                languageId,
                translatedText);

            return Ok(translation);
        }
    }
}
