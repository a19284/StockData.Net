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
    public class Tonghuashunxuangu2 : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Tonghuashunxuangu2));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Tonghuashunxuangu2");
            string sqlstring = @"select * from tonghuashunxuangu where record_date ='"+ DateTime.Today.ToString("yyyy-MM-dd") + "' and firsttitle in ('技术指标')";
            DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            DataTable data = ds.Tables[0];

            TonghuashunCommon common = new TonghuashunCommon();

            Parallel.For(0, data.Rows.Count -1, (i, loopState) =>
            {
                string url = data.Rows[i]["url"].ToString();
                string id = data.Rows[i]["ID"].ToString();
                common.SaveDataJSON(url, id,logger,"tonghuashunxuangu2");
            });
            logger.Info("End job Tonghuashunxuangu2");
        }
    }
}