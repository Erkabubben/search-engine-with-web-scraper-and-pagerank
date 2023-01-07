using System;
using System.Text;
using System.Diagnostics;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

partial class Program
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
        //CreateLinksAndWordsCollectionsFromPage("Artificial_intelligence", "Artificial_intelligence", 200).Wait();
        CreateLinksAndWordsCollectionsFromPage("South_Korea", "South_Korea", 200).Wait();
    }

    /// <summary>
    /// First takes the URL of a Wikipedia page to use as a starting point and scrapes links from it, then scrapes
    /// the pages of all those links and stores the contents as Words and Links files for a search engine.
    /// </summary>
    /// <param name="collectionName">The name of the new collection folders to be created.</param>
    /// <param name="pageUrl">The Url of the starting point page.</param>
    /// <param name="maxPages">The maximum amount of pages to be scraped (optional).</param>
    /// <returns>A Task.</returns>
    private async Task CreateLinksAndWordsCollectionsFromPage(string collectionName, string pageUrl, int maxPages = -1)
    {
        // Get Content Node of the Wikipedia page to be used as starting point.
        var startPageContentNode = await GetWikipediaContentNodeAsync(_baseUrl + "wiki/" + pageUrl);
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

        // Extract list of links from the starting point page's Content Node, trim list if larger than maxPages.
        var startPageLinksList = GetWikipediaLinksFromContentNode(startPageContentNode);
        // Remove duplicate links.
        startPageLinksList = startPageLinksList.Distinct().ToList();
        if (maxPages > -1 && maxPages < startPageLinksList.Count)
            startPageLinksList.RemoveRange(maxPages, startPageLinksList.Count - maxPages);

        Console.WriteLine($"Amount of pages to be added to collection '{collectionName}': {startPageLinksList.Count}");

        // Iterate links list and scrape pages.
        async Task<HtmlNode[]> WaitForAllGetRequestsToFinish(List<string> startPageLinksList)
        {
            int amountOfTasksFinished = 0;

            async Task<HtmlNode> SendRequestAndPrintMessages(int i)
            {
                Console.WriteLine($"Requested page {i}: " + startPageLinksList[i]);
                var node = await GetWikipediaContentNodeAsync(_baseUrl + startPageLinksList[i]);
                if (node != null)
                {
                    amountOfTasksFinished++;
                    Console.WriteLine(
                        $"Success:({i})" + startPageLinksList[i] + $" ({amountOfTasksFinished} / { startPageLinksList.Count})");
                }
                else
                    Console.WriteLine(
                        $"Failure:({i})" + startPageLinksList[i] + $" ({amountOfTasksFinished} / { startPageLinksList.Count})");
                return node;
            }

            var contentNodes = new HtmlNode[startPageLinksList.Count];
            var tasks = new Task<HtmlNode>[startPageLinksList.Count];
            
            for (int i = 0; i < tasks.Length; i++)
                tasks[i] = SendRequestAndPrintMessages(i);
            Console.WriteLine($"Awaiting GET requests...");
            await Task.WhenAll(tasks);
            for (int i = 0; i < contentNodes.Length; i++)
                contentNodes[i] = tasks[i].Result;

            return contentNodes;
        }

        var contentNodes = await WaitForAllGetRequestsToFinish(startPageLinksList);

        // Iterate content nodes of retrieved pages and add Words and Links files to folders.
        for (int i = 0; i < startPageLinksList.Count; i++)
        {
            if (maxPages > -1 && i >= maxPages)
                break;
            string? link = startPageLinksList[i];
            var pageContentNode = contentNodes[i];
            var wordsString = GetWordsFromContentNode(pageContentNode);
            var links = GetWikipediaLinksFromContentNode(pageContentNode);
            string linksString = string.Join("\n", links);
            string filename = link.StartsWith("/wiki/") ? link.Substring("/wiki/".Length) : link;
            Console.WriteLine($"Creating Words and Links files for page {i}: {filename}");
            File.WriteAllText(wordsFolder + '\\' + filename, wordsString);
            File.WriteAllText(linksFolder + '\\' + filename, linksString);
        }

        Console.WriteLine($"Collection '{collectionName}' was successfully created.");
    }

    /// <summary>
    /// Takes a Wikipedia page's Content node and returns its text content as Words file data.
    /// </summary>
    /// <param name="contentNode">A Wikipedia page's Content node.</param>
    /// <returns>A string of Words file data.</returns>
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

    /// <summary>
    /// Asynchronously sends a GET request for a Wikipedia page and returns its Content node if the
    /// request succeeds.
    /// </summary>
    /// <param name="url">The Url of a Wikipedia page.</param>
    /// <returns>A Task that resolves into a Wikipedia page's Content node.</returns>
    private async Task<HtmlNode> GetWikipediaContentNodeAsync(string url)
    {
        var doc = await GetDocumentAsync(url);
        if (doc == null)
            return null;

        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//body/div[@id='content']");
        return nodes[0];
    }

    /// <summary>
    /// Synchronously sends a GET request for a Wikipedia page and returns its Content node if the
    /// request succeeds.
    /// </summary>
    /// <param name="url">The Url of a Wikipedia page.</param>
    /// <returns>A Wikipedia page's Content node.</returns>
    private HtmlNode GetWikipediaContentNodeSync(string url)
    {
        var doc = GetDocumentSync(url);
        if (doc == null)
            return null;

        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//body/div[@id='content']");
        return nodes[0];
    }

    /// <summary>
    /// Asynchronously sends a GET request for a Wikipedia page and returns it as an HtmlDocument.
    /// </summary>
    /// <param name="url">The Url of a Wikipedia page.</param>
    /// <returns>A Task that resolves into an HtmlDocument.</returns>
    private async Task<HtmlDocument> GetDocumentAsync(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    /// <summary>
    /// Synchronously sends a GET request for a Wikipedia page and returns it as an HtmlDocument.
    /// </summary>
    /// <param name="url">The Url of a Wikipedia page.</param>
    /// <returns>An HtmlDocument.</returns>
    private HtmlDocument GetDocumentSync(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        return doc;
    }

    /// <summary>
    /// Extracts all links from a Wikipedia page's Content node and returns them as a list of strings.
    /// </summary>
    /// <param name="contentNode">A Wikipedia page's Content node.</param>
    /// <returns>A list of strings.</returns>
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
}
