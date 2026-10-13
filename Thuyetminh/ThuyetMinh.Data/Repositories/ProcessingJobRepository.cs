using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class ProcessingJobRepository
    {
        private static readonly List<ProcessingJob> _jobs = new();

        public List<ProcessingJob> GetAll()
        {
            return _jobs;
        }

        public ProcessingJob? GetById(int id)
        {
            return _jobs.FirstOrDefault(x => x.Id == id);
        }

        public ProcessingJob Add(ProcessingJob job)
        {
            job.Id = _jobs.Count + 1;
            _jobs.Add(job);

            return job;
        }

        public ProcessingJob? UpdateStatus(
            int id,
            string status,
            string? errorMessage = null)
        {
            var job = GetById(id);

            if (job == null)
            {
                return null;
            }

            job.Status = status;
            job.ErrorMessage = errorMessage;

            if (status == "Success" || status == "Failed")
            {
                job.CompletedAt = DateTime.UtcNow;
            }

            return job;
        }
    }
}