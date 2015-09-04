using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using Microsoft.Win32;
using AxSHDocVw;
using MSHTML;

using UWin32;

namespace UWin32
{
	public class user32
	{
		[DllImport("user32.dll", EntryPoint="TrackMouseEvent", SetLastError=true, CharSet=CharSet.Auto)]
		public static extern bool TrackMouseEvent(
		[In, Out, MarshalAs(UnmanagedType.Struct)] ref TRACKMOUSEEVENT lpEventTrack);

		[StructLayout(LayoutKind.Sequential)]
		public struct TRACKMOUSEEVENT
		{
			[MarshalAs(UnmanagedType.U4)]
			public int cbSize;
			[MarshalAs(UnmanagedType.U4)]
				public int dwFlags;
			public IntPtr hwndTrack;
			[MarshalAs(UnmanagedType.U4)]
			public int dwHoverTime;
		}
	}

}

namespace bg
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class BgGraph : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox m_picb;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.HScrollBar m_sbh;
		private SortedList m_slbge;
		private System.Windows.Forms.Button m_pbPrint;
		private Hover m_ch = null;
		private GrapherParams m_gp;
		private int m_iFirstQuarter;
		private AxSHDocVw.AxWebBrowser m_axWebBrowser1;

		public BgGraph()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//   

			m_grph = new Grapher(m_picb.Width, m_picb.Height, this.CreateGraphics());
			m_gp.dBgLow = 30.0;
			m_gp.dBgHigh = 410.0;
			m_gp.nDays = 7;
			m_gp.nIntervals = 19;
			m_gp.fShowMeals = false;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(_bg));
			this.m_picb = new System.Windows.Forms.PictureBox();
			this.m_sbh = new System.Windows.Forms.HScrollBar();
			this.m_pbPrint = new System.Windows.Forms.Button();
//			this.m_axWebBrowser1 = new AxSHDocVw.AxWebBrowser();
//			((System.ComponentModel.ISupportInitialize)(this.m_axWebBrowser1)).BeginInit();
			this.SuspendLayout();
			// 
			// m_picb
			// 
			this.m_picb.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_picb.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_picb.Location = new System.Drawing.Point(16, 32);
			this.m_picb.Name = "m_picb";
			this.m_picb.Size = new System.Drawing.Size(656, 360);
			this.m_picb.TabIndex = 0;
			this.m_picb.TabStop = false;
			this.m_picb.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintGraph);
			this.m_picb.MouseHover += new System.EventHandler(this.HoverGraph);
			this.m_picb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
			// 
			// m_sbh
			// 
			this.m_sbh.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbh.Location = new System.Drawing.Point(16, 392);
			this.m_sbh.Name = "m_sbh";
			this.m_sbh.Size = new System.Drawing.Size(656, 17);
			this.m_sbh.SmallChange = 10;
			this.m_sbh.TabIndex = 1;
			this.m_sbh.Visible = false;
			this.m_sbh.ValueChanged += new System.EventHandler(this.ScrollPaint);
			// 
			// m_pbPrint
			// 
			this.m_pbPrint.Location = new System.Drawing.Point(600, 8);
			this.m_pbPrint.Name = "m_pbPrint";
			this.m_pbPrint.TabIndex = 2;
			this.m_pbPrint.Text = "Print";
			this.m_pbPrint.Click += new System.EventHandler(this.PrintGraph);
			// 
			// m_axWebBrowser1
			// 
			this.m_axWebBrowser1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_axWebBrowser1.Enabled = true;
			this.m_axWebBrowser1.Location = new System.Drawing.Point(16, 409);
			object o = (resources.GetObject("m_axWebBrowser1.OcxState"));
			this.m_axWebBrowser1.OcxState = ((System.Windows.Forms.AxHost.State)(o));
			this.m_axWebBrowser1.Size = new System.Drawing.Size(656, 100);
			this.m_axWebBrowser1.TabIndex = 10;
			this.m_axWebBrowser1.Visible = false;
