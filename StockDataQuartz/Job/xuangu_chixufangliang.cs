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
    ///�г��ʽ���
    /// </summary>
    public class xuangu_chixufangliang : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(xuangu_chixufangliang));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job xuangu_chixufangliang");

            var url = "http://data.10jqka.com.cn/rank/cxfl/field/count/order/desc/page/";
            var urlend = "/ajax/1/";
            for (var i = 1; i < 50; i++)
            {
                SaveData(url + i + urlend, "��������");
            }

            url = "http://data.10jqka.com.cn/rank/cxsl/field/count/order/desc/page/";
            for (var i = 1; i < 50; i++)
            {
                SaveData(url + i + urlend, "��������");
            }
            logger.Info("End job xuangu_chixufangliang");
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
                        var tdnodes = item.SelectNodes("td");    //���е��ӽڵ�
                        if (tdnodes[0].InnerText.Trim().Length > 0 && tdnodes[1].InnerText.Trim().Length > 0)
                        {
                            string sqlstring = string.Format(@"Insert into jishuxuangu(stock_code, stock_name, price_change, close_price, trade_number, baseday_trade_number, rise_days, section_change, business, record_date,record_time, select_mode)values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
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
                logger.Info("xuangu_chixufangliang," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}