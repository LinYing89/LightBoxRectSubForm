using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.app {

    public class BoxStateEventArgs : EventArgs {

        private string[] stateInfo = { "正常", "未安装点击", "触发限位", "温度过低", "温度过高", "电压过低", "电压过高", "电流过大", "参数异常" };
        public BoxStateEventArgs() { }

        public BoxStateEventArgs(int boxId, int stateCode) {
            this.boxId = boxId;
            this.stateCode = stateCode;
        }

        public int boxId;
        public int stateCode;

        public string getStateInfo() {
            return stateInfo[stateCode];
        }
    }
}
