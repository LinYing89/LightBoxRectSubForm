using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using LightBoxRectSubForm.app;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.netty {
    public class EchoServerHandler : ChannelHandlerAdapter {

        public override void ChannelActive(IChannelHandlerContext context) {
            base.ChannelActive(context);
            Console.WriteLine("ChannelActive");
        }

        public override void ChannelRead(IChannelHandlerContext context, object message) {
            var buffer = message as IByteBuffer;
            if (buffer != null) {
                String msg = buffer.ToString(Encoding.UTF8);
                switch (msg) {
                    case "Control:Reset":
                        //复位
                        context.WriteAndFlushAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Control:Reset")));
                        LightBoxHelper.reset();
                        break;
                    case "Control:Pause":
                        //暂停
                        context.WriteAndFlushAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Control:Pause")));
                        MainWindow.ins.pasued();
                        break;
                    case "Control:Stop":
                        //停止
                        context.WriteAndFlushAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Control:Stop")));
                        MainWindow.ins.runStop();
                        break;
                    case "Control:Resume":
                        //继续
                        context.WriteAndFlushAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Control:Resume")));
                        MainWindow.ins.resume();
                        break;
                    case "Page:Prev":
                        //上一页
                        context.WriteAndFlushAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Page:Prev")));
                        LightBoxHelper.prevPage();
                        break;
                    case "Page:Next":
                        //下一页
                        context.WriteAndFlushAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Page:Next")));
                        LightBoxHelper.nextPage();
                        break;
                    default:
                        context.WriteAndFlushAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes(msg)));
                        break;
                }
                Console.WriteLine("Received from client: " + msg);
            }
            //context.WriteAsync(message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ChannelInactive(IChannelHandlerContext context) {
            base.ChannelInactive(context);
            Console.WriteLine("ChannelInactive");
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }

    }
}
