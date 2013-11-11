using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace armsimGUI
{
    public static class BarrelShifter
    {
        public static uint rotateVal(uint data, byte shiftType, uint shiftVal)
        {
            uint shiftedData;
            //shiftVal *= 2; // compensate for the instruction compression
            switch (shiftType)
            {
                case 0x00:
                    shiftedData = LSL(data, (byte)shiftVal);
                    return shiftedData;
                case 0x01:
                    shiftedData = LSR(data, (byte)shiftVal);
                    return shiftedData;
                case 0x02:
                    shiftedData = ASR(data, (byte)shiftVal);
                    return shiftedData;
                case 0x03:
                    shiftedData = ROR(data, (byte)shiftVal);
                    return shiftedData;
                default:
                    return 0x101; // invalid shifted value, so impossible result; signifies invalid instruction
            }
        }

        //--------------------------------------------------------------
        // Purpose: Performs a logical shift left on <data> by <shiftVal> positions
        // Returns: nothing
        //--------------------------------------------------------------
        private static uint LSL(uint data, byte shiftVal)
        {
            return data << shiftVal;
        }

        //--------------------------------------------------------------
        // Purpose: Performs a logical shift right on <data> by <shiftVal> positions
        // Returns: nothing
        //--------------------------------------------------------------
        private static uint LSR(uint data, byte shiftVal)
        {
            return data >> shiftVal;
        }

        //--------------------------------------------------------------
        // Purpose: Performs an arithmetic shift right on <data> by <shiftVal> positions
        // Returns: nothing
        //--------------------------------------------------------------
        private static uint ASR(uint data, byte shiftVal)
        {
            uint MSB = data & 0x80000000; // test MSB
            if (MSB == 0x80000000)
            {
                for (int i = 0; i < shiftVal; i++)
                {
                    data = data >> 1;
                    data = data | 0x80000000;
                }
                return data;
            }
            else return data >> shiftVal;
        }

        //--------------------------------------------------------------
        // Purpose: Performs a rotate right on <data> by <shiftVal> positions
        // Returns: nothing
        //--------------------------------------------------------------
        private static uint ROR(uint data, byte shiftVal)
        {
            // rotate <data> right <shiftVal> positions
            uint rightShift = data >> shiftVal;
            // rotate <data> left 32 - <shiftVal> positions
            uint leftShift = data << (32 - shiftVal);
            // bitwise OR the two together
            return (rightShift | leftShift);
        }
    }
}
