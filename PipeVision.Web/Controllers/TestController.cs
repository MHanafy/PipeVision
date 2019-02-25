using System;
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
        public async Task<IActionResult> Index()
        {
            var cacheKey = $"{nameof(TestController)}.{nameof(Index)}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Test> failingTests))
            {
                failingTests = await _pipelineService.GetFailingTests(DateTime.Today.AddDays(-30));
                _cache.Set(cacheKey, failingTests, TimeSpan.FromMinutes(5));
            }
             
            return View(_mapper.Map<IEnumerable<TestViewModel>>(failingTests).OrderByDescending(x=>x.StartTime));
        }

        public async Task<IActionResult> FailedRuns(string testName)
        {
            var testRuns = await _pipelineService.GetTestFailures(HttpUtility.UrlDecode(testName));
            return View(_mapper.Map<TestDetailedViewModel>(testRuns));
        }
    }
}