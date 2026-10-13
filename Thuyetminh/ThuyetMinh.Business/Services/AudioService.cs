
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services
{
    public class AudioService
    {
        private readonly AudioRepository _audioRepository;
        public AudioService(AudioRepository audioRepository)
        {
            _audioRepository = audioRepository;
        }
        public List<Audio> GetAllAudios()
        {
            return _audioRepository.GetAll();
        }
        public Audio? GetAudioById(int id)
        {
            return _audioRepository.GetById(id);
        }
        public Audio? GetAudioByTranslationId(int translationId)
        {
            return _audioRepository.GetByTranslationId(translationId);
        }
        public Audio CreateAudio(int translationId, string filePath, int durationSeconds)
        {
            var audio = new Audio
            {
                TranslationId = translationId,
                FilePath = filePath,
                DurationSeconds = durationSeconds,
                Status = "Success"
            };
            return _audioRepository.Add(audio);
        }
    }
}
