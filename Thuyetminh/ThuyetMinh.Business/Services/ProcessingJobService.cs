using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services
{
    public class ProcessingJobService
    {
        private readonly ProcessingJobRepository _jobRepository;

        public ProcessingJobService()
        {
            _jobRepository = new ProcessingJobRepository();
        }

        public List<ProcessingJob> GetAllJobs()
        {
            return _jobRepository.GetAll();
        }

        public ProcessingJob? GetJobById(int id)
        {
            return _jobRepository.GetById(id);
        }

        public ProcessingJob CreateJob(
            int poiId,
            int versionId,
            int languageId)
        {
            var job = new ProcessingJob
            {
                PoiId = poiId,
                VersionId = versionId,
                LanguageId = languageId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return _jobRepository.Add(job);
        }

        public ProcessingJob? StartJob(int id)
        {
            return _jobRepository.UpdateStatus(
                id,
                "Processing");
        }

        public ProcessingJob? CompleteJob(int id)
        {
            return _jobRepository.UpdateStatus(
                id,
                "Success");
        }

        public ProcessingJob? FailJob(
            int id,
            string errorMessage)
        {
            return _jobRepository.UpdateStatus(
                id,
                "Failed",
                errorMessage);
        }
    }
}