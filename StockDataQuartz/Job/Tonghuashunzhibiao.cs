using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace StockDataQuartz.Job
{
    class Tonghuashunzhibiao
    {
        static void Start()
        {
            Console.WriteLine("The Start");
            //List<string> indexname = new List<string> { "KDJ", "MACD", "BOLL", "CCI", "RSI", "WR", "BIAS", "SKDJ", "VR", "MTM", "OBV", "DMI" };
            //for (int i = 0; i < indexname.Count; i++)
            //{
            //    Console.WriteLine(indexname[i]);
            //    var url = "http://www.iwencai.com/stockpick/tipssummary?index_name=" + indexname[i];
            //    SaveDataJSON(url, indexname[i]);
            //}

            List<string> indexname = new List<string> { "平台", "均线形态", "单K线阳线", "单K线阴线", "多K线形态" };
            for (int i = 0; i < indexname.Count; i++)
            {
                Console.WriteLine(indexname[i]);
                var url = "http://www.iwencai.com/stockpick/tipssummary?is_term=1&index_name=" + indexname[i];
                SaveDataXingtai(url, indexname[i]);
            }

            Console.WriteLine("The end");
            Console.ReadKey();
        }
        //技术指标
        static void SaveDataJSON(string URLAddress, string typeid)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");
            try
            {
                string html = client.DownloadString(URLAddress);
                int ind1 = html.IndexOf("data_item.typeData");
                html = html.Substring(ind1 + 2 + "data_item.typeData".Length).Trim();
                int ind2 = html.IndexOf("}];");
                html = html.Substring(0, ind2 + 2);

                var serializer = new JavaScriptSerializer();
                object[] data = (object[])serializer.Deserialize(html, typeof(object));

                string url = "http://www.iwencai.com/stockpick/search?ts=1&tid=stockpick&queryarea=all&qs=hd_ma_all&w=";

                for (int i = 0; i < data.Length; i++)
                {
                    var d = (Dictionary<string, object>)data[i];
                    string typename = d["sub_type_name"].ToString();
                    string summary = d["summary"].ToString();
                    string sub_querys = d["sub_querys"].ToString();
                    string[] querys = sub_querys.Split(new char[] { '_' });

                    for (int j = 0; j < querys.Length; j++)
                    {
                        string tempurl = url + querys[j];
                        string sqlstring = string.Format(@"Insert into tonghuashunzhibiao(indexname,typename,summay,title,url)values('{0}','{1}','{2}','{3}','{4}')",
                                       typeid, typename, summary, querys[j], tempurl);
                        try
                        {
                            Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
                        }
                        catch (Exception ex)
                        {
                            //logger.Info(sqlstring);
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(URLAddress);
                Console.WriteLine(ex.Message);
            }
        }

        //技术指标
        static void SaveDataXingtai(string URLAddress, string typeid)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");
            try
            {
                string html = client.DownloadString(URLAddress);
                int ind1 = html.IndexOf("data_item.typeData");
                html = html.Substring(ind1 + 2 + "data_item.typeData".Length).Trim();
                int ind2 = html.IndexOf("}];");
                html = html.Substring(0, ind2 + 2);

                var serializer = new JavaScriptSerializer();
                object[] data = (object[])serializer.Deserialize(html, typeof(object));

                string url = "http://www.iwencai.com/stockpick/search?ts=1&tid=stockpick&queryarea=all&qs=hd_ma_all&w=";

                for (int i = 0; i < data.Length; i++)
                {
                    var d = (Dictionary<string, object>)data[i];
                    string typename = d["sub_type_name"].ToString();
                    string summary = d["summary"].ToString();
                    string sub_querys = d["sub_querys"].ToString();
                    string[] querys = sub_querys.Split(new char[] { '_' });

                    for (int j = 0; j < querys.Length; j++)
                    {
                        string tempurl = url + querys[j];
                        string sqlstring = string.Format(@"Insert into tonghuashunzhibiao(indexname,typename,summay,title,url)values('{0}','{1}','{2}','{3}','{4}')",
                                       typeid, typename, summary, querys[j], tempurl);
                        try
                        {
                            Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
                        }
                        catch (Exception ex)
                        {
                            //logger.Info(sqlstring);
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(URLAddress);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
