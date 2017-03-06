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
        static int times = 0;
        static void Main(string[] args)
        {
            List<string> proxyUrl = SaveData();
            times = 0;
            downdata(proxyUrl);

            Console.WriteLine("The end");
            Console.ReadKey();
        }
        static void downdata(List<string> proxyUrl)
        {
            WebClient client = new WebClient();
            times++;
            if (proxyUrl.Count > 0)
            {
                Random ran = new Random();
                int RandKey = ran.Next(1, proxyUrl.Count);

                WebProxy proxy = new WebProxy();
                proxy.UseDefaultCredentials = false;
                proxy.Address = new Uri("http://" + proxyUrl[RandKey]);
                client.Proxy = proxy;
            }
            try
            {
                string html = client.DownloadString("http://www.iwencai.com/stockpick/search?ts=1&tid=stockpick&qs=lm_zldx_a&w=%E4%B8%BB%E5%8A%9B%E6%94%BB%E5%87%BB");
                Console.WriteLine(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (times < 5)
                {
                    downdata(proxyUrl);
                }
            }
        }
        static List<string> SaveData()
        {
            var URLAddress = "http://www.66ip.cn/areaindex_19/1.html";
            List<string> proxy = new List<string>();
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);
                var txts = Regex.Matches(html, "(?is)<tr>(.+?)</tr>").OfType<Match>().Select(x => x.Groups[1].Value);
                foreach (var item in txts)
                {
                    if (item.Contains("广东省"))
                    {
                        var tdtxts = Regex.Matches(item, "(?is)<td>(.+?)</td>").OfType<Match>().Select(x => x.Groups[1].Value).ToList();
                        proxy.Add(tdtxts[0] + ":" + tdtxts[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return proxy;
        }
    }
}
