using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Mesen.GUI.Config;
using Mesen.GUI.Controls;
using Mesen.GUI.Debugger;

namespace Mesen.GUI.Forms
{
	public partial class ctrlCheatFinder : BaseControl
	{
		public event EventHandler OnAddCheat;
		public bool TabIsFocused
		{
			set
			{
				_tabIsFocused = value;
				if(chkPauseGameWhileWindowActive.Checked) {
					if(_tabIsFocused) {
						EmuApi.Pause();
					} else {
						EmuApi.Resume();
					}
				}
			}
		}

		private enum BitMode
	  	{
			Bit8,
			Bit16
		}
		private BitMode _bitMode = BitMode.Bit8;

		private List<int[]> _memorySnapshots;
		private List<FilterInfo> _filters;
		private bool _tabIsFocused = false;
		
		private enum CheatPrevFilterType
		{
			Smaller,
			Equal,
			NotEqual,
			Greater
		}

		private enum CheatCurrentFilterType
		{
			Smaller,
			Equal,
			NotEqual,
			Greater
		}

		private class FilterInfo
		{
			public CheatCurrentFilterType? CurrentType;
			public CheatPrevFilterType? PrevType;
			public int Operand;
		}

		public ctrlCheatFinder()
		{
			InitializeComponent();

			BaseConfigForm.InitializeComboBox(cboPrevFilterType, typeof(CheatPrevFilterType));
			BaseConfigForm.InitializeComboBox(cboCurrentFilterType, typeof(CheatCurrentFilterType));
			cboPrevFilterType.SelectedIndex = 0;
			cboCurrentFilterType.SelectedIndex = 0;

			btnUndo.Enabled = false;
			
			lstAddresses.SelectedLineChanged += lstAddresses_SelectedLineChanged;
			lstAddresses.AddressSize = 5;
			lstAddresses.MarginWidth = 6;

			if(LicenseManager.UsageMode != LicenseUsageMode.Designtime) {
				Reset();
				tmrRefresh.Start();
			}
		}
				
		private void Reset()
		{
			_filters = new List<FilterInfo>();
			_memorySnapshots = new List<int[]>();
			TakeSnapshot();
		}

		private void TakeSnapshot()
		{
			if(!EmuApi.IsRunning()) {
				return;
			}

			byte[] memory = GetSnapshot();
			if(_bitMode == BitMode.Bit8) {
				_memorySnapshots.Add(memory.Select((b) => (int)b).ToArray());
			} else {
				// Note the endianness
				List<int> pairs = new List<int>(memory.Length - 1);
				for(int i = 0; i < memory.Length - 1; ++i)
					pairs.Add((int)memory[i + 1] * 256 + (int)memory[i]);
				_memorySnapshots.Add(pairs.ToArray());
			}
			RefreshAddressList();

			UpdateUI();
		}

		private byte[] GetSnapshot()
		{
			return DebugApi.GetMemoryState(SnesMemoryType.WorkRam);
		}

		private void tmrRefresh_Tick(object sender, EventArgs e)
		{
			if(_tabIsFocused) {
				RefreshAddressList();
			}
		}

		private void lstAddresses_SelectedLineChanged(object sender, EventArgs e)
		{
			UpdateUI();
		}

		private void chkHex_CheckedChanged(object sender, EventArgs e)
		{
			nudCurrentFilterValue.Hexadecimal = chkHex.Checked;
			RefreshAddressList();
		}

		private void RefreshAddressList()
		{
			UpdateUI();
			if(!EmuApi.IsRunning()) {
				return;
			}

			int[] memory = _memorySnapshots[_memorySnapshots.Count - 1];
			HashSet<int> matchingAddresses = new HashSet<int>();
			for(int i = 0; i < memory.Length; i++) {
				matchingAddresses.Add(i);
			}

			if(_memorySnapshots.Count > 1) {
				for(int i = 0; i < _memorySnapshots.Count - 1; i++) {
					matchingAddresses = new HashSet<int>(matchingAddresses.Where(addr => {
						if(_filters[i].PrevType.HasValue) {
							switch(_filters[i].PrevType) {
								case CheatPrevFilterType.Smaller: return _memorySnapshots[i+1][addr] > _memorySnapshots[i][addr];
								case CheatPrevFilterType.Equal: return _memorySnapshots[i+1][addr] == _memorySnapshots[i][addr];
								case CheatPrevFilterType.NotEqual: return _memorySnapshots[i+1][addr] != _memorySnapshots[i][addr];
								case CheatPrevFilterType.Greater: return _memorySnapshots[i+1][addr] < _memorySnapshots[i][addr];
							}
						} else {
							switch(_filters[i].CurrentType) {
								case CheatCurrentFilterType.Smaller: return _memorySnapshots[i+1][addr] < _filters[i].Operand;
								case CheatCurrentFilterType.Equal: return _memorySnapshots[i+1][addr] == _filters[i].Operand;
								case CheatCurrentFilterType.NotEqual: return _memorySnapshots[i+1][addr] != _filters[i].Operand;
								case CheatCurrentFilterType.Greater: return _memorySnapshots[i+1][addr] > _filters[i].Operand;
							}
						}
						return false;
					}));
				}
			}

			List<int> values = new List<int>(memory.Length);
			List<int> addresses = new List<int>(memory.Length);
			for(int i = 0; i < memory.Length; i++) {
				if(matchingAddresses.Contains(i)) {
					addresses.Add(i);
					values.Add(memory[i]);
				}
			}
			lstAddresses.SetData(values.ToArray(), addresses.ToArray(), chkHex.Checked, _bitMode == BitMode.Bit8 ? 2 : 4);
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			Reset();
		}

