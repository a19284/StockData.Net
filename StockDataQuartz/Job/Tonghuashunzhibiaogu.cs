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
    public class Tonghuashunzhibiaogu : IJob
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(Tonghuashunzhibiaogu));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("Start job Tonghuashunzhibiaogu");
            string sqlstring = @"SELECT * from tonghuashunzhibiao";
            DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
            DataTable data = ds.Tables[0];

            TonghuashunCommon common = new TonghuashunCommon();
            List<string> proxyUrl = common.GetProxyURL(logger);
            for (int i = 0; i < data.Rows.Count; i++)
            {
                string url = data.Rows[i]["url"].ToString();
                string id = data.Rows[i]["ID"].ToString();
                common.SaveDataJSON(url, id, logger, "tonghuashunzhibiaogu", proxyUrl);
            };
            logger.Info("End job Tonghuashunzhibiaogu");
        }
    }
}