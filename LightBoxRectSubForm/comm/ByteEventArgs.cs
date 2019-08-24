using LightBoxRectSubForm.app;
using LightBoxRectSubForm.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.comm {

    public class ByteEventArgs : EventArgs {

        public BytesWithInfo bi;

        public DateTime time;

        public ByteEventArgs(BytesWithInfo bi) {
            this.bi = bi;
        }

        public ByteEventArgs(DateTime time, BytesWithInfo bi) {
            this.time = time;
            this.bi = bi;
        }

        public string getBytesString() {
            return Untils.ToHexString(bi.bytes);
        }

        public override string ToString() {
            return time.ToLongTimeString() + " - " + bi.info + " - " + getBytesString();
        }
    }
}
