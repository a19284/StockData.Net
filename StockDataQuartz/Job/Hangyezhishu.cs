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
    /// 同花顺行业资金指数
    /// </summary>
    public class Hangyezhishu : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Hangyezhishu));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Hangyezhishu");

            string sqlstring = @"select * from hangyezhishu where record_date ='" + DateTime.Today.ToString("yyyy-MM-dd") + "' and business_type='行业资金'";
            DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            DataTable data = ds.Tables[0];
            if (data.Rows.Count == 0)
            {
                var hangye1 = "http://data.10jqka.com.cn/funds/hyzjl/field/tradezdf/order/desc/page/1/ajax/1/";
                var hangye2 = "http://data.10jqka.com.cn/funds/hyzjl/field/tradezdf/order/desc/page/2/ajax/1/";
                SaveData(hangye1, "行业资金");
                SaveData(hangye2, "行业资金");
            }

            string sqlstring2 = @"select * from hangyezhishu where record_date ='" + DateTime.Today.ToString("yyyy-MM-dd") + "' and business_type='概念资金'";
            DataSet ds2 = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring2, null);
            DataTable data2 = ds2.Tables[0];
            if (data2.Rows.Count == 0)
            {
                var ganlian1 = "http://data.10jqka.com.cn/funds/gnzjl/field/tradezdf/order/desc/page/1/ajax/1/";
                var ganlian2 = "http://data.10jqka.com.cn/funds/gnzjl/field/tradezdf/order/desc/page/2/ajax/1/";
                var ganlian3 = "http://data.10jqka.com.cn/funds/gnzjl/field/tradezdf/order/desc/page/3/ajax/1/";

                SaveData(ganlian1, "概念资金");
                SaveData(ganlian2, "概念资金");
                SaveData(ganlian3, "概念资金");
            }
            logger.Info("End job Hangyezhishu");
        }
        public void SaveData(string URLAddress, string hangye)
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
                            string sqlstring = string.Format(@"Insert into hangyezhishu(name,indexs,index_change,inflow,outflow,net_amount,company_count,lead_stock,lead_change,record_date,record_time,business_type)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
                                    tdnodes[1].InnerText.Trim(), tdnodes[2].InnerText.Trim(), tdnodes[3].InnerText.Trim().Replace("%", ""),
                                    tdnodes[4].InnerText.Trim(), tdnodes[5].InnerText.Trim(), tdnodes[6].InnerText.Trim(), tdnodes[7].InnerText.Trim(),
                                    tdnodes[8].InnerText.Trim(), tdnodes[9].InnerText.Trim().Replace("%", ""), DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), hangye);
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
                logger.Info("Hangyezhishu," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}