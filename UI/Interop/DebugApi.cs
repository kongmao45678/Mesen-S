﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mesen.GUI.Config;
using Mesen.GUI.Debugger;
using Mesen.GUI.Forms;

namespace Mesen.GUI
{
	public class DebugApi
	{
		private const string DllPath = "MesenSCore.dll";
		[DllImport(DllPath)] public static extern void InitializeDebugger();
		[DllImport(DllPath)] public static extern void ReleaseDebugger();

		[DllImport(DllPath)] public static extern void ResumeExecution();
		[DllImport(DllPath)] public static extern void Step(CpuType cpuType, Int32 instructionCount, StepType type = StepType.Step);

		[DllImport(DllPath)] public static extern void StartTraceLogger([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]string filename);
		[DllImport(DllPath)] public static extern void StopTraceLogger();
		[DllImport(DllPath)] public static extern void SetTraceOptions(InteropTraceLoggerOptions options);
		[DllImport(DllPath)] public static extern void ClearTraceLog();

		[DllImport(DllPath, EntryPoint = "GetDebuggerLog")] private static extern IntPtr GetDebuggerLogWrapper();
		public static string GetLog() { return Utf8Marshaler.PtrToStringUtf8(DebugApi.GetDebuggerLogWrapper()).Replace("\n", Environment.NewLine); }

		[DllImport(DllPath, EntryPoint = "GetDisassemblyLineData")] private static extern void GetDisassemblyLineDataWrapper(CpuType type, UInt32 lineIndex, ref InteropCodeLineData lineData);
		public static CodeLineData GetDisassemblyLineData(CpuType type, UInt32 lineIndex)
		{
			InteropCodeLineData data = new InteropCodeLineData();
			data.Comment = new byte[1000];
			data.Text = new byte[1000];
			data.ByteCode = new byte[4];

			DebugApi.GetDisassemblyLineDataWrapper(type, lineIndex, ref data);
			return new CodeLineData(data, type);
		}

		[DllImport(DllPath)] public static extern void RefreshDisassembly(CpuType type);
		[DllImport(DllPath)] public static extern UInt32 GetDisassemblyLineCount(CpuType type);
		[DllImport(DllPath)] public static extern UInt32 GetDisassemblyLineIndex(CpuType type, UInt32 cpuAddress);
		[DllImport(DllPath)] public static extern int SearchDisassembly(CpuType type, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]string searchString, int startPosition, int endPosition, [MarshalAs(UnmanagedType.I1)]bool searchBackwards);

		[DllImport(DllPath, EntryPoint = "GetExecutionTrace")] private static extern IntPtr GetExecutionTraceWrapper(UInt32 lineCount);
		public static string GetExecutionTrace(UInt32 lineCount) { return Utf8Marshaler.PtrToStringUtf8(DebugApi.GetExecutionTraceWrapper(lineCount)); }

		[DllImport(DllPath, EntryPoint = "GetState")] private static extern void GetStateWrapper(ref DebugState state);
		public static DebugState GetState()
		{
			DebugState state = new DebugState();
			DebugApi.GetStateWrapper(ref state);
			return state;
		}

		[DllImport(DllPath)] public static extern void SetCpuRegister(CpuRegister reg, UInt16 value);
		[DllImport(DllPath)] public static extern void SetCpuProcFlag(ProcFlags flag, [MarshalAs(UnmanagedType.I1)]bool set);
		[DllImport(DllPath)] public static extern void SetSpcRegister(SpcRegister reg, UInt16 value);
		[DllImport(DllPath)] public static extern void SetNecDspRegister(NecDspRegister reg, UInt16 value);
		[DllImport(DllPath)] public static extern void SetSa1Register(CpuRegister reg, UInt16 value);
		[DllImport(DllPath)] public static extern void SetGsuRegister(GsuRegister reg, UInt16 value);
		[DllImport(DllPath)] public static extern void SetCx4Register(Cx4Register reg, UInt32 value);
		[DllImport(DllPath)] public static extern void SetGameboyRegister(GbRegister reg, UInt16 value);


