using System;
using System.Threading;
using Common.Logging;
using Quartz;
using System.Net;
using HtmlAgilityPack;
using System.Data;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Text;

namespace StockDataQuartz
{
    /// <summary>
    ///市场资金流
    /// </summary>
    public class TonghuashunCommon
    {
        private int times = 0;
        /// <summary>
        /// 获取html页面内容
        /// </summary>
        /// <param name="urlAddress"></param>
        /// <param name="logger"></param>
        /// <param name="proxyUrl"></param>
        /// <param name="utf8"></param>
        /// <returns></returns>
        public Task<string> GetHtml(string urlAddress, ILog logger, List<string> proxyUrl,string utf8 = "")
        {
            times = 0;
            return Task.Run(() =>
            {
                return DownData(proxyUrl, urlAddress, logger, utf8);
            });
        }
        /// <summary>
        /// 下载数据
        /// </summary>
        /// <param name="proxyUrl"></param>
        /// <param name="urlAddress"></param>
        /// <param name="logger"></param>
        /// <param name="utf8"></param>
        /// <returns></returns>
        private string DownData(List<string> proxyUrl,string urlAddress, ILog logger,string utf8)
        {
            WebClient client = new WebClient();
            if (utf8.Length > 0)
            {
                client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");
            }
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
                string html = client.DownloadString(urlAddress);
                return html;
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                if (times < 20)
                {
                    DownData(proxyUrl, urlAddress, logger, utf8);
                }
            }
            return string.Empty;
        }
        public async void SaveDataJSON(string URLAddress, string typeid, ILog logger, string table, List<string> proxyUrl)
        {
            try
            {
                string html = await GetHtml(URLAddress, logger, proxyUrl);
                if (html.Length == 0) return;

                int ind1 = html.IndexOf("allResult");
                html = html.Substring(ind1 + 2 + "allResult".Length).Trim();
                int ind2 = html.IndexOf("};");
                html = html.Substring(0, ind2 + 1);

                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));
                if (data.ContainsKey("title") && data.ContainsKey("result"))
                {
                    var title = (object[])data["title"];
                    var list = (object[])data["result"];
                    for (int i = 0; i < list.Length; i++)
                    {
                        object[] list1 = (object[])list[i];
                        List<string> valueList = new List<string>();
                        for (int k = 2; k < list1.Length; k++)
                        {
                            valueList.Add(string.Format("\"{0}\":\"{1}\"", title[k], list1[k].ToString().Replace("'", "")));
                        }
                        string valuesstring = "{" + string.Join(",", valueList) + "}";
                        string stockcode = list1[0].ToString().Split(new char[] { '.' })[0];

                        string sqlstring = string.Format("Insert into " + table + "(stock_code,stock_name,valuestring,typeid,record_date,record_time) values('{0}','{1}','{2}','{3}','{4}','{5}')",
                            stockcode, list1[1].ToString(), valuesstring, typeid, DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));

                        try
                        {
                            Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
                        }
                        catch (Exception ex)
                        {
                            logger.Info(sqlstring);
                            throw new Exception(ex.Message);
                        }
                    }
                }
                if (data.ContainsKey("token") && data.ContainsKey("total"))
                {
                    string taken = data["token"].ToString();
                    int total = int.Parse(data["total"].ToString());
                    int perpage = int.Parse(data["perpage"].ToString());
                    int pages = total / perpage;
                    for (int i = 0; i < pages; i++)
                    {
                        int p = i + 2;
                        string url2 = "http://www.iwencai.com/stockpick/cache?token=" + taken + "&p=" + p + "&perpage=" + perpage;
                        SaveDataJSONSecond(url2, typeid, logger, table, proxyUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(URLAddress);
                logger.Info(ex.Message);
            }
        }
        private async void SaveDataJSONSecond(string URLAddress, string typeid, ILog logger, string table, List<string> proxyUrl)
        {
            try
            {
                string html = await GetHtml(URLAddress, logger, proxyUrl);
                if (html.Length == 0) return;

                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));
                if (data.ContainsKey("title") && data.ContainsKey("result"))
                {
                    var title = (object[])data["title"];
                    var list = (object[])data["result"];
                    for (int i = 0; i < list.Length; i++)
                    {
                        object[] list1 = (object[])list[i];
                        List<string> valueList = new List<string>();
                        for (int k = 2; k < list1.Length; k++)
                        {
                            valueList.Add(string.Format("\"{0}\":\"{1}\"", title[k], list1[k].ToString().Replace("'", "")));
                        }
                        string valuesstring = "{" + string.Join(",", valueList) + "}";
                        string stockcode = list1[0].ToString().Split(new char[] { '.' })[0];

                        string sqlstring = string.Format("Insert into " + table + "(stock_code,stock_name,valuestring,typeid,record_date,record_time) values('{0}','{1}','{2}','{3}','{4}','{5}')",
                        stockcode, list1[1].ToString(), valuesstring, typeid, DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                        try
                        {
                            Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
                        }
                        catch (Exception ex)
                        {
                            logger.Info(sqlstring);
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(URLAddress);
                logger.Info(ex.Message);
            }
        }
        /// <summary>
        /// 获取广东省内免费代理服务地址
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public List<string> GetProxyURL(ILog logger)
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
                logger.Info(ex.Message);
            }
            return proxy;
        }
    }
}