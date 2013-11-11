using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public class InstructionFactory
    {
        public Instruction Create(uint data)
        {
            byte operationType = Instruction.GetOperationType(data);
            bool bit4 = Memory.TestFlagInData(data, 4);
            switch (operationType)
            {
                case 0x0: // 000
                    if (Memory.ExtractBits(data, 21, 24) == 0 && (Memory.ExtractBits(data, 4, 7) >> 4) == 0x09) // instruction is a MUL 
                    {
                        Data dInstr = new Data(data);
                        dInstr.DataMul();
                        dInstr.ExecuteInstruction(true);
                        return dInstr;
                    }
                    else if (bit4 == false) // Data Register, Immediate-shifted Register
                    {
                        Data dInstr = new Data(data);
                        dInstr.DataRegImmShiftedReg();
                        dInstr.ExecuteInstruction(true);
                        return dInstr;
                    }
                    else if (bit4 == true && Memory.ExtractBits(data, 20, 24) >> 20 == 0x12) // BX instruction
                    {
                        Branch bInstr = new Branch(data);
                        bInstr.Branch();
                        bInstr.ExecuteInstruction(true);
                        return bInstr;
                    }
                    else // Data Register-shifted Immediate
                    {
                        Data dInstr = new Data(data);
                        dInstr.DataRegShiftedReg();
                        dInstr.ExecuteInstruction(true);
                        return dInstr;
                    }
                case 0x1: // 001 - Data Immediate
                    Data dataInstr = new Data(data);
                    dataInstr.DataImmediate();
                    dataInstr.ExecuteInstruction(true); // generate disassembly
                    return dataInstr;
                case 0x2: // 010 - Load/Store Immediate
                    LdStore ldstrInstr = new LdStore(data);
                    ldstrInstr.LdStrImmediate();
                    ldstrInstr.ExecuteInstruction(true);
                    return ldstrInstr;
                case 0x3: // 011 - Load/Store Immediate-shifted Register
                    LdStore ldstInstr = new LdStore(data);
                    ldstInstr.LdStrImmShiftedReg();
                    ldstInstr.ExecuteInstruction(true);
                    return ldstInstr;
                case 0x4: // 100 - Load/Store Multiple
                    LoadStoreMultiple ldstrMul = new LoadStoreMultiple(data);
                    ldstrMul.LdStrMultiple();
                    ldstrMul.ExecuteInstruction(true);
                    return ldstrMul;
                case 0x5: // 101 - Branch 
                    Branch b = new Branch(data);
                    b.Branch();
                    b.ExecuteInstruction(true);
                    return b;
                case 0x07: // 111 - SWI
                    SWI swiInstr = new SWI(data);
                    swiInstr.LoadDataSWI();
                    swiInstr.ExecuteInstruction(true);
                    return swiInstr;
                default:
                    // an unsupported instruction was provided
                    Instruction i = new Instruction();
                    return i;
            }
        }
    }
}
