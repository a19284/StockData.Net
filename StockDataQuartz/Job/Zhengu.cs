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
    /// 诊股
    /// </summary>
    public class Zhengu : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Zhengu));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Zhengu");

            string sqlstring = @"select DISTINCT stock_code from hangyestockinfo where stock_code not in (SELECT DISTINCT stock_code from zhengu where record_date='"+ DateTime.Today.ToString("yyyy-MM-dd") +"')";
            DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            DataTable data = ds.Tables[0];

            Parallel.For(0, data.Rows.Count - 1, (i, loopState) =>
            {
                string URLAddress = "http://doctor.10jqka.com.cn/" + data.Rows[i][0].ToString() + "/";
                SaveData(data.Rows[i][0].ToString(), URLAddress);
            });
            logger.Info("End job Zhengu");
        }
        public void SaveData(string code, string URLAddress)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);
                html = html.Replace(" ", "");

                int indx10 = html.IndexOf("<title>");
                string inxstring0 = html.Substring(indx10 + 7, 10);
                string stockname = inxstring0.Substring(0, inxstring0.IndexOf("("));

                int inx11 = html.IndexOf("综合诊断");
                string inxstring1 = html.Substring(inx11 + 5, 20);
                inxstring1 = inxstring1.Substring(0, inxstring1.IndexOf("股票") - 1);
                string[] scorearr = inxstring1.Split(new string[] { "分打败了" }, StringSplitOptions.RemoveEmptyEntries);
                string score = scorearr[0];
                string percent = scorearr[1].Replace("%", "");

                string buystr = string.Empty;

                int inx12 = html.IndexOf("卖出");
                string inxstring2 = html.Substring(inx12 - 10, 10);
                if (inxstring2.IndexOf("cur") > 0)
                {
                    buystr = "卖出";
                }

                int inx13 = html.IndexOf("减持");
                string inxstring3 = html.Substring(inx13 - 10, 10);
                if (inxstring3.IndexOf("cur") > 0)
                {
                    buystr = "减持";
                }

                int inx14 = html.IndexOf("中性");
                string inxstring4 = html.Substring(inx14 - 10, 10);
                if (inxstring4.IndexOf("cur") > 0)
                {
                    buystr = "中性";
                }

                int inx15 = html.IndexOf("增持");
                string inxstring5 = html.Substring(inx15 - 10, 10);
                if (inxstring5.IndexOf("cur") > 0)
                {
                    buystr = "增持";
                }

                int inx16 = html.IndexOf("买入");
                string inxstring6 = html.Substring(inx16 - 10, 10);
                if (inxstring6.IndexOf("cur") > 0)
                {
                    buystr = "买入";
                }

                int inx17 = html.IndexOf("短期趋势");
                string inxstring7 = html.Substring(inx17, 60).Replace("短期趋势：</span><p>", "");
                inxstring7 = inxstring7.Substring(0, inxstring7.IndexOf("</p>"));

                int inx18 = html.IndexOf("中期趋势");
                string inxstring8 = html.Substring(inx18, 60).Replace("中期趋势：</span><p>", "");
                inxstring8 = inxstring8.Substring(0, inxstring8.IndexOf("</p>"));

                int inx19 = html.IndexOf("长期趋势");
                string inxstring9 = html.Substring(inx19, 60).Replace("长期趋势：</span><p>", "");
                inxstring9 = inxstring9.Substring(0, inxstring9.IndexOf("</p>"));

                string sqlstring = string.Format(@"Insert into zhengu(stock_code,stock_name,score,percent,result,short_des,medium_des,long_dec,record_date,record_time)values(
                                    '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
                                    code, stockname, score, percent, buystr, inxstring7, inxstring8, inxstring9,
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
            catch (Exception ex)
            {
                logger.Info("Zhengu," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}