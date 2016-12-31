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
    public class Tonghuashunzhibiaogu60 : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(Tonghuashunzhibiaogu60));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Tonghuashunzhibiaogu60");
            string sqlstring = @"SELECT * from tonghuashunzhibiao where title like '%60%'";
            DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            DataTable data = ds.Tables[0];

            TonghuashunCommon common = new TonghuashunCommon();
            Parallel.For(0, data.Rows.Count, (i, loopState) =>
            {
                string url = data.Rows[i]["url"].ToString();
                string id = data.Rows[i]["ID"].ToString();
                common.SaveDataJSON(url, id, logger, "tonghuashunzhibiaogu");
            });
            logger.Info("End job Tonghuashunzhibiaogu60");
        }
    }
}