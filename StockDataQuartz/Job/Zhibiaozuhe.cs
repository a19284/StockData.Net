using System;
using System.Threading;
using Common.Logging;
using Quartz;
using System.Net;
using HtmlAgilityPack;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockDataQuartz
{
    /// <summary>
    /// A sample job that just prints info on console for demostration purposes.
    /// </summary>
    public class Zhibiaozuhe : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Zhibiaozuhe));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Zhibiaozuhe");
            try
            {
                string sqlstring = string.Format(@"SELECT stock_code,stock_name,count(1) FROM tonghuashunzhibiaogu
                                    WHERE record_date = '{0}' GROUP BY stock_code,stock_name ORDER BY count(1) DESC", DateTime.Today.ToString("yyyy-MM-dd"));
                DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
                DataTable data = ds.Tables[0];

                List<string> zuheList = new List<string>();
                string sqlstring3 = string.Format(@"SELECT * from zhibiaozuhe");
                DataSet ds3 = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring3, null);
                DataTable data3 = ds3.Tables[0];
                for (int i = 0; i < data3.Rows.Count; i++)
                {
                    zuheList.Add(data3.Rows[i]["ids"].ToString());
                }

                sqlstring = string.Format(@"SELECT stock_code,stock_name,typeid FROM tonghuashunzhibiaogu
                                    WHERE record_date = '{0}' order by typeid", DateTime.Today.ToString("yyyy-MM-dd"));
                DataSet ds2 = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
                DataTable data2 = ds2.Tables[0];

                Parallel.For(0, data.Rows.Count, (i, loopState) =>
                {
                    var code = data.Rows[i]["stock_code"].ToString();
                    DataRow[] drs = data2.Select("stock_code='" + code + "'");
                    List<int> typeidList = new List<int>();
                    for (int j = 0; j < drs.Length; j++)
                    {
                        int typeid = int.Parse(drs[j]["typeid"].ToString());
                        if (!typeidList.Contains(typeid))
                        {
                            typeidList.Add(typeid);
                        }
                    }
                    string zuheids = string.Join(",", typeidList.ToArray());

                    if (!zuheList.Contains(zuheids))
                    {
                        zuheList.Add(zuheids);
                        string sqlstring2 = string.Format(@"Insert into zhibiaozuhe(name,ids,number)values('指标组合','{0}',{1})", zuheids, typeidList.Count);
                        Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring2);
                    }

                    string sqlstring4 = string.Format(@"Insert into zhibiaozuhegu(zuheids,stock_code,stock_name,record_date,record_time)values('{0}','{1}','{2}','{3}','{4}')",
                        zuheids, drs[0]["stock_code"].ToString(), drs[0]["stock_name"].ToString(),
                        DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"));
                    Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring4);
                });
            }
            catch (Exception ex)
            {
                logger.Info("发生错误：" + ex.Message);
            }
            logger.Info("End job Zhibiaozuhe");
        }
    }
}