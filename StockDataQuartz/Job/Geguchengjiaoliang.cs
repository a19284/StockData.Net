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
    /// 
    /// </summary>
    public class Geguchengjiaoliang : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Geguchengjiaoliang));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Geguchengjiaoliang");
            for (int i = 0; i < 10; i++)
            {
                Random ran = new Random();
                int RandKey = ran.Next(10000, 99999);

                var url1 = "http://quotes.money.163.com/hs/realtimedata/service/turnoverupdown.php?host=/hs/realtimedata/service/turnoverupdown.php&page=" + i + "&query=upDown:1&fields=RN,SYMBOL,NAME,PRICE,LB,VOLUME,TURNOVER,HS,,PERCENT,CODE&sort=LB&order=desc&count=50&type=query&callback=callback_534099419&req=" + RandKey;
                SaveData(url1, i, "成交量骤增");
            }
            for (int i = 0; i < 10; i++)
            {
                Random ran = new Random();
                int RandKey = ran.Next(10000, 99999);
                var url2 = " http://quotes.money.163.com/hs/realtimedata/service/turnoverupdown.php?host=/hs/realtimedata/service/turnoverupdown.php&page=" + i + "&query=upDown:-1&fields=RN,SYMBOL,NAME,PRICE,LB,VOLUME,TURNOVER,HS,,PERCENT,CODE&sort=LB&order=asc&count=25&type=query&callback=callback_1506993718&req=" + RandKey;
                SaveData(url2, i, "成交量骤减");
            }
            logger.Info("End job Geguchengjiaoliang");
        }
        public void SaveData(string URLAddress, int p, string leixin)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);
                int ind1 = html.IndexOf("({");
                html = html.Substring(ind1 + 1);
                html = html.Substring(0, html.Length - 1);
                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));

                var list = (object[])data["list"];
                if (p > int.Parse(data["pagecount"].ToString())) return;
                Parallel.For(0, list.Length, i => {
                    Dictionary<string, object> list1 = (Dictionary<string, object>)list[i];
                    List<string> keys = new List<string>();
                    List<string> values = new List<string>();
                    foreach (var item in list1)
                    {
                        keys.Add(item.Key);
                        values.Add("'" + item.Value.ToString() + "'");
                    }

                    string sqlstring = string.Format("Insert into geguchengjiaoliang({0},record_date,record_time,leixin) values({1},'{2}','{3}','{4}')", string.Join(",", keys), string.Join(",", values), DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), leixin);
                    try
                    {
                        Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
                    }
                    catch (Exception ex)
                    {
                        logger.Info(sqlstring);
                        throw new Exception(ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Info("Geguchengjiaoliang," + URLAddress);
                logger.Info(ex.Message);
            }

        }
    }
}