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
    public class Shichangzijinliu : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Shichangzijinliu));

        public void Execute(IJobExecutionContext context)
        {
            //logger.Info("Start job Shichangzijinliu");
            var url = "http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/MoneyFlow.ssi_get_extend?id=3";
            SaveData(url);
            //logger.Info("End job Shichangzijinliu");
        }
        public void SaveData(string URLAddress)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);
                int ind1 = html.IndexOf("(\"");
                html = html.Substring(ind1 + 2);
                html = html.Substring(0, html.Length - 3);

                string[] arr1 = html.Split(new char[] { ',' });
                for (int i = 0; i < arr1.Length; i++)
                {
                    string[] valarr = arr1[i].Split(new char[] { '|' });
                    string code = valarr[6].Substring(2, 6);
                    string sqlstring = string.Format(@"Insert into shichangzijinliu(code,name,indexs,index_change,net_inflow,record_date,record_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                                          code, valarr[3], valarr[4], valarr[5], valarr[1], DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
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
            catch (Exception ex)
            {
                logger.Info("Shichangzijinliu," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}