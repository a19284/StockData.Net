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
    public class SampleJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SampleJob));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("QuartZ Job running...");
            WebClient client = new WebClient();
            string URLAddress = "http://quotes.money.163.com/old/#query=leadIndustry&DataType=industryPlate&sort=PERCENT&order=desc&count=25&page=0";
            try
            {
                string html = client.DownloadString(URLAddress);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.LoadHtml(html);
                HtmlNode Node1 = htmlDoc.GetElementbyId("dadanTable");
                var trnodes1 = Node1.SelectNodes("div[1]/div[1]/table[1]/tr");
                if (trnodes1 != null)
                {
                    foreach (var item in trnodes1)
                    {
                        var tdnodes = item.SelectNodes("td");    //所有的子节点
                        if (tdnodes[0].InnerText.Trim().Length > 0 && tdnodes[1].InnerText.Trim().Length > 0)
                        {
                            string sqlstring = string.Format(@"Insert into wangyidadan(stock_code,trade_time,trade_price,trade_number,trade_amount,trade_type,record_date)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}')", "", tdnodes[0].InnerText.Trim(), tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim(), tdnodes[4].InnerText.Trim(), DateTime.Today.ToShortDateString());
                            //Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info("下载数据发生错误：" + ex.Message);
                logger.Info(URLAddress);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
            }

            logger.Info("QuartZ Job run finished.");
        }
    }
}