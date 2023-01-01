using System;
using System.Text;
using System.Diagnostics;
using HtmlAgilityPack;

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
}
