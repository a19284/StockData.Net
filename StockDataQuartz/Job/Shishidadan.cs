using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Quartz;

namespace StockDataQuartz
{
    /// <summary>
    /// 网易数据，下载实时大单
    /// </summary>
    public class Shishidadan: IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Shishidadan));
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Shishidadan");
            try
            {
                logger.Info("开始下载数据任务........");
                string sqlstring = @"select DISTINCT stock_code from hangyestockinfo where stock_code not in (SELECT DISTINCT stock_code from shishidadan where record_date='"+ DateTime.Today.ToString("yyyy-MM-dd") +"')";
                DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring);
                DataTable data = ds.Tables[0];
                List<string> nodatacode = new List<string>();

                Dictionary<string, int> codepage = new Dictionary<string, int>();
                logger.Info("获取股票代码，记录数：" + data.Rows.Count.ToString());
                logger.Info("开始获取页数。");

                Parallel.For(0, data.Rows.Count - 1, (i, loopState) =>
                {
                    if (data.Rows[i][0] != null && data.Rows[i][0].ToString().Length > 1)
                    {
                        var code = data.Rows[i][0].ToString();
                        var url = "http://quotes.money.163.com/trade/ddtj_" + code + ".html?amount=1000000";
                        int page = GetPages(url);
                        if (page > 0)
                        {
                            codepage.Add(code, page);
                        }
                    }
                });
                logger.Info("开始下载数据。");
                int ind = 0;
                Parallel.ForEach(codepage, (item, loopState) =>
                {
                    ind++;
                    //logger.Info(string.Format("进度：{0}/{1}", ind, codepage.Count));
                    for (int i = 0; i < item.Value; i++)
                    {
                        var url = "http://quotes.money.163.com/trade/ddtj_" + item.Key + "," + i + ".html?amount=1000000";
                        GetContent(url, item.Key);
                    }
                });
                logger.Info("下载数据完成。");
            }
            catch (Exception ex)
            {
                logger.Info("发生错误：" + ex.Message);
            }
            logger.Info("End job Shishidadan");
        }
        private string GetContent(string URLAddress,string code)
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");

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
                            string sqlstring = string.Format(@"Insert into shishidadan(stock_code,trade_time,trade_price,trade_number,trade_amount,trade_type,record_date,record_time)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", code, tdnodes[0].InnerText.Trim(), tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim(), tdnodes[4].InnerText.Trim(),
                                    DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                            Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
                        }
                    }
                }
                var trnodes2 = Node1.SelectNodes("div[2]/div[1]/table[1]/tr");
                if (trnodes2 != null)
                {
                    foreach (var item in trnodes2)
                    {
                        var tdnodes = item.SelectNodes("td");    //所有的子节点
                        if (tdnodes[0].InnerText.Trim().Length > 0 && tdnodes[1].InnerText.Trim().Length > 0)
                        {
                            string sqlstring = string.Format(@"Insert into shishidadan(stock_code,trade_time,trade_price,trade_number,trade_amount,trade_type,record_date,record_time)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", code, tdnodes[0].InnerText.Trim(), tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim(), tdnodes[4].InnerText.Trim(),
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
            }
            catch (Exception ex)
            {
                logger.Info("Shishidadan," + URLAddress + "," + ex.Message);
            }
            return string.Empty;
        }
        private int GetPages(string URLAddress)
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");

                string html = client.DownloadString(URLAddress);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.LoadHtml(html);
                HtmlNode Node1 = htmlDoc.GetElementbyId("pageBar");

                var nodecollection = Node1.SelectNodes("div[1]/a");
                if (nodecollection != null)
                {
                    return nodecollection.Count;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                logger.Info("获取页数发生错误：" + ex.Message);
                logger.Info(URLAddress);
                return 0;
            }
        }
    }
}
