using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public class ObserverTest : ObserverBridge
    {
        public void ChangeText(char c)
        {
            Notify(c);
        }

        public void ChangeText(string stepNum, string dest)
        {
            Notify(stepNum, dest);
        }
    }
}
