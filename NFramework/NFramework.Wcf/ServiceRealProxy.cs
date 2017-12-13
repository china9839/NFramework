using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Wcf
{
    internal sealed class ServiceRealProxy<IService> : RealProxy
         where IService : class
    {
        private EndpointAddress address;
        private Binding binding;

        public ServiceRealProxy(
            EndpointAddress address,
            Binding binding)
            : base(typeof(IService))
        {
            this.address = address;
            this.binding = binding;
        }
        public override IMessage Invoke(IMessage msg)
        {
            IMethodReturnMessage methodReturn = null;
            IMethodCallMessage methodCall = (IMethodCallMessage)msg;
            var client = new ChannelFactory<IService>(binding, address);
            var channel = client.CreateChannel();
            try
            {
                object[] copiedArgs = Array.CreateInstance(typeof(object), methodCall.Args.Length) as object[];
                methodCall.Args.CopyTo(copiedArgs, 0);
                object returnValue = methodCall.MethodBase.Invoke(channel, copiedArgs);
                methodReturn = new ReturnMessage(returnValue,
                                                copiedArgs,
                                                copiedArgs.Length,
                                                methodCall.LogicalCallContext,
                                                methodCall);
                //TODO:Write log
            }
            catch (Exception ex)
            {
                var exception = ex;
                if (ex.InnerException != null)
                    exception = ex.InnerException;
                methodReturn = new ReturnMessage(exception, methodCall);
            }
            finally
            {
                var commObj = client as ICommunicationObject;
                if (commObj != null)
                {
                    try
                    {
                        commObj.Close();
                    }
                    catch (CommunicationException communicationException)
                    {
                        commObj.Abort();
                        throw communicationException;
                    }
                    catch (TimeoutException timeoutException)
                    {
                        commObj.Abort();
                        throw timeoutException;
                    }
                    catch (Exception exception)
                    {
                        commObj.Abort();
                        //TODO:Logging exception
                        throw exception;
                    }
                }
            }
            return methodReturn;
        }
    }
}
