using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public class Branch : Instruction
    {
        public bool LBit { get; private set; }
        public int immediateVal { get; private set; }
        public byte type { get; private set; }

        public Branch(uint data)
        {
            this.instructionData = data;
            isBranch = true;
        }

        public override void ExecuteInstruction(bool generateDisasm)
        {
            uint data = this.instructionData;
            uint test = Memory.ExtractBits(this.instructionData, 20, 27);
            if (type == 0x0a) ops.B(this, generateDisasm);
            else if (type == 0x0b) ops.BL(this, generateDisasm);
            else if (Memory.ExtractBits(this.instructionData, 20, 27) >> 20 == 0x12 && Memory.ExtractBits(this.instructionData, 4, 7) >> 4 == 0x01) ops.BX(this, generateDisasm);
        }

        public void LoadLBit(uint data)
        {
            LBit = Memory.TestFlagInData(data, 24);
        }

        internal override void LoadImmediateInt()
        {
            immediateVal = (int)Memory.ExtractBits(instructionData, 0, 23);
        }

        internal override void LoadType()
        {
            type = (byte)(Memory.ExtractBits(instructionData, 24, 27) >> 24);
        }
    }
}