		[DllImport(DllPath)] public static extern void SetScriptTimeout(UInt32 timeout);
		[DllImport(DllPath)] public static extern Int32 LoadScript([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]string name, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]string content, Int32 scriptId = -1);
		[DllImport(DllPath)] public static extern void RemoveScript(Int32 scriptId);
		[DllImport(DllPath, EntryPoint = "GetScriptLog")] private static extern IntPtr GetScriptLogWrapper(Int32 scriptId);
		public static string GetScriptLog(Int32 scriptId) { return Utf8Marshaler.PtrToStringUtf8(DebugApi.GetScriptLogWrapper(scriptId)).Replace("\n", Environment.NewLine); }

		[DllImport(DllPath)] public static extern Int32 EvaluateExpression([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]string expression, CpuType cpuType, out EvalResultType resultType, [MarshalAs(UnmanagedType.I1)]bool useCache);

		[DllImport(DllPath)] public static extern Int32 GetMemorySize(SnesMemoryType type);
		[DllImport(DllPath)] public static extern Byte GetMemoryValue(SnesMemoryType type, UInt32 address);
		[DllImport(DllPath)] public static extern void SetMemoryValue(SnesMemoryType type, UInt32 address, byte value);
		[DllImport(DllPath)] public static extern void SetMemoryValues(SnesMemoryType type, UInt32 address, [In] byte[] data, Int32 length);
		[DllImport(DllPath)] public static extern void SetMemoryState(SnesMemoryType type, [In] byte[] buffer, Int32 length);

		[DllImport(DllPath)] public static extern AddressInfo GetAbsoluteAddress(AddressInfo relAddress);
		[DllImport(DllPath)] public static extern AddressInfo GetRelativeAddress(AddressInfo absAddress, CpuType cpuType);

		[DllImport(DllPath)] public static extern void SetLabel(uint address, SnesMemoryType memType, string label, string comment);
		[DllImport(DllPath)] public static extern void ClearLabels();

		[DllImport(DllPath)] public static extern void SetBreakpoints([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]InteropBreakpoint[] breakpoints, UInt32 length);
		[DllImport(DllPath)] public static extern void GetBreakpoints(CpuType cpuType, [In, Out] Breakpoint[] breakpoints, ref Int32 execs, ref Int32 reads, ref Int32 writes);

		[DllImport(DllPath)] public static extern void SaveRomToDisk([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]string filename, [MarshalAs(UnmanagedType.I1)]bool saveAsIps, CdlStripOption cdlStripOption);

		[DllImport(DllPath, EntryPoint = "GetMemoryState")] private static extern void GetMemoryStateWrapper(SnesMemoryType type, [In, Out] byte[] buffer);
		public static byte[] GetMemoryState(SnesMemoryType type)
		{
			byte[] buffer = new byte[DebugApi.GetMemorySize(type)];
			DebugApi.GetMemoryStateWrapper(type, buffer);
			return buffer;
		}

		[DllImport(DllPath)] public static extern void GetTilemap(GetTilemapOptions options, PpuState state, byte[] vram, byte[] cgram, [In, Out] byte[] buffer);
		[DllImport(DllPath)] public static extern void GetTileView(GetTileViewOptions options, byte[] source, int srcSize, byte[] cgram, [In, Out] byte[] buffer);
		[DllImport(DllPath)] public static extern void GetSpritePreview(GetSpritePreviewOptions options, PpuState state, byte[] vram, byte[] oamRam, byte[] cgram, [In, Out] byte[] buffer);
		[DllImport(DllPath)] public static extern void GetSpritePreviewWithBackgroundColor(GetSpritePreviewOptions options, PpuState state, byte[] vram, byte[] oamRam, byte[] cgram, int backgroundColor, [In, Out] byte[] buffer);

		[DllImport(DllPath)] public static extern void GetGameboyTilemap(byte[] vram, GbPpuState state, UInt16 offset, [In, Out] byte[] buffer);
		[DllImport(DllPath)] public static extern void GetGameboySpritePreview(GetSpritePreviewOptions options, GbPpuState state, byte[] vram, byte[] oamRam, [In, Out] byte[] buffer);

		[DllImport(DllPath)] public static extern void SetViewerUpdateTiming(Int32 viewerId, Int32 scanline, Int32 cycle, CpuType cpuType);

		[DllImport(DllPath)] private static extern UInt32 GetDebugEventCount(CpuType cpuType, EventViewerDisplayOptions options);
		[DllImport(DllPath, EntryPoint = "GetDebugEvents")] private static extern void GetDebugEventsWrapper(CpuType cpuType, [In, Out]DebugEventInfo[] eventArray, ref UInt32 maxEventCount);
		public static DebugEventInfo[] GetDebugEvents(CpuType cpuType, EventViewerDisplayOptions options)
		{
			UInt32 maxEventCount = GetDebugEventCount(cpuType, options);
			DebugEventInfo[] debugEvents = new DebugEventInfo[maxEventCount];

			DebugApi.GetDebugEventsWrapper(cpuType, debugEvents, ref maxEventCount);
			if(maxEventCount < debugEvents.Length) {
				//Remove the excess from the array if needed
				Array.Resize(ref debugEvents, (int)maxEventCount);
			}

			return debugEvents;
		}

		[DllImport(DllPath)] public static extern void GetEventViewerEvent(CpuType cpuType, ref DebugEventInfo evtInfo, UInt16 scanline, UInt16 cycle, EventViewerDisplayOptions options);
		[DllImport(DllPath)] public static extern UInt32 TakeEventSnapshot(CpuType cpuType, EventViewerDisplayOptions options);

		[DllImport(DllPath, EntryPoint = "GetEventViewerOutput")] private static extern void GetEventViewerOutputWrapper(CpuType cpuType, [In, Out]byte[] buffer, UInt32 bufferSize, EventViewerDisplayOptions options);
		public static byte[] GetEventViewerOutput(CpuType cpuType, int scanlineWidth, UInt32 scanlineCount, EventViewerDisplayOptions options)
		{
			UInt32 bufferSize = (UInt32)(scanlineWidth * scanlineCount * 2 * 4);
			byte[] buffer = new byte[bufferSize];
			DebugApi.GetEventViewerOutputWrapper(cpuType, buffer, bufferSize, options);
			return buffer;
		}

		[DllImport(DllPath, EntryPoint = "GetCallstack")] private static extern void GetCallstackWrapper(CpuType type, [In, Out]StackFrameInfo[] callstackArray, ref UInt32 callstackSize);
		public static StackFrameInfo[] GetCallstack(CpuType type)
		{
			StackFrameInfo[] callstack = new StackFrameInfo[512];
			UInt32 callstackSize = 0;

			DebugApi.GetCallstackWrapper(type, callstack, ref callstackSize);
			Array.Resize(ref callstack, (int)callstackSize);

			return callstack;
		}

		[DllImport(DllPath)] public static extern void ResetProfiler(CpuType type);
		[DllImport(DllPath, EntryPoint = "GetProfilerData")] private static extern void GetProfilerDataWrapper(CpuType type, [In, Out]ProfiledFunction[] profilerData, ref UInt32 functionCount);
		public static ProfiledFunction[] GetProfilerData(CpuType type)
		{
			ProfiledFunction[] profilerData = new ProfiledFunction[100000];
			UInt32 functionCount = 0;

			DebugApi.GetProfilerDataWrapper(type, profilerData, ref functionCount);
			Array.Resize(ref profilerData, (int)functionCount);

			return profilerData;
		}

		[DllImport(DllPath)] public static extern void ResetMemoryAccessCounts();
		public static void GetMemoryAccessCounts(SnesMemoryType type, ref AddressCounters[] counters)
		{
			int size = DebugApi.GetMemorySize(type);
			Array.Resize(ref counters, size);
			DebugApi.GetMemoryAccessCountsWrapper(0, (uint)size, type, counters);
		}

		[DllImport(DllPath, EntryPoint = "GetMemoryAccessCounts")] private static extern void GetMemoryAccessCountsWrapper(UInt32 offset, UInt32 length, SnesMemoryType type, [In,Out]AddressCounters[] counts);
		public static AddressCounters[] GetMemoryAccessCounts(UInt32 offset, UInt32 length, SnesMemoryType type)
		{
			AddressCounters[] counts = new AddressCounters[length];
			DebugApi.GetMemoryAccessCountsWrapper(offset, length, type, counts);
			return counts;
		}

		[DllImport(DllPath, EntryPoint = "GetCdlData")] private static extern void GetCdlDataWrapper(UInt32 offset, UInt32 length, SnesMemoryType memType, [In,Out] byte[] cdlData);
		public static byte[] GetCdlData(UInt32 offset, UInt32 length, SnesMemoryType memType)
		{
			byte[] cdlData = new byte[length];
			DebugApi.GetCdlDataWrapper(offset, length, memType, cdlData);
			return cdlData;
		}

		[DllImport(DllPath)] public static extern void SetCdlData(CpuType cpuType, [In]byte[] cdlData, Int32 length);
		[DllImport(DllPath)] public static extern void MarkBytesAs(CpuType cpuType, UInt32 start, UInt32 end, CdlFlags type);

		[DllImport(DllPath, EntryPoint = "AssembleCode")] private static extern UInt32 AssembleCodeWrapper(CpuType cpuType, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))]string code, UInt32 startAddress, [In, Out]Int16[] assembledCodeBuffer);
		public static Int16[] AssembleCode(CpuType cpuType, string code, UInt32 startAddress)
		{
			code = code.Replace(Environment.NewLine, "\n");
			Int16[] assembledCode = new Int16[100000];
			UInt32 size = DebugApi.AssembleCodeWrapper(cpuType, code, startAddress, assembledCode);
			Array.Resize(ref assembledCode, (int)size);
			return assembledCode;
		}
	}

	public enum SnesMemoryType
	{
		CpuMemory,
		SpcMemory,
		Sa1Memory,
		NecDspMemory,
		GsuMemory,
		Cx4Memory,
		GameboyMemory,
		PrgRom,
		WorkRam,
		SaveRam,
		VideoRam,
		SpriteRam,
		CGRam,
		SpcRam,
		SpcRom,
		DspProgramRom,
		DspDataRom,
		DspDataRam,
		Sa1InternalRam,
		GsuWorkRam,
		Cx4DataRam,
		BsxPsRam,
		BsxMemoryPack,
		GbPrgRom,
		GbWorkRam,
		GbCartRam,
		GbHighRam,
		GbBootRom,
		GbVideoRam,
		GbSpriteRam,
		Register,
	}

	public static class SnesMemoryTypeExtensions
	{
		public static CpuType ToCpuType(this SnesMemoryType memType)
		{
			switch(memType) {
				case SnesMemoryType.SpcMemory:
				case SnesMemoryType.SpcRam:
				case SnesMemoryType.SpcRom:
					return CpuType.Spc;

				case SnesMemoryType.GsuMemory:
				case SnesMemoryType.GsuWorkRam:
					return CpuType.Gsu;

				case SnesMemoryType.Sa1InternalRam:
				case SnesMemoryType.Sa1Memory:
					return CpuType.Sa1;

				case SnesMemoryType.DspDataRam:
				case SnesMemoryType.DspDataRom:
				case SnesMemoryType.DspProgramRom:
					return CpuType.NecDsp;

				case SnesMemoryType.GbPrgRom:
				case SnesMemoryType.GbWorkRam:
				case SnesMemoryType.GbCartRam:
				case SnesMemoryType.GbHighRam:
				case SnesMemoryType.GbBootRom:
				case SnesMemoryType.GbVideoRam:
				case SnesMemoryType.GbSpriteRam:
				case SnesMemoryType.GameboyMemory:
					return CpuType.Gameboy;

				default:
					return CpuType.Cpu;
			}
		}

		public static bool IsPpuMemory(this SnesMemoryType memType)
		{
			switch(memType) {
				case SnesMemoryType.VideoRam:
				case SnesMemoryType.SpriteRam:
				case SnesMemoryType.CGRam:
				case SnesMemoryType.GbVideoRam:
				case SnesMemoryType.GbSpriteRam:
					return true;

				default:
					return false;
			}
		}

		public static bool IsRelativeMemory(this SnesMemoryType memType)
		{
			switch(memType) {
				case SnesMemoryType.CpuMemory:
				case SnesMemoryType.SpcMemory:
				case SnesMemoryType.Sa1Memory:
				case SnesMemoryType.GsuMemory:
				case SnesMemoryType.NecDspMemory:
				case SnesMemoryType.Cx4Memory:
				case SnesMemoryType.GameboyMemory:
					return true;
			}
			return false;
		}

		public static bool SupportsLabels(this SnesMemoryType memType)
		{
			switch(memType) {
				case SnesMemoryType.PrgRom:
				case SnesMemoryType.WorkRam:
				case SnesMemoryType.SaveRam:
				case SnesMemoryType.Register:
				case SnesMemoryType.SpcRam:
				case SnesMemoryType.SpcRom:
				case SnesMemoryType.Sa1InternalRam:
				case SnesMemoryType.GbPrgRom:
				case SnesMemoryType.GbWorkRam:
				case SnesMemoryType.GbCartRam:
				case SnesMemoryType.GbHighRam:
				case SnesMemoryType.GbBootRom:
					return true;
			}

			return false;
		}

		public static bool SupportsWatch(this SnesMemoryType memType)
		{
			switch(memType) {
				case SnesMemoryType.CpuMemory:
				case SnesMemoryType.SpcMemory:
				case SnesMemoryType.Sa1Memory:
				case SnesMemoryType.GsuMemory:
				case SnesMemoryType.NecDspMemory:
				case SnesMemoryType.Cx4Memory:
				case SnesMemoryType.GameboyMemory:
					return true;
			}

			return false;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AddressCounters
	{
		public UInt32 Address;
		public UInt32 ReadCount;
		public UInt64 ReadStamp;

		public byte UninitRead;
		public UInt32 WriteCount;
		public UInt64 WriteStamp;

		public UInt32 ExecCount;
		public UInt64 ExecStamp;
	}

	public struct AddressInfo
	{
		public Int32 Address;
		public SnesMemoryType Type;
	}

	public enum MemoryOperationType
	{
		Read = 0,
		Write = 1,
		ExecOpCode = 2,
		ExecOperand = 3,
		DmaRead = 4,
		DmaWrite = 5,
		DummyRead = 6
	}

	public struct MemoryOperationInfo
	{
		public UInt32 Address;
		public Int32 Value;
		public MemoryOperationType Type;
	}

	public enum DebugEventType
	{
		Register,
		Nmi,
		Irq,
		Breakpoint
	}

	public struct DmaChannelConfig
	{
		[MarshalAs(UnmanagedType.I1)] public bool DmaActive;

		[MarshalAs(UnmanagedType.I1)] public bool InvertDirection;
		[MarshalAs(UnmanagedType.I1)] public bool Decrement;
		[MarshalAs(UnmanagedType.I1)] public bool FixedTransfer;
		[MarshalAs(UnmanagedType.I1)] public bool HdmaIndirectAddressing;

		public byte TransferMode;

		public UInt16 SrcAddress;
		public byte SrcBank;

		public UInt16 TransferSize;
		public byte DestAddress;

		public UInt16 HdmaTableAddress;
		public byte HdmaBank;
		public byte HdmaLineCounterAndRepeat;

		[MarshalAs(UnmanagedType.I1)] public bool DoTransfer;
		[MarshalAs(UnmanagedType.I1)] public bool HdmaFinished;
		[MarshalAs(UnmanagedType.I1)] public bool UnusedFlag;
	}

	public struct DebugEventInfo
	{
		public MemoryOperationInfo Operation;
		public DebugEventType Type;
		public UInt32 ProgramCounter;
		public UInt16 Scanline;
		public UInt16 Cycle;
		public Int16 BreakpointId;
		public byte DmaChannel;
		public DmaChannelConfig DmaChannelInfo;
	};

	public struct EventViewerDisplayOptions
	{
		public UInt32 IrqColor;
		public UInt32 NmiColor;
		public UInt32 BreakpointColor;

		public UInt32 PpuRegisterReadColor;
		public UInt32 PpuRegisterWriteCgramColor;
		public UInt32 PpuRegisterWriteVramColor;
		public UInt32 PpuRegisterWriteOamColor;
		public UInt32 PpuRegisterWriteMode7Color;
		public UInt32 PpuRegisterWriteBgOptionColor;
		public UInt32 PpuRegisterWriteBgScrollColor;
		public UInt32 PpuRegisterWriteWindowColor;
		public UInt32 PpuRegisterWriteOtherColor;

		public UInt32 ApuRegisterReadColor;
		public UInt32 ApuRegisterWriteColor;
		public UInt32 CpuRegisterReadColor;
		public UInt32 CpuRegisterWriteColor;
		public UInt32 WorkRamRegisterReadColor;
		public UInt32 WorkRamRegisterWriteColor;

		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterCgramWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterVramWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterOamWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterMode7Writes;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterBgOptionWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterBgScrollWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterWindowWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterOtherWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPpuRegisterReads;

		[MarshalAs(UnmanagedType.I1)] public bool ShowCpuRegisterWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowCpuRegisterReads;

		[MarshalAs(UnmanagedType.I1)] public bool ShowApuRegisterWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowApuRegisterReads;
		[MarshalAs(UnmanagedType.I1)] public bool ShowWorkRamRegisterWrites;
		[MarshalAs(UnmanagedType.I1)] public bool ShowWorkRamRegisterReads;

		[MarshalAs(UnmanagedType.I1)] public bool ShowNmi;
		[MarshalAs(UnmanagedType.I1)] public bool ShowIrq;

		[MarshalAs(UnmanagedType.I1)] public bool ShowMarkedBreakpoints;
		[MarshalAs(UnmanagedType.I1)] public bool ShowPreviousFrameEvents;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] ShowDmaChannels;
	}

	public struct GetTilemapOptions
	{
		public byte Layer;
	}

	public enum TileBackground
	{
		Default = 0,
		PaletteColor = 1,
		Black = 2,
		White = 3,
		Magenta = 4
	}

	public struct GetTileViewOptions
	{
		public TileFormat Format;
		public TileLayout Layout;
		public TileBackground Background;
		public Int32 Width;
		public Int32 Palette;
		public Int32 PageSize;
	}

	public struct GetSpritePreviewOptions
	{
		public Int32 SelectedSprite;
	}

	public enum TileFormat
	{
		Bpp2,
		Bpp4,
		Bpp8,
		DirectColor,
		Mode7,
		Mode7DirectColor,
	}

	public enum TileLayout
	{
		Normal,
		SingleLine8x16,
		SingleLine16x16
	};

	[Serializable]
	public struct InteropTraceLoggerOptions
	{
		[MarshalAs(UnmanagedType.I1)] public bool LogCpu;
		[MarshalAs(UnmanagedType.I1)] public bool LogSpc;
		[MarshalAs(UnmanagedType.I1)] public bool LogNecDsp;
		[MarshalAs(UnmanagedType.I1)] public bool LogSa1;
		[MarshalAs(UnmanagedType.I1)] public bool LogGsu;
		[MarshalAs(UnmanagedType.I1)] public bool LogCx4;
		[MarshalAs(UnmanagedType.I1)] public bool LogGameboy;

		[MarshalAs(UnmanagedType.I1)] public bool ShowExtraInfo;
		[MarshalAs(UnmanagedType.I1)] public bool IndentCode;
		[MarshalAs(UnmanagedType.I1)] public bool UseLabels;
		[MarshalAs(UnmanagedType.I1)] public bool UseWindowsEol;
		[MarshalAs(UnmanagedType.I1)] public bool ExtendZeroPage;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
		public byte[] Condition;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
		public byte[] Format;
	}

	public enum EvalResultType
	{
		Numeric = 0,
		Boolean = 1,
		Invalid = 2,
		DivideBy0 = 3,
		OutOfScope = 4
	}

	public struct StackFrameInfo
	{
		public UInt32 Source;
		public UInt32 Target;
		public UInt32 Return;
		public AddressInfo AbsReturn;
		public StackFrameFlags Flags;
	};

	public enum StackFrameFlags
	{
		None = 0,
		Nmi = 1,
		Irq = 2
	}

	public enum CpuType : byte
	{
		Cpu,
		Spc,
		NecDsp,
		Sa1,
		Gsu,
		Cx4,
		Gameboy
	}

	public static class CpuTypeExtensions
	{
		public static SnesMemoryType ToMemoryType(this CpuType cpuType)
		{
			switch(cpuType) {
				case CpuType.Cpu: return SnesMemoryType.CpuMemory;
				case CpuType.Spc: return SnesMemoryType.SpcMemory;
				case CpuType.NecDsp: return SnesMemoryType.NecDspMemory;
				case CpuType.Sa1: return SnesMemoryType.Sa1Memory;
				case CpuType.Gsu: return SnesMemoryType.GsuMemory;
				case CpuType.Cx4: return SnesMemoryType.Cx4Memory;
				case CpuType.Gameboy: return SnesMemoryType.GameboyMemory;

				default:
					throw new Exception("Invalid CPU type");
			}
		}

		public static int GetAddressSize(this CpuType cpuType)
		{
			switch(cpuType) {
				case CpuType.Cpu: return 6;
				case CpuType.Spc: return 4;
				case CpuType.NecDsp: return 4;
				case CpuType.Sa1: return 6;
				case CpuType.Gsu: return 6;
				case CpuType.Cx4: return 6;
				case CpuType.Gameboy: return 4;

				default:
					throw new Exception("Invalid CPU type");
			}
		}
	}

	public enum StepType
	{
		Step,
		StepOut,
		StepOver,
		PpuStep,
		SpecificScanline,
	}

	public enum BreakSource
	{
		Unspecified = -1,
		Breakpoint = 0,
		CpuStep = 1,
		PpuStep = 2,
		BreakOnBrk = 3,
		BreakOnCop = 4,
		BreakOnWdm = 5,
		BreakOnStp = 6,
		BreakOnUninitMemoryRead = 7,

		GbInvalidOamAccess = 8,
		GbInvalidVramAccess = 9,
		GbDisableLcdOutsideVblank = 10,
		GbInvalidOpCode = 11,
		GbNopLoad = 12,
		GbOamCorruption = 13,
	}

	public struct BreakEvent
	{
		public BreakSource Source;
		public MemoryOperationInfo Operation;
		public Int32 BreakpointId;
	}

	public enum CdlStripOption
	{
		StripNone = 0,
		StripUnused = 1,
		StripUsed = 2
	}

	public enum CdlFlags : byte
	{
		None = 0x00,
		Code = 0x01,
		Data = 0x02,
		JumpTarget = 0x04,
		SubEntryPoint = 0x08,

		IndexMode8 = 0x10,
		MemoryMode8 = 0x20,
	}

	public struct ProfiledFunction
	{
		public UInt64 ExclusiveCycles;
		public UInt64 InclusiveCycles;
		public UInt64 CallCount;
		public UInt64 MinCycles;
		public UInt64 MaxCycles;
		public AddressInfo Address;
	}
}
