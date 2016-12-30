using System;
using System.Threading;
using Common.Logging;
using Quartz;
using System.Net;
using HtmlAgilityPack;
using System.Data;

namespace StockDataQuartz
{
    /// <summary>
    /// A sample job that just prints info on console for demostration purposes.
    /// </summary>
    public class Tonghuashunxuangu : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Tonghuashunxuangu));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Tonghuashunxuangu");

            string URLAddress = "http://www.iwencai.com/stockpick?tid=stockpick&ts=1&qs=1";
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");
            try
            {
                string html = client.DownloadString(URLAddress);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.LoadHtml(html);
                HtmlNode Node1 = htmlDoc.DocumentNode;
                var trnodes1 = Node1.SelectNodes("//*[@id=\"area_list\"]/div");
                if (trnodes1 != null)
                {
                    foreach (var item in trnodes1)
                    {
                        var firsttitle = item.SelectSingleNode("child::h4[1]").SelectSingleNode("child::span[1]").InnerText.Trim();
                        if (firsttitle == "�����Ƽ�")
                        {
                            var hltj = Node1.SelectNodes("//*[@id=\"hltj_right_hc_query_li_st\"]/ul/li");
                            if (hltj != null)
                            {
                                foreach (var item2 in hltj)
                                {
                                    var typename = item2.SelectSingleNode("child::span[1]").InnerText.Trim();
                                    var secondtitle = item2.SelectSingleNode("child::div[1]").SelectSingleNode("child::a[1]").InnerText.Trim();
                                    var url = item2.SelectSingleNode("child::div[1]").SelectSingleNode("child::a[1]").Attributes["href"].Value;

                                    SaveDataDB(firsttitle, typename, secondtitle, url);
                                }
                            }
                        }
                        if (firsttitle == "�ʾ伯��")
                        {
                            var wjjj = Node1.SelectNodes("//*[@id=\"lm_zxwj\"]/dl");
                            if (wjjj != null)
                            {
                                foreach (var item2 in wjjj)
                                {
                                    var secondtitle = item2.SelectSingleNode("child::dd[1]").SelectSingleNode("child::a[1]").InnerText.Trim();
                                    var url = item2.SelectSingleNode("child::dd[1]").SelectSingleNode("child::a[1]").Attributes["href"].Value;
                                    SaveDataDB(firsttitle, "", secondtitle, url);
                                }
                            }
                            var jdwj = Node1.SelectNodes("//*[@id=\"lm_jdwj\"]/dl");
                            if (jdwj != null)
                            {
                                foreach (var item2 in jdwj)
                                {
                                    var secondtitle = item2.SelectSingleNode("child::dd[1]").SelectSingleNode("child::a[1]").InnerText.Trim();
                                    var url = item2.SelectSingleNode("child::dd[1]").SelectSingleNode("child::a[1]").Attributes["href"].Value;
                                    SaveDataDB(firsttitle, "", secondtitle, url);
                                }
                            }
                        }
                        if (firsttitle == "����ָ��")
                        {
                            var jszb = Node1.SelectNodes("//*[@id=\"area_list\"]/div[4]/div/div/div/div/div/dl");
                            jishuzhibiao(jszb, firsttitle);
                        }
                        if (firsttitle == "��̬ѡ��")
                        {
                            var xtxg = Node1.SelectNodes("//*[@id=\"area_list\"]/div[5]/div/div/div/div/div/dl");
                            jishuzhibiao(xtxg, firsttitle);
                        }
                        if (firsttitle == "����ָ��")
                        {
                            var xtxg = Node1.SelectNodes("//*[@id=\"area_list\"]/div[7]/div/div/div/div/div/dl");
                            jishuzhibiao(xtxg, firsttitle);
                        }
                        if (firsttitle == "ţ��ץѡ")
                        {
                            var xtxg = Node1.SelectNodes("//*[@id=\"area_list\"]/div[8]/div/div/div/div/div/dl");
                            jishuzhibiao(xtxg, firsttitle);
                        }
                        if (firsttitle == "�ֹ�����")
                        {
                            var xtxg = Node1.SelectNodes("//*[@id=\"area_list\"]/div[9]/div/div/div/div/div/dl");
                            jishuzhibiao(xtxg, firsttitle);
                        }
                        if (firsttitle == "�ɱ��ɶ�")
                        {
                            var xtxg = Node1.SelectNodes("//*[@id=\"area_list\"]/div[10]/div/div/div/div/div/dl");
                            jishuzhibiao(xtxg, firsttitle);
                        }
                        if (firsttitle == "��������")
                        {
                            var xtxg = Node1.SelectNodes("//*[@id=\"area_list\"]/div[11]/div/div/div/div/div/dl");
                            jishuzhibiao(xtxg, firsttitle);
                        }
                        if (firsttitle == "��ɫ����")
                        {
                            var xtxg = Node1.SelectNodes("//*[@id=\"area_list\"]/div[12]/div/div/div/div/div/dl");
                            jishuzhibiao(xtxg, firsttitle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            logger.Info("End job Tonghuashunxuangu");
        }
        private void jishuzhibiao(HtmlNodeCollection nodeList, string title)
        {
            if (nodeList != null)
            {
                foreach (var item2 in nodeList)
                {
                    string typename = item2.SelectSingleNode("child::dt[1]").SelectSingleNode("child::span[1]").InnerText.Trim();

                    var ddnodes = item2.SelectNodes("child::dd");
                    if (ddnodes != null)
                    {
                        foreach (var item3 in ddnodes)
                        {
                            var secondtitle = item3.SelectSingleNode("child::a[1]").InnerText.Trim();
                            var url = item3.SelectSingleNode("child::a[1]").Attributes["href"].Value;
                            SaveDataDB(title, typename, secondtitle, url);
                        }
                    }
                }
            }
        }
        private void SaveDataDB(string firsttitle, string typename, string secondtitle, string url)
        {
            url = "http://www.iwencai.com" + url;
            string sqlstring = string.Format(@"Insert into tonghuashunxuangu(firsttitle,typename,secondtitle,url,record_date,record_time)values('{0}','{1}','{2}','{3}','{4}','{5}')",
                firsttitle, typename, secondtitle, url, DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
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