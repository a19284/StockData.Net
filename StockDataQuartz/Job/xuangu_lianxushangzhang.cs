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
    public class xuangu_lianxushangzhang : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(xuangu_lianxushangzhang));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job xuangu_lianxushangzhang");

            var url = "http://data.10jqka.com.cn/rank/lxsz/field/lxts/order/desc/page/";
            var urlend = "/ajax/1/";
            for (var i = 1; i < 50; i++)
            {
                SaveData(url + i + urlend, "连续上涨");
            }

            url = "http://data.10jqka.com.cn/rank/lxxd/field/lxts/order/desc/page/";
            for (var i = 1; i < 50; i++)
            {
                SaveData(url + i + urlend, "连续下跌");
            }
            logger.Info("End job xuangu_lianxushangzhang");
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
                        if (tdnodes[0].InnerText.Trim().Length > 0 && tdnodes[1].InnerText.Trim().Length > 0)
                        {
                            string sqlstring = string.Format(@"Insert into jishuxuangu(stock_code,stock_name,close_price,high_price,low_price,rise_days,section_change,handover_rate,business,record_date,record_time,select_mode)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
                                    tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim(),
                                    tdnodes[4].InnerText.Trim(), tdnodes[5].InnerText.Trim(), tdnodes[6].InnerText.Trim(), tdnodes[7].InnerText.Trim(),
                                    tdnodes[8].InnerText.Trim(), tdnodes[9].InnerText.Trim(), DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), leixin);
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
                logger.Info("xuangu_lianxushangzhang," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}