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
    public class Shichangyidong : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Shichangyidong));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Shichangyidong");

            Random ran = new Random();
            int RandKey = ran.Next(10000, 99999);
            for (int i = 0; i < 200; i++)
            {
                var url = "http://quotes.money.163.com/hs/realtimedata/service/radar.php?host=/hs/realtimedata/service/radar.php&page=" + i + "&fields=CODE,NAME,PRICE,PERCENT,DATE,TYPES,SYMBOL,NUMBER,HSL&sort=DATE&order=desc&count=25&type=query&callback=callback_" + RandKey + "&req=6187";
                SaveData(url, i);
            }
            logger.Info("End job Shichangyidong");
        }
        public void SaveData(string URLAddress,int p)
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
                if (p > int.Parse(data["pagecount"].ToString())) return;

                var list = (object[])data["list"];
                Parallel.For(0, list.Length, i =>
                {
                    Dictionary<string, object> list1 = (Dictionary<string, object>)list[i];

                    string sqlstring = string.Format(@"Insert into shichangyidong(stock_code,stock_name,trade_time,trade_type,trade_info,trade_price,index_change,turnover,title, record_date,record_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}')",
                        list1["SYMBOL"], list1["NAME"], list1["DATE"], list1["TYPE"], list1["INFO"], list1["PRICE"], list1["PERCENT"], list1["HSL"], list1["TITLE"],
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
                logger.Info("Shichangyidong," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}