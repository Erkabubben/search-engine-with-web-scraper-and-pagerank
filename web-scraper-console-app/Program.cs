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

    static void Main(string[] args)
    {
        _instance = new Program(args);
    }

    public Program(string[] args)
    {
        var baseUrl = "https://en.wikipedia.org/wiki/";
        var testdoc = GetDocument(baseUrl + "Dark_triad");
        Console.WriteLine(GetWords(testdoc.DocumentNode));
        var links = GetLinks(baseUrl + "Dark_triad");
        foreach (var link in links)
            Console.WriteLine(link);
        _httpClient = new HttpClient();
        //Console.WriteLine($"{jObject["query"]["categorymembers"].ToString()}\n");
        //Console.WriteLine($"{jsonResponse}\n");
        /*_pagesGathered = 0;
        var pages = GetCategoryPages(new List<string>(), "Category:Psychology", 2000, 1);
        _pagesGathered = 0;
        foreach (var page in pages)
            Console.WriteLine(page);

        Console.WriteLine("Amount of pages: " + pages.Count);
        Console.WriteLine(GetDocument(baseUrl + pages[0]).Text);*/
    }

    private string GetWords(HtmlNode documentNode)
    {
        void GetInnerHTMLFromChildNodes(StringBuilder sb, HtmlNode node)
        {
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.InnerText != null)
                {
                    var innerText = Regex.Replace(childNode.InnerText.ToLower(), "[^a-zA-Z0-9 ]", "");
                    innerText = Regex.Replace(innerText, @"\s+", " ");
                    sb.Append(innerText);
                }
                //GetInnerHTMLFromChildNodes(sb, node);
            }
        }

        StringBuilder sb = new StringBuilder();
        GetInnerHTMLFromChildNodes(sb, documentNode);
        return sb.ToString();
    }

    public HtmlDocument GetDocument(string url)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        return doc;
    }

    public List<string> GetLinks(string url)
    {
        var doc = GetDocument(url);
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a");
        var links = new List<string>();
        if (nodes == null)
            return links;

        var baseUri = new Uri(uriString: url);
        foreach (var node in nodes)
        {
            if (!node.Attributes.Contains("href"))
                continue;
            string href = node.Attributes[name: "href"].Value;
            links.Add(new Uri(baseUri, href).AbsoluteUri);
        }
        return links;
    }

    public List<string> GetCategoryPages(List<string> pagesList, string uri, int maxPages = 1000, int depth = 1)
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
            if (!pageTitle.StartsWith("Category:") && !pageTitle.StartsWith("Portal:"))
            {
                if (_pagesGathered >= maxPages)
                    return pagesList;

                pagesList.Add(pageTitle);
                _pagesGathered++;
            }
            else if (pageTitle.StartsWith("Category:") && depth > 0)
                GetCategoryPages(pagesList, pageTitle, maxPages, depth - 1);
        }
        return pagesList;
    }

    public string GetSync(string uri)
    {
        Console.WriteLine($"GET: {uri}");
        HttpResponseMessage response = _httpClient.GetAsync(uri).Result;
        response.EnsureSuccessStatusCode();
        var jsonResponse = response.Content.ReadAsStringAsync().Result;
        return jsonResponse;
    }
}
