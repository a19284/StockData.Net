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
    /// 个股交易
    /// </summary>
    public class Gegujiaoyi : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Gegujiaoyi));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Gegujiaoyi");
            for (int i = 0; i < 130; i++)
            {
                var url = "http://quotes.money.163.com/hs/service/diyrank.php?host=http%3A%2F%2Fquotes.money.163.com%2Fhs%2Fservice%2Fdiyrank.php&page=" + i.ToString() + "&query=STYPE%3AEQA&fields=NO%2CSYMBOL%2CNAME%2CPRICE%2CPERCENT%2CUPDOWN%2CFIVE_MINUTE%2COPEN%2CYESTCLOSE%2CHIGH%2CLOW%2CVOLUME%2CTURNOVER%2CHS%2CLB%2CWB%2CZF%2CPE%2CMCAP%2CTCAP%2CMFSUM%2CMFRATIO.MFRATIO2%2CMFRATIO.MFRATIO10%2CSNAME%2CCODE%2CANNOUNMT%2CUVSNEWS&sort=PERCENT&order=desc&count=24&type=query";
                SaveData(url);
            }
            logger.Info("End job Gegujiaoyi");
        }
        public void SaveData(string URLAddress)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);

                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));

                var list = (object[])data["list"];
                for (int i = 0; i < list.Length; i++)
                {
                    Dictionary<string, object> list1 = (Dictionary<string, object>)list[i];
                    List<string> keys = new List<string>();
                    List<string> values = new List<string>();
                    foreach (var item in list1)
                    {
                        if (item.Key != "ANNOUNMT" && item.Key != "UVSNEWS")
                        {
                            if (item.Key == "MFRATIO")
                            {
                                Dictionary<string, object> obj = (Dictionary<string, object>)item.Value;
                                foreach (var it in obj)
                                {
                                    keys.Add(it.Key);
                                    values.Add("'" + it.Value.ToString() + "'");
                                }
                            }
                            else
                            {
                                keys.Add(item.Key);
                                values.Add("'" + item.Value.ToString() + "'");
                            }
                        }
                    }

                    string sqlstring = string.Format("Insert into gegujiaoyi({0},record_date,record_time) values({1},'{2}','{3}')", string.Join(",", keys), string.Join(",", values), DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
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
                logger.Info("Gegujiaoyi," + URLAddress);
                logger.Info(ex.Message);
            }

        }
    }
}