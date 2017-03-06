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

namespace StockDataQuartz
{
    /// <summary>
    /// 主要指数
    /// </summary>
    public class FundNetVal : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FundNetVal));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job FundNetVal");
            var url = "http://fund.eastmoney.com/fundguzhi.html";
            SaveData(url);
            logger.Info("End job FundNetVal");
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
                if (Node1 == null) return;

                var trnodes1 = Node1.SelectNodes("//*[@id=\"oTable\"]/tbody/tr");
                if (trnodes1 != null)
                {
                    Parallel.ForEach(trnodes1, item =>
                    {
                        var tdnodes = item.SelectNodes("td");    //所有的子节点
                        if (tdnodes[2].InnerText.Trim().Length > 0)
                        {
                            string fundname = tdnodes[3].InnerText.Trim().Replace("估算图基金吧档案", "");
                            string sqlstring = string.Format(@"Insert into fundnetval(fundcode,fundname,gusuanvalue,gusuanrate,realvalue,realrate,gusuanpc,record_date,record_time)values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
                                    tdnodes[2].InnerText.Trim(), fundname, tdnodes[4].InnerText.Trim(),
                                    tdnodes[5].InnerText.Trim(), tdnodes[6].InnerText.Trim(), tdnodes[7].InnerText.Trim(), tdnodes[8].InnerText.Trim(),
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
                    });
                }

            }
            catch (Exception ex)
            {
                logger.Info("FundNetVal," + URLAddress);
                logger.Info(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
            }
        }
    }
}