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
    private bool _useSearchEngineAPIDatasetsFolder = true;

    static void Main(string[] args)
    {
        _instance = new Program(args);
    }

    public Program(string[] args)
    {
        _httpClient = new HttpClient();
        CreateLinksAndWordsCollectionsFromPage("Artificial_intelligence", "Artificial_intelligence", 200);
    }

    private void CreateLinksAndWordsCollectionsFromPage(string collectionName, string pageUrl, int maxPages = -1)
    {
        // Get Content Node of the Wikipedia page to be used as starting point.
        var startPageContentNode = GetWikipediaContentNode(_baseUrl +"wiki/" + pageUrl);
        if (startPageContentNode == null)
            return;

        // Determine dataset folder to write to (console app's or directly to SearchEngineAPI).
        string wikipediaFolder = _useSearchEngineAPIDatasetsFolder
            ? Path.GetDirectoryName(appFolderPath) + @$"\SearchEngineAPI\datasets\wikipedia"
            : appFolderPath + @$"\datasets\wikipedia";

        // Determine Words and Links subfolder paths.
        string wordsFolder = wikipediaFolder + @$"\Words\" + collectionName;
        string linksFolder = wikipediaFolder + @$"\Links\" + collectionName;

        // Create collection folders.
        if (Directory.Exists(wordsFolder))
            Directory.Delete(wordsFolder, true);
        Directory.CreateDirectory(wordsFolder);
        if (Directory.Exists(linksFolder))
            Directory.Delete(linksFolder, true);
        Directory.CreateDirectory(linksFolder);

        // Extract list of links from the starting point page's Content Node.
        var startPageLinksList = GetWikipediaLinksFromContentNode(startPageContentNode);

        Console.WriteLine($"Amount of pages to be added to collection '{collectionName}': "
            + (maxPages > -1 ? $"{maxPages} out of " : "")
            + startPageLinksList.Count);

        // Iterate links list, scrape pages and add Words and Links files to folders.
        for (int i = 0; i < startPageLinksList.Count; i++)
        {
            if (maxPages > -1 && i >= maxPages)
                break;
            string? link = startPageLinksList[i];
            var pageContentNode = GetWikipediaContentNode(_baseUrl + link);
            var wordsString = GetWordsFromContentNode(pageContentNode);
            var links = GetWikipediaLinksFromContentNode(pageContentNode);
            string linksString = string.Join("\n", links);
            string filename = link.StartsWith("/wiki/") ? link.Substring("/wiki/".Length) : link;
            Console.WriteLine($"Adding page {i} '{filename}'...");
            File.WriteAllText(wordsFolder + '\\' + filename, wordsString);
            File.WriteAllText(linksFolder + '\\' + filename, linksString);
        }

        Console.WriteLine($"Collection '{collectionName}' was successfully created.");
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
