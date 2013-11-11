// Filename: RegisterEnumerator.cs
// Author: Taylor Eernisse
// Date: 9/18/12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armsimGUI
{
    /// <summary>
    /// enum regs
    /// Purpose: provides an easy way of referencing registers within simulated memory
    /// </summary>
    public enum regs
    {
        r0 = 0,
        r1 = 4,
        r2 = 8,
        r3 = 12,
        r4 = 16,
        r5 = 20,
        r6 = 24,
        r7 = 28,
        r8 = 32,
        r9 = 36,
        r10 = 40,
        r11 = 44,
        FP = 48, // frame pointer
        SP = 52, // stack pointer
        LR = 56, // link register
        PC = 60, // program counter
        CPSR = 64, 
        SPSR = 68
    }
}
