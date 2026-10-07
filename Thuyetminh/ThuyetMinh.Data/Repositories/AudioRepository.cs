using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class AudioRepository
    {
        private static readonly List<Audio> _audios = new();

        public List<Audio> GetAll()
        {
            return _audios;
        }

        public Audio? GetById(int id)
        {
            return _audios.FirstOrDefault(x => x.Id == id);
        }

        public Audio? GetByTranslationId(int translationId)
        {
            return _audios.FirstOrDefault(
                x => x.TranslationId == translationId);
        }

        public Audio Add(Audio audio)
        {
            audio.Id = _audios.Count + 1;
            _audios.Add(audio);

            return audio;
        }
    }
}