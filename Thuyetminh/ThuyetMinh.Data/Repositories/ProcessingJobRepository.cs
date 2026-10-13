
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Data.Repositories
{
    public class ProcessingJobRepository
    {
        private readonly AppDbContext _context;
        public ProcessingJobRepository(AppDbContext context)
        {
            _context = context;
        }
        public List<ProcessingJob> GetAll()
        {
            return _context.ProcessingJobs.AsNoTracking().ToList();
        }
        public ProcessingJob? GetById(int id)
        {
            return _context.ProcessingJobs.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }
        public ProcessingJob Add(ProcessingJob job)
        {
            _context.ProcessingJobs.Add(job);
            _context.SaveChanges();
            return job;
        }
        public ProcessingJob? UpdateStatus(int id, string status, string? errorMessage = null)
        {
            var job = _context.ProcessingJobs.FirstOrDefault(x => x.Id == id);
            if (job == null)
                return null;
            job.Status = status;
            job.ErrorMessage = errorMessage;
            if (status == "Success" || status == "Failed")
                job.CompletedAt = DateTime.UtcNow;
            else
                job.CompletedAt = null;
            _context.SaveChanges();
            return job;
        }
    }
}
