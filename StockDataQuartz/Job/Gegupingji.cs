using System;
using System.Threading;
using Common.Logging;
using Quartz;
using System.Net;
using HtmlAgilityPack;
using System.Data;
using System.Threading.Tasks;

namespace StockDataQuartz
{
    /// <summary>
    /// 同花顺行业资金指数
    /// </summary>
    public class Gegupingji : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Gegupingji));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Gegupingji");
            var url1 = "http://vip.stock.finance.sina.com.cn/q/go.php/vIR_RatingUp/index.phtml?p=";
            var url2 = "http://vip.stock.finance.sina.com.cn/q/go.php/vIR_RatingDown/index.phtml?p=";
            for (var i = 1; i < 8; i++)
            {
                SaveData(url1 + i, "上调");
                SaveData(url2 + i, "下调");
            }
            logger.Info("End job Gegupingji");
        }
        public void SaveData(string URLAddress, string mode)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                htmlDoc.LoadHtml(html);
                HtmlNode Node1 = htmlDoc.DocumentNode;
                var trnodes1 = Node1.SelectNodes("/html/body/div[1]/div[5]/div[2]/div/div[1]/table/tr");
                if (trnodes1 != null)
                {
                    Parallel.ForEach(trnodes1, item =>
                    {
                        var tdnodes = item.SelectNodes("td");    //所有的子节点
                        if (tdnodes[0].InnerText.Trim().Length > 0 && tdnodes[1].InnerText.Trim().Length > 0)
                        {
                            if (tdnodes[6].InnerText.Trim() == DateTime.Today.ToString("yyyy-MM-dd"))
                            {
                                string sqlstring = string.Format(@"Insert into gegupingji(stock_code,stock_name,new_rating,rating_agency,analyst,business,rating_date,last_rating,rating_mode,record_date,record_time)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}')",
                                        tdnodes[0].InnerText.Trim(), tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim().Replace("%", ""),
                                        tdnodes[4].InnerText.Trim(), tdnodes[5].InnerText.Trim(), tdnodes[6].InnerText.Trim(), tdnodes[7].InnerText.Trim(),
                                        mode, DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
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
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Info("Gegupingji," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}