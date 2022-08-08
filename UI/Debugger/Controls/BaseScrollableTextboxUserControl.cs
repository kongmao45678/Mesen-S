using System.Drawing;
using Mesen.GUI.Controls;

namespace Mesen.GUI.Debugger.Controls
{
	public class BaseScrollableTextboxUserControl : BaseControl
	{
		virtual protected ctrlScrollableTextbox ScrollableTextbox
		{
			get
			{
				return null;
			}
		}

		public void OpenSearchBox()
		{
			this.ScrollableTextbox.OpenSearchBox();
		}

		public void FindNext()
		{
			this.ScrollableTextbox.FindNext();
		}

		public void FindPrevious()
		{
			this.ScrollableTextbox.FindPrevious();
		}

		public void ScrollToLineIndex(int lineIndex)
		{
			this.ScrollableTextbox.ScrollToLineIndex(lineIndex);
		}

		public bool HideSelection
		{
			get { return this.ScrollableTextbox.HideSelection; }
			set { this.ScrollableTextbox.HideSelection = value; }
		}

		public int GetCurrentLine()
		{
			return this.ScrollableTextbox.CurrentLine;
		}

		public string GetWordUnderLocation(Point position)
		{
			return this.ScrollableTextbox.GetWordUnderLocation(position);
		}
	}
}
