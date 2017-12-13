using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Common
{
      /***************************************************
      * Author  :jony
      * Date    :2017-12
      * Describe:定义一个日志类
      * 
      * 
      ***************************************************/
    public class LogHelper
    {
        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");
        public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");

        public static void Init()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void Init(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        /// <summary>
        /// 普通的文件记录日志
        /// </summary>
        /// <param name="info"></param>
        public static void Info(string info)
        {
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(info);
            }
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void Error(string info, Exception exp)
        {
            if (logerror.IsErrorEnabled)
            {
                logerror.Error(info, exp);
            }
        }

        /// <summary>
        /// 一个日志写入方法。。写入到一个txt中，按照日期分类
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="directPath"></param>
        public static void WriteSimpleLog(string logText,string directPath = "")
        {
            StreamWriter streamWriter = null; //写文件
            try
            {
                if (string.IsNullOrEmpty(directPath))
                {
                    directPath = Path.Combine(System.Environment.CurrentDirectory,"logs");
                }
                if (!Directory.Exists(directPath))   //判断文件夹是否存在，如果不存在则创建  
                {
                    Directory.CreateDirectory(directPath);
                }
                directPath += string.Format(@"\{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
                if (streamWriter == null)
                {
                    streamWriter = !File.Exists(directPath) ? File.CreateText(directPath) : File.AppendText(directPath);    //判断文件是否存在如果不存在则创建，如果存在则添加。  
                }
                streamWriter.WriteLine("***********************************************************************");
                streamWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
                streamWriter.WriteLine("输出信息：");
                if (logText != null)
                {
                    streamWriter.WriteLine(logText);
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Flush();
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
        }
    }
}
