using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    /// <summary>
    /// Class: Instruction
    /// Purpose: Holds core instruction values and provides methods to load each
    /// Methods: TestBit(uint, byte)
    ///          GetOperationType(uint)
    ///          DataRegImmShiftedReg()
    ///          DataRegShiftedReg()
    ///          DataImmediate()
    ///          GetLdStoreAddress()
    ///          LdStrImmShiftedReg()
    ///          LdStrMultiple()
    ///          LoadCond()
    ///          LoadOptype()
    ///          LoadRD()
    ///          LoadRM()
    ///          LoadRN()
    ///          LoadShiftVal()
    ///          LoadShiftType()
    ///          LoadDataBasics()
    ///          LoadLdStoreBasics()
    ///          
    ///          virtual LoadRS()
    ///          virtual LoadP() 
    ///          virtual LoadB() 
    ///          virtual LoadU() 
    ///          virtual LoadW() 
    ///          virtual LoadL() 
    ///          virtual LoadOpcode() 
    ///          virtual LoadSbit()  
    ///          virtual LoadRotate() 
    ///          virtual LoadRegList()
    ///          virtual LoadImmediateByte()
    ///          virtual LoadImmediateInt() 
    /// </summary>
    public class Instruction
    {
        public byte cond { get; internal set; } // condition value
        public byte optype { get; internal set; } // operation type
        public byte RN { get; internal set; } // base register RN
        public byte RD { get; internal set; } // destination register RD
        public byte shiftVal { get; internal set; } // value to shift immediate by
        public byte shiftType { get; internal set; } // type of shift operation
        public byte RM { get; internal set; } // register shift RM
        public uint instructionData { get; internal set; } // copy of instruction data
        public bool haltExecution { get; internal set; } // is true if instruction is SWI 0x11, otherwise is false
        public bool isBranch { get; internal set; }
        public Disassembly disasm { get; internal set; } // object to hold disassembly pieces
        public Operation ops { get; internal set; } // operation object to perform operations

        public Instruction()
        {
            Disassembly d = new Disassembly();
            ops = new Operation();
            disasm = d;
        }
        

        #region Public Members
        //--------------------------------------------------------------
        // Purpose: Tests the value of <bit> in <data>
        // Returns: A bool depending on value of <bit>
        //--------------------------------------------------------------
        public static bool TestBit(uint data, byte bit)
        {
            uint bits = data & (1U << bit);
            return (bits & (1 << bit)) != 0;
        }

        //--------------------------------------------------------------
        // Purpose: Extracts bits 25-27 of <data> (the operation type bits)
        // Returns: A byte containing bits 25-27 of <data>
        //--------------------------------------------------------------
        public static byte GetOperationType(uint data)
        {
            uint bits = Memory.ExtractBits(data, 25, 27);
            byte operationType = (byte)(bits >> 25);
            return operationType;
        }

        //--------------------------------------------------------------
        // Purpose: Loads properties corresponding with a register/immediate-shifted register 
        //          data processing instruction
        // Returns: nothing
        //--------------------------------------------------------------
        public void DataRegImmShiftedReg()
        {
            this.LoadDataBasics();
            this.LoadShiftVal();
            this.LoadShiftType();
            this.LoadRM();
        }

        //--------------------------------------------------------------
        // Purpose: Loads properties corresponding with a register/shifted-register 
        //          data processing instruction
        // Returns: nothing
        //--------------------------------------------------------------
        public void DataRegShiftedReg()
        {
            this.LoadDataBasics();
            this.LoadRS();
            this.LoadShiftType();
            this.LoadRM();
        }

        //--------------------------------------------------------------
        // Purpose: Loads properties corresponding with an immediate data processing instruction
        // Returns: nothing
        //--------------------------------------------------------------
        public void DataImmediate()
        {
            this.LoadDataBasics();
            this.LoadRotate();
            this.LoadImmediateByte();
        }

        //--------------------------------------------------------------
        // Purpose: Loads properties corresponding with an immediate load/store instruction
        // Returns: nothing
        //--------------------------------------------------------------
        public void LdStrImmediate()
        {
            this.LoadLdStoreBasics();
            this.LoadImmediateInt();
            this.LoadRD();
        }

        //--------------------------------------------------------------
        // Purpose: Loads properties corresponding with an immediate-shifted register 
        //          load/store instruction
        // Returns: nothing
        //--------------------------------------------------------------
        public void LdStrImmShiftedReg()
        {
            this.LoadLdStoreBasics();
            this.LoadShiftVal();
            this.LoadShiftType();
            this.LoadRM();
            this.LoadRD();
        }

        //--------------------------------------------------------------
        // Purpose: Loads properties corresponding with a load/store multiple instruction
        // Returns: nothing
        //--------------------------------------------------------------
        public void LdStrMultiple()
        {
            this.LoadLdStoreBasics();
            this.LoadRegList();
        }

        public void LoadDataSWI()
        {
            this.LoadCond();
            optype = 0x0f;
            this.LoadDisasmAddrAndCode();
        }

        public void DataMul()
        {
            this.LoadCond();
            this.LoadOptype();
            this.LoadOpcode();
            this.LoadSbit();
            RD = (byte)(Memory.ExtractBits(this.instructionData, 16, 19) >> 16);
            this.LoadRM();
            this.LoadRS();
            this.LoadDisasmAddrAndCode();
        }

        public void Branch()
        {
            this.LoadBranchBasics();
        }

        public string ShiftTypeToString()
        {
            switch (this.shiftType)
            {
                case 0x00:
                    return "lsl";
                case 0x01:
                    return "lsr";
                case 0x02:
                    return "asr";
                case 0x03:
                    return "ror";
            }
            return "";
        }
        #endregion


        #region Private Members
        //--------------------------------------------------------------
        // Purpose: Loads condition property
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadCond()
        {
            uint bits = Memory.ExtractBits(instructionData, 28, 31);
            cond = (byte)(bits >> 28);
        }

        //--------------------------------------------------------------
        // Purpose: Loads optype property
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadOptype()
        {
            uint bits = Memory.ExtractBits(instructionData, 25, 27);
            optype = (byte)(bits >> 25);
        }

        //--------------------------------------------------------------
        // Purpose: Loads RD property
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadRD()
        {
            uint bits = Memory.ExtractBits(instructionData, 12, 15);
            RD = (byte)(bits >> 12);
        }

        //--------------------------------------------------------------
        // Purpose: Loads RM property
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadRM()
        {
            RM = (byte)Memory.ExtractBits(instructionData, 0, 3);
        }

        //--------------------------------------------------------------
        // Purpose: Loads RN property
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadRN()
        {
            uint bits = Memory.ExtractBits(instructionData, 16, 19);
            RN = (byte)(bits >> 16);
        }

        //--------------------------------------------------------------
        // Purpose: Loads shiftVal property
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadShiftVal()
        {
            uint bits = Memory.ExtractBits(instructionData, 7, 11);
            shiftVal = (byte)(bits >> 7);
        }

        //--------------------------------------------------------------
        // Purpose: Loads shiftType property
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadShiftType()
        {
            uint bits = Memory.ExtractBits(instructionData, 5, 6);
            shiftType = (byte)(bits >> 5);
        }

        //--------------------------------------------------------------
        // Purpose: Loads the properties common to all data processing instructions
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadDataBasics()
        {
            this.LoadBit4();
            this.LoadCond();
            this.LoadOptype();
            this.LoadOpcode();
            this.LoadRN();
            this.LoadRD();
            this.LoadSbit();
            this.LoadDisasmAddrAndCode();
        }

        //--------------------------------------------------------------
        // Purpose: Loads the properties common to all load/store instructions
        // Returns: nothing
        //--------------------------------------------------------------
        private void LoadLdStoreBasics()
        {
            this.LoadP();
            this.LoadB();
            this.LoadU();
            this.LoadW();
            this.LoadL();
            this.LoadRN();
            this.LoadDisasmAddrAndCode();
        }

        private void LoadBranchBasics()
        {
            this.LoadDisasmAddrAndCode();
            this.LoadCond();
            this.LoadType();
            this.LoadImmediateInt();
            this.LoadLBit();
            this.LoadRM();
        }

        private void LoadDisasmAddrAndCode()
        {
            this.disasm.address = "0x" + Computer.GetCPU().registers.ReadWord((uint)regs.PC).ToString("x8");
            this.disasm.instrCode = "0x" + this.instructionData.ToString("x8");
        }
        #endregion


        #region Virtual Methods
        internal virtual void LoadRS() { }
        internal virtual void LoadP() { }
        internal virtual void LoadB() { }
        internal virtual void LoadU() { }
        internal virtual void LoadW() { }
        internal virtual void LoadL() { }
        internal virtual void LoadOpcode() { }
        internal virtual void LoadSbit() { }
        internal virtual void LoadRotate() { }
        internal virtual void LoadRegList() { }
        internal virtual void LoadImmediateByte() { }
        internal virtual void LoadImmediateInt() { }
        internal virtual void LoadBit4() { }
        internal virtual void LoadLBit() { }
        internal virtual void LoadType() { }
        public virtual void ExecuteInstruction(bool b) { }
        #endregion
    }
}
