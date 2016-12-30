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
    /// 个股资金流
    /// </summary>
    public class Geguzijinliu : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Geguzijinliu));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Geguzijinliu");
            var url = "http://data.10jqka.com.cn/funds/ggzjl/field/zdf/order/desc/page/";
            var urlend = "/ajax/1/";
            for (var i = 1; i < 52; i++)
            {
                SaveData(url + i + urlend);
            }
            logger.Info("End job Geguzijinliu");
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
                var trnodes1 = Node1.SelectNodes("/table/tbody/tr");
                if (trnodes1 != null)
                {
                    foreach (var item in trnodes1)
                    {
                        var tdnodes = item.SelectNodes("td");    //所有的子节点
                        if (tdnodes[0].InnerText.Trim().Length > 0 && tdnodes[1].InnerText.Trim().Length > 0)
                        {
                            string sqlstring = string.Format(@"Insert into geguzijinliu(stock_code,stock_name,close_price,price_change,handover_rate,inflow,outflow,net_flow,trade_amount,inflow_bigbill,record_date,record_time)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
                                    tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim(),
                                    tdnodes[4].InnerText.Trim().Replace("%", ""), tdnodes[5].InnerText.Trim().Replace("%", ""), tdnodes[6].InnerText.Trim(), tdnodes[7].InnerText.Trim(),
                                    tdnodes[8].InnerText.Trim(), tdnodes[9].InnerText.Trim().Replace("%", ""), tdnodes[9].InnerText.Trim(), DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
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
            }
            catch (Exception ex)
            {
                logger.Info("Geguzijinliu," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}