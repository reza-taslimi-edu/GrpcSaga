using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace Shared.Extensions
{
    public static class ExtensionGrpc
    {
        public static GrpcChannelOptions GetDefaultGrpcChannelOptions()
        {
            return new GrpcChannelOptions
            {
                // تنظیمات دلخواه برای کانال
                MaxReceiveMessageSize = 50 * 1024 * 1024, // 50MB
                MaxSendMessageSize = 50 * 1024 * 1024,    // 50MB
                HttpHandler = new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true
                }
            };
        }
    }
}
