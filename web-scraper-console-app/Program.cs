using System;
using System.Text;
using System.Diagnostics;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

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
    private HttpClient _httpClient;
    private int _pagesGathered = 0;
    private string _baseUrl = "https://en.wikipedia.org/";

    static void Main(string[] args)
    {
        _instance = new Program(args);
    }

    public Program(string[] args)
    {
        //var testdoc = GetDocument(baseUrl + "Dark_triad");
        _httpClient = new HttpClient();
        /*var testContentNode = GetWikipediaContentNode(_baseUrl + "wiki/" + "Dark_triad");
        Console.WriteLine(GetWordsFromContentNode(testContentNode));
        var links = GetWikipediaLinksFromContentNode(testContentNode);
        Console.WriteLine("Amount of links: " + links.Count);
        foreach (var link in links)
            Console.WriteLine(link);*/
        
        //Console.WriteLine($"{jObject["query"]["categorymembers"].ToString()}\n");
        //Console.WriteLine($"{jsonResponse}\n");
        /*_pagesGathered = 0;
        var pages = GetCategoryPages(new List<string>(), "Category:Psychology", 2000, 1);
        _pagesGathered = 0;
        foreach (var page in pages)
            Console.WriteLine(page);

        Console.WriteLine("Amount of pages: " + pages.Count);
        Console.WriteLine(GetDocument(baseUrl + pages[0]).Text);*/
        CreateLinksAndWordsCollectionsFromPage("Artificial_intelligence", "Artificial_intelligence", 200);
    }

    private void CreateLinksAndWordsCollectionsFromPage(string collectionName, string pageUrl, int maxPages = -1)
    {
        var startPageContentNode = GetWikipediaContentNode(_baseUrl +"wiki/" + pageUrl);
        if (startPageContentNode == null)
            return;

        var wordsFolder = appFolderPath + @$"\datasets\wikipedia\Words\" + collectionName;
        var linksFolder = appFolderPath + @$"\datasets\wikipedia\Links\" + collectionName;
        if (Directory.Exists(wordsFolder))
            Directory.Delete(wordsFolder, true);
        Directory.CreateDirectory(wordsFolder);
        if (Directory.Exists(linksFolder))
            Directory.Delete(linksFolder, true);
        Directory.CreateDirectory(linksFolder);

        var startPageLinksList = GetWikipediaLinksFromContentNode(startPageContentNode);
        Console.WriteLine($"Amount of pages to be added to collection '{collectionName}': " + startPageLinksList.Count);

        int currentPageID = 0;
        foreach (var link in startPageLinksList)
        {
            var pageContentNode = GetWikipediaContentNode(_baseUrl + link);
            var wordsString = GetWordsFromContentNode(pageContentNode);
            var links = GetWikipediaLinksFromContentNode(pageContentNode);
            string linksString = string.Join("\n", links);
            string filename = link.StartsWith("/wiki/") ? link.Substring("/wiki/".Length) : link;
            Console.WriteLine($"Adding page {currentPageID} '{filename}'...");
            //filename = Regex.Replace(filename, "[^a-zA-Z0-9 _]", " ");
            File.WriteAllText(wordsFolder + '\\' + filename, wordsString);
            File.WriteAllText(linksFolder + '\\' + filename, linksString);
            currentPageID++;
            if (maxPages > -1 && currentPageID > maxPages)
                break;
        }
    }

    private string GetWordsFromContentNode(HtmlNode contentNode)
    {
        string RemoveSpaces(string text) => Regex.Replace(text, @"\s+", " ");
        string ReplaceSpecialCharactersWithSpaces(string text) =>
            Regex.Replace(text, "[^a-zA-Z0-9 ]", " ");

        void GetInnerHTMLFromChildNodes(StringBuilder sb, HtmlNode node)
        {
            if (node.InnerText == null)
                return;

            var text = ReplaceSpecialCharactersWithSpaces(node.InnerText.ToLower());
            text = RemoveSpaces(text);
            sb.Append(text.Trim() + " ");
        }

        StringBuilder sb = new StringBuilder();
        GetInnerHTMLFromChildNodes(sb, contentNode);
        while (sb[0] == ' ')
            sb.Remove(0, 1);
        return RemoveSpaces(sb.ToString());
    }

    private HtmlNode GetWikipediaContentNode(string url)
    {
        var doc = GetDocument(url);
        if (doc == null)
            return null;

        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//body/div[@id='content']");
        return nodes[0];
    }

    private HtmlDocument GetDocument(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        return doc;
    }

    private List<string> GetWikipediaLinksFromContentNode(HtmlNode contentNode)
    {
        var nodes = contentNode.SelectNodes("//a");
        var links = new List<string>();
        if (nodes == null)
            return links;

        foreach (var node in nodes)
        {
            if (!node.Attributes.Contains("href"))
                continue;
            string href = node.Attributes[name: "href"].Value;
            if (!href.StartsWith("/wiki/") || href.Contains(':') || href.EndsWith("(identifier)"))
                continue;

            if (href.Contains('#'))
                href = href.Split('#')[0];

            links.Add(href);
        }
        return links;
    }

    private List<string> GetWikipediaCategoryPages(List<string> pagesList, string uri, int maxPages = 1000, int depth = 1)
    {
        if (_pagesGathered >= maxPages)
            return pagesList;

        var fullUri = $"https://en.wikipedia.org/w/api.php?action=query&list=categorymembers&cmtitle=" +
            $"{uri}&format=json&cmlimit=500&cmprop=title";
        var jsonResponse = GetSync(fullUri);
        var jObject = JObject.Parse(jsonResponse);
        foreach (var page in jObject["query"]["categorymembers"])
        {
            string pageTitle = (string)page["title"];
            if (!pageTitle.StartsWith("Category:")
                && !pageTitle.StartsWith("Portal:"))
            {
                if (_pagesGathered >= maxPages)
                    return pagesList;

                pagesList.Add(pageTitle);
                _pagesGathered++;
            }
            else if (pageTitle.StartsWith("Category:") && depth > 0)
                GetWikipediaCategoryPages(pagesList, pageTitle, maxPages, depth - 1);
        }
        return pagesList;
    }

    private string GetSync(string uri)
    {
        Console.WriteLine($"GET: {uri}");
        HttpResponseMessage response = _httpClient.GetAsync(uri).Result;
        response.EnsureSuccessStatusCode();
        var jsonResponse = response.Content.ReadAsStringAsync().Result;
        return jsonResponse;
    }
}
