﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Vfs;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Modules.rtc
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public class sceRtc : HleModuleHost
	{
		/// <summary>
		/// Get the resolution of the tick counter
		/// </summary>
		/// <returns>Number of ticks per second</returns>
		[HlePspFunction(NID = 0xC41C2853, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public uint sceRtcGetTickResolution()
		{
			return (uint)(TimeSpan.FromSeconds(1).TotalMilliseconds * 1000);
		}

		/// <summary>
		/// Get current tick count (number of microseconds)
		/// </summary>
		/// <param name="Tick">pointer to u64 to receive tick count</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x3F7AD767, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceRtcGetCurrentTick(long* Tick)
		{
			HleState.PspRtc.Update();
			*Tick = HleState.PspRtc.ElapsedTime.TotalMicroseconds;
			return 0;
		}

		Calendar Calendar = new GregorianCalendar(GregorianCalendarTypes.USEnglish);

		/// <summary>
		/// Get number of days in a specific month
		/// </summary>
		/// <param name="Year">Year in which to check (accounts for leap year)</param>
		/// <param name="Month">Month to get number of days for</param>
		/// <returns># of days in month, less than 0 on error (?)</returns>
		[HlePspFunction(NID = 0x05EF322C, FirmwareVersion = 150)]
		public int sceRtcGetDaysInMonth(int Year, int Month)
		{
			return Calendar.GetDaysInMonth(Year, Month);
			//new DateTime(Year, Month, 1).
			//return Date(Year, Month, 1).daysInMonth;
		}

		public enum PspDaysOfWeek : int
		{
			Monday = 0,
			Tuesday = 1,
			Wednesday = 2,
			Thursday = 3,
			Friday = 4,
			Saturday = 5,
			Sunday = 6,
		}

		/// <summary>
		/// Get day of the week for a date
		/// </summary>
		/// <param name="Year">Year in which to check (accounts for leap year)</param>
		/// <param name="Month">Month that day is in</param>
		/// <param name="Day">Day to get day of week for</param>
		/// <returns>Day of week with 0 representing Monday</returns>
		[HlePspFunction(NID = 0x57726BC1, FirmwareVersion = 150)]
		public PspDaysOfWeek sceRtcGetDayOfWeek(int Year, int Month, int Day)
		{
			switch (Calendar.GetDayOfWeek(new DateTime(Year, Month, Day)))
			{
				case DayOfWeek.Monday: return PspDaysOfWeek.Monday;
				case DayOfWeek.Tuesday: return PspDaysOfWeek.Tuesday;
				case DayOfWeek.Wednesday: return PspDaysOfWeek.Wednesday;
				case DayOfWeek.Thursday: return PspDaysOfWeek.Thursday;
				case DayOfWeek.Friday: return PspDaysOfWeek.Friday;
				case DayOfWeek.Saturday: return PspDaysOfWeek.Saturday;
				case DayOfWeek.Sunday: return PspDaysOfWeek.Sunday;
				default: throw(new InvalidCastException());
			}
		}

		/// <summary>
		/// Add two ticks
		/// </summary>
		/// <param name="destTick">pointer to tick to hold result</param>
		/// <param name="srcTick">pointer to source tick</param>
		/// <param name="numTicks">number of ticks to add</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x44F45E05, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceRtcTickAddTicks(ulong* destTick, ulong* srcTick, ulong numTicks)
		{
			throw(new NotImplementedException());
			/*
			*destTick = *srcTick + numTicks;
			return 0;
			*/
		}

		/// <summary>
		/// Set ticks based on a pspTime struct
		/// </summary>
		/// <param name="Date">pointer to pspTime to convert</param>
		/// <param name="Tick">pointer to tick to set</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x6FF40ACC, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceRtcGetTick(ScePspDateTime* Date, ulong* Tick)
		{
			*Tick = (ulong)Date->ToDateTime().GetTotalNanoseconds();
			return 0;
		}

		/// <summary>
		/// Set a pspTime struct based on ticks
		/// </summary>
		/// <param name="date">pointer to pspTime struct to set</param>
		/// <param name="tick">pointer to ticks to convert</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x7ED29E40, FirmwareVersion = 150)]
		public int sceRtcSetTick(ScePspDateTime* date, ulong* tick)
		{
			throw (new NotImplementedException());
			/*
			date.parse(*tick);
			return 0;
			*/
		}

		/// <summary>
		/// Get current local time into a pspTime struct
		/// </summary>
		/// <param name="time">pointer to pspTime struct to receive time</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xE7C27D1B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceRtcGetCurrentClockLocalTime(ScePspDateTime *Time)
		{
			if (Time == null) throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_ARGUMENT));

			HleState.PspRtc.Update();

			Time->Year = (ushort)HleState.PspRtc.CurrentDateTime.Year;
			Time->Month = (ushort)HleState.PspRtc.CurrentDateTime.Month;
			Time->Day = (ushort)HleState.PspRtc.CurrentDateTime.Day;
			Time->Hour = (ushort)HleState.PspRtc.CurrentDateTime.Hour;
			Time->Minute = (ushort)HleState.PspRtc.CurrentDateTime.Minute;
			Time->Second = (ushort)HleState.PspRtc.CurrentDateTime.Second;
			Time->Microsecond = (uint)(HleState.PspRtc.CurrentDateTime.Millisecond * 1000);

			return 0;
		}

		/// <summary>
		/// Add an amount of ms to a tick
		/// </summary>
		/// <param name="dstPtr">pointer to tick to hold result</param>
		/// <param name="srcPtr">pointer to source tick</param>
		/// <param name="value">number of ms to add</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x26D25A5D, FirmwareVersion = 150)]
		public int sceRtcTickAddMicroseconds(long* dstPtr, long* srcPtr, long value)
		{
			*dstPtr = *srcPtr + value;
			return 0;
		}
	}
}
