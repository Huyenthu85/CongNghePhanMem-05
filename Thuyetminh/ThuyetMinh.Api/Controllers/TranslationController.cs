
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data;
namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly TranslationService _translationService;
        private readonly AppDbContext _db;
        public TranslationController(TranslationService translationService, AppDbContext db)
        {
            _translationService = translationService;
            _db = db;
        }
        private static object ToResponse(Translation translation)
        {
            return new
            {
                translation.Id,
                translation.ShopSubmissionId,
                translation.LanguageId,
                translation.TranslatedText,
                translation.Status
            };
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var translations = _translationService.GetAllTranslations();
            return Ok(translations.Select(t => ToResponse(t)));
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var translation = _translationService.GetTranslationById(id);
            if (translation == null)
            {
                return NotFound();
            }
            return Ok(ToResponse(translation));
        }
        [HttpGet("search")]
        public IActionResult GetTranslation(long shopSubmissionId, int languageId)
        {
            if (!_db.ShopSubmissions.Any(s => s.Id == shopSubmissionId &&
                s.Status == SubmissionStatus.Approved && s.Shop.PublishStatus == PublishStatus.Approved &&
                s.Shop.PublishedVersionId == s.Id) ||
                !_db.Languages.Any(l => l.Id == languageId && l.IsActive))
                return NotFound();
            var translation = _translationService.GetTranslation(shopSubmissionId, languageId);
            if (translation == null || translation.Status != "Success")
            {
                return NotFound();
            }
            return Ok(ToResponse(translation));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(long shopSubmissionId, int languageId, string translatedText)
        {
            if (shopSubmissionId <= 0 || languageId <= 0)
            {
                return BadRequest("ID hồ sơ hoặc ngôn ngữ không hợp lệ.");
            }
            if (string.IsNullOrWhiteSpace(translatedText))
            {
                return BadRequest("Nội dung bản dịch không được để trống.");
            }
            try
            {
                var translation = _translationService.CreateTranslation(shopSubmissionId, languageId, translatedText);
                return Ok(ToResponse(translation));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
