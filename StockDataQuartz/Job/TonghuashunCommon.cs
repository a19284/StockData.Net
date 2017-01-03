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
    ///市场资金流
    /// </summary>
    public class TonghuashunCommon
    {
        private DataTable oldData = new DataTable();
        public void SaveDataJSON(string URLAddress, string typeid, ILog logger,string table,DataTable olddata = null)
        {
            WebClient client = new WebClient();
            try
            {

                string html = client.DownloadString(URLAddress);
                int ind1 = html.IndexOf("allResult");
                html = html.Substring(ind1 + 2 + "allResult".Length).Trim();
                int ind2 = html.IndexOf("};");
                html = html.Substring(0, ind2 + 1);

                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));
                if (data.ContainsKey("title") && data.ContainsKey("result"))
                {
                    var title = (object[])data["title"];
                    var list = (object[])data["result"];
                    for (int i = 0; i < list.Length; i++)
                    {
                        object[] list1 = (object[])list[i];
                        List<string> valueList = new List<string>();
                        for (int k = 2; k < list1.Length; k++)
                        {
                            valueList.Add(string.Format("\"{0}\":\"{1}\"", title[k], list1[k].ToString().Replace("'", "")));
                        }
                        string valuesstring = "{" + string.Join(",", valueList) + "}";
                        string stockcode = list1[0].ToString().Split(new char[] { '.' })[0];

                        if (oldData == null || oldData.Select("stock_code = '" + stockcode + "' and typeid = " + typeid).Length == 0)
                        {
                            string sqlstring = string.Format("Insert into " + table + "(stock_code,stock_name,valuestring,typeid,record_date,record_time) values('{0}','{1}','{2}','{3}','{4}','{5}')",
                                stockcode, list1[1].ToString(), valuesstring, typeid, DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));

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
                if (data.ContainsKey("token") && data.ContainsKey("total"))
                {
                    string taken = data["token"].ToString();
                    int total = int.Parse(data["total"].ToString());
                    int perpage = int.Parse(data["perpage"].ToString());
                    int pages = total / perpage;
                    for (int i = 0; i < pages; i++)
                    {
                        int p = i + 2;
                        string url2 = "http://www.iwencai.com/stockpick/cache?token=" + taken + "&p=" + p + "&perpage=" + perpage;
                        SaveDataJSONSecond(url2, typeid, logger,table);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(URLAddress);
                logger.Info(ex.Message);
            }
        }
        private void SaveDataJSONSecond(string URLAddress, string typeid, ILog logger,string table)
        {
            WebClient client = new WebClient();
            try
            {
                string html = client.DownloadString(URLAddress);

                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));
                if (data.ContainsKey("title") && data.ContainsKey("result"))
                {
                    var title = (object[])data["title"];
                    var list = (object[])data["result"];
                    for (int i = 0; i < list.Length; i++)
                    {
                        object[] list1 = (object[])list[i];
                        List<string> valueList = new List<string>();
                        for (int k = 2; k < list1.Length; k++)
                        {
                            valueList.Add(string.Format("\"{0}\":\"{1}\"", title[k], list1[k].ToString().Replace("'", "")));
                        }
                        string valuesstring = "{" + string.Join(",", valueList) + "}";
                        string sqlstring = string.Format("Insert into "+ table +"(stock_code,stock_name,valuestring,typeid,record_date,record_time) values('{0}','{1}','{2}','{3}','{4}','{5}')",
                            list1[0].ToString().Split(new char[] { '.' })[0], list1[1].ToString(), valuesstring, typeid, DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
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
            catch (Exception ex)
            {
                logger.Info(URLAddress);
                logger.Info(ex.Message);
            }
        }
    }
}