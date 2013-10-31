using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsimGUI
{
    class CharBuffer
    {
        public char[] charBuffer { get; private set; }
        public uint Size { get; set; }

        public CharBuffer()
        {
            charBuffer = new char[100];
            Size = 0;
        }

        public char this[uint key]
        {
            get { return charBuffer[key]; }
            set { SetValue(key, value); }
        }

        private void SetValue(uint key, int value)
        {
            charBuffer[key] = (char)value;
            Size++;
        }

        public void Reset()
        {
            Size = 0;
            charBuffer = new char[100];
        }
    }
}
