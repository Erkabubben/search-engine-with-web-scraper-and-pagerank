using Microsoft.AspNetCore.Mvc;
using SearchEngineAPI.Models;
using SearchEngineAPI.Services;

namespace SearchEngineAPI.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    [Route("SearchEngine")]
    public class SearchEngineController : ControllerBase
    {
        private readonly SearchEngineService _searchEngineService;

        public SearchEngineController()
        {
            _searchEngineService = new SearchEngineService("wikipedia");
        }

        [HttpGet(Name = "Index")]
        public ActionResult<bool> Index()
        {
            return true;
        }

        // {baseURL}/api/recommendation/{methodName}
        [HttpPost][Route("Search")]
        public ActionResult<SearchResponse> Search(SearchRequest request)
        {
            return _searchEngineService.ExecuteSearchRequest(request);
        }
    }
}
