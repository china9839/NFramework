using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Wcf
{
    public static class WCFFactory
    {
        private static Dictionary<string, ServiceConfig> serviceConfigList
            = new Dictionary<string, ServiceConfig>();

        #region WCF服务工厂
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">指定服务类型</typeparam>
        /// <param name="serviceEndPoint">指定服务的终结点名称(一个服务可以发布多个终结点)</param>
        /// <returns></returns>
        public static T CreateService<T>(string serviceEndPoint)
            where T : class
        {
            try
            {
                var returnList = LoadClientServiceModel<T>(serviceEndPoint);
                //利用代理生产服务对象，aop处理日志及关闭代理
                var serviceRealProxy = new ServiceRealProxy<T>(returnList.Item1, returnList.Item2);
                return serviceRealProxy.GetTransparentProxy() as T;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        /// <summary>
        /// 加载客户端WCF配置文件，生成终结点和绑定实例
        /// </summary>
        /// <returns></returns>
        private static Tuple<EndpointAddress, Binding> LoadClientServiceModel<T>(string serviceEndPoint)
            where T : class
        {
            ServiceConfig serviceConfig;
            if (serviceConfigList.ContainsKey(serviceEndPoint))
            {
                serviceConfig = serviceConfigList[serviceEndPoint];
            }
            else
            {
                serviceConfig = new ServiceConfig();
                Configuration config = GetConfig();
                var serviceModel = ServiceModelSectionGroup.GetSectionGroup(config);
                //找到指定的客户端服务终结点名称
                var endpoint = serviceModel.Client.Endpoints.Cast<ChannelEndpointElement>().SingleOrDefault(e => e.Name.Equals(serviceEndPoint));
                if (endpoint == null)
                {
                    throw new Exception("没有找到client配置中的有关WCF终结点配置信息.");
                }
                serviceConfig.channelEndpointElement = endpoint;
                //获取bindingConfiguration
                var bindingElement = serviceModel.Bindings.BindingCollections.SingleOrDefault(bb => bb.BindingName.Equals(endpoint.Binding));
                if (!string.IsNullOrEmpty(endpoint.BindingConfiguration))
                {
                    var bindingConfigurationElement =
                        bindingElement.ConfiguredBindings.Single<IBindingConfigurationElement>(c => c.Name.Equals(endpoint.BindingConfiguration));
                    serviceConfig.bindingConfigurationElement = bindingConfigurationElement;
                }
                //serviceConfigList.Add(serviceEndPoint, serviceConfig);
                serviceConfigList[serviceEndPoint] = serviceConfig;
            }
            //根据终结点配置信息生成绑定
            Binding binding = CreateBindingByName(serviceConfig.channelEndpointElement.Binding);
            var address = new EndpointAddress(serviceConfig.channelEndpointElement.Address);
            if (serviceConfig.bindingConfigurationElement != null)
            {
                serviceConfig.bindingConfigurationElement.ApplyConfiguration(binding);
            }
            var address_binding = new Tuple<EndpointAddress, Binding>(address, binding);
            return address_binding;
        }

        /// <summary>
        /// 获得指定配置文件对象
        /// </summary>
        /// <returns></returns>
        private static Configuration GetConfig()
        {
            var pathString = Path.Combine(GetDllPath(), "wcfclient.config");
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = pathString;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            return config;
        }

        /// <summary>
        /// 获得wcf工厂程序集路径
        /// </summary>
        /// <returns></returns>
        public static string GetDllPath()
        {
            string dllpath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            dllpath = dllpath.Substring(8, dllpath.Length - 8);
            return System.IO.Path.GetDirectoryName(dllpath);
        }

        /// <summary>
        /// 根据绑定协议创建具体绑定对象
        /// </summary>
        /// <param name="bindingName"></param>
        /// <returns></returns>
        private static Binding CreateBindingByName(string bindingName)
        {
            Binding binding = null;
            switch (bindingName)
            {
                case BindingName.NetTcpBinding:
                    binding = new NetTcpBinding();
                    break;
                case BindingName.BasicHttpBinding:
                    binding = new BasicHttpBinding();
                    break;
                case BindingName.WSHttpBinding:
                    binding = new WSHttpBinding();
                    break;
                case BindingName.WSDualHttpBinding:
                    binding = new WSDualHttpBinding();
                    break;
                case BindingName.WSFederationHttpBinding:
                    binding = new WSFederationHttpBinding();
                    break;
                case BindingName.NetNamedPipeBinding:
                    binding = new NetNamedPipeBinding();
                    break;
                case BindingName.NetMsmqBinding:
                    binding = new NetMsmqBinding();
                    break;
                case BindingName.NetPeerTcpBinding:
                    binding = new NetPeerTcpBinding();
                    break;
                case BindingName.CustomBinding:
                    binding = new CustomBinding();
                    break;
                default:
                    binding = new NetTcpBinding();
                    break;
            }
            return binding;
        }
    }

    public class ServiceConfig
    {
        public ChannelEndpointElement channelEndpointElement { get; set; }
        public IBindingConfigurationElement bindingConfigurationElement { get; set; }
    }

    public class BindingName
    {
        public const string NetTcpBinding = "netTcpBinding";
        public const string BasicHttpBinding = "basicHttpBinding";
        public const string WSHttpBinding = "wsHttpBinding";
        public const string WSDualHttpBinding = "wsDualHttpBinding";
        public const string WSFederationHttpBinding = "wsFederationHttpBinding";
        public const string NetNamedPipeBinding = "netNamedPipeBinding";
        public const string NetMsmqBinding = "netMsmqBinding";
        public const string NetPeerTcpBinding = "netPeerTcpBinding";
        public const string MsmqIntegrationBinding = "msmqIntegrationBinding";
        public const string CustomBinding = "customBinding";
    }
}
