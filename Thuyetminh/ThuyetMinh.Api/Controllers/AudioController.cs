
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AudioService _audioService;
        private readonly AppDbContext _db;
        public AudioController(AudioService audioService, AppDbContext db)
        {
            _audioService = audioService;
            _db = db;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var audios = _audioService.GetAllAudios();
            return Ok(audios.Select(a => new
            {
                a.Id,
                a.TranslationId,
                a.FilePath,
                a.DurationSeconds,
                a.Status
            }));
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var audio = _audioService.GetAudioById(id);
            if (audio == null) return NotFound();
            return Ok(new
            {
                audio.Id,
                audio.TranslationId,
                audio.FilePath,
                audio.DurationSeconds,
                audio.Status
            });
        }
        [HttpGet("translation/{translationId}")]
        public IActionResult GetByTranslationId(int translationId)
        {
            if (!_db.Translations.Any(t => t.Id == translationId && t.Status == "Success" &&
                t.Language.IsActive && t.ShopSubmission.Status == SubmissionStatus.Approved &&
                t.ShopSubmission.Shop.PublishStatus == PublishStatus.Approved &&
                t.ShopSubmission.Shop.PublishedVersionId == t.ShopSubmissionId))
                return NotFound();
            var audio = _audioService.GetAudioByTranslationId(translationId);
            if (audio == null || audio.Status != "Success") return NotFound();
            return Ok(new
            {
                audio.Id,
                audio.TranslationId,
                audio.FilePath,
                audio.DurationSeconds,
                audio.Status
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(int translationId, string filePath, int durationSeconds)
        {
            if (translationId <= 0)
                return BadRequest("ID bản dịch không hợp lệ.");
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("Đường dẫn audio không được để trống.");
            if (durationSeconds <= 0)
                return BadRequest("Thời lượng audio phải lớn hơn 0.");
            try
            {
                var audio = _audioService.CreateAudio(
                    translationId, filePath, durationSeconds);
                return Ok(new
                {
                    audio.Id,
                    audio.TranslationId,
                    audio.FilePath,
                    audio.DurationSeconds,
                    audio.Status
                });
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
