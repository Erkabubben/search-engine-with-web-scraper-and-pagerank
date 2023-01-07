using System;
using System.Text;
using System.Diagnostics;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

partial class Program
{
    private List<string> GetWikipediaCategoryPages(List<string> pagesList, string uri, int maxPages = 1000, int depth = 1)
    {
        if (_pagesGathered >= maxPages)
            return pagesList;

        var fullUri = $"https://en.wikipedia.org/w/api.php?action=query&list=categorymembers&cmtitle=" +
            $"{uri}&format=json&cmlimit=500&cmprop=title";
        var jsonResponse = GetJSONSync(fullUri);
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

    private string GetJSONSync(string uri)
    {
        HttpResponseMessage response = _httpClient.GetAsync(uri).Result;
        response.EnsureSuccessStatusCode();
        var jsonResponse = response.Content.ReadAsStringAsync().Result;
        return jsonResponse;
    }

    private async Task<string> GetJSONAsync(string uri)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }
}