		private void btnUndo_Click(object sender, EventArgs e)
		{
			if(_filters.Count > 0) {
				_filters.RemoveAt(_filters.Count-1);
				_memorySnapshots.RemoveAt(_memorySnapshots.Count-1);
				RefreshAddressList();
			}
		}

		private void btn8Bit_Click(object sender, EventArgs e)
		{
			_bitMode = BitMode.Bit8;
			selectedBitButton.Checked = false;
			selectedBitButton = (System.Windows.Forms.RadioButton)sender;
			selectedBitButton.Checked = true;
			Reset();
		}

		private void btn16Bit_Click(object sender, EventArgs e)
		{
			_bitMode = BitMode.Bit16;
			selectedBitButton.Checked = false;
			selectedBitButton = (System.Windows.Forms.RadioButton)sender;
			selectedBitButton.Checked = true;
			Reset();
		}

		private void UpdateUI()
		{
			btnUndo.Enabled = _filters.Count > 0;
			chkPauseGameWhileWindowActive.Enabled = btnAddCurrentFilter.Enabled = btnAddPrevFilter.Enabled = btnReset.Enabled = cboCurrentFilterType.Enabled = cboPrevFilterType.Enabled = nudCurrentFilterValue.Enabled = chkHex.Enabled = btn8Bit.Enabled = btn16Bit.Enabled = EmuApi.IsRunning();
			mnuCreateCheat.Enabled = btnCreateCheat.Enabled = mnuAddWatch.Enabled = btnAddWatch.Enabled = lstAddresses.CurrentAddress.HasValue;

			if(lstAddresses.CurrentAddress.HasValue) {
				lblAddress.Visible = true;
				lblAtAddress.Visible = true;
				lblAddress.Text = "$" + lstAddresses.CurrentAddress?.ToString("X5");
			} else {
				lblAddress.Visible = false;
				lblAtAddress.Visible = false;
			}
		}

		private void btnAddPrevFilter_Click(object sender, EventArgs e)
		{
			_filters.Add(new FilterInfo { PrevType = (CheatPrevFilterType)cboPrevFilterType.SelectedIndex });
			TakeSnapshot();
		}

		private void btnAddCurrentFilter_Click(object sender, EventArgs e)
		{
			_filters.Add(new FilterInfo { CurrentType = (CheatCurrentFilterType)cboCurrentFilterType.SelectedIndex, Operand = (int)nudCurrentFilterValue.Value });
			TakeSnapshot();
		}

		private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			UpdateUI();
		}

		private void btnCreateCheat_Click(object sender, EventArgs e)
		{
			RomInfo romInfo = EmuApi.GetRomInfo();
			int addr = lstAddresses.CurrentAddress.Value;
			int val = lstAddresses.CurrentValue.Value;

			string codes;
			if(_bitMode == BitMode.Bit8) {
				codes = ((0x7E0000 + addr) * 256 + val).ToString("X8");
			} else {
				// Note the endianness
				codes = ((0x7E0000 + addr) * 256 + val % 256).ToString("X8");
				codes += "\n";
				codes += ((0x7E0000 + addr + 1) * 256 + val / 256).ToString("X8");
			}

			CheatCode newCheat = new CheatCode {
				Description = romInfo.GetRomName(),
				Format = CheatFormat.ProActionReplay,
				Enabled = true,
				Codes = codes
			};
			using(frmCheat frm = new frmCheat(newCheat)) {
				if(frm.ShowDialog() == DialogResult.OK) {
					OnAddCheat?.Invoke(newCheat, new EventArgs());
				}
			}
		}

		private void btnAddWatch_Click(object sender, EventArgs e)
		{
			int addr = lstAddresses.CurrentAddress.Value;

			// We're letting the adding with 0x7E0000 happen in the watch window,
			// so that it's easier to see which WRAM value we're dealing with
			string watch;
			if(_bitMode == BitMode.Bit8) {
				watch = "[$7E0000 + $" + addr.ToString("X5") + "]";
			} else {
				watch = "{$7E0000 + $" + addr.ToString("X5") + "}";
			}
			if(!chkHex.Checked)
				watch += ",U";

			WatchManager.GetWatchManager(CpuType.Cpu).AddWatch(watch);
		}

		private void chkPauseGameWhileWindowActive_CheckedChanged(object sender, EventArgs e)
		{
			if(chkPauseGameWhileWindowActive.Checked) {
				EmuApi.Pause();
			} else {
				EmuApi.Resume();
			}
		}
	}
}
