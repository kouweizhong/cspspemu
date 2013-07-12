﻿using CSharpUtils;
using CSPspEmu.Core.Cpu.VFpu;
using System;
using System.Runtime.CompilerServices;

namespace CSPspEmu.Core.Cpu.Emitter
{
	public unsafe class CpuEmitterUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _rotr_impl(uint Value, int Offset) { return (Value >> Offset) | (Value << (32 - Offset)); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static int _max_impl(int Left, int Right) { return (Left > Right) ? Left : Right; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static int _min_impl(int Left, int Right) { return (Left < Right) ? Left : Right; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _bitrev_impl(uint v)
		{
			v = ((v >> 1) & 0x55555555) | ((v & 0x55555555) << 1); // swap odd and even bits
			v = ((v >> 2) & 0x33333333) | ((v & 0x33333333) << 2); // swap consecutive pairs
			v = ((v >> 4) & 0x0F0F0F0F) | ((v & 0x0F0F0F0F) << 4); // swap nibbles ... 
			v = ((v >> 8) & 0x00FF00FF) | ((v & 0x00FF00FF) << 8); // swap bytes
			v = (v >> 16) | (v << 16); // swap 2-byte long pairs
			return v;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void _div_impl(CpuThreadState CpuThreadState, int Left, int Right)
		{
			if (Right == 0)
			{
				CpuThreadState.LO = Right;
				CpuThreadState.HI = Left;
			}
			else if (Left == int.MinValue && Right == -1)
			{
				CpuThreadState.LO = int.MinValue;
				CpuThreadState.HI = 0;
			}
			else
			{
				CpuThreadState.LO = unchecked(Left / Right);
				CpuThreadState.HI = unchecked(Left % Right);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void _divu_impl(CpuThreadState CpuThreadState, uint Left, uint Right)
		{
			if (Right == 0)
			{
				CpuThreadState.LO = (int)Right;
				CpuThreadState.HI = (int)Left;
			}
			else
			{
				CpuThreadState.LO = unchecked((int)(Left / Right));
				CpuThreadState.HI = unchecked((int)(Left % Right));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _ext_impl(uint Data, int Pos, int Size) { return BitUtils.Extract(Data, Pos, Size); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _ins_impl(uint InitialData, uint Data, int Pos, int Size) { return BitUtils.Insert(InitialData, Pos, Size, Data); }

		// http://aggregate.org/MAGIC/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _clo_impl(uint x)
		{
			uint ret = 0;
			while ((x & 0x80000000) != 0) { x <<= 1; ret++; }
			return ret;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _clz_impl(uint x)
		{
			return _clo_impl(~x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _wsbh_impl(uint v)
		{
			// swap bytes
			return ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint _wsbw_impl(uint v)
		{
			// BSWAP
			return (
				((v & 0xFF000000) >> 24) |
				((v & 0x00FF0000) >> 8) |
				((v & 0x0000FF00) << 8) |
				((v & 0x000000FF) << 24)
			);
		}

		public static int _cvt_w_s_impl(CpuThreadState CpuThreadState, float FS)
		{
			//Console.WriteLine("_cvt_w_s_impl: {0}", CpuThreadState.FPR[FS]);
			switch (CpuThreadState.Fcr31.RM)
			{
				case CpuThreadState.FCR31.TypeEnum.Rint: return (int)MathFloat.Rint(FS);
				case CpuThreadState.FCR31.TypeEnum.Cast: return (int)FS;
				case CpuThreadState.FCR31.TypeEnum.Ceil: return (int)MathFloat.Ceil(FS);
				case CpuThreadState.FCR31.TypeEnum.Floor: return (int)MathFloat.Floor(FS);
			}

			throw(new InvalidCastException("RM has an invalid value!!"));
			//case CpuThreadState.FCR31.TypeEnum.Floor: CpuThreadState.FPR_I[FD] = (int)MathFloat.Floor(CpuThreadState.FPR[FS]); break;
		}

		public static void _cfc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
		{
			switch (RD)
			{
				case 0: // readonly?
					throw (new NotImplementedException("_cfc1_impl.RD=0"));
				case 31:
					CpuThreadState.GPR[RT] = (int)CpuThreadState.Fcr31.Value;
					break;
				default: throw (new Exception(String.Format("Unsupported CFC1({0})", RD)));
			}
		}

		public static void _ctc1_impl(CpuThreadState CpuThreadState, int RD, int RT)
		{
			switch (RD)
			{
				case 31:
					CpuThreadState.Fcr31.Value = (uint)CpuThreadState.GPR[RT];
					break;
				default: throw (new Exception(String.Format("Unsupported CFC1({0})", RD)));
			}
		}

		public static void _comp_impl(CpuThreadState CpuThreadState, float s, float t, bool fc_unordererd, bool fc_equal, bool fc_less, bool fc_inv_qnan)
		{
			if (float.IsNaN(s) || float.IsNaN(t))
			{
				CpuThreadState.Fcr31.CC = fc_unordererd;
			}
			else
			{
				//bool cc = false;
				//if (fc_equal) cc = cc || (s == t);
				//if (fc_less) cc = cc || (s < t);
				//return cc;
				bool equal = (fc_equal) && (s == t);
				bool less = (fc_less) && (s < t);

				CpuThreadState.Fcr31.CC = (less || equal);
			}
		}
		public static void _break_impl(CpuThreadState CpuThreadState, uint PC, uint Value)
		{
			CpuThreadState.PC = PC;
			Console.Error.WriteLine("-------------------------------------------------------------------");
			Console.Error.WriteLine("-- BREAK  ---------------------------------------------------------");
			Console.Error.WriteLine("-------------------------------------------------------------------");
			throw (new PspBreakException("Break!"));
		}

		public static void _cache_impl(CpuThreadState CpuThreadState, uint PC, uint Value)
		{
			CpuThreadState.PC = PC;
			//Console.Error.WriteLine("cache! : 0x{0:X}", Value);
			//CpuThreadState.CpuProcessor.sceKernelIcacheInvalidateAll();
		}

		public static void _sync_impl(CpuThreadState CpuThreadState, uint PC, uint Value)
		{
			CpuThreadState.PC = PC;
			Console.WriteLine("Not implemented 'sync' instruction");
		}

		private static readonly uint[] LwrMask = new uint[] { 0x00000000, 0xFF000000, 0xFFFF0000, 0xFFFFFF00 };
		private static readonly int[] LwrShift = new int[] { 0, 8, 16, 24 };

		private static readonly uint[] LwlMask = new uint[] { 0x00FFFFFF, 0x0000FFFF, 0x000000FF, 0x00000000 };
		private static readonly int[] LwlShift = new int[] { 24, 16, 8, 0 };

		public static uint _lwl_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint Value = *(uint*)CpuThreadState.GetMemoryPtr(Address & unchecked((uint)~3));
			return (uint)((Value << LwlShift[AddressAlign]) | (RT & LwlMask[AddressAlign]));
		}

		public static uint _lwr_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint Value = *(uint*)CpuThreadState.GetMemoryPtr(Address & unchecked((uint)~3));
			return (uint)((Value >> LwrShift[AddressAlign]) | (RT & LwrMask[AddressAlign]));
		}

		private static readonly uint[] SwlMask = new uint[] { 0xFFFFFF00, 0xFFFF0000, 0xFF000000, 0x00000000 };
		private static readonly int[] SwlShift = new int[] { 24, 16, 8, 0 };

		private static readonly uint[] SwrMask = new uint[] { 0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF };
		private static readonly int[] SwrShift = new int[] { 0, 8, 16, 24 };

		public static void _swl_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint* AddressPointer = (uint*)CpuThreadState.GetMemoryPtr(Address & 0xFFFFFFFC);

			*AddressPointer = (RT >> SwlShift[AddressAlign]) | (*AddressPointer & SwlMask[AddressAlign]);
		}

		public static void _swr_exec(CpuThreadState CpuThreadState, uint RS, int Offset, uint RT)
		{
			uint Address = (uint)(RS + Offset);
			uint AddressAlign = (uint)Address & 3;
			uint* AddressPointer = (uint*)CpuThreadState.GetMemoryPtr(Address & 0xFFFFFFFC);

			*AddressPointer = (RT << SwrShift[AddressAlign]) | (*AddressPointer & SwrMask[AddressAlign]);
		}

		public static void _lvl_svl_q(CpuThreadState CpuThreadState, bool Save, float* r0, float* r1, float* r2, float* r3, uint Address)
		{
			//Console.Error.WriteLine("+LLLLLLLLLLLLL {0:X8}", Address);

			int k = (int)(3 - ((Address >> 2) & 3));
			Address &= unchecked((uint)~0xF);

			float*[] registers = new float*[] { r0, r1, r2, r3 };

			fixed (float* VFPR = &CpuThreadState.VFR0)
				for (int j = k; j < 4; j++, Address += 4)
				{
					float* ptr = registers[j];
					var memory_address = Address;
					var memory = (float*)CpuThreadState.GetMemoryPtr(memory_address);

					//Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);

					LanguageUtils.Transfer(
						ref *memory,
						ref *ptr,
						Save
					);

					//Console.Error.WriteLine("_lvl_svl_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);
				}

			//Console.Error.WriteLine("--------------");
		}

		public static void _lvr_svr_q(CpuThreadState CpuThreadState, bool Save, float* r0, float* r1, float* r2, float* r3, uint Address)
		{
			//Console.Error.WriteLine("+RRRRRRRRRRRRR {0:X8}", Address);

			int k = (int)(4 - ((Address >> 2) & 3));
			//Address &= unchecked((uint)~0xF);

			float*[] registers = new float*[] { r0, r1, r2, r3 };

			fixed (float* VFPR = &CpuThreadState.VFR0)
				for (int j = 0; j < k; j++, Address += 4)
				{
					float* ptr = registers[j];
					var memory_address = Address;
					var memory = (float*)CpuThreadState.GetMemoryPtr(memory_address);

					//Console.Error.WriteLine("_lvl_svr_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);

					LanguageUtils.Transfer(
						ref *memory,
						ref *ptr,
						Save
					);

					//Console.Error.WriteLine("_lvl_svr_q({0}): {1:X8}: Reg({2:X8}) {3} Mem({4:X8})", j, memory_address, *(int*)ptr, Save ? "->" : "<-", *(int*)memory);
				}

			//Console.Error.WriteLine("--------------");
		}

		public static void _vrnds(CpuThreadState CpuThreadState, int Seed)
		{
			CpuThreadState.Random = new Random(Seed);
		}

		public static int _vrndi(CpuThreadState CpuThreadState)
		{
			byte[] Data = new byte[4];
			CpuThreadState.Random.NextBytes(Data);
			return BitConverter.ToInt32(Data, 0);
		}

		public static float _vrndf1(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 2.0f);
		}

		public static float _vrndf2(CpuThreadState CpuThreadState)
		{
			return (float)(CpuThreadState.Random.NextDouble() * 4.0f);
		}

		public static void _vpfxd_impl(CpuThreadState CpuThreadState, uint Value) { CpuThreadState.PrefixDestination.Value = Value; }
		public static void _vpfxs_impl(CpuThreadState CpuThreadState, uint Value) { CpuThreadState.PrefixSource.Value = Value; }
		public static void _vpfxt_impl(CpuThreadState CpuThreadState, uint Value) { CpuThreadState.PrefixTarget.Value = Value; }


		static public float _vqmul_row0(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return +(l0 * r3) + (l1 * r2) - (l2 * r1) + (l3 * r0);
			//v3[0] = +(v1[0] * v2[3]) + (v1[1] * v2[2]) - (v1[2] * v2[1]) + (v1[3] * v2[0]);
		}

		static public float _vqmul_row1(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return -(l0 * r2) + (l1 * r3) + (l2 * r0) + (l3 * r1);
			//v3[1] = -(v1[0] * v2[2]) + (v1[1] * v2[3]) + (v1[2] * v2[0]) + (v1[3] * v2[1]);
		}

		static public float _vqmul_row2(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return +(l0 * r1) - (l1 * r0) + (l2 * r3) + (l3 * r2);
			//v3[2] = +(v1[0] * v2[1]) - (v1[1] * v2[0]) + (v1[2] * v2[3]) + (v1[3] * v2[2]);
		}

		static public float _vqmul_row3(float l0, float l1, float l2, float l3, float r0, float r1, float r2, float r3)
		{
			return -(l0 * r0) - (l1 * r1) - (l2 * r2) + (l3 * r3);
			//v3[3] = -(v1[0] * v2[0]) - (v1[1] * v2[1]) - (v1[2] * v2[2]) + (v1[3] * v2[3]);
		}

		public static uint _vi2uc_impl(int x, int y, int z, int w)
		{
			return (0
				| (uint)((x < 0) ? 0 : ((x >> 23) << 0))
				| (uint)((y < 0) ? 0 : ((y >> 23) << 8))
				| (uint)((z < 0) ? 0 : ((z >> 23) << 16))
				| (uint)((w < 0) ? 0 : ((w >> 23) << 24))
			);
		}

		public static uint _vi2c_impl(uint x, uint y, uint z, uint w)
		{
			return ((x >> 24) << 0) | ((y >> 24) << 8) | ((z >> 24) << 16) | ((w >> 24) << 24) | 0;
		}

		public static float _vf2iz(float Value, int imm5)
		{
			float ScalabValue = MathFloat.Scalb(Value, imm5);
			return (Value >= 0) ? (int)MathFloat.Floor(ScalabValue) : (int)MathFloat.Ceil(ScalabValue);
		}

		public static uint _vi2s(uint v1, uint v2)
		{
			return (
				((v1 >> 16) << 0) |
				((v2 >> 16) << 16)
			);
		}

		public static int _vi2us(int x, int y)
		{
			return (
				((x < 0) ? 0 : ((x >> 15) << 0)) |
				((y < 0) ? 0 : ((y >> 15) << 16))
			);
		}

		public static uint _mfvc_impl(CpuThreadState CpuThreadState, VfpuControlRegistersEnum VfpuControlRegister)
		{
			switch (VfpuControlRegister)
			{
				case VfpuControlRegistersEnum.VFPU_PFXS: return CpuThreadState.PrefixSource.Value;
				case VfpuControlRegistersEnum.VFPU_PFXT: return CpuThreadState.PrefixTarget.Value;
				case VfpuControlRegistersEnum.VFPU_PFXD: return CpuThreadState.PrefixDestination.Value;
				case VfpuControlRegistersEnum.VFPU_CC: return CpuThreadState.VFR_CC_Value;
				case VfpuControlRegistersEnum.VFPU_RCX0: return (uint)MathFloat.ReinterpretFloatAsInt((float)(new Random().NextDouble()));
				case VfpuControlRegistersEnum.VFPU_RCX1:
				case VfpuControlRegistersEnum.VFPU_RCX2:
				case VfpuControlRegistersEnum.VFPU_RCX3:
				case VfpuControlRegistersEnum.VFPU_RCX4:
				case VfpuControlRegistersEnum.VFPU_RCX5:
				case VfpuControlRegistersEnum.VFPU_RCX6:
				case VfpuControlRegistersEnum.VFPU_RCX7:
					return (uint)MathFloat.ReinterpretFloatAsInt(1.0f);
				default:
					throw (new NotImplementedException("_mfvc_impl: " + VfpuControlRegister));
			}
		}
	}
}
