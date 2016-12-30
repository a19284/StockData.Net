using System;
using System.Threading;
using Common.Logging;
using Quartz;
using System.Net;
using HtmlAgilityPack;
using System.Data;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace StockDataQuartz
{
    /// <summary>
    ///同花顺智能选股
    /// </summary>
    public class Zhinengxuangu1 : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Zhinengxuangu1));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Zhinengxuangu1");

            var url = "http://stock.10jqka.com.cn/api/znxg/index.html";
            SaveData(url);
            logger.Info("End job Zhinengxuangu1");
        }
        public void SaveData(string URLAddress)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.LoadHtml(html);
                HtmlNode Node1 = htmlDoc.DocumentNode;
                var trnodes1 = Node1.SelectNodes("/html/body/div[2]/div[2]/div");
                if (trnodes1 != null)
                {
                    foreach (var item in trnodes1)
                    {
                        var typename = item.SelectSingleNode("child::div[1]").SelectSingleNode("child::h1[1]").InnerText.Trim();
                        var czlx = item.SelectSingleNode("child::div[1]").SelectSingleNode("child::p[2]").InnerText.Trim();
                        var hxyf = item.SelectSingleNode("child::div[1]").SelectSingleNode("child::p[3]").InnerText.Trim();
                        var cgl = item.SelectSingleNode("child::div[1]").SelectSingleNode("child::p[1]").SelectSingleNode("child::span[1]").SelectSingleNode("child::span[1]").InnerText.Trim();

                        string forId = string.Empty;
                        string calsstext = item.SelectSingleNode("child::div[2]").SelectSingleNode("child::div[1]").SelectSingleNode("child::div[1]").Attributes["class"].Value;
                        string[] formula = calsstext.Split(new char[] { ' ' });  //screen-table-wrapper formula-526841
                        for (int i = 0; i < formula.Length; i++)
                        {
                            if (formula[i].Contains("formula"))
                            {
                                forId = formula[i].Split(new char[] { '-' })[1];
                            }
                        }

                        string sqlstring = string.Format(@"Insert into zhinengxuangu(name,right_rate,optype,uses,formulaid,record_date,record_time)values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                                    typename, cgl, czlx, hxyf, forId,
                                    DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
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
                logger.Info("Zhinengxuangu1," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}