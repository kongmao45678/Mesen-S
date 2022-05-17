#include "stdafx.h"
#include "../Core/Console.h"
#include "../Core/Debugger.h"
#include "../Core/TraceLogger.h"
#include "../Core/MemoryDumper.h"
#include "../Core/MemoryAccessCounter.h"
#include "../Core/Disassembler.h"
#include "../Core/DebugTypes.h"
#include "../Core/Breakpoint.h"
#include "../Core/BreakpointManager.h"
#include "../Core/PpuTools.h"
#include "../Core/CodeDataLogger.h"
#include "../Core/EventManager.h"
#include "../Core/CallstackManager.h"
#include "../Core/LabelManager.h"
#include "../Core/ScriptManager.h"
#include "../Core/Profiler.h"
#include "../Core/Assembler.h"
#include "../Core/BaseEventManager.h"

extern shared_ptr<Console> _console;
static string _logString;

shared_ptr<Debugger> GetDebugger()
{
	return _console->GetDebugger();
}

extern "C"
{
	//Debugger wrapper
	DllExport void __stdcall InitializeDebugger()
	{
		GetDebugger();
	}

	DllExport void __stdcall ReleaseDebugger()
	{
		_console->StopDebugger();
	}

	DllExport bool __stdcall IsDebuggerRunning()
	{
		return _console->GetDebugger(false).get() != nullptr;
	}

	DllExport bool __stdcall IsExecutionStopped() { return GetDebugger()->IsExecutionStopped(); }
	DllExport void __stdcall ResumeExecution() { if(IsDebuggerRunning()) GetDebugger()->Run(); }
	DllExport void __stdcall Step(CpuType cpuType, uint32_t count, StepType type) { GetDebugger()->Step(cpuType, count, type); }

	DllExport void __stdcall RefreshDisassembly(CpuType type) { GetDebugger()->GetDisassembler()->RefreshDisassembly(type); }
	DllExport void __stdcall GetDisassemblyLineData(CpuType type, uint32_t lineIndex, CodeLineData &data) { GetDebugger()->GetDisassembler()->GetLineData(type, lineIndex, data); }
	DllExport uint32_t __stdcall GetDisassemblyLineCount(CpuType type) { return GetDebugger()->GetDisassembler()->GetLineCount(type); }
	DllExport uint32_t __stdcall GetDisassemblyLineIndex(CpuType type, uint32_t cpuAddress) { return GetDebugger()->GetDisassembler()->GetLineIndex(type, cpuAddress); }
	DllExport int32_t __stdcall SearchDisassembly(CpuType type, const char* searchString, int32_t startPosition, int32_t endPosition, bool searchBackwards) { return GetDebugger()->GetDisassembler()->SearchDisassembly(type, searchString, startPosition, endPosition, searchBackwards); }

	DllExport void __stdcall SetTraceOptions(TraceLoggerOptions options) { GetDebugger()->GetTraceLogger()->SetOptions(options); }
	DllExport void __stdcall StartTraceLogger(char* filename) { GetDebugger()->GetTraceLogger()->StartLogging(filename); }
	DllExport void __stdcall StopTraceLogger() { GetDebugger()->GetTraceLogger()->StopLogging(); }
	DllExport void __stdcall ClearTraceLog() { GetDebugger()->GetTraceLogger()->Clear(); }
	DllExport const char* GetExecutionTrace(uint32_t lineCount) { return GetDebugger()->GetTraceLogger()->GetExecutionTrace(lineCount); }

	DllExport void __stdcall SetBreakpoints(Breakpoint breakpoints[], uint32_t length) { GetDebugger()->SetBreakpoints(breakpoints, length); }
	DllExport void __stdcall GetBreakpoints(CpuType cpuType, Breakpoint* breakpoints, int& execs, int& reads, int& writes) { GetDebugger()->GetBreakpoints(cpuType, breakpoints, execs, reads, writes); }

	DllExport int32_t __stdcall EvaluateExpression(char* expression, CpuType cpuType, EvalResultType *resultType, bool useCache) { return GetDebugger()->EvaluateExpression(expression, cpuType, *resultType, useCache); }
	DllExport void __stdcall GetCallstack(CpuType cpuType, StackFrameInfo *callstackArray, uint32_t &callstackSize) { GetDebugger()->GetCallstackManager(cpuType)->GetCallstack(callstackArray, callstackSize); }
	DllExport void __stdcall GetProfilerData(CpuType cpuType, ProfiledFunction* profilerData, uint32_t& functionCount) { GetDebugger()->GetCallstackManager(cpuType)->GetProfiler()->GetProfilerData(profilerData, functionCount); }
	DllExport void __stdcall ResetProfiler(CpuType cpuType) { GetDebugger()->GetCallstackManager(cpuType)->GetProfiler()->Reset(); }

	DllExport void __stdcall GetState(DebugState& state) { GetDebugger()->GetState(state, false); }
	DllExport bool __stdcall GetCpuProcFlag(ProcFlags::ProcFlags flag) { return GetDebugger()->GetCpuProcFlag(flag); }

	DllExport void __stdcall SetCpuRegister(CpuRegister reg, uint16_t value) { GetDebugger()->SetCpuRegister(reg, value); }
	DllExport void __stdcall SetCpuProcFlag(ProcFlags::ProcFlags flag, bool set) { GetDebugger()->SetCpuProcFlag(flag, set); };
	DllExport void __stdcall SetSpcRegister(SpcRegister reg, uint16_t value) { GetDebugger()->SetSpcRegister(reg, value); }
	DllExport void __stdcall SetNecDspRegister(NecDspRegister reg, uint16_t value) { GetDebugger()->SetNecDspRegister(reg, value); }
	DllExport void __stdcall SetSa1Register(CpuRegister reg, uint16_t value) { GetDebugger()->SetSa1Register(reg, value); }
	DllExport void __stdcall SetGsuRegister(GsuRegister reg, uint16_t value) { GetDebugger()->SetGsuRegister(reg, value); }
	DllExport void __stdcall SetCx4Register(Cx4Register reg, uint32_t value) { GetDebugger()->SetCx4Register(reg, value); }
	DllExport void __stdcall SetGameboyRegister(GbRegister reg, uint16_t value) { GetDebugger()->SetGameboyRegister(reg, value); }

	DllExport const char* __stdcall GetDebuggerLog()
	{
		_logString = GetDebugger()->GetLog();
		return _logString.c_str();
	}

	DllExport void __stdcall SetMemoryState(SnesMemoryType type, uint8_t *buffer, int32_t length) { GetDebugger()->GetMemoryDumper()->SetMemoryState(type, buffer, length); }
	DllExport uint32_t __stdcall GetMemorySize(SnesMemoryType type) { return GetDebugger()->GetMemoryDumper()->GetMemorySize(type); }
	DllExport void __stdcall GetMemoryState(SnesMemoryType type, uint8_t *buffer) { GetDebugger()->GetMemoryDumper()->GetMemoryState(type, buffer); }
	DllExport uint8_t __stdcall GetMemoryValue(SnesMemoryType type, uint32_t address) { return GetDebugger()->GetMemoryDumper()->GetMemoryValue(type, address); }
	DllExport void __stdcall SetMemoryValue(SnesMemoryType type, uint32_t address, uint8_t value) { return GetDebugger()->GetMemoryDumper()->SetMemoryValue(type, address, value); }
	DllExport void __stdcall SetMemoryValues(SnesMemoryType type, uint32_t address, uint8_t* data, int32_t length) { return GetDebugger()->GetMemoryDumper()->SetMemoryValues(type, address, data, length); }

	DllExport AddressInfo __stdcall GetAbsoluteAddress(AddressInfo relAddress) { return GetDebugger()->GetAbsoluteAddress(relAddress); }
	DllExport AddressInfo __stdcall GetRelativeAddress(AddressInfo absAddress, CpuType cpuType) { return GetDebugger()->GetRelativeAddress(absAddress, cpuType); }

	DllExport void __stdcall SetLabel(uint32_t address, SnesMemoryType memType, char* label, char* comment) { GetDebugger()->GetLabelManager()->SetLabel(address, memType, label, comment); }
	DllExport void __stdcall ClearLabels() { GetDebugger()->GetLabelManager()->ClearLabels(); }

	DllExport void __stdcall ResetMemoryAccessCounts() { GetDebugger()->GetMemoryAccessCounter()->ResetCounts(); }
	DllExport void __stdcall GetMemoryAccessCounts(uint32_t offset, uint32_t length, SnesMemoryType memoryType, AddressCounters* counts) { GetDebugger()->GetMemoryAccessCounter()->GetAccessCounts(offset, length, memoryType, counts); }

	DllExport void __stdcall GetCdlData(uint32_t offset, uint32_t length, SnesMemoryType memoryType, uint8_t* cdlData) { GetDebugger()->GetCdlData(offset, length, memoryType, cdlData); }
	DllExport void __stdcall SetCdlData(CpuType cpuType, uint8_t* cdlData, uint32_t length) { GetDebugger()->SetCdlData(cpuType, cdlData, length); }
	DllExport void __stdcall MarkBytesAs(CpuType cpuType, uint32_t start, uint32_t end, uint8_t flags) { GetDebugger()->MarkBytesAs(cpuType, start, end, flags); }

	DllExport void __stdcall GetTilemap(GetTilemapOptions options, PpuState state, uint8_t *vram, uint8_t *cgram, uint32_t *buffer) { GetDebugger()->GetPpuTools()->GetTilemap(options, state, vram, cgram, buffer); }
	DllExport void __stdcall GetTileView(GetTileViewOptions options, uint8_t *source, uint32_t srcSize, uint8_t *cgram, uint32_t *buffer) { GetDebugger()->GetPpuTools()->GetTileView(options, source, srcSize, cgram, buffer); }
	DllExport void __stdcall GetSpritePreview(GetSpritePreviewOptions options, PpuState state, uint8_t* vram, uint8_t *oamRam, uint8_t *cgram, uint32_t *buffer) { GetDebugger()->GetPpuTools()->GetSpritePreview(options, state, vram, oamRam, cgram, buffer); }
	DllExport void __stdcall GetSpritePreviewWithBackgroundColor(GetSpritePreviewOptions options, PpuState state, uint8_t* vram, uint8_t *oamRam, uint8_t *cgram, uint32_t backgroundColor, uint32_t *buffer) { GetDebugger()->GetPpuTools()->GetSpritePreviewWithBackgroundColor(options, state, vram, oamRam, cgram, backgroundColor, buffer); }
	DllExport void __stdcall SetViewerUpdateTiming(uint32_t viewerId, uint16_t scanline, uint16_t cycle, CpuType cpuType) { GetDebugger()->GetPpuTools()->SetViewerUpdateTiming(viewerId, scanline, cycle, cpuType); }

	DllExport void __stdcall GetGameboyTilemap(uint8_t* vram, GbPpuState state, uint16_t offset, uint32_t* buffer) { GetDebugger()->GetPpuTools()->GetGameboyTilemap(vram, state, offset, buffer); }
	DllExport void __stdcall GetGameboySpritePreview(GetSpritePreviewOptions options, GbPpuState state, uint8_t* vram, uint8_t* oamRam, uint32_t* buffer) { GetDebugger()->GetPpuTools()->GetGameboySpritePreview(options, state, vram, oamRam, buffer); }

	DllExport void __stdcall GetDebugEvents(CpuType cpuType, DebugEventInfo *infoArray, uint32_t &maxEventCount) { GetDebugger()->GetEventManager(cpuType)->GetEvents(infoArray, maxEventCount); }
	DllExport uint32_t __stdcall GetDebugEventCount(CpuType cpuType, EventViewerDisplayOptions options) { return GetDebugger()->GetEventManager(cpuType)->GetEventCount(options); }
	DllExport void __stdcall GetEventViewerOutput(CpuType cpuType, uint32_t *buffer, uint32_t bufferSize, EventViewerDisplayOptions options) { GetDebugger()->GetEventManager(cpuType)->GetDisplayBuffer(buffer, bufferSize, options); }
	DllExport void __stdcall GetEventViewerEvent(CpuType cpuType, DebugEventInfo *evtInfo, uint16_t scanline, uint16_t cycle, EventViewerDisplayOptions options) { *evtInfo = GetDebugger()->GetEventManager(cpuType)->GetEvent(scanline, cycle, options); }
	DllExport uint32_t __stdcall TakeEventSnapshot(CpuType cpuType, EventViewerDisplayOptions options) { return GetDebugger()->GetEventManager(cpuType)->TakeEventSnapshot(options); }

	DllExport int32_t __stdcall LoadScript(char* name, char* content, int32_t scriptId) { return GetDebugger()->GetScriptManager()->LoadScript(name, content, scriptId); }
	DllExport void __stdcall RemoveScript(int32_t scriptId) { GetDebugger()->GetScriptManager()->RemoveScript(scriptId); }
	DllExport const char* __stdcall GetScriptLog(int32_t scriptId) { return GetDebugger()->GetScriptManager()->GetScriptLog(scriptId); }
	//DllExport void __stdcall DebugSetScriptTimeout(uint32_t timeout) { LuaScriptingContext::SetScriptTimeout(timeout); }

	DllExport uint32_t __stdcall AssembleCode(CpuType cpuType, char* code, uint32_t startAddress, int16_t* assembledOutput) { return GetDebugger()->GetAssembler(cpuType)->AssembleCode(code, startAddress, assembledOutput); }

	DllExport void __stdcall SaveRomToDisk(char* filename, bool saveIpsFile, CdlStripOption cdlStripOption) { GetDebugger()->SaveRomToDisk(filename, saveIpsFile, cdlStripOption); }
};