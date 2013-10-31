using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    /// <summary>
    /// Class: Data
    /// Inherits: Instruction
    /// Purpose: Contains logic to load each of the Data properties corresponding with
    ///          a data processing instruction
    /// Methods: Data(uint)
    ///          LoadOpcode()
    ///          LoadSbit()
    ///          LoadRotate()
    ///          LoadBit4()
    ///          LoadRS()
    ///          LoadImmediateByte()
    /// </summary>
    public class Data : Instruction
    {
        public byte opcode { get; private set; } // instruction opcode
        public bool sbit { get; private set; } // instruction sbit
        public byte rotateVal { get; private set; } // rotate value
        public bool bit4 { get; private set; } // bit 4 value
        public byte RS { get; private set; } // register to shift by
        public byte immediateVal { get; private set; } // value of immediate data

        public Data(uint data)
        {
            instructionData = data;
            isBranch = false;
        }

        public override void ExecuteInstruction(bool generateDisasm)
        {
            switch (this.opcode)
            {
                case 0x00: // AND or MUL instruction
                    if (Memory.ExtractBits(this.instructionData, 4, 7) >> 4 == 0x09) ops.MUL(this, generateDisasm);
                    else ops.AND(this, generateDisasm);
                    break;
                case 0x01: // EOR instruction
                    ops.EOR(this, generateDisasm);
                    break;
                case 0x02: // SUB instruction
                    ops.SUB(this, generateDisasm);
                    break;
                case 0x03: // RSB instruction
                    ops.RSB(this, generateDisasm);
                    break;
                case 0x04: // ADD instruction
                    ops.ADD(this, generateDisasm);
                    break;
                case 0x0C: // ORR instruction
                    ops.ORR(this, generateDisasm);
                    break;
                case 0x0D: // MOV instruction
                    ops.MOV(this, generateDisasm);
                    break;
                case 0x0E: // BIC instruction
                    ops.BIC(this, generateDisasm);
                    break;
                case 0x0F: // MVN instruction
                    ops.MVN(this, generateDisasm);
                    break;
                case 0x0A:
                    ops.CMP(this, generateDisasm);
                    break;
            }
        }

        #region Internal Members
        //--------------------------------------------------------------
        // Purpose: Loads opcode property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadOpcode()
        {
            uint bits = Memory.ExtractBits(instructionData, 21, 24);
            opcode = (byte)(bits >> 21);
        }

        //--------------------------------------------------------------
        // Purpose: Loads sbit property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadSbit()
        {
            uint bits = Memory.ExtractBits(instructionData, 20, 20);
            byte b = (byte)(bits >> 20);
            if (b == 0) sbit = false;
            else sbit = true;
        }

        //--------------------------------------------------------------
        // Purpose: Loads rotate property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadRotate()
        {
            uint bits = Memory.ExtractBits(instructionData, 8, 11);
            rotateVal = (byte)(bits >> 8);
        }

        //--------------------------------------------------------------
        // Purpose: Loads bit4 property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadBit4()
        {
            uint bits = Memory.ExtractBits(instructionData, 4, 4);
            byte b = (byte)(bits >> 4);
            if (b == 0) bit4 = false;
            else bit4 = true;
        }

        //--------------------------------------------------------------
        // Purpose: Loads RS property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadRS()
        {
            uint bits = Memory.ExtractBits(instructionData, 8, 11);
            RS = (byte)(bits >> 8);
        }

        //--------------------------------------------------------------
        // Purpose: Loads immediateVal property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadImmediateByte()
        {
            immediateVal = (byte)Memory.ExtractBits(instructionData, 0, 7);
        }
        #endregion
    }
}
