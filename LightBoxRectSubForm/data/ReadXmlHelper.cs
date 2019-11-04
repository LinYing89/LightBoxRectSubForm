using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LightBoxRectSubForm.data {
    public class ReadXmlHelper {
        /// <summary>
        /// 获取模式下所有灯箱的信息
        /// </summary>
        /// <returns></returns>
        public static LBModel getListBoxMsgs(string fileName) {
            LBModel lbModel = new LBModel();
            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            XmlNode rootElement = document.SelectSingleNode(WriteXmlHelper.KEY_MODEL);

            List<LBMsg> listLightBoxMsg = new List<LBMsg>();
            //try {

            XmlNode xmlModelWaitTime = rootElement.SelectSingleNode(WriteXmlHelper.KEY_MODEL_WAIT_TIME);
            XmlNode xmlModelWaitMM = xmlModelWaitTime.SelectSingleNode(WriteXmlHelper.KEY_MODEL_WAITMM);
            lbModel.MWaitTime.WaitMM = Convert.ToDouble(xmlModelWaitMM.InnerText);

            XmlNode xmlModelWaitSS = xmlModelWaitTime.SelectSingleNode(WriteXmlHelper.KEY_MODEL_WAITSS);
            lbModel.MWaitTime.WaitSS = Convert.ToDouble(xmlModelWaitSS.InnerText);

            XmlNode xmlModelSortIndex = rootElement.SelectSingleNode(WriteXmlHelper.KEY_MODEL_SORT_INDEX);
            lbModel.sortIndex = Convert.ToInt32(xmlModelSortIndex.InnerText);

            XmlNode xmlModelActive = rootElement.SelectSingleNode(WriteXmlHelper.KEY_MODEL_ACTIVE);
            if (null != xmlModelActive) {
                lbModel.Active = Convert.ToBoolean(xmlModelActive.InnerText);
            }

            XmlNode xmlRunAll = rootElement.SelectSingleNode(WriteXmlHelper.KEY_MODEL_RUN_ALL);
            if (null != xmlRunAll) {
                lbModel.runAll = Convert.ToBoolean(xmlRunAll.InnerText);
            }

            XmlNodeList xmlLightBoxMsgs = rootElement.SelectNodes(WriteXmlHelper.KEY_LIGHT_BOX);

            foreach (XmlNode xmlLBMsg in xmlLightBoxMsgs) {
                LBMsg lbmsg = new LBMsg();

                XmlNode xmlLBAddr = xmlLBMsg.SelectSingleNode(WriteXmlHelper.KEY_ID);
                lbmsg.Id = Convert.ToInt16(xmlLBAddr.InnerText);

                XmlNode xmlLBAddrNum = xmlLBMsg.SelectSingleNode(WriteXmlHelper.KEY_LINE);
                lbmsg.Row1 = Convert.ToInt16(xmlLBAddrNum.InnerText);

                XmlNode xmlRotateSpeed = xmlLBMsg.SelectSingleNode(WriteXmlHelper.KEY_ROW);
                lbmsg.Column = Convert.ToInt16(xmlRotateSpeed.InnerText);

                XmlNode xmlRunTime = xmlLBMsg.SelectSingleNode(WriteXmlHelper.KEY_RUN_TIME);
                lbmsg.RunTime = Convert.ToDouble(xmlRunTime.InnerText);

                XmlNode xmlKeepTime = xmlLBMsg.SelectSingleNode(WriteXmlHelper.KEY_KEEP_TIME);
                lbmsg.KeepTime = Convert.ToDouble(xmlKeepTime.InnerText);

                XmlNode xmlWaitTime = xmlLBMsg.SelectSingleNode(WriteXmlHelper.KEY_WAIT_TIME);
                lbmsg.WaitTime = Convert.ToDouble(xmlWaitTime.InnerText);

                XmlNode xmlRepeatCount = xmlLBMsg.SelectSingleNode(WriteXmlHelper.KEY_REPEAT_COUNT);
                if (null != xmlRepeatCount) { 
                    lbmsg.RepeatCount = Convert.ToInt16(xmlRepeatCount.InnerText);
                }

                listLightBoxMsg.Add(lbmsg);
            }
            //} catch (NullReferenceException) {
            //    MessageBox.Show("文件格式不正确");
            //}
            lbModel.ListLBMsg = listLightBoxMsg;
            return lbModel;
        }

    }
}
