using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.app {
    class BaseConfig {
        public static BaseConfig ins = new BaseConfig();

        private BaseConfig() {
        }

        /// <summary>
        /// 灯箱总个数
        /// </summary>
        public int boxCount;

        /// <summary>
        /// 开机时间A
        /// </summary>
        /// <value>The power on a.</value>
        public string PowerOnA { get; set; } = "00:00";

        /// <summary>
        /// 关机时间A
        /// </summary>
        /// <value>The power off a.</value>
        public string PowerOffA { get; set; } = "00:00";

        /// <summary>
        /// 开机时间B
        /// </summary>
        /// <value>The power on b.</value>
        public string PowerOnB { get; set; } = "00:00";

        /// <summary>
        /// 关机时间B
        /// </summary>
        /// <value>The power off b.</value>
        public string PowerOffB { get; set; } = "00:00";

        /// <summary>
        /// 是否已经开机
        /// </summary>
        /// <value><c>true</c> if this instance is power on; otherwise, <c>false</c>.</value>
        public bool IsPowerOn { get; set; } = true;

        public int Rows { get; set; }

        public int Columns { get; set; } = 0;

        public string PhoneNum { get; set; } = "";

        public int SmsModel { get; set; } = 0;

        public void init() {
            Rows = Convert.ToInt32(ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.ROWS));
            Columns = Convert.ToInt32(ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.COLUMNS));
            PowerOnA = ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOnA);
            PowerOffA = ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOffA);
            PowerOnB = ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOnB);
            PowerOffB = ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOffB);
            PhoneNum = ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.phoneNum);
            SmsModel = Convert.ToInt32(ConfigHelper.ReadConfig(ConfigHelper.LightBoxNote, ConfigHelper.smsModel));
            boxCount = Rows * Columns;
        }

        public void writeRowColumn() {
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.ROWS, Convert.ToString(Rows));
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.COLUMNS, Convert.ToString(Columns));
        }

        public void writeConfig() {
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOnA, PowerOnA);
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOffA, PowerOffA);
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOnB, PowerOnB);
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.powerOffB, PowerOffB);
        }

        public void writeSmsSet() {
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.phoneNum, PhoneNum);
            ConfigHelper.WriteConfig(ConfigHelper.LightBoxNote, ConfigHelper.smsModel, Convert.ToString(SmsModel));
        }
    }
}
