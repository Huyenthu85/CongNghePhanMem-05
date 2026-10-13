using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AudioService _audioService;

        public AudioController()
        {
            _audioService = new AudioService();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var audios = _audioService.GetAllAudios();
            return Ok(audios);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var audio = _audioService.GetAudioById(id);

            if (audio == null)
            {
                return NotFound();
            }

            return Ok(audio);
        }

        [HttpGet("translation/{translationId}")]
        public IActionResult GetByTranslationId(int translationId)
        {
            var audio = _audioService.GetAudioByTranslationId(translationId);

            if (audio == null)
            {
                return NotFound();
            }

            return Ok(audio);
        }

        [HttpPost]
        public IActionResult Create(
            int translationId,
            string filePath,
            int durationSeconds)
        {
            var audio = _audioService.CreateAudio(
                translationId,
                filePath,
                durationSeconds);

            return Ok(audio);
        }
    }
}