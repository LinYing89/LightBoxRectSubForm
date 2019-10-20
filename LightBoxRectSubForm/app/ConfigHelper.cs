using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.app {
    class ConfigHelper {
        /// <summary>
		/// 配置文件名
		/// </summary>
		public static string fileName = "doc\\config.ini";
        public static string logFileName = "doc\\log.ini";

        /// <summary>
        /// 灯箱段落名
        /// </summary>
        public static string LightBoxNote = "LIGHT_BOX";
        /// <summary>
        /// 灯箱行
        /// </summary>
        public static string ROWS = "rows";
        /// <summary>
        /// 灯箱列
        /// </summary>
        public static string COLUMNS = "columns";
        public static string powerOnA = "powerOnA";
        public static string powerOffA = "powerOffA";
        public static string powerOnB = "powerOnB";
        public static string powerOffB = "powerOffB";

        public static string phoneNum = "phoneNum";
        public static string smsModel = "smsModel";

        /// <summary>
        /// 软件段落名
        /// </summary>
        public static string APPLICATION = "application";
        /// <summary>
        /// 开机启动
        /// </summary>
        public static string POWER_RUN = "powerRun";
        /// <summary>
        /// 是否等待视频信号运行模式
        /// 如果等待, 则模式列表运行一个周期结束, 等待视频信号运行下一周期
        /// 否则, 模式列表循环运行
        /// </summary>
        public static string WAIT_VIDEO = "waitVideo";
        /// <summary>
        /// 播放动画键
        /// </summary>
        public static string DISPLAY = "display";
        /// <summary>
        /// 是否开机启动
        /// </summary>
        public static bool IS_POWER_RUN = false;
        /// <summary>
        /// 是否等待视频信号
        /// </summary>
        public static bool IS_WAIT_VIDEO = false;
        /// <summary>
        /// 是否播放动画
        /// </summary>
        public static bool IS_DISPALY = false;


        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">节点名称[如[TypeName]]</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="def">值</param>
        /// <param name="retval">stringbulider对象</param>
        /// <param name="size">字节大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);


        public ConfigHelper() {
        }

        /// <summary>
        /// 自定义读取INI文件中的内容方法
        /// </summary>
        /// <param name="Section">段落名</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static string ReadConfig(string Section, string key) {
            string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string file = appPath + fileName;
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, "", temp, 255, file);
            return temp.ToString();
        }

        public static string ReadConfig(string Section, string key, string def) {
            string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string file = appPath + fileName;
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, def, temp, 255, file);
            return temp.ToString();
        }


        /// <summary>
        /// 自定义写INI文件中的方法
        /// </summary>
        /// <param name="Section">段落名</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void WriteConfig(string Section, string key, string value) {
            string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string file = appPath + fileName;
            WritePrivateProfileString(Section, key, value, file);
        }

        /// <summary>
        /// 写软件运行和出错信息
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void WriteErrMsg(string Section, string key, string value) {
            string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string file = appPath + logFileName;
            WritePrivateProfileString(Section, DateTime.Now.ToLongTimeString() + "-" + key, value, file);
        }

        public static void init() {
            string powerRun = ReadConfig(ConfigHelper.APPLICATION, ConfigHelper.POWER_RUN);
            if (powerRun.Equals("") || powerRun.Equals("False") || powerRun.Equals("false")) {
                IS_POWER_RUN = false;
            } else {
                IS_POWER_RUN = true;
            }

            string waitVideo = ReadConfig(ConfigHelper.APPLICATION, ConfigHelper.WAIT_VIDEO);
            if (waitVideo.Equals("") || waitVideo.Equals("False") || waitVideo.Equals("false")) {
                IS_WAIT_VIDEO = false;
            } else {
                IS_WAIT_VIDEO = true;
            }

            string display = ReadConfig(ConfigHelper.APPLICATION, ConfigHelper.DISPLAY);
            if (display.Equals("") || display.Equals("False") || display.Equals("false")) {
                IS_DISPALY = false;
            } else {
                IS_DISPALY = true;
            }
        }
    }
}
