using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TestData
{
    class Program
    {
        static void Main(string[] args)
        {
            SaveData("http://www.iwencai.com/asyn/search?q=kdj%E9%87%91%E5%8F%89&queryType=stock&app=qnas&qid=");
            Console.WriteLine("The end");
            Console.ReadKey();
        }
        static void SaveData(string URLAddress)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");
            try
            {
                string html = client.DownloadString(URLAddress);
                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));
                Dictionary<string, object> suggest =(Dictionary<string, object>)data["suggest"];
                
                foreach (var item in suggest)
                {
                    string value = item.Value.ToString();
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                    htmlDoc.LoadHtml(value);
                    HtmlNode Node1 = htmlDoc.DocumentNode;
                    var linodes = Node1.SelectSingleNode("child::ul[1]").SelectNodes("child::li");
                    for (int i = 1; i < linodes.Count; i++)
                    {
                        var lititle = linodes[i].SelectSingleNode("child::a[1]").Attributes["title"].Value;
                        var url = linodes[i].SelectSingleNode("child::a[1]").Attributes["href"].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Zhinengxuangu1," + URLAddress);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
