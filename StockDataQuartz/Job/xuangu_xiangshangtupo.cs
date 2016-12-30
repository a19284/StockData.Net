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
    ///市场资金流
    /// </summary>
    public class xuangu_xiangshangtupo : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(xuangu_xiangshangtupo));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job xuangu_xiangshangtupo");

            var url = "http://data.10jqka.com.cn/rank/xstp/board/5/order/asc/page/";
            var urlend = "/ajax/1/";
            for (var i = 1; i < 50; i++)
            {
                SaveData(url + i + urlend, "向上突破");
            }

            url = "http://data.10jqka.com.cn/rank/xxtp/board/5/order/asc/page/";
            for (var i = 1; i < 50; i++)
            {
                SaveData(url + i + urlend, "向下突破");
            }
            logger.Info("End job xuangu_xiangshangtupo");
        }
        public void SaveData(string URLAddress, string leixin)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.LoadHtml(html);
                HtmlNode Node1 = htmlDoc.DocumentNode;
                if (Node1 == null) return;

                var trnodes1 = Node1.SelectNodes("/table/tbody/tr");
                if (trnodes1 != null)
                {
                    foreach (var item in trnodes1)
                    {
                        var tdnodes = item.SelectNodes("td");    //所有的子节点
                        if (tdnodes.Count > 6 && tdnodes[0].InnerText.Trim().Length > 0 && tdnodes[1].InnerText.Trim().Length > 0)
                        {
                            string sqlstring = string.Format(@"Insert into jishuxuangu(stock_code,stock_name,close_price,trade_amount,trade_number,section_change,handover_rate,record_date,record_time,select_mode)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
                                    tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim(),
                                    tdnodes[4].InnerText.Trim(), tdnodes[5].InnerText.Trim(), tdnodes[6].InnerText.Trim(), tdnodes[7].InnerText.Trim(),
                                    DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), leixin);
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
                logger.Info("xuangu_xiangshangtupo," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}