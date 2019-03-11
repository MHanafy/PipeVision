using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PipeVision.Domain;
using PipeVision.Web.Models;

namespace PipeVision.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly IPipelineService _pipelineService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public TestController(IPipelineService pipelineService, IMapper mapper, IMemoryCache cache)
        {
            _pipelineService = pipelineService;
            _mapper = mapper;
            _cache = cache;
        }
        public IActionResult Index()
        {
            //Failing tests are populated by the CacheHelper
            var failingTests = _cache.Get(CacheHelper.FailingTestsKey) ?? new List<Test>();
            return View(_mapper.Map<IEnumerable<TestViewModel>>(failingTests).OrderByDescending(x=>x.StartTime));
        }

        public async Task<IActionResult> FailedRuns(string testName)
        {
            var testRuns = await _pipelineService.GetUniqueTestFailures(HttpUtility.UrlDecode(testName));
            if (testRuns.Any()) return View(_mapper.Map<TestDetailedViewModel>(testRuns));

            var lastSuccessDate = await _pipelineService.GetLastSuccessDate(testName);
            //If neither failures not successes were found, then a non existent test name was provided.
            if (lastSuccessDate == null) return NotFound();

            ViewBag.LastSuccessDate = lastSuccessDate;
            ViewBag.TestName = testName;
            return View(_mapper.Map<TestDetailedViewModel>(testRuns));
        }
    }
}