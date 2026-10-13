using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingJobController : ControllerBase
    {
        private readonly ProcessingJobService _jobService;

        public ProcessingJobController()
        {
            _jobService = new ProcessingJobService();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var jobs = _jobService.GetAllJobs();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var job = _jobService.GetJobById(id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        [HttpPost]
        public IActionResult Create(
            int poiId,
            int versionId,
            int languageId)
        {
            var job = _jobService.CreateJob(
                poiId,
                versionId,
                languageId);

            return Ok(job);
        }

        [HttpPut("{id}/start")]
        public IActionResult Start(int id)
        {
            var job = _jobService.StartJob(id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        [HttpPut("{id}/complete")]
        public IActionResult Complete(int id)
        {
            var job = _jobService.CompleteJob(id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        [HttpPut("{id}/fail")]
        public IActionResult Fail(
            int id,
            string errorMessage)
        {
            var job = _jobService.FailJob(
                id,
                errorMessage);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }
    }
}