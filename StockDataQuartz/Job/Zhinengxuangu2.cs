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
    public class Zhinengxuangu2 : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Zhinengxuangu2));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Zhinengxuangu2");

            Random ran = new Random();
            long RandKey = ran.Next(10000000, 999999999);
            var url1 = "http://comment.10jqka.com.cn/znxg/formula_stocks_pc.json?_=" + RandKey;
            SaveData(url1);

            logger.Info("End job Zhinengxuangu2");
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
                foreach (var item in data)
                {
                    Dictionary<string, object> valdata = item.Value as Dictionary<string, object>;
                    if (valdata == null) continue;

                    foreach (var item1 in valdata)
                    {
                        var list = item1.Value as object[];
                        if (list != null)
                        {
                            for (int i = 0; i < list.Length; i++)
                            {
                                Dictionary<string, object> list1 = list[i] as Dictionary<string, object>;
                                if (list1 != null)
                                {
                                    string index_change = string.Empty;
                                    int inx = 0;
                                    foreach (var item2 in list1)
                                    {
                                        if (inx == 0)
                                        {
                                            index_change = item2.Value.ToString();
                                        }
                                        inx++;
                                    }

                                    string sqlstring = string.Format(@"Insert into zhinengxuangulist(formulaid,stock_code,stock_name,new_price,index_change,open_price,
                                last_price,selectdate,record_date,record_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
                                        item.Key, list1["code"], list1["name"], list1["10"], index_change, list1["7"], list1["6"], list1["date"],
                                        DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), "");
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
                }
            }
            catch (Exception ex)
            {
                logger.Info("Zhinengxuangu2," + URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}