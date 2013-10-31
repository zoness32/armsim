using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public class SWI : Instruction
    {
        public uint immediateVal { get; private set; }

        public SWI(uint data)
        {
            instructionData = data;
            isBranch = false;
            immediateVal = Memory.ExtractBits(instructionData, 0, 23);
        }

        public override void ExecuteInstruction(bool generateDisasm)
        {
            ops.SWI(this, generateDisasm);
        }
    }
}
