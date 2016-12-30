using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TestData
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> msglist = new List<string>();
            try
            {
                string sqlstring1 = string.Format("SELECT count(*) as records from geguchengjiaoliang  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("geguchengjiaoliang：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring1).ToString())));

                string sqlstring2 = string.Format("SELECT count(*) as records from  gegujiaoyi where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("gegujiaoyi：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring2).ToString())));

                string sqlstring3 = string.Format("SELECT count(*) as records from gegupingji  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("gegupingji：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring3).ToString())));

                string sqlstring4 = string.Format("SELECT count(*) as records from geguzijinliu  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("geguzijinliu：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring4).ToString())));

                string sqlstring5 = string.Format("SELECT count(*) as records from hangyezhishu where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("hangyezhishu：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring5).ToString())));

                string sqlstring6 = string.Format("SELECT count(*) as records from shichangyidong  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("shichangyidong：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring6).ToString())));

                string sqlstring7 = string.Format("SELECT count(*) as records from shichangzijinliu  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("shichangzijinliu：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring7).ToString())));

                string sqlstring8 = string.Format("SELECT count(*) as records from shishidadan  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("shishidadan：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring8).ToString())));

                string sqlstring9 = string.Format("SELECT count(*) as records from jishuxuangu  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("jishuxuangu：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring9).ToString())));

                string sqlstring10 = string.Format("SELECT count(*) as records from zhinengxuangu where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("zhinengxuangu：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring10).ToString())));

                string sqlstring11 = string.Format("SELECT count(*) as records from zhinengxuangulist  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("zhinengxuangulist：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring11).ToString())));

                string sqlstring12 = string.Format("SELECT count(*) as records from zhuyaozhishu  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("zhuyaozhishu：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring12).ToString())));

                string sqlstring13 = string.Format("SELECT count(*) as records from shichangjiaoyi  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("shichangjiaoyi：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring13).ToString())));

                string sqlstring14 = string.Format("SELECT count(*) as records from zhengu  where record_date = '{0}'", DateTime.Today.ToString("yyyy-MM-dd"));
                msglist.Add(string.Format("zhengu：{0}", int.Parse(Dbhelper.ExecuteScalar(Dbhelper.Conn, System.Data.CommandType.Text, sqlstring14).ToString())));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            EmailHelper email = new EmailHelper();
            string content = string.Join("<br />", msglist);
            email.SendNoticeEmail("抓数据监测结果", content);
            Console.WriteLine("发送邮件完成。");
            Console.ReadKey();
        }
    }
}
