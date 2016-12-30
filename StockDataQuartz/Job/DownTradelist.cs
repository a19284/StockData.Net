using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Quartz;

namespace StockDataQuartz
{
    /// <summary>
    /// 下载历史交易记录
    /// </summary>
    public class DownTradelist:IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DownTradelist));
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                string path = Environment.CurrentDirectory + "\\stock_data";
                WebClient client = new WebClient();

                string sqlstring = @"select DISTINCT stock_code from hangyestockinfo 
                                where stock_code not in(select DISTINCT stock_code from StockDataQuartz)";
                DataSet ds = Dbhelper.ExecuteDataset(Dbhelper.Conn, CommandType.Text, sqlstring, null);
                DataTable data = ds.Tables[0];
                List<string> nodatacode = new List<string>();

                Console.WriteLine("The first time download data.");
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    Console.WriteLine(i.ToString() + "、" + data.Rows[i][0].ToString());
                    string url = "http://quotes.money.163.com/service/chddata.html?code=0" + data.Rows[i][0].ToString() + "&start=20000101&end=20161208&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;TURNOVER;VOTURNOVER;VATURNOVER;TCAP;MCAP";

                    string fileName;
                    if (HttpDownloadFile(client, url, path, data.Rows[i][0].ToString(), out fileName))
                    {
                        nodatacode.Add(data.Rows[i][0].ToString());
                    }
                    else
                    {
                        SaveFile(fileName);
                    }
                }
                Console.WriteLine("The second time download data.");
                for (int i = 0; i < nodatacode.Count; i++)
                {
                    Console.WriteLine(i.ToString() + "、" + nodatacode[i]);
                    string url = "http://quotes.money.163.com/service/chddata.html?code=1" + nodatacode[i] + "&start=20000101&end=20161208&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;TURNOVER;VOTURNOVER;VATURNOVER;TCAP;MCAP";
                    string fileName;
                    if (HttpDownloadFile(client, url, path, nodatacode[i], out fileName))
                    {
                        nodatacode.Add(nodatacode[i]);
                    }
                    else
                    {
                        SaveFile(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred." + ex.Message);
            }
            Console.WriteLine("download data done");

            Console.ReadKey();
        }
        public void SaveFile(string filePath)
        {
            if (!File.Exists(filePath)) return;
            string[] content = File.ReadAllLines(filePath, Encoding.Default);
            for (int j = 1; j < content.Length; j++)
            {
                string[] valarr = content[j].Split(new char[] { ',' });
                for (int k = 0; k < valarr.Length; k++)
                {
                    if (k == 1)
                    {
                        valarr[k] = valarr[k] + "'";
                    }
                    else
                    {
                        valarr[k] = "'" + valarr[k] + "'";
                    }
                }

                string valstring = string.Join(",", valarr);

                string sqlstring = "Insert into StockDataQuartz values(" + valstring + ")";
                if (!sqlstring.Contains("none"))
                {
                    try
                    {
                        Dbhelper.ExecuteNonQuery(Dbhelper.Conn, CommandType.Text, sqlstring, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Http下载文件
        /// </summary>
        public bool HttpDownloadFile(WebClient client, string URLAddress, string receivePath, string code, out string filePath)
        {
            bool nodata = false;
            filePath = string.Empty;

            using (Stream str = client.OpenRead(URLAddress))
            {
                using (StreamReader reader = new StreamReader(str))
                {
                    byte[] mbyte = new byte[1000000];
                    int allmybyte = (int)mbyte.Length;
                    int startmbyte = 0;

                    while (allmybyte > 0)
                    {

                        int m = str.Read(mbyte, startmbyte, allmybyte);
                        if (m == 0)
                            break;

                        startmbyte += m;
                        allmybyte -= m;
                    }
                    if (startmbyte < 120)
                    {
                        nodata = true;
                    }
                    reader.Dispose();
                    str.Dispose();

                    filePath = receivePath + "\\" + code + ".csv";
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    using (FileStream fstr = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        fstr.Write(mbyte, 0, startmbyte);
                        fstr.Flush();
                        fstr.Close();
                        FileInfo fi = new FileInfo(filePath);
                        long len = fi.Length;
                    }
                }
            }

            return nodata;
        }
    }
}
