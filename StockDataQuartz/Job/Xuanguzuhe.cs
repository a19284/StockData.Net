using System;
using System.Threading;
using Common.Logging;
using Quartz;
using System.Net;
using HtmlAgilityPack;
using System.Data;
using System.Collections.Generic;

namespace StockDataQuartz
{
    /// <summary>
    /// A sample job that just prints info on console for demostration purposes.
    /// </summary>
    public class Xuanguzuhe : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Xuanguzuhe));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Xuanguzuhe");
            try
            {
                string sqlstring = string.Format(@"SELECT stock_code,stock_name,count(1) FROM tonghuashunxuangu2
                                    WHERE record_date = '{0}' and typeid IN (
		                                    SELECT ID FROM tonghuashunxuangu  WHERE  record_date = '{0}')
                                    GROUP BY stock_code,stock_name ORDER BY count(1) DESC", DateTime.Today.ToString("yyyy-MM-dd"));
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
                                    WHERE record_date = '{0}' and stock_code='{1}' order by typeid", DateTime.Today.ToString("yyyy-MM-dd"), code);
                    DataSet ds2 = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
                    DataTable data2 = ds2.Tables[0];

                    Console.WriteLine(code);

                    List<int> typeidList = new List<int>();
                    for (int j = 0; j < data2.Rows.Count; j++)
                    {
                        typeidList.Add(int.Parse(data2.Rows[j]["typeid"].ToString()));
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
            }
            catch (Exception ex)
            {
                logger.Info("发生错误：" + ex.Message);
            }
            logger.Info("End job Xuanguzuhe");
        }
    }
}