using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TestData
{
    class Program
    {
        static void Main(string[] args)
        {
            string sqlstring = string.Format(@"SELECT stock_code,stock_name,count(1) FROM tonghuashunxuangu2
                                    WHERE record_date = '{0}' GROUP BY stock_code,stock_name ORDER BY count(1) DESC", DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"));
            DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            DataTable data = ds.Tables[0];

            List<string> zuheList = new List<string>();
            string sqlstring3 = string.Format(@"SELECT * from xuanguzuhe");
            DataSet ds3 = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring3, null);
            DataTable data3 = ds3.Tables[0];
            for (int i = 0; i < data3.Rows.Count; i++)
            {
                zuheList.Add(data3.Rows[i]["ids"].ToString());
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                var code = data.Rows[i]["stock_code"].ToString();

                sqlstring = string.Format(@"SELECT stock_code,stock_name,typeid FROM tonghuashunxuangu2
                                    WHERE record_date = '{0}' and stock_code='{1}' order by typeid", DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), code);
                DataSet ds2 = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
                DataTable data2 = ds2.Tables[0];

                Console.WriteLine(code);

                List<int> typeidList = new List<int>();
                for (int j = 0; j < data2.Rows.Count; j++)
                {
                    int typeid = int.Parse(data2.Rows[j]["typeid"].ToString());
                    if (!typeidList.Contains(typeid))
                    {
                        typeidList.Add(typeid);
                    }
                }
                string zuheids = string.Join(",", typeidList.ToArray());

                if (!zuheList.Contains(zuheids))
                {
                    zuheList.Add(zuheids);
                    string sqlstring2 = string.Format(@"Insert into xuanguzuhe(name,ids,number)values('选股组合','{0}',{1})", zuheids, typeidList.Count);
                    Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring2);
                }

                string sqlstring4 = string.Format(@"Insert into xuanguzuhegu(zuheids,stock_code,stock_name,record_date,record_time)values('{0}','{1}','{2}','{3}','{4}')",
                    zuheids, data2.Rows[0]["stock_code"].ToString(), data2.Rows[0]["stock_name"].ToString(),
                    DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring4);
            }

            Console.WriteLine("The end");
            Console.ReadKey();
        }
        static void SaveData(string URLAddress)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.GetEncoding("UTF-8");
            try
            {
                string html = client.DownloadString(URLAddress);
                var serializer = new JavaScriptSerializer();
                Dictionary<string, object> data = (Dictionary<string, object>)serializer.Deserialize(html, typeof(object));
                Dictionary<string, object> suggest =(Dictionary<string, object>)data["suggest"];
                
                foreach (var item in suggest)
                {
                    string value = item.Value.ToString();
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                    htmlDoc.LoadHtml(value);
                    HtmlNode Node1 = htmlDoc.DocumentNode;
                    var linodes = Node1.SelectSingleNode("child::ul[1]").SelectNodes("child::li");
                    for (int i = 1; i < linodes.Count; i++)
                    {
                        var lititle = linodes[i].SelectSingleNode("child::a[1]").Attributes["title"].Value;
                        var url = linodes[i].SelectSingleNode("child::a[1]").Attributes["href"].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Zhinengxuangu1," + URLAddress);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
