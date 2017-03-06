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
    public class Shichangjiaoyi : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Shichangjiaoyi));

        public void Execute(IJobExecutionContext context)
        {
            //logger.Info("Start job Shichangjiaoyi");

            Random ran = new Random();
            int RandKey = ran.Next(10000, 99999);
            var url = "http://quotes.money.163.com/hs/service/hsindexrank.php?host=/hs/service/hsindexrank.php&page=0&query=CODE:_in_0000001,0000300,1399006,1399001,0000016&fields=no,SYMBOL,NAME,PRICE,UPDOWN,PERCENT,zhenfu,VOLUME,TURNOVER,YESTCLOSE,OPEN,HIGH,LOW&sort=SYMBOL&order=asc&count=25&type=query&callback=callback_" + RandKey + "&req=6100";
            SaveData(url);
            //logger.Info("End job Shichangjiaoyi");
        }
        public void SaveData(string URLAddress)
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
                Parallel.For(0, list.Length, i =>
                {
                    Dictionary<string, object> list1 = (Dictionary<string, object>)list[i];
                    string sqlstring = string.Format(@"Insert into shichangjiaoyi(stock_code,stock_name,price,updown,percent,deal,record_date,record_time)values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                                                        list1["SYMBOL"], list1["NAME"], list1["PRICE"], list1["UPDOWN"], list1["PERCENT"], list1["TURNOVER"],
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
                });

            }
            catch (Exception ex)
            {
                logger.Info("Shichangjiaoyi," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}