using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LightBoxRectSubForm.data {
    public class WriteXmlHelper {
        public static readonly string FILE_DOC = "doc\\";
        public static readonly string FILE_MODEL = FILE_DOC + "model\\";

        public static readonly string KEY_MODEL = "model";
        public static readonly string KEY_MODEL_ROTATE_ANGLE = "modelRotateAngle";
        public static readonly string KEY_MODEL_WAIT_TIME = "modelWaitTime";
        public static readonly string KEY_MODEL_WAITMM = "modelWaitMM";
        public static readonly string KEY_MODEL_WAITSS = "modelWaitSS";
        public static readonly string KEY_MODEL_SORT_INDEX = "sortIndex";
        public static readonly string KEY_MODEL_ACTIVE = "active";
        public static readonly string KEY_MODEL_RUN_ALL = "runAll";
        public static readonly string KEY_LIGHT_BOX = "lightBox";
        public static readonly string KEY_ID = "id";
        public static readonly string KEY_LINE = "line";
        public static readonly string KEY_ROW = "row";
        public static readonly string KEY_RUN_TIME = "runTime";
        public static readonly string KEY_KEEP_TIME = "keepTime";
        public static readonly string KEY_WAIT_TIME = "waitTime";

        private static XmlDocument doc;
        private static XmlElement root;

        private static void initDoc() {
            doc = new XmlDocument();
            XmlDeclaration xmlDec = doc.CreateXmlDeclaration("1.0", "", "yes");
            doc.PrependChild(xmlDec);
            root = doc.CreateElement(KEY_MODEL);
            doc.AppendChild(root);
        }

        /// <summary>
        /// 写入灯箱配置信息
        /// </summary>
        /// <param name="lbModel">Lb model.</param>
        private static void write(LBModel lbModel) {
            XmlElement xmlModelWaitTime = doc.CreateElement(KEY_MODEL_WAIT_TIME);

            XmlElement xmlModelWaitMM = doc.CreateElement(KEY_MODEL_WAITMM);
            XmlText txtModelWaitMM = doc.CreateTextNode(lbModel.MWaitTime.WaitMM.ToString());
            xmlModelWaitMM.AppendChild(txtModelWaitMM);

            XmlElement xmlModelWaitSS = doc.CreateElement(KEY_MODEL_WAITSS);
            XmlText txtModelWaitSS = doc.CreateTextNode(lbModel.MWaitTime.WaitSS.ToString());
            xmlModelWaitSS.AppendChild(txtModelWaitSS);
            xmlModelWaitTime.AppendChild(xmlModelWaitMM);
            xmlModelWaitTime.AppendChild(xmlModelWaitSS);

            root.AppendChild(xmlModelWaitTime);

            XmlElement xmlModelSortIndex = doc.CreateElement(KEY_MODEL_SORT_INDEX);
            XmlText txtModelSortIndex = doc.CreateTextNode(lbModel.sortIndex.ToString());
            xmlModelSortIndex.AppendChild(txtModelSortIndex);
            root.AppendChild(xmlModelSortIndex);

            XmlElement xmlModelActive = doc.CreateElement(KEY_MODEL_ACTIVE);
            XmlText txtModelActive = doc.CreateTextNode(lbModel.Active.ToString());
            xmlModelActive.AppendChild(txtModelActive);
            root.AppendChild(xmlModelActive);

            XmlElement xmlRunAll = doc.CreateElement(KEY_MODEL_RUN_ALL);
            XmlText txtRunAll = doc.CreateTextNode(lbModel.runAll.ToString());
            xmlRunAll.AppendChild(txtRunAll);
            root.AppendChild(xmlRunAll);

            foreach (LBMsg lbMsg in lbModel.ListLBMsg) {
                XmlElement xmlLightBox = doc.CreateElement(KEY_LIGHT_BOX);

                XmlElement xmlId = doc.CreateElement(KEY_ID);
                XmlText txtlbAddr = doc.CreateTextNode(lbMsg.Id.ToString());
                xmlId.AppendChild(txtlbAddr);

                XmlElement xmlLBX = doc.CreateElement(KEY_LINE);
                XmlText txtlbRotateAngle = doc.CreateTextNode(lbMsg.Row1.ToString());
                xmlLBX.AppendChild(txtlbRotateAngle);

                XmlElement xmlLBY = doc.CreateElement(KEY_ROW);
                XmlText txtlbRotateSpeed = doc.CreateTextNode(lbMsg.Column.ToString());
                xmlLBY.AppendChild(txtlbRotateSpeed);

                XmlElement xmlRunTime = doc.CreateElement(KEY_RUN_TIME);
                XmlText txtlbRunTime = doc.CreateTextNode(lbMsg.RunTime.ToString());
                xmlRunTime.AppendChild(txtlbRunTime);

                XmlElement xmlKeepTime = doc.CreateElement(KEY_KEEP_TIME);
                XmlText txtlbKeepTime = doc.CreateTextNode(lbMsg.KeepTime.ToString());
                xmlKeepTime.AppendChild(txtlbKeepTime);

                XmlElement xmlWaitTime = doc.CreateElement(KEY_WAIT_TIME);
                XmlText txtlbWaitTime = doc.CreateTextNode(lbMsg.WaitTime.ToString());
                xmlWaitTime.AppendChild(txtlbWaitTime);

                xmlLightBox.AppendChild(xmlId);
                xmlLightBox.AppendChild(xmlLBX);
                xmlLightBox.AppendChild(xmlLBY);
                xmlLightBox.AppendChild(xmlRunTime);
                xmlLightBox.AppendChild(xmlKeepTime);
                xmlLightBox.AppendChild(xmlWaitTime);

                root.AppendChild(xmlLightBox);
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path">路径</param>
        private static void save(string path) {
            doc.Save(path);
        }

        public static void saveModel(LBModel lbModel) {
            initDoc();
            write(lbModel);
            save(FILE_MODEL + lbModel.ModelName + ".xml");
        }

        public static void saveLBConfig(LBModel lbModel) {
            initDoc();
            write(lbModel);
            save(FILE_DOC + "lbConfig.xml");
        }
    }
}
