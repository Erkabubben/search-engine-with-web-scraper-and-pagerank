using System;
using System.Text;
using System.Diagnostics;

class Program
{
    private static string appFolderPath = PathGetDirectoryNameTimes(4, AppDomain.CurrentDomain.BaseDirectory);

    private static string PathGetDirectoryNameTimes(int times, string s)
    {
        for (int i = 0; i < times; i++)
            s = Path.GetDirectoryName(s);
        return s;
    }

    private static Program? _instance;
    private PageDB _pageDB;

    static void Main(string[] args)
    {
        _instance = new Program(args);
    }

    public Program(string[] args)
    {
        ReadDatasets("wikipedia");
        CalculatePageRank(20);
        //_pageDB.PrintContentsOfPages(new string[] { "Action_game" });
        var results = Query(
        //    "java"
        //    "super mario"
        "java programming"
        //    "?????"
        );
        PrintSearchResults(results);
    }

    private void PrintSearchResults(List<PageWithScore> results)
    {
        Console.WriteLine($"Search results:");
        for (int i = 0; i < results.Count; i++)
        {
            var fs = Math.Round(results[i].FinalScore, 2);
            var cs = Math.Round(results[i].ContentScore, 2);
            var ls = Math.Round(results[i].LocationScore, 2);
            var prs = Math.Round(results[i].PageRankScore, 2);
            Console.WriteLine($"\t{i}. {results[i].Page.Url}\t\t\tFS:{fs}\tCS:{cs}\tLS:{ls}\tPRS:{prs}");
        }
    }

    private class PageWithScore
    {
        private Page _page;
        private double _contentScore;
        private double _locationScore;
        private double _pageRankScore;
        private double _finalScore;

        public Page Page { get => _page; set => _page = value; }
        public double ContentScore { get => _contentScore; set => _contentScore = value; }
        public double LocationScore { get => _locationScore; set => _locationScore = value; }
        public double PageRankScore { get => _pageRankScore; set => _pageRankScore = value; }
        public double FinalScore { get => _finalScore; set => _finalScore = value; }

        public PageWithScore(Page page)
        {
            _page = page;
        }
    }

    private void CalculatePageRank(int maxIterations)
    {
        // Iterate over all pages for a number of iterations
        for (int i = 0; i < maxIterations; i++)
        {
            foreach (var page in _pageDB.Pages)
            {
                IteratePageRank(page);
            }
        }
    }

    private void IteratePageRank(Page p)
    {
        // Calculate PageRank value for a page
        double pr = 0;
        foreach (var po in _pageDB.Pages)
        {
            if (po.HasLinkTo(p))
            {
                // Sum of all pages
                pr += po.PageRank / po.Links.Count;
            }
        }
        // Calculate PR
        p.PageRank = 0.85 * pr + 0.15;
    }

    private List<PageWithScore> Query(string query, bool usePageRank = true)
    {
        //var results = new List<Page>();
        var contentScores = new double[_pageDB.Pages.Count];
        var locationScores = new double[_pageDB.Pages.Count];
        var pageRankScores = new double[_pageDB.Pages.Count];
        var results = new List<PageWithScore>();
        var queryWordInts = GetQueryAsWordInts(query);

        // Calculate score for each page in the pages database
        for (int i = 0; i < _pageDB.Pages.Count; i++)
        {
            Page page = _pageDB.Pages[i];
            contentScores[i] = GetFrequencyScore(page, queryWordInts);
            locationScores[i] = GetLocationScore(page, queryWordInts);
            pageRankScores[i] = page.PageRank;
        }

        // Normalize scores
        Normalize(contentScores, false);
        Normalize(locationScores, true);
        Normalize(pageRankScores, false);

        // Generate results list
        for (int i = 0; i < _pageDB.Pages.Count; i++)
        {
            Page page = _pageDB.Pages[i];
            var pageWithScore = new PageWithScore(page);
            pageWithScore.ContentScore = contentScores[i];
            pageWithScore.LocationScore = contentScores[i] > 0 ? 0.8 * locationScores[i] : 0;
            pageWithScore.PageRankScore = pageRankScores[i] * 0.5;
            if (usePageRank)
                pageWithScore.FinalScore = pageWithScore.ContentScore + pageWithScore.LocationScore + pageWithScore.PageRankScore;
            else
                pageWithScore.FinalScore = pageWithScore.ContentScore + pageWithScore.LocationScore;
            results.Add(pageWithScore);
        }

        // Sort results list with highest score first
        results.Sort((a, b) => (b.FinalScore.CompareTo(a.FinalScore)));

        // Return results list
        return results;
    }

