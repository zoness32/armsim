using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    /// <summary>
    /// Class: LdStore
    /// Inherits: Instruction
    /// Purpose: Contains logic to load each of the Load/Store properties corresponding with
    ///          a Load/Store instruction
    /// Methods: LdStore(uint)
    ///          LoadP()
    ///          LoadU()
    ///          LoadB()
    ///          LoadW()
    ///          LoadL()
    ///          LoadImmediateInt()
    /// </summary>
    public class LdStore : Instruction
    {
        public bool P { get; private set; } // P bit
        public bool U { get; private set; } // U bit
        public bool B { get; private set; } // B bit
        public bool W { get; private set; } // W bit 
        public bool L { get; private set; } // L bit
        public uint immediateVal { get; private set; } // value of immediate data

        public LdStore(uint data)
        {
            instructionData = data;
            isBranch = false;
        }

        public override void ExecuteInstruction(bool generateDisasm)
        {
            bool bit20 = Instruction.TestBit(this.instructionData, 20); // test bit 20    
            if (bit20) ops.LDR(this, generateDisasm);
            else ops.STR(this, generateDisasm);
        }

        #region Internal Members
        //--------------------------------------------------------------
        // Purpose: Loads the P property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadP()
        {
            P = Memory.TestFlagInData(instructionData, 24);
        }

        //--------------------------------------------------------------
        // Purpose: Loads the B property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadB()
        {
            B = Memory.TestFlagInData(instructionData, 22);
        }

        //--------------------------------------------------------------
        // Purpose: Loads the L property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadL()
        {
            L = Memory.TestFlagInData(instructionData, 20);            
        }

        //--------------------------------------------------------------
        // Purpose: Loads the U property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadU()
        {
            U = Memory.TestFlagInData(instructionData, 23);
        }

        //--------------------------------------------------------------
        // Purpose: Loads the W property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadW()
        {
            W = Memory.TestFlagInData(instructionData, 21);
        }

        //--------------------------------------------------------------
        // Purpose: Loads the immediateVal property
        // Returns: nothing
        //--------------------------------------------------------------
        internal override void LoadImmediateInt()
        {
            immediateVal = Memory.ExtractBits(instructionData, 0, 11); 
        }
        #endregion
    }
}
