using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public class Disassembly
    {
        public string address { get; set; }
        public string instrCode { get; set; }
        public string instruction { get; set; }
        public string value { get; set; }

        public Disassembly()
        {
            address = "unsupported";
            instrCode = "unsupported";
            instruction = "unsupported";
            value = "unsupported";
        }

        public static string ImmData(Instruction instr, uint value)
        {
            return "r" + instr.RD.ToString() + ", r" + instr.RN.ToString() + ", #" + value.ToString();
        }

        public static string RegImmData(Instruction instr)
        {
            string disassembly = "r" + instr.RD.ToString() + ", r" + instr.RN.ToString() + ", r" + instr.RM.ToString();
            if (instr.shiftVal != 0)
            {
                int rotShiftVal = instr.shiftVal * 2;
                disassembly += ", " + instr.ShiftTypeToString() + " #" + rotShiftVal.ToString();
            }
            return disassembly;
        }

        public static string RegRegData(Data instr)
        {
            return "r" + instr.RD.ToString() + ", r" + instr.RN.ToString() + ", r" + instr.RM.ToString() + ", " + instr.ShiftTypeToString() + " r" + instr.RS.ToString();
        }

        public static string ImmMov(Data instr, uint value)
        {
            return "r" + instr.RD.ToString() + ", #" + value.ToString();
        }

        public static string RegImmMov(Data instr)
        {
            string disassembly = "r" + instr.RD.ToString() + ", r" + instr.RM.ToString();
            if (instr.shiftVal != 0) disassembly += ", " + instr.ShiftTypeToString() + " #" + instr.shiftVal.ToString();
            return disassembly;
        }
        public static string RegRegMov(Data instr)
        {
            return "r" + instr.RD.ToString() + ", r" + instr.RM.ToString() + ", " + instr.ShiftTypeToString() + " r" + instr.RS.ToString();
        }

        public static string DataMul(Data instr)
        {
            return "r" + instr.RD.ToString() + ", r" + instr.RM.ToString() + ", r" + instr.RS.ToString();
        }
    }
}
