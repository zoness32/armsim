using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public abstract class ObserverBridge
    {
        public delegate void StatusUpdate(char c);
        public delegate void TextUpdate(string text, string dest);
        public event StatusUpdate OnStatusUpdate = null;
        public event TextUpdate OnTextUpdate = null;

        public void Attach(Observer obs)
        {
            OnStatusUpdate += new StatusUpdate(obs.SetText);
            OnTextUpdate += new TextUpdate(obs.SetLabelText);
        }

        public void Notify(char c)
        {
            if (OnStatusUpdate != null)
            {
                OnStatusUpdate(c);
            }
        }

        public void Notify(string text, string dest)
        {
            if (OnTextUpdate != null)
            {
                OnTextUpdate(text, dest);
            }
        }
    }
}
