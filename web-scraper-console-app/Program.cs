using System;
using System.Text;
using System.Diagnostics;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    static void Main(string[] args)
    {
        _instance = new Program(args);
    }

    public Program(string[] args)
    {
        var testdoc = GetDocument("https://en.wikipedia.org/wiki/Dark_triad");
        //Console.WriteLine(testdoc.Text);
        var links = GetLinks("https://en.wikipedia.org/wiki/Dark_triad");
        foreach (var link in links)
            Console.WriteLine(link);
        HttpClient httpClient = new HttpClient();
        //Console.WriteLine($"{jObject["query"]["categorymembers"].ToString()}\n");
        //Console.WriteLine($"{jsonResponse}\n");
        var pages = GetCategoryPages(httpClient, "Category:Psychology", 1);
        foreach (var page in pages)
        {
            Console.WriteLine(page);
        }
        Console.WriteLine("END");
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

    public List<string> GetCategoryPages(HttpClient httpClient, string uri, int depth = 0)
    {
        var pages = new List<string>();
        var jsonResponse = GetSync(httpClient,
            $"https://en.wikipedia.org/w/api.php?action=query&list=categorymembers&cmtitle={uri}&format=json&cmlimit=500&cmprop=title");
        var jObject = JObject.Parse(jsonResponse);
        foreach (var page in jObject["query"]["categorymembers"])
        {
            pages.Add((string)page["title"]);
        }
        return pages;
    }

    public string GetSync(HttpClient httpClient, string uri)
    {
        HttpResponseMessage response = httpClient.GetAsync(uri).Result;
        response.EnsureSuccessStatusCode();
        var jsonResponse = response.Content.ReadAsStringAsync().Result;
        return jsonResponse;
    }
}
