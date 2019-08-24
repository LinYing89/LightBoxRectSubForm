using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.app {
    public class Untils {
        public static string ToHexString(byte[] bytes) {
            string hexString = string.Empty;
            if (bytes != null) {
                StringBuilder strB = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    strB.Append(bytes[i].ToString("X2") + " ");
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
    }
}
