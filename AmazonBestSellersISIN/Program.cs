using HtmlAgilityPack;
using Newtonsoft.Json;

string FixedStr(string s, int length, char padChar = ' ')
        => string.IsNullOrEmpty(s) ? new string(padChar, length) : s.Length > length ? s.Remove(length) : s.Length < length ? s.PadRight(length) : s;
string CategoriesToStr(List<string> categories)
{
    string res = "";
    foreach (var category in categories)
    {
        res += string.Format("\t{0}\n", category);
    }
    return res;
}

string country = "Canada";
string urlCountry = "https://www.amazon.ca/";
List<string> categories = new();
List<string> urlsCategories = new();
string bestsellers = "/bestsellers/";

Console.Write("Welcome to Amazon Best Sellers ASIN getter.\n");

while(true)
{
    Console.Write(
    string.Format("\nCountry : {0}\n", country) +
    "Categories : \n" +
    CategoriesToStr(categories) +
    "\n1 -> Country Settings,\n" +
    "2 -> Category Settings,\n" +
    "3 -> Get ASIN Numbers,\n" +
    "Please type your options number : ");

    string input = Console.ReadLine();
    Console.WriteLine("\n");

    switch (input)
    {
        case "1":
            SetCountry();
            break;
        case "2":
            SetCategory();
            break;
        case "3":
            GetASIN();
            break;
    }
}

void SetCountry()
{
    Console.Clear();
    var html = @"https://www.amazon.com/customer-preferences/country";
    HtmlWeb web = new HtmlWeb();
    var htmlDoc = web.Load(html);
    HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//[@id=\"icp-dropdown\"]/option"); // //*[@id="icp-dropdown"]
    List<HtmlNode> countries = new();

    for(int i=0; i<nodes.Count; i++)
    {
        if (nodes[i].Attributes["value"] != null && !string.IsNullOrEmpty(nodes[i].Attributes["value"].Value))
        {
            countries.Add(nodes[i]);
            string country = FixedStr(countries.Count + ") " + nodes[i].InnerText.Trim().Split('(')[0], 25);
            Console.WriteLine(country + " -> " + nodes[i].Attributes["value"].Value);
        }
    }

    Console.Write("Please type country number : ");
    int index;
    if(int.TryParse(Console.ReadLine(), out index) && index > 0 && index < nodes.Count)
    {
        urlCountry = countries[index-1].Attributes["value"].Value;
        country = countries[index - 1].InnerText.Trim().Split('(')[0];
        categories = new List<string>();
        urlsCategories = new List<string>();
        Console.WriteLine("Country Changed Succesfull.\n");
    }
    else
    {
        Console.WriteLine("Input error! Check your input.");
    }
}

void SetCategory()
{
    Console.Clear();
    HtmlWeb web = new HtmlWeb();
    var htmlDoc = web.Load(urlCountry + bestsellers);
    HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"zg_left_col2\"]/div[1]/div[1]/div[2]/div"); // ///*[@id=\"zg_left_col2\"]/div/div/div/div //*[@id=\"zg_left_col2\"]/div[1]/div[1]/div[2]
    List<HtmlNode> listCategories = new List<HtmlNode>();

    for (int i = 0; i < nodes.Count; i++)
    {
        HtmlNode node = nodes[i];
        if (node.Attributes["role"].Value == "treeitem")
        {
            listCategories.Add(node);
            Console.WriteLine(listCategories.Count + ") " + node.InnerText.Trim().Replace("&amp;", "&"));
        }
    }

    Console.Write("Please type category numbers (eg. 2,4,11) : ");
    string[] getCategories = Console.ReadLine().Replace(" ", "").Split(',');
    categories = new List<string>();
    urlsCategories = new List<string>();
    foreach (string category in getCategories)
    {
        int index;
        if (int.TryParse(category, out index) && index > 0 && index <= nodes.Count)
        {
            string cate = listCategories[index - 1].InnerText.Trim().Replace("&amp;", "&");
            urlsCategories.Add(listCategories[index - 1].FirstChild.Attributes["href"].Value);
            categories.Add(cate);
            Console.WriteLine(string.Format("Category Changed Succesfull => {0} : {1}", index, cate));
        }
        else
        {
            Console.WriteLine(string.Format("Category Change Failed => {0}", index));
        }
    }
}

void GetASIN()
{
    Console.Clear();
    for(int i = 0; i < urlsCategories.Count; i++)
    {
        Console.WriteLine(string.Format("<--    {0}    -->", categories[i]));

        HtmlWeb web = new HtmlWeb();
        var htmlDoc = web.Load(urlCountry + urlsCategories[i]);
        //HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"zg-right-col\"]/div[1]/div[1]/div[2]/div[1]");
        HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//*[@class=\"p13n-desktop-grid\"]");

        string json = node.Attributes["data-client-recs-list"].DeEntitizeValue;
        dynamic data = JsonConvert.DeserializeObject(json);
        foreach (var item in data)
        {
            Console.WriteLine(item["id"]);
        }
    }
}