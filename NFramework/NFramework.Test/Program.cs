using NFramework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            //初始化log4g
            LogHelper.Init();
            LogHelper.WriteSimpleLog("hello world!!!!!!");
            Console.ReadLine();
        }
    }
}
