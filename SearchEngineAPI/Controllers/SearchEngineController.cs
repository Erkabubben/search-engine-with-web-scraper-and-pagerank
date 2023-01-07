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
        // TODO: This is just a temporary solution to avoid a new service being created every time
        // a new controller is instantiated on a request. Find out how to register service with
        // the dependency injection system rather than storing it as a static!
        private static SearchEngineService _searchEngineService;
        public static void SearchEngineServiceSetup()
        {
            if (_searchEngineService == null)
                _searchEngineService = new SearchEngineService("wikipedia");
        }

        public SearchEngineController()
        {
            SearchEngineServiceSetup();
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