//			this.m_axWebBrowser1.DocumentComplete += new AxSHDocVw.DWebBrowserEvents2_DocumentCompleteEventHandler(this.TriggerDocumentDone);

			// 
			// BgGraph
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(688, 590);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pbPrint,
																		  this.m_sbh,
																		  this.m_picb,
																		  this.m_axWebBrowser1});
			this.Name = "BgGraph";
			this.Text = "Form1";
			this.SizeChanged += new System.EventHandler(this.HandleSizeChange);
			((System.ComponentModel.ISupportInitialize)(this.m_axWebBrowser1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		public void SetDataPoints(SortedList slbge, string sReport)
		{
			m_slbge = slbge;
			m_grph.SetDataPoints(m_slbge);
			if (sReport != null)
				{
				object Zero = 0;
				object EmptyString = "";
				m_axWebBrowser1.Navigate(sReport, ref Zero, ref EmptyString, ref EmptyString, ref EmptyString);
				m_axWebBrowser1.Visible = true;
				}

		}

		public void SetBounds(double dLow, double dHigh, int nDays, int nBgIntervals, bool fShowMeals)
		{
			m_gp.dBgLow = dLow;
			m_gp.dBgHigh = dHigh;
			m_gp.nDays = nDays;
			m_gp.nIntervals = nBgIntervals;
			m_gp.fShowMeals = fShowMeals;

			m_grph.SetProps(m_gp);
		}


		public Grapher m_grph;


		private void PaintGraph(object sender, System.Windows.Forms.PaintEventArgs e) 
		{
			e.Graphics.Clear(this.BackColor);
			m_grph.Paint(e.Graphics);
		}


		private void ScrollPaint(object sender, System.EventArgs e) 
		{
			HScrollBar sbh = (HScrollBar)sender;

			m_grph.SetFirstQuarter(m_iFirstQuarter = sbh.Value);
			m_picb.Invalidate();
		}

		private void HandleSizeChange(object sender, System.EventArgs e) 
		{
			m_grph = new Grapher(m_picb.Width, m_picb.Height, this.CreateGraphics());
			m_grph.SetProps(m_gp);
			m_grph.SetDataPoints(m_slbge);
			m_grph.SetFirstQuarter(m_iFirstQuarter);

			m_grph.CalcGraph(m_sbh);
			m_picb.Invalidate();
		}

		public void CalcGraph()
		{
			m_grph.CalcGraph(m_sbh);
		}

		private void HoverGraph(object sender, System.EventArgs e)
		{
			PictureBox pb = (PictureBox)sender;

			Point ptRaw = Cursor.Position;
			Point pt =  pb.PointToClient(ptRaw);

			PTFI ptfiHit = new PTFI();
			bool fHit = false;
			RectangleF rectfHit;

			fHit = m_grph.FHitTest(pt, out ptfiHit, out rectfHit);

			if (fHit)
				{
				if (m_ch == null)
					m_ch = new Hover();
				m_ch.ShowTip(ptRaw, ptfiHit.bge);
				this.Focus();
				m_fTipShowing = true;
				m_rectfTipHitRegion = rectfHit;
				}

			this.Focus();

			// now lets register for this again
			user32.TRACKMOUSEEVENT tme = new user32.TRACKMOUSEEVENT();

			tme.cbSize = Marshal.SizeOf(tme);
			tme.dwFlags = 1;
			tme.dwHoverTime = -1;
			tme.hwndTrack = pb.Handle;
			user32.TrackMouseEvent(ref tme);
		}

		private void HandleMouse(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (m_fTipShowing == false)
				return;

			if (m_rectfTipHitRegion.Contains(new PointF((float)e.X, (float)e.Y)))
				return;

			m_fTipShowing = false;
			m_ch.Hide();
		}

		private bool m_fTipShowing;
		private RectangleF m_rectfTipHitRegion;

		private void PrintPageHandler(object sender, PrintPageEventArgs ev)
		{
			Rectangle rectMargins = ev.MarginBounds;
			Rectangle rectPage = ev.PageBounds;

			Grapher grph = new Grapher(rectPage.Width, rectPage.Height, ev.Graphics);
			grph.SetProps(m_gp);
			grph.SetMargins(rectMargins.Left, rectPage.Width - rectMargins.Right, rectMargins.Top, rectPage.Height - rectMargins.Bottom);
			grph.SetFirstQuarter(m_iFirstQuarter);
			grph.SetDataPoints(m_slbge);
			grph.CalcGraph(null);
			grph.Paint(ev.Graphics);
		}

		PrinterSettings m_prtSettings = null;
		PageSettings m_pgSettings = null;

		private void PrintGraph(object sender, System.EventArgs e) 
		{
			PrintDocument ppd = new PrintDocument();
			PrintDialog dlgPrint = new PrintDialog();
			if (m_pgSettings != null)
				{
				ppd.DefaultPageSettings = m_pgSettings;
				}
			else
				{
				ppd.DefaultPageSettings.Landscape = true;
				ppd.DefaultPageSettings.Margins = new Margins(25, 25, 25, 25);
				}

			if (m_prtSettings != null)
				{
				ppd.PrinterSettings = m_prtSettings;
				}
			

			ppd.PrintPage += new PrintPageEventHandler(PrintPageHandler);
			
			dlgPrint.Document = ppd;

			dlgPrint.ShowDialog();
			m_prtSettings = ppd.PrinterSettings;
			m_pgSettings = ppd.DefaultPageSettings;
			ppd.Print();

		}

	}
}
