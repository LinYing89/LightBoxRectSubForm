using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.data {
    public class LBMsg : ICloneable {
        public event EventHandler EventIdChanged;

        public bool runAble;

        private int id;

        /// <summary>
        /// 灯箱信息类
        /// </summary>
        public LBMsg() {

        }

        public LBMsg(int id) {
            Id = id;
        }
        /// <summary>
        /// 灯箱的ID
        /// </summary>
        /// <value>The identifier.</value>
        public int Id {
            get {
                return id;
            }
            set {
                if (id != value) {
                    id = value;
                    if (null != EventIdChanged) {
                        EventIdChanged(this, null);
                    }
                }
            }
        }

        /// <summary>
        /// 延迟时间,单位s
        /// </summary>
        /// <value>The wait time.</value>
        public double WaitTime { get; set; }

        public int Row1 { get; set; }

        public int Column { get; set; }

        public int GroupId { get; set; }

        private double keepTime = 4;

        private double runTime = 8;
        /// <summary>
        /// 灯箱正转的时间, 单位秒
        /// </summary>
        public double RunTime {
            get {
                return runTime;
            }

            set {
                runTime = value;
            }
        }

        /// <summary>
        /// 灯箱正转完成后维持现状的时间, 单位秒
        /// </summary>
        public double KeepTime {
            get {
                return keepTime;
            }

            set {
                keepTime = value;
            }
        }

        #region ICloneable implementation

        public object Clone() {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
