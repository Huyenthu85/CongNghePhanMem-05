using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingJobController : ControllerBase
    {
        private readonly ProcessingJobService _jobService;

        public ProcessingJobController(ProcessingJobService jobService)
        {
            _jobService = jobService;
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


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(
            long shopSubmissionId,
            int languageId)
        {
            var job = _jobService.CreateJob(
                shopSubmissionId,
                languageId);
            return Ok(job);
        }


        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/fail")]
        public IActionResult Fail(int id, string errorMessage)
        {
            var job = _jobService.FailJob(id, errorMessage);
            if (job == null)
            {
                return NotFound();
            }
            return Ok(job);
        }
    }
}
