using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Common
{
    /***************************************************
    * Author  :jony
    * Date    :2017-12
    * Describe:定义一个http请求类
    * 
    * 
    ***************************************************/
    public class HttpHelper : WebClient
    {
        public HttpHelper(int TimeOut)
        {
            this.TimeOut = TimeOut;
        }
        public int TimeOut { get; set; }

        #region 重写GetWebRequest,添加WebRequest对象超时时间
        /// <summary>
        /// 重写GetWebRequest,添加WebRequest对象超时时间
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = 1000 * TimeOut;
            request.ReadWriteTimeout = 1000 * TimeOut;
            return request;
        }
        #endregion

        #region POST提交数据并下载字节数组
        /// <summary>
        /// POST提交数据并下载字节数组
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] PostLoadData(string url, string data)
        {
            this.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            return this.UploadData(url, "POST", this.Encoding.GetBytes(data));
        }

        /// <summary>
        /// POST提交数据并下载字节数组
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] PostLoadData(string url, Dictionary<string, string> param)
        {
            string data = string.Join("&", param.Select(a => a.Key + "=" + System.Web.HttpUtility.UrlEncode(a.Value, this.Encoding)));
            return PostLoadData(url, data);
        }
        #endregion

        #region POST提交数据并下载字符串
        /// <summary>
        /// POST提交数据并下载字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string PostLoadString(string url, string data)
        {
            return this.Encoding.GetString(PostLoadData(url, data));
        }

        /// <summary>
        /// POST提交数据并下载字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string PostLoadString(string url, Dictionary<string, string> param)
        {
            string data = string.Join("&", param.Select(a => a.Key + "=" + System.Web.HttpUtility.UrlEncode(a.Value, this.Encoding)));
            return PostLoadString(url, data);
        }
        #endregion

        #region GET提交数据并下载字节数组
        /// <summary>
        /// GET提交数据并下载字节数组
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] GetLoadData(string url, string data)
        {
            this.Headers.Add("Content-Type", "text/html");
            var _url = url;
            if (!string.IsNullOrEmpty(data))
            {
                _url = _url + "?" + data;
            }
            return this.DownloadData(_url);
        }

        /// <summary>
        /// POST提交数据并下载字节数组
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] GetLoadData(string url, Dictionary<string, string> param)
        {
            string data = string.Join("&", param.Select(a => a.Key + "=" + System.Web.HttpUtility.UrlEncode(a.Value, this.Encoding)));
            return GetLoadData(url, data);
        }
        #endregion

        #region Get提交数据并下载字符串
        /// <summary>
        /// Get提交数据并下载字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetLoadString(string url, string data)
        {
            return this.Encoding.GetString(GetLoadData(url, data));
        }

        /// <summary>
        /// Get提交数据并下载字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetLoadString(string url, Dictionary<string, string> param)
        {
            string data = string.Join("&", param.Select(a => a.Key + "=" + System.Web.HttpUtility.UrlEncode(a.Value, this.Encoding)));
            return GetLoadString(url, data);
        }
        #endregion
    }

    public class ThreadWebClientFactory
    {
        public const string WEBCLIENT = "WebClient";

        #region 获取一个WebClient对象
        /// <summary>
        /// 获取一个WebClient对象
        /// </summary>
        /// <returns></returns>
        public static HttpHelper GetWebClient()
        {
            return GetWebClient(WEBCLIENT);
        }

        /// <summary>
        /// 获取一个WebClient对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static HttpHelper GetWebClient(string key)
        {
            HttpHelper wc = (HttpHelper)CallContext.GetData(key);
            if (wc == null)
            {
                wc = new HttpHelper(100);

                CallContext.SetData(key, wc);
            }
            return wc;
        }

        /// <summary>
        /// 获取一个WebClient对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="time">过期时间 ，单位秒</param>
        /// <returns></returns>
        public static HttpHelper GetWebClient(string key, int time)
        {
            HttpHelper wc = (HttpHelper)CallContext.GetData(key);
            if (wc == null)
            {
                wc = new HttpHelper(time);

                CallContext.SetData(key, wc);
            }
            return wc;
        }
        #endregion
    }
}
