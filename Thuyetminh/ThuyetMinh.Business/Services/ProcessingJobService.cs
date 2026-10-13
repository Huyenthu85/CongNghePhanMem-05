
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Business.Services
{
    public class ProcessingJobService
    {
        private readonly ProcessingJobRepository _jobRepository;
        public ProcessingJobService(ProcessingJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public List<ProcessingJob> GetAllJobs()
        {
            return _jobRepository.GetAll();
        }
        public ProcessingJob? GetJobById(int id)
        {
            return _jobRepository.GetById(id);
        }
        public ProcessingJob CreateJob(long shopSubmissionId, int languageId)
        {
            var job = new ProcessingJob
            {
                ShopSubmissionId = shopSubmissionId,
                LanguageId = languageId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            return _jobRepository.Add(job);
        }

        public ProcessingJob? StartJob(int id)
        {
            var job = _jobRepository.GetById(id);
            if (job == null)
                return null;
            if (job.Status != "Pending")
                throw new InvalidOperationException("Chi co the bat dau Job dang Pending.");
            return _jobRepository.UpdateStatus(id, "Processing");
        }
        public ProcessingJob? CompleteJob(int id)
        {
            var job = _jobRepository.GetById(id);
            if (job == null)
                return null;
            if (job.Status != "Processing")
                throw new InvalidOperationException("Chi co the hoan thanh Job dang Processing.");
            return _jobRepository.UpdateStatus(id, "Success");
        }
        public ProcessingJob? FailJob(int id, string errorMessage)
        {
            var job = _jobRepository.GetById(id);
            if (job == null)
                return null;
            if (job.Status != "Processing")
                throw new InvalidOperationException("Chi co the bao loi Job dang Processing.");
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException("Phai cung cap ly do xu ly that bai.");
            return _jobRepository.UpdateStatus(id, "Failed", errorMessage.Trim());
        }

    }
}
