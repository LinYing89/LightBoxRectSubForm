using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBoxRectSubForm.comm {
    public class CommStateEventArgs : EventArgs {
        public bool opened;
        public string message = "";
    }
}
