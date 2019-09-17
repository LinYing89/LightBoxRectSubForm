using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.netty {
    public class NettyHelper {
        public static bool SERVER_STARTING = false;

        private static IEventLoopGroup bossGroup;
        private static IEventLoopGroup workerGroup;
        private static IChannel boundChannel;

        static async Task RunServerAsync() {
            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup();
            try {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<TcpServerSocketChannel>();
                bootstrap.Option(ChannelOption.SoKeepalive, true);
                bootstrap
                    .Option(ChannelOption.SoBacklog, 100)
                    .Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel => {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("echo", new EchoServerHandler());
                    }));

                boundChannel = await bootstrap.BindAsync(9001);
                //Console.ReadLine();
                //await boundChannel.CloseAsync();
            } finally {
                //await Task.WhenAll(
                //    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                //    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        public static void stop() {
            boundChannel.CloseAsync();
            bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            SERVER_STARTING = false;
        }

        public static void start() {
            //RunServerAsync().Wait();
            //RunServerAsync().Start();
            Thread th = new Thread(new ThreadStart(startTask));
            th.Name = "netty";
            th.IsBackground = true;
            th.Start();
            SERVER_STARTING = true;
        }

        public static void startTask() {
            RunServerAsync().Wait();
        }
    }
}
