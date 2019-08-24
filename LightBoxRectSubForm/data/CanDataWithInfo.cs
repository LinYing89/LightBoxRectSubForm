using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.data {

    public class CanDataWithInfo : BytesWithInfo{
        public uint canId;

        public CanDataWithInfo(uint canId, byte[] bytes, string info) : base(bytes, info) {
            this.canId = canId;
        }
    }
}
