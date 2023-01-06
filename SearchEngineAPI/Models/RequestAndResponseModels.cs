using SearchEngineAPI.Services;

namespace SearchEngineAPI.Models
{
    public class SearchRequest
    {
        public string Query { get; set; }
        public int MaxAmount { get; set; }
    }

    public class SearchResponse
    {
        public class ResponsePage
        {
            private string _pageName;
            private double _contentScore;
            private double _locationScore;
            private double _pageRankScore;
            private double _finalScore;

            public string PageName { get => _pageName; set => _pageName = value; }
            public double ContentScore { get => _contentScore; set => _contentScore = value; }
            public double LocationScore { get => _locationScore; set => _locationScore = value; }
            public double PageRankScore { get => _pageRankScore; set => _pageRankScore = value; }
            public double FinalScore { get => _finalScore; set => _finalScore = value; }
        }

        public List<ResponsePage> Pages { get; set; }

        public SearchResponse(List<ResponsePage> responsePageList, int maxAmount = -1)
        {
            List<T> CutList<T>(List<T> originalList, int maxAmount)
            {
                var newList = new List<T>();
                for (int i = 0; i < Math.Min(originalList.Count, maxAmount); i++)
                    newList.Add(originalList[i]);
                return newList;
            }
            Pages = (maxAmount == -1) ? responsePageList : CutList<ResponsePage>(responsePageList, maxAmount);
        }
    }
}