    private void Normalize(double[] scores, bool smallIsBetter)
    {
        if (smallIsBetter)
        {
            // Smaller values should be inverted to higher values
            // and scaled between 0 and 1
            // Find min value in the array
            double min = scores.Min();
            // divide the min value by the score, avoiding division by zero
            for (int i = 0; i < scores.Length; i++)
                scores[i] = min / Math.Max(scores[i], 0.00001);
        }
        else
        {
            // Higher values should be scaled between 0 and 1
            // Find max value in the array
            double max = Math.Max(scores.Max(), 0.00001);
            // Divide all scores by max value
            for (int i = 0; i < scores.Length; i++)
                scores[i] = scores[i] / max;
        }
    }

    private double GetLocationScore(Page page, int[] queryWordInts)
    {
        double score = -1;
        for (int i = 0; i < queryWordInts.Length; i++)
        {
            if (score == -1)
                score = GetLocationScoreForWordInt(page, queryWordInts[i]);
            else
                score += GetLocationScoreForWordInt(page, queryWordInts[i]);
        }
        return score;
    }

    private double GetWordDistanceScore(Page page, int[] queryWordInts)
    {
        var score = 0;
        for (int i = 0; i < page.Words.Count; i++)
        {

        }
        return score;
    }

    private double GetLocationScoreForWordInt(Page page, int wordInt)
    {
        if (!page.WordFrequencies.ContainsKey(wordInt))
            return 100000;

        var score = 0;
        for (int i = 0; i < page.Words.Count; i++)
        {
            if (page.Words[i] == wordInt)
            {
                score = i;
                break;
            }
        }
        return score;
    }

    private double GetFrequencyScore(Page page, int[] queryWordInts)
    {
        var score = 0;
        foreach (var wordInt in queryWordInts)
        {
            if (page.WordFrequencies.ContainsKey(wordInt))
                score += page.WordFrequencies[wordInt];
        }
        return score;
    }

    private int[] GetQueryAsWordInts(string query)
    {
        var splitQuery = query.Split(' ');
        var wordInts = new int[splitQuery.Length];
        for (int i = 0; i < splitQuery.Length; i++)
        {
            //wordInts[i] = _pageDB.GetIdForWord(splitQuery[i]);
            wordInts[i] = _pageDB.WordToId.ContainsKey(splitQuery[i]) ? _pageDB.WordToId[splitQuery[i]] : -1;
        }
        return wordInts;
    }

    public class PageDB
    {
        private Dictionary<string, int> _wordToId;
        private Dictionary<int, string> _idToWord;
        private List<Page> _pages;

        public Dictionary<string, int> WordToId { get => _wordToId; set => _wordToId = value; }
        public List<Page> Pages { get => _pages; set => _pages = value; }
        public Dictionary<int, string> IdToWord { get => _idToWord; set => _idToWord = value; }

        public PageDB()
        {
            _wordToId = new Dictionary<string, int>();
            _idToWord = new Dictionary<int,string>();
            _pages = new List<Page>();
        }

        public int GetIdForWord(string word)
        {
            if (_wordToId.ContainsKey(word))
                // Word found in Dictionary.
                return _wordToId[word];
            else
            {
                // Add missing word to Dictionary.
                int id = _wordToId.Count;
                _wordToId.Add(word, id);
                _idToWord.Add(id, word);
                return id;
            }
        }

