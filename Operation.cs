using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace armsimGUI
{
    /// <summary>
    /// Class: Operation
    /// Purpose: Performs various assembly instructions
    /// Methods: ADD(Data)
    ///          MOV(Data)
    ///          MVN(Data)
    ///          SUB(Data)
    ///          RSB(Data)
    ///          MUL(Data)
    ///          AND(Data)
    ///          ORR(Data)
    ///          EOR(Data)
    ///          BIC(Data)
    ///          LDR(LdStore)
    ///          STR(LdStore)
    ///          STMFD(LoadStoreMultiple)
    ///          SWI()
    ///          LDMFD(LoadStoreMultiple)
    ///          CheckOperand2(Data)
    ///          CheckShiftType(Data)
    ///          GetLdStoreAddress(LdStore)
    ///          SetDataDisassembly(Data, uint, bool)
    ///          SetLdStoreDisassembly(LdStore)
    /// </summary>
    public class Operation
    {
        enum op2Type { imm = 0, reg_imm = 1, reg_reg = 2, invalid = -1 }
        enum shiftType { lsl = 0x00, lsr = 0x01, asr = 0x02, ror = 0x03, invalid = -1 }

        ObserverTest observer;

        public Operation()
        {
            observer = new ObserverTest();
            observer.Attach(Computer.GetObserver());
        }

        //--------------------------------------------------------------
        // Purpose: Performs an addition according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void ADD(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            uint value = 0, sum;
            instr.disasm.instruction = "add";
            value = ComputeValue(instr);

            if (generateDisasm) SetDataDisassembly(instr, value, false);
            else
            {
                sum = value + registers.ReadWord((uint)instr.RN * 4);
                registers.WriteWord((uint)(instr.RD * 4), sum);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs a move according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void MOV(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            uint value = ComputeValue(instr);
            instr.disasm.instruction = "mov";

            if (generateDisasm) SetDataDisassembly(instr, value, true);
            else Computer.GetCPU().registers.WriteWord((uint)(instr.RD * 4), value);
        }

        //--------------------------------------------------------------
        // Purpose: Performs a "move not" according to data in <instr>;
        //          negates the bits of the immediate value being moved before
        //          moving into instr.RD
        // Returns: nothing
        //--------------------------------------------------------------
        public void MVN(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            uint value = ComputeValue(instr);
            instr.disasm.instruction = "mvn";

            if (generateDisasm) SetDataDisassembly(instr, value, true);
            else
            {
                value = ~value;
                Computer.GetCPU().registers.WriteWord((uint)(instr.RD * 4), value);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs a subtraction according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void SUB(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            instr.disasm.instruction = "sub";
            uint value = ComputeValue(instr), difference;

            if (generateDisasm) SetDataDisassembly(instr, value, false);
            else // if not generating disassembly, execute the instruction
            {
                difference = registers.ReadWord((uint)instr.RN * 4) - value;
                registers.WriteWord((uint)(instr.RD * 4), difference);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs a reverse subtraction according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void RSB(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            instr.disasm.instruction = "rsb";
            uint value = ComputeValue(instr), difference;

            if (generateDisasm) SetDataDisassembly(instr, value, false);
            else
            {
                difference = value - registers.ReadWord((uint)instr.RN * 4);
                registers.WriteWord((uint)(instr.RD * 4), difference);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs an multiply instruction according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void MUL(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            instr.disasm.instruction = "mul";
            uint value = registers.ReadWord((uint)(instr.RM * 4)) * registers.ReadWord((uint)(instr.RS * 4));

            if (generateDisasm) instr.disasm.value = Disassembly.DataMul(instr);
            else Computer.GetCPU().registers.WriteWord((uint)(instr.RD * 4), value);
        }

        //--------------------------------------------------------------
        // Purpose: Performs a bitwise AND according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void AND(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            instr.disasm.instruction = "and";
            uint value = ComputeValue(instr);

            if (generateDisasm) SetDataDisassembly(instr, value, false);
            else
            {
                uint andVal = value & registers.ReadWord((uint)instr.RN * 4);
                registers.WriteWord((uint)(instr.RD * 4), andVal);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs a bitwise OR according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void ORR(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            instr.disasm.instruction = "orr";
            uint value = ComputeValue(instr);

            if (generateDisasm) SetDataDisassembly(instr, value, false);
            else
            {
                uint orVal = value | registers.ReadWord((uint)instr.RN * 4);
                registers.WriteWord((uint)(instr.RD * 4), orVal);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs a bitwise exclusive OR according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void EOR(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            instr.disasm.instruction = "eor";
            uint value = ComputeValue(instr);

            if (generateDisasm) SetDataDisassembly(instr, value, false);
            else
            {
                uint eorVal = value ^ registers.ReadWord((uint)instr.RN * 4);
                registers.WriteWord((uint)(instr.RD * 4), eorVal);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs a bit clear according to data in <instr>;
        //          a bitwise AND is performed on one value and the complement
        //          of a second value
        // Returns: nothing
        //--------------------------------------------------------------
        public void BIC(Data instr, bool generateDisasm)
        {
            Memory registers = Computer.GetCPU().registers;
            instr.disasm.instruction = "eor";
            uint value = ComputeValue(instr);

            if (generateDisasm) SetDataDisassembly(instr, value, false);
            else
            {
                uint bicVal = registers.ReadWord((uint)instr.RN * 4) & ~value;
                registers.WriteWord((uint)(instr.RD * 4), bicVal);
            }
        }

        public void CMP(Data instr, bool generateDisasm)
        {
            uint RN = Computer.GetCPU().registers.ReadWord((uint)instr.RN * 4);
            instr.disasm.instruction = "cmp";
            uint value = ComputeValue(instr);

            if (generateDisasm) SetCMPDisassembly(instr, value);
            else
            {
                CPU cpu = Computer.GetCPU();
                cpu.SetNFlag(Memory.ExtractBits(RN - value, 31, 31) == 0 ? false : true);
                cpu.SetZFlag(value - RN == 0 ? true : false);
                cpu.SetCFlag(value > RN ? false : true);
                cpu.SetFFlag(ComputeFFlag((int)RN, (int)value, "sub"));
            }
        }

        public void B(Branch instr, bool generateDisasm)
        {
            if (generateDisasm)
            {
                instr.disasm.instruction = "b" + GetSuffix(instr);
                SetBranchDisassembly(instr);
            }
            else
            {
                if (CheckCond(instr))
                {
                    uint addr = ComputeBranchAddress(instr) - 4;
                    Computer.GetCPU().registers.WriteWord((uint)regs.PC, addr);
                }
            }
        }

        public void BL(Branch instr, bool generateDisasm)
        {
            if (generateDisasm)
            {
                instr.disasm.instruction = "bl" + GetSuffix(instr);
                SetBranchDisassembly(instr);
            }
            else
            {
                if (CheckCond(instr))
                {
                    Memory registers = Computer.GetCPU().registers;
                    uint addr = ComputeBranchAddress(instr) - 4;
                    registers.WriteWord((uint)regs.LR, registers.ReadWord((uint)regs.PC));
                    registers.WriteWord((uint)regs.PC, addr);
                }
            }
        }

        public void BX(Branch instr, bool generateDisasm)
        {
            if (generateDisasm)
            {
                instr.disasm.instruction = "bx" + GetSuffix(instr);
                instr.disasm.value = "r" + instr.RM.ToString();
            }
            else
            {
                Memory mem = Computer.GetCPU().registers;
                uint data = mem.ReadWord((uint)instr.RM * 4);
                uint d = data & 0xFFFFFFFE;
                if (CheckCond(instr)) mem.WriteWord((uint)regs.PC, mem.ReadWord((uint)instr.RM * 4) & 0xFFFFFFFE);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Loads data into instr.RD according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void LDR(LdStore instr, bool generateDisasm)
        {
            if (!generateDisasm)
            {
                uint refAddress = GetLdStoreAddress(instr);
                if (instr.B)
                {
                    byte data = Computer.GetCPU().progRAM.ReadByte(refAddress);
                    Computer.GetCPU().registers.WriteByte((uint)(instr.RD * 4), data);
                }
                else
                {
                    uint data = Computer.GetCPU().progRAM.ReadWord(refAddress);
                    Computer.GetCPU().registers.WriteWord((uint)(instr.RD * 4), data);
                }
            }
            else
            {
                if (!instr.B) instr.disasm.instruction = "ldr";
                else instr.disasm.instruction = "ldrb";
                SetLdStoreDisassembly(instr);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Stores data in instr.RD into address according to data in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void STR(LdStore instr, bool generateDisasm)
        {
            if (!generateDisasm)
            {
                uint refAddress = GetLdStoreAddress(instr);
                if (instr.B)
                {
                    byte dataToStore = Computer.GetCPU().registers.ReadByte((uint)(instr.RD * 4));
                    Computer.GetCPU().progRAM.WriteByte(refAddress, dataToStore);
                }
                else
                {
                    uint dataToStore = Computer.GetCPU().registers.ReadWord((uint)(instr.RD * 4));
                    Computer.GetCPU().progRAM.WriteWord(refAddress, dataToStore);
                }
            }
            else
            {
                if (!instr.B) instr.disasm.instruction = "str";
                else instr.disasm.instruction = "strb";
                SetLdStoreDisassembly(instr);
            }
        }

        //--------------------------------------------------------------
        // Purpose: Stores multiple registers into memory starting at address
        //          in instr.RN
        // Returns: nothing
        //--------------------------------------------------------------
        public void STMFD(LoadStoreMultiple instr, bool generateDisasm)
        {
            instr.disasm.instruction = "stmfd";
            uint count = 0;
            uint backup = instr.regList, mask = 0x00000001;
            ArrayList regList = new ArrayList(); // store addresses of registers to be stored
            ArrayList disasmList = new ArrayList();
            if (!generateDisasm)
            {
                for (int i = 0; i < 17; i++, count += 4)
                {
                    byte lsb = (byte)(backup & mask);
                    if (lsb == 1) regList.Add(count);
                    backup >>= 1;
                }

                uint RN = Computer.GetCPU().registers.ReadWord((uint)instr.RN * 4);
                uint address = RN - ((uint)regList.Count * 4);
                for (int i = 0; i < regList.Count; i++)
                {
                    //if (!instr.U) address -= 4;
                    uint data = Computer.GetCPU().registers.ReadWord((uint)regList[i]);
                    Computer.GetCPU().progRAM.WriteWord(address, data);
                    address += 4;
                }

                if (instr.W) Computer.GetCPU().registers.WriteWord((uint)instr.RN * 4, RN - ((uint)regList.Count * 4));
            }
            else
            {
                for (int i = 0; i < 17; i++, count += 4)
                {
                    byte lsb = (byte)(backup & mask);
                    if (lsb == 1) disasmList.Add("r" + count / 4);
                    backup >>= 1;
                }
                instr.disasm.value = "r" + instr.RN.ToString() + (instr.W ? "" : "!") + ", {";
                foreach (string s in disasmList) instr.disasm.value += s + ", ";
                instr.disasm.value = instr.disasm.value.Substring(0, instr.disasm.value.Length - 2) + "}";
            }
            // store values in registers held in instr.regList to address in instr.RN
            // if instr.W is set, write the current PC value back into the PC
        }

        //--------------------------------------------------------------
        // Purpose: "Software Interrupt" stops the CPU
        // Returns: nothing
        //--------------------------------------------------------------
        public void SWI(SWI instr, bool generateDisasm)
        {
            if (generateDisasm)
            {
                instr.disasm.instruction = "swi";
                instr.disasm.value = "0x" + instr.immediateVal.ToString("x2");
            }
            else
            {
                if (instr.immediateVal == 0x11) instr.haltExecution = true;
                else if (instr.immediateVal == 0x00)
                {
                    observer.ChangeText((char)Computer.GetCPU().registers.ReadByte((uint)regs.r0));
                }
                else if (instr.immediateVal == 0x6a)
                {
                    uint addr = Computer.GetCPU().registers.ReadWord((uint)regs.r1);
                    Memory progRAM = Computer.GetCPU().progRAM;
                    char c = ' ';
                    uint max = Computer.GetCPU().registers.ReadWord((uint)regs.r2);
                    while (!Computer.crTyped) { } // wait
                    for (uint i = 0; i < max; i++)
                    {
                        c = Computer.charBuffer[i];
                        progRAM.WriteByte(addr + i, (byte)c);
                        if (c == '\0') break;
                    }
                }
                else { } // do nothing
            }
        }

        //--------------------------------------------------------------
        // Purpose: Loads contents of sequential memory addresses starting at
        //          instr.RN into registers specified in <instr>
        // Returns: nothing
        //--------------------------------------------------------------
        public void LDMFD(LoadStoreMultiple instr, bool generateDisasm)
        {
            instr.disasm.instruction = "ldmfd";
            uint count = 0;
            uint backup = instr.regList, mask = 0x00000001;
            ArrayList regList = new ArrayList(); // store addresses of registers to be stored
            ArrayList disasmList = new ArrayList();
            if (!generateDisasm)
            {
                for (int i = 0; i < 17; i++, count += 4)
                {
                    byte lsb = (byte)(backup & mask);
                    if (lsb == 1) regList.Add(count);
                    backup >>= 1;
                }

                uint RN = Computer.GetCPU().registers.ReadWord((uint)instr.RN * 4);
                uint address = RN;
                for (int i = 0; i < regList.Count; i++)
                {
                    //if (!instr.U) address -= 4;
                    uint data = Computer.GetCPU().progRAM.ReadWord(address);
                    Computer.GetCPU().registers.WriteWord((uint)regList[i], data);
                    address += 4;
                }

                if (instr.W) Computer.GetCPU().registers.WriteWord((uint)instr.RN * 4, RN + ((uint)regList.Count * 4));
            }
            else
            {
                for (int i = 0; i < 17; i++, count += 4)
                {
                    byte lsb = (byte)(backup & mask);
                    if (lsb == 1) disasmList.Add("r" + count / 4);
                    backup >>= 1;
                }
                instr.disasm.value = "r" + instr.RN.ToString() + (instr.W ? "" : "!") + ", {";
                foreach (string s in disasmList) instr.disasm.value += s + ", ";
                instr.disasm.value = instr.disasm.value.Substring(0, instr.disasm.value.Length - 2) + "}";
            }
            // read value at instr.RN into lowest register in instr.regList
            // continue reading values in sequential memory addresses into registers listed from least to greatest
            // read direction is specified by instr.U
        }

        //--------------------------------------------------------------
        // Purpose: Checks the type of instr.optype
        // Returns: An op2Type enum object corresponding to the operation type
        //--------------------------------------------------------------
        private static op2Type CheckOperand2(Data instr)
        {
            if (instr.optype == 0x01) return op2Type.imm;
            else if (instr.optype == 0x00 && !instr.bit4) return op2Type.reg_imm;
            else if (instr.optype == 0x00 && instr.bit4) return op2Type.reg_reg;
            else return op2Type.invalid;
        }

        //--------------------------------------------------------------
        // Purpose: Checks the shift type of instr.shiftType
        // Returns: A shiftType enum object corresponding to the shift type
        //--------------------------------------------------------------
        private static shiftType CheckShiftType(Data instr)
        {
            if (instr.shiftType == 0x00) return shiftType.lsl;
            else if (instr.shiftType == 0x01) return shiftType.lsr;
            else if (instr.shiftType == 0x02) return shiftType.asr;
            else if (instr.shiftType == 0x03) return shiftType.ror;
            else return shiftType.invalid;
        }

        //--------------------------------------------------------------
        // Purpose: Computes the value of operand2 in data processing instructions
        // Returns: The uint value of the instruction's operand2
        //--------------------------------------------------------------
        private static uint ComputeValue(Data instr)
        {
            op2Type op2 = Operation.CheckOperand2(instr);
            shiftType shift = Operation.CheckShiftType(instr);
            Memory registers = Computer.GetCPU().registers;
            uint value = 0, adjustPC = 0;
            if (instr.RM * 4 == (int)regs.PC || instr.RS * 4 == (int)regs.PC) adjustPC = 4;
            switch (op2)
            {
                case op2Type.imm:
                    value = BarrelShifter.rotateVal((uint)instr.immediateVal, (byte)shiftType.ror, (uint)instr.rotateVal * 2);
                    break;
                case op2Type.reg_imm:
                    value = BarrelShifter.rotateVal(registers.ReadWord((uint)instr.RM * 4) + adjustPC, instr.shiftType, instr.shiftVal);
                    break;
                case op2Type.reg_reg:
                    value = BarrelShifter.rotateVal(registers.ReadWord((uint)instr.RM * 4) + adjustPC, instr.shiftType, registers.ReadWord((uint)instr.RS * 4) + adjustPC);
                    break;
            }

            return value;
        }

        //--------------------------------------------------------------
        // Purpose: Creates disassembly for data processing instructions and sets it
        // Returns: Nothing
        //--------------------------------------------------------------
        private static void SetDataDisassembly(Data instr, uint value, bool isMov)
        {
            op2Type op2 = Operation.CheckOperand2(instr);
            shiftType shift = Operation.CheckShiftType(instr);
            if (isMov)
            {
                switch (op2)
                {
                    case op2Type.imm:
                        instr.disasm.value = Disassembly.ImmMov(instr, value);
                        break;
                    case op2Type.reg_imm:
                        instr.disasm.value = Disassembly.RegImmMov(instr);
                        break;
                    case op2Type.reg_reg:
                        instr.disasm.value = Disassembly.RegRegMov(instr);
                        break;
                }
            }
            else
            {
                switch (op2)
                {
                    case op2Type.imm:
                        instr.disasm.value = Disassembly.ImmData(instr, value);
                        break;
                    case op2Type.reg_imm:
                        instr.disasm.value = Disassembly.RegImmData(instr);
                        break;
                    case op2Type.reg_reg:
                        instr.disasm.value = Disassembly.RegRegData(instr);
                        break;
                }
            }
        }

        //--------------------------------------------------------------
        // Purpose: Creates disassembly for Load/Store instructions and sets it
        // Returns: Nothing
        //--------------------------------------------------------------
        private static void SetLdStoreDisassembly(LdStore instr)
        {
            if (Memory.ExtractBits(instr.instructionData, 25, 25) == 0)
            {
                if (instr.immediateVal == 0) instr.disasm.value = "r" + instr.RD.ToString() + ", [r" + instr.RN.ToString() + "]";
                else instr.disasm.value = "r" + instr.RD.ToString() + ", [r" + instr.RN.ToString() + ", #" + (instr.U ? "" : "-") + instr.immediateVal.ToString() + "]";
            }
            else
            {
                instr.disasm.value = "r" + instr.RD.ToString() + ", [r" + instr.RN.ToString() + ", " + (instr.U ? "r" : "-r") + instr.RM.ToString();
                if (instr.shiftVal == 0) instr.disasm.value += "]";
                else instr.disasm.value += ", " + instr.ShiftTypeToString() + " #" + instr.shiftVal.ToString() + "]";
            }
        }

        private static void SetCMPDisassembly(Data instr, uint value)
        {
            SetDataDisassembly(instr, value, false);
            instr.disasm.value = instr.disasm.value.Substring(4);
        }

        private static void SetBranchDisassembly(Branch instr)
        {
            uint addr = ComputeBranchAddress(instr); // +4 to compensate for PC adjustment 
            instr.disasm.value = "0x" + addr.ToString("x8");
        }

        private static uint ComputeBranchAddress(Branch instr)
        {
            uint val = (uint)instr.immediateVal;
            uint val2 = val << 8;
            uint addr = BarrelShifter.rotateVal((uint)instr.immediateVal << 8, 0x02, 6);
            //addr <<= 2;
            uint PC = Computer.GetCPU().registers.ReadWord((uint)regs.PC) + 8;
            addr += Computer.GetCPU().registers.ReadWord((uint)regs.PC) + 8;
            return addr;
        }

        //--------------------------------------------------------------
        // Purpose: Calculates the address to reference for Load/Store instructions
        // Returns: The uint value of the address to reference
        //--------------------------------------------------------------
        private static uint GetLdStoreAddress(LdStore instr)
        {
            uint refAddress, offset;
            if (Memory.ExtractBits(instr.instructionData, 25, 25) == 0) offset = (uint)instr.immediateVal; // immediate offset
            else offset = BarrelShifter.rotateVal(Computer.GetCPU().registers.ReadWord((uint)(instr.RM * 4)), instr.shiftType, instr.shiftVal); // imm shifted register

            if (instr.RN * 4 == (int)regs.PC) offset += 4; // to take into account PC reference

            if (!instr.U) refAddress = Computer.GetCPU().registers.ReadWord((uint)(instr.RN * 4)) - offset;
            else
            {
                uint val = Computer.GetCPU().registers.ReadWord((uint)(instr.RN * 4));
                refAddress = val + offset;
            }

            return refAddress;
        }

        private static bool ComputeFFlag(int Rn, int shifter_operand, string type)
        {
            int result;
            switch (type)
            {
                case "sub":
                    result = Rn - shifter_operand;
                    return EvaluateSigns(Rn, shifter_operand, result);
                case "add":
                    result = Rn + shifter_operand;
                    return EvaluateSigns(Rn, shifter_operand, result);
            }
            return false;
        }

        private static bool EvaluateSigns(int Rn, int shifter_operand, int result)
        {
            if (Rn >= 0 && shifter_operand < 0 && result < 0) return true;
            else if (Rn < 0 && shifter_operand >= 0 && result >= 0) return true;
            else return false;
        }

        private static string GetSuffix(Instruction instr)
        {
            string suffix = "";
            switch (instr.cond)
            {
                case 0x00:
                    suffix = "eq";
                    break;
                case 0x01:
                    suffix = "ne";
                    break;
                case 0x02:
                    suffix = "cs";
                    break;
                case 0x03:
                    suffix = "cc";
                    break;
                case 0x04:
                    suffix = "mi";
                    break;
                case 0x05:
                    suffix = "pl";
                    break;
                case 0x06:
                    suffix = "vs";
                    break;
                case 0x07:
                    suffix = "vc";
                    break;
                case 0x08:
                    suffix = "hi";
                    break;
                case 0x09:
                    suffix = "ls";
                    break;
                case 0x0a:
                    suffix = "ge";
                    break;
                case 0x0b:
                    suffix = "lt";
                    break;
                case 0x0c:
                    suffix = "gt";
                    break;
                case 0x0d:
                    suffix = "le";
                    break;
                case 0x0e:
                    suffix = "";
                    break;
            }
            return suffix;
        }

        private static bool CheckCond(Instruction instr)
        {
            CPU cpu = Computer.GetCPU();
            switch (instr.cond)
            {
                case 0x00:
                    if (cpu.ZFlag) return true;
                    break;
                case 0x01:
                    if (!cpu.ZFlag) return true;
                    break;
                case 0x02:
                    if (cpu.CFlag) return true;
                    break;
                case 0x03:
                    if (!cpu.CFlag) return true;
                    break;
                case 0x04:
                    if (cpu.NFlag) return true;
                    break;
                case 0x05:
                    if (!cpu.NFlag) return true;
                    break;
                case 0x06:
                    if (cpu.FFlag) return true;
                    break;
                case 0x07:
                    if (!cpu.FFlag) return true;
                    break;
                case 0x08:
                    if (cpu.CFlag && !cpu.ZFlag) return true;
                    break;
                case 0x09:
                    if (!cpu.CFlag || cpu.ZFlag) return true;
                    break;
                case 0x0a:
                    if ((cpu.NFlag && cpu.FFlag) || (!cpu.NFlag && !cpu.FFlag)) return true;
                    break;
                case 0x0b:
                    if ((cpu.NFlag && !cpu.FFlag) || (!cpu.CFlag && cpu.FFlag)) return true;
                    break;
                case 0x0c:
                    if (!cpu.ZFlag && ((cpu.NFlag && cpu.FFlag) || (!cpu.NFlag && !cpu.FFlag))) return true;
                    break;
                case 0x0d:
                    if (cpu.ZFlag || ((cpu.NFlag && !cpu.FFlag) || (!cpu.NFlag && cpu.FFlag))) return true;
                    break;
                case 0x0e:
                    return true;
            }
            return false;
        }
    }
}
