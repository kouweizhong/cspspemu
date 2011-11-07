﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Cpu.Cpu.Emiter
{
	sealed public class CpuBranchAnalyzer
	{
		public Instruction Instruction;

		public enum Flags
		{
			NormalInstruction = (1 << 0),
			BranchOrJumpInstruction = (1 << 1),
			JumpInstruction = (1 << 2),
			JumpAlways = (1 << 3),
			Likely = (1 << 4),
			AndLink = (1 << 5),

			FixedJump = 0,
			DynamicJump = (1 << 6),
		}

		public Flags bvf() { throw (new NotImplementedException()); }
		public Flags bvfl() { throw (new NotImplementedException()); }
		public Flags bvt() { throw (new NotImplementedException()); }
		public Flags bvtl() { throw (new NotImplementedException()); }

		public Flags beq() { return Flags.BranchOrJumpInstruction | ((Instruction.RS == Instruction.RT) ? Flags.JumpAlways : 0); }
		public Flags beql() { return beq() | Flags.Likely; }
		public Flags bne() { return Flags.BranchOrJumpInstruction; }
		public Flags bnel() { return bne() | Flags.Likely; }

		public Flags bltz() { return Flags.BranchOrJumpInstruction; }
		public Flags bltzl() { return bltz() | Flags.Likely; }
		public Flags bltzal() { return Flags.BranchOrJumpInstruction | Flags.AndLink; }
		public Flags bltzall() { return bltzal() | Flags.Likely; }

		public Flags blez() { return Flags.BranchOrJumpInstruction; }
		public Flags blezl() { return blez() | Flags.Likely; }

		public Flags bgtz() { return Flags.BranchOrJumpInstruction; }
		public Flags bgtzl() { throw (new NotImplementedException()); }
		public Flags bgez() { return Flags.BranchOrJumpInstruction; }
		public Flags bgezl() { throw (new NotImplementedException()); }
		public Flags bgezal() { return Flags.BranchOrJumpInstruction | Flags.AndLink; }
		public Flags bgezall() { throw (new NotImplementedException()); }

		public Flags j() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways; }
		public Flags jr() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways | Flags.DynamicJump; }
		public Flags jalr() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways | Flags.AndLink | Flags.DynamicJump; }
		public Flags jal() { return Flags.BranchOrJumpInstruction | Flags.JumpInstruction | Flags.JumpAlways | Flags.AndLink; }

		public Flags bc1f() { throw (new NotImplementedException()); }
		public Flags bc1t() { throw (new NotImplementedException()); }
		public Flags bc1fl() { throw (new NotImplementedException()); }
		public Flags bc1tl() { throw (new NotImplementedException()); }

		public Flags unknown() { return Flags.NormalInstruction; }
	}
}
