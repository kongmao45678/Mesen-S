#pragma once
#include "stdafx.h"

enum class  WindowMaskLogic
{
	Or = 0,
	And = 1,
	Xor = 2,
	Xnor = 3
};

enum class ColorWindowMode
{
	Never = 0,
	OutsideWindow = 1,
	InsideWindow = 2,
	Always = 3
};

struct SpriteInfo
{
	int16_t X;
	uint8_t Y;
	uint8_t Index;
	uint8_t Width;
	uint8_t Height;
	bool HorizontalMirror;
	uint8_t Priority;

	uint8_t Palette;
	int8_t ColumnOffset;

	int16_t DrawX;
	uint16_t FetchAddress;
	uint16_t ChrData[2];

	bool IsVisible(uint16_t scanline, bool interlace)
	{
		if(X != -256 && (X + Width <= 0 || X > 255)) {
			//Sprite is not visible (and must be ignored for time/range flag calculations)
			//Sprites at X=-256 are always used when considering Time/Range flag calculations, but not actually drawn.
			return false;
		}

		uint16_t endY = (Y + (interlace ? (Height >> 1) : Height));
		uint8_t endY_8 = endY & 0xFF;
		return (scanline >= Y && scanline < endY) || (endY_8 < Y && scanline < endY_8);
	}
};

struct TileData
{
	uint16_t TilemapData;
	uint16_t VScroll;
	uint16_t ChrData[4];
};

struct LayerData
{
	TileData Tiles[33];
};

struct LayerConfig
{
	uint16_t TilemapAddress;
	uint16_t ChrAddress;

	uint16_t HScroll;
	uint16_t VScroll;

	bool DoubleWidth;
	bool DoubleHeight;

	bool LargeTiles;
};

struct Mode7Config
{
	int16_t Matrix[4];

	int16_t HScroll;
	int16_t VScroll;
	int16_t CenterX;
	int16_t CenterY;

	uint8_t ValueLatch;
	
	bool LargeMap;
	bool FillWithTile0;
	bool HorizontalMirroring;
	bool VerticalMirroring;

	//Holds the scroll values at the start of a scanline for the entire scanline
	int16_t HScrollLatch;
	int16_t VScrollLatch;
};

struct WindowConfig
{
	bool ActiveLayers[6];
	bool InvertedLayers[6];
	uint8_t Left;
	uint8_t Right;

	template<uint8_t layerIndex>
	bool PixelNeedsMasking(int x)
	{
		if(InvertedLayers[layerIndex]) {
			if(Left > Right) {
				return true;
			} else {
				return x < Left || x > Right;
			}
		} else {
			if(Left > Right) {
				return false;
			} else {
				return x >= Left && x <= Right;
			}
		}
	}
};

struct PpuState
{
	uint16_t Cycle;
	uint16_t Scanline;
	uint16_t HClock;
	uint32_t FrameCount;

	bool ForcedVblank;
	uint8_t ScreenBrightness;

	Mode7Config Mode7;

	uint8_t BgMode;
	bool Mode1Bg3Priority;

	uint8_t MainScreenLayers;
	uint8_t SubScreenLayers;
	LayerConfig Layers[4];

	WindowConfig Window[2];
	WindowMaskLogic MaskLogic[6];
	bool WindowMaskMain[5];
	bool WindowMaskSub[5];

	uint16_t VramAddress;
	uint8_t VramIncrementValue;
	uint8_t VramAddressRemapping;
	bool VramAddrIncrementOnSecondReg;
	uint16_t VramReadBuffer;

	uint8_t Ppu1OpenBus;
	uint8_t Ppu2OpenBus;

	uint8_t CgramAddress;
	uint8_t CgramWriteBuffer;
	bool CgramAddressLatch;

	uint8_t MosaicSize = 0;
	uint8_t MosaicEnabled = 0;

	uint16_t OamRamAddress = 0;

	uint8_t OamMode;
	uint16_t OamBaseAddress;
	uint16_t OamAddressOffset;
	bool EnableOamPriority;

	bool ExtBgEnabled = false;
	bool HiResMode = false;
	bool ScreenInterlace = false;
	bool ObjInterlace = false;
	bool OverscanMode = false;
	bool DirectColorMode = false;

	ColorWindowMode ColorMathClipMode = ColorWindowMode::Never;
	ColorWindowMode ColorMathPreventMode = ColorWindowMode::Never;
	bool ColorMathAddSubscreen = false;
	uint8_t ColorMathEnabled = 0;
	bool ColorMathSubstractMode = false;
	bool ColorMathHalveResult = false;
	uint16_t FixedColor = 0;
};


enum PixelFlags
{
	AllowColorMath = 0x80,
};