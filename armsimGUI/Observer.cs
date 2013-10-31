using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public interface Observer
    {
        void SetText(char c);
        void SetLabelText(string text, string dest);
    }
}
