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

            string sqlstring = @"SELECT secondtitle,ID FROM tonghuashunxuangu where record_date='2016-12-30'";
            DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            DataTable data = ds.Tables[0];
            Dictionary<string, int> dic30 = new Dictionary<string, int>();
            Parallel.For(0, data.Rows.Count, (i, loopState) =>
            {
                dic30.Add(data.Rows[i]["secondtitle"].ToString(), int.Parse(data.Rows[i]["id"].ToString()));
            });

            sqlstring = @"SELECT secondtitle,ID FROM tonghuashunxuangu where record_date='2016-12-29' or record_date='2016-12-28'";
            ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            data = ds.Tables[0];
            Dictionary<int, int> dic29 = new Dictionary<int, int>();
            Parallel.For(0, data.Rows.Count, (i, loopState) =>
            {
                int id = int.Parse(data.Rows[i]["id"].ToString());
                if (dic30.ContainsKey(data.Rows[i]["secondtitle"].ToString()))
                {
                    dic29.Add(id, dic30[data.Rows[i]["secondtitle"].ToString()]);
                }
            });

            Parallel.ForEach(dic29, (item, loopState) =>
            {
                sqlstring =string.Format("update tonghuashunxuangu2 set typeid={0} where temptypeid={1}", item.Value,item.Key);
                Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring);
            });

            Console.WriteLine("The end");
            Console.ReadKey();
        }
    }
}
