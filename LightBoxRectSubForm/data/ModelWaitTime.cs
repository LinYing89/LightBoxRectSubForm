using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.data {
    public class ModelWaitTime {
        private double waitMM;
        private double waitSS;

        public ModelWaitTime() {
        }
        /// <summary>
        /// 灯箱停留时间分钟数
        /// </summary>
        /// <value>The wait M.</value>
        public double WaitMM {
            get {
                return waitMM;
            }
            set {
                waitMM = value;
            }
        }
        /// <summary>
        /// 灯箱停留时间秒数
        /// </summary>
        /// <value>The wait S.</value>
        public double WaitSS {
            get {
                return waitSS;
            }
            set {
                waitSS = value;
            }
        }

        public override string ToString() {
            return string.Format("{0}:{1}", WaitMM, WaitSS);
        }
    }
}
