using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mesen.GUI.Config;
using Mesen.GUI.Controls;
using Mesen.GUI.Debugger.Code;

namespace Mesen.GUI.Debugger.Controls
{
	public partial class ctrlAddressList : BaseScrollableTextboxUserControl
	{
		private int[] _addresses;
		private int[] _values;

		public ctrlAddressList()
		{
			InitializeComponent();
		}

		protected override ctrlScrollableTextbox ScrollableTextbox
		{
			get { return this.ctrlDataViewer; }
		}

		public event EventHandler SelectedLineChanged {
			add {
				this.ctrlDataViewer.SelectedLineChanged += value;
			}
			remove {
				this.ctrlDataViewer.SelectedLineChanged -= value;
			}
		}

		public int AddressSize { set { this.ctrlDataViewer.AddressSize = value; } }
		public int MarginWidth { set { this.ctrlDataViewer.MarginWidth = value; } }

		public void SetData(int[] values, int padding, int[] addresses)
		{
			this.ctrlDataViewer.DataProvider = new TraceLoggerCodeDataProvider(
				values.Select((v) => v.ToString("X" + padding.ToString())).ToList(),
				addresses.ToList(),
				values.Select((v) => "").ToList(),
				addresses.Select((a) => 0).ToList()
			);
			_addresses = addresses;
			_values = values;
		}

		public int? CurrentAddress
		{
			get
			{
				if(this._addresses?.Length > 0 && this.ctrlDataViewer.SelectedLine >= 0) {
					return _addresses[_addresses.Length - this.ctrlDataViewer.SelectedLine - 1];
				} else {
					return null;
				}
			}
		}

		public int? CurrentValue
		{
			get
			{
				if(this._values?.Length > 0 && this.ctrlDataViewer.SelectedLine >= 0) {
					return _values[_values.Length - this.ctrlDataViewer.SelectedLine - 1];
				} else {
					return null;
				}
			}
		}
	}
}
