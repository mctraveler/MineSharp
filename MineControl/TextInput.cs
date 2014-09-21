using System;
using System.Windows.Forms;

namespace MineControl
{
	public class TextInput : Form
	{
		TextBox t;
		
		private TextInput (string question)
		{
			Width = 200;
			Height = 150;
			StartPosition = FormStartPosition.CenterParent;
			
			Label l = new Label ();
			l.Text = question;
			l.Left = 20;
			l.Top = 20;
			l.Width = 160;
			l.Height = 20;
			Controls.Add (l);
			
			t = new TextBox ();
			t.Left = 20;
			t.Width = 160;
			t.Top = 60;
			t.Height = 20;
			Controls.Add (t);
			
			Button b = new Button ();
			b.Text = "Ok";
			b.Left = 150;
			b.Width = 40;
			b.Top = 90;
			b.Height = 20;
			b.Click += OkClicked;
			Controls.Add (b);	
		}

		void OkClicked (object sender, EventArgs e)
		{
			Result = t.Text;
			Close ();
		}
		
		public string Result { get; set; }
		
		public static string GetInput (IWin32Window owner, string question)
		{
			TextInput ti = new TextInput (question);
			ti.ShowDialog (owner);
			return ti.Result;
		}
	}
}

