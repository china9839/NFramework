using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Serialization;
using WebServiceCore = System.Web.Services.Description;

namespace NFramework.Common
{
    /***************************************************
    * Author  :jony
    * Date    :2018-01
    * Describe:定义一个动态调用webservice的类
    * 
    * 
    ***************************************************/
    public class WSHelper
    {
        /// <summary>
        /// 实例化WebServices
        /// </summary>
        /// <param name="url">WebServices地址</param>
        /// <param name="methodname">调用的方法</param>
        /// <param name="args">把webservices里需要的参数按顺序放到这个object[]里</param>
        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            string @namespace = "WSTools.Client";
            try
            {
                //获取WSDL
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url);
                WebServiceCore.ServiceDescription sd = WebServiceCore.ServiceDescription.Read(stream);
                string classname = sd.Services[0].Name;
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);

                //生成客户端代理类代码
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider csc = new CSharpCodeProvider();

                //设定编译参数
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");

                //编译代理类
                CompilerResults cr = csc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }

                //生成代理实例，并调用方法
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                var parameters = mi.GetParameters();

                return mi.Invoke(obj, args);
            }
            catch (Exception ex)
            {
                return args[2] = ex.Message + ex.InnerException != null ? ex.InnerException.Message : "";
            }
        }
    }
}
