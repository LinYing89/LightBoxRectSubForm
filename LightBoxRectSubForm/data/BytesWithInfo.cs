using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.data {
    public class BytesWithInfo {
        public byte[] bytes;
        public string info;

        public BytesWithInfo(byte[] bytes, string info) {
            this.bytes = bytes;
            this.info = info;
        }
    }
}
