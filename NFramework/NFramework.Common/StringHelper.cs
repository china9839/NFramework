using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Common
{
    /***************************************************
    * Author  :jony
    * Date    :2017-12
    * Describe:定义一个字符串处理类
    * 
    * 
    ***************************************************/
    public class StringHelper
    {
        /// <summary>
        /// 获取一个随机字符串
        /// </summary>
        /// <param name="len">字符串长度</param>
        /// <returns></returns>
        public static string GetRandomString(int len)
        {
            var str = new TempString();
            return str.GenerateCheckCode(len);
        }
    }

    internal class TempString
    {
        private int rep = 0;
        /// 
        /// 生成随机字母字符串(数字字母混和)
        /// 
        /// 待生成的位数
        /// 生成的字母字符串
        public string GenerateCheckCode(int codeCount)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + this.rep;
            this.rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> this.rep)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }
    }
}
