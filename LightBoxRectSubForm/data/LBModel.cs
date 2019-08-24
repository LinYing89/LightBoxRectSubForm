using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.data {
    public class LBModel : ICloneable, IComparable<LBModel> {
        public event EventHandler EventSelected;

        private ModelWaitTime mWaitTime;

        private List<LBMsg> listLBMsg;

        public List<double> listWaitTime;

        public bool runAll;

        //是否可以运行
        private bool active = true;
        //是否被选中
        private bool selected = false;

        private int endCount;

        public int sortIndex = 0;

        public LBModel() {

        }

        public LBModel(string modelName) : this() {
            ModelName = modelName;
        }

        /// <summary>
        /// 模式名称
        /// </summary>
        /// <value>The name of the model.</value>
        public string ModelName { get; set; }

        /// <summary>
        /// 模式停留时间
        /// </summary>
        /// <value>The M wait time.</value>
        public ModelWaitTime MWaitTime {
            get {
                if (mWaitTime == null) {
                    mWaitTime = new ModelWaitTime();
                }
                return mWaitTime;
            }
            set {
                mWaitTime = value;
            }
        }
        /// <summary>
        /// 模式下灯箱信息集合
        /// </summary>
        /// <value>The list domido message.</value>
        public List<LBMsg> ListLBMsg {
            get {
                if (listLBMsg == null) {
                    listLBMsg = new List<LBMsg>();
                }
                return listLBMsg;
            }
            set {
                listLBMsg = value;
            }
        }

        public bool Active {
            get {
                return active;
            }

            set {
                active = value;
            }
        }

        public bool Selected {
            get {
                return selected;
            }

            set {
                selected = value;
                if (selected) {
                    if (null != EventSelected) {
                        EventSelected(this, null);
                    }
                }
            }
        }

        /// <summary>
        /// 获得停留时间的毫秒数
        /// </summary>
        /// <returns>The wait time M.</returns>
        public int getWaitTimeMS() {
            return (int)(mWaitTime.WaitMM * 60 + mWaitTime.WaitSS) * 1000;
        }

        /// <summary>
        /// 获取等待时间集合,单位s
        /// </summary>
        /// <returns></returns>
        public List<double> getListWaitTime() {
            listWaitTime = new List<double>();
            foreach (LBMsg lbMsg in this.ListLBMsg) {
                double waitTime = lbMsg.WaitTime;
                if (!listWaitTime.Contains(waitTime)) {
                    listWaitTime.Add(waitTime);
                }
            }
            listWaitTime.Sort();
            return listWaitTime;
        }

        /// <summary>
        /// 获取ID对应的灯箱，如果没有返回NULL
        /// </summary>
        /// <returns>The light box message.</returns>
        /// <param name="id">Identifier.</param>
        public LBMsg getLightBoxMsg(int id) {
            foreach (LBMsg lb in ListLBMsg) {
                if (lb.Id == id) {
                    return lb;
                }
            }
            return null;
        }

        public int getMinId() {
            int id = ListLBMsg[0].Id;
            foreach (LBMsg lb in ListLBMsg) {
                if (lb.Id < id) {
                    id = lb.Id;
                }
            }
            return id;
        }

        /// <summary>
        /// 获取行列对应的灯箱，如果没有返回NULL
        /// </summary>
        /// <returns>The light box message.</returns>
        /// <param name="id">Identifier.</param>
        public LBMsg getLightBoxMsg(int line, int row) {
            foreach (LBMsg lb in ListLBMsg) {
                if (lb.Row1 == line && lb.Column == row) {
                    return lb;
                }
            }
            return null;
        }

        /// <summary>
        /// 一个灯箱运行结束
        /// </summary>
        public void endCountAdd() {
            endCount++;
        }

        public void cleanEndCount() {
            endCount = 0;
        }

        /// <summary>
        /// 模式是否运行结束
        /// </summary>
        /// <returns><c>true</c>, if run end was ised, <c>false</c> otherwise.</returns>
        public bool isRunEnd() {
            if (endCount >= ListLBMsg.Count) {
                return true;
            } else {
                return false;
            }
        }



        #region ICloneable implementation
        public object Clone() {
            LBModel model = new LBModel();
            for (int i = 0; i < ListLBMsg.Count; i++) {
                model.ListLBMsg.Add((LBMsg)ListLBMsg[i].Clone());
            }
            return model;
        }

        public int CompareTo(LBModel other) {
            return this.sortIndex.CompareTo(other.sortIndex);
        }
        #endregion
    }
}