        public void PrintIdToWord()
        {
            Console.WriteLine($"Printing contents of IdToWord:");
            foreach (var keyvalpair in IdToWord)
                Console.WriteLine($"\t {keyvalpair.Key} : {keyvalpair.Value}");
        }

        public void PrintWordToId()
        {
            Console.WriteLine($"Printing contents of WordToId:");
            foreach (var keyvalpair in WordToId)
                Console.WriteLine($"\t {keyvalpair.Key} : {keyvalpair.Value}");
        }

        public void PrintContentsOfPages(string[] pageNames)
        {
            foreach (var pageName in pageNames)
            {
                var page = Pages.Find(searchedPage => searchedPage.Url == pageName);
                if (page != null)
                {
                    Console.WriteLine($"Printing Words of page '{pageName}':");
                    foreach (var wordInt in page.Words)
                    {
                        var word = IdToWord[wordInt];
                        Console.WriteLine($"\t {word}");
                    }
                    Console.WriteLine($"Printing Links of page '{pageName}':");
                    foreach (var link in page.Links)
                        Console.WriteLine($"\t {link}");
                }
                else
                    Console.WriteLine($"Page '{pageName}' was not found.");
            }
        }
    }

    public class Page
    {
        private string _url;
        private List<int> _words;
        private Dictionary<int, int> _wordFrequencies;
        private List<string> _links;
        private double _pageRank = 1.0;
        public string Url { get => _url; set => _url = value; }
        public List<int> Words { get => _words; set => _words = value; }
        public Dictionary<int, int> WordFrequencies { get => _wordFrequencies; set => _wordFrequencies = value; }
        public List<string> Links { get => _links; set => _links = value; }
        public double PageRank { get => _pageRank; set => _pageRank = value; }

        public Page(string url, List<int> words)
        {
            _url = url;
            _words = words;
            _wordFrequencies = CreateWordFrequenciesDictionary(_words);
        }

        private Dictionary<int, int> CreateWordFrequenciesDictionary(List<int> words)
        {
            var dictionary = new Dictionary<int, int>();
            foreach (var wordInt in words)
            {
                if (dictionary.ContainsKey(wordInt))
                    dictionary[wordInt]++;
                else
                    dictionary[wordInt] = 1;
            }
            return dictionary;
        }

        public bool HasLinkTo(Page p) => _links.Contains("/wiki/" + p.Url);
    }

    void ReadDatasets(string nameOfDataset)
    {
        List<int> ReadBagOfWords(string path)
        {
            void AddWord(List<int> list, StringBuilder builder)
            {
                int id = _pageDB.GetIdForWord(builder.ToString());
                list.Add(id);
                builder.Clear();
            }

            using (var reader = new StreamReader(path))
            {
                var list = new List<int>();
                var builder = new StringBuilder();
                while (!reader.EndOfStream)
                {
                    if ((char)reader.Peek() == ' ' && builder.Length > 0)
                    {
                        AddWord(list, builder);
                        var discardChar = reader.Read();
                    }
                    else
                        builder.Append((char)reader.Read());
                }
                if (builder.Length > 0)
                    AddWord(list, builder);
                return list;
            }
        }

        List<string> ReadLinks(string linksFile)
        {
            using (var reader = new StreamReader(linksFile))
            {
                var list = new List<string>();
                while (!reader.EndOfStream)
                    list.Add(reader.ReadLine());

                return list;
            }
        }

        _pageDB = new PageDB();
        
        var wordsDirectories = Directory.GetDirectories(appFolderPath + @$"\datasets\{nameOfDataset}\Words");
        foreach (var directory in wordsDirectories)
        {
            var files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                var page = new Page(Path.GetFileName(file), ReadBagOfWords(file));
                var linksFile = $"{PathGetDirectoryNameTimes(3, file)}\\Links\\"
                    + $"{Path.GetFileName(Path.GetDirectoryName(file))}\\{Path.GetFileName(file)}";
                if (File.Exists(linksFile))
                    page.Links = ReadLinks(linksFile);

                _pageDB.Pages.Add(page);
            }
        }
    }
}
