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
		private System.Windows.Forms.PictureBox m_picbUpper;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.HScrollBar m_sbhUpper;
		private SortedList m_slbge;
		private System.Windows.Forms.Button m_pbPrint;
		private Hover m_ch = null;
		private GrapherParams m_gp;
		private System.Windows.Forms.VScrollBar m_sbvUpper;
		private System.Windows.Forms.PictureBox m_picbLower;
		private System.Windows.Forms.HScrollBar m_sbhLower;
		private System.Windows.Forms.VScrollBar m_sbvLower;

		public BgGraph()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//   

			m_gp.dBgLow = 30.0;
			m_gp.dBgHigh = 220.0;
			m_gp.nDays = 7;
			m_gp.nIntervals = 19;
			m_gp.fShowMeals = false;
			m_bvUpper = BoxView.Graph;
			m_bvLower = BoxView.Log;

			m_sbvUpper.Tag = m_picbUpper;
			m_sbvLower.Tag = m_picbLower;

			m_sbhUpper.Tag = m_picbUpper;
			m_sbhLower.Tag = m_picbLower;

			SetupViews(this.ClientSize.Height);
		}

		/* S E T U P  V I E W S */
		/*----------------------------------------------------------------------------
			%%Function: SetupViews
			%%Qualified: bg.BgGraph.SetupViews
			%%Contact: rlittle
			
		----------------------------------------------------------------------------*/
		void SetupViews(int nHeight)
		{
			int nMarginTop = 32;
			int nMarginBottom = 13;
			int nMarginBetween = 0;
			int nPctUpper = 68;
			int nPctLower = 32;
			int nHeightAvail = (nHeight - nMarginTop - nMarginBottom - m_sbhUpper.Height - m_sbhLower.Height);

			if (m_bvUpper == BoxView.None)
				{
				nPctUpper = 0;
				nPctLower = 100;
				m_picbUpper.Visible = false;
				m_sbhUpper.Visible = false;
				m_sbvUpper.Visible = false;
				}
			if (m_bvLower == BoxView.None)
				{
				nPctLower = 0;
				nPctUpper = 100;
				m_picbLower.Visible = false;
				m_sbhLower.Visible = false;
				m_sbvLower.Visible = false;
				}

			m_picbUpper.Location = new Point(m_picbUpper.Location.X, nMarginTop);
			m_picbUpper.Height = (nHeightAvail * nPctUpper) / 100;

			m_sbhUpper.Location = new Point(m_sbhUpper.Location.X, m_picbUpper.Location.Y + m_picbUpper.Height);
			m_sbvUpper.Location = new Point(m_sbvUpper.Location.X, m_picbUpper.Location.Y);
			m_sbvUpper.Height = m_picbUpper.Height;

			m_picbLower.Location = new Point(m_picbLower.Location.X, m_sbhUpper.Location.Y + m_sbhUpper.Height + nMarginBetween);
			m_picbLower.Height = (nHeightAvail * nPctLower) / 100;

			m_sbhLower.Location = new Point(m_sbhLower.Location.X, m_picbLower.Location.Y + m_picbLower.Height);
			m_sbvLower.Location = new Point(m_sbvLower.Location.X, m_picbLower.Location.Y);
			m_sbvLower.Height = m_picbLower.Height;
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
			this.m_picbUpper = new System.Windows.Forms.PictureBox();
			this.m_sbhUpper = new System.Windows.Forms.HScrollBar();
			this.m_pbPrint = new System.Windows.Forms.Button();
			this.m_sbvUpper = new System.Windows.Forms.VScrollBar();
			this.m_picbLower = new System.Windows.Forms.PictureBox();
			this.m_sbhLower = new System.Windows.Forms.HScrollBar();
			this.m_sbvLower = new System.Windows.Forms.VScrollBar();
			this.SuspendLayout();
			// 
			// m_picbUpper
			// 
			this.m_picbUpper.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_picbUpper.BackColor = System.Drawing.SystemColors.Window;
			this.m_picbUpper.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_picbUpper.Location = new System.Drawing.Point(16, 32);
			this.m_picbUpper.Name = "m_picbUpper";
			this.m_picbUpper.Size = new System.Drawing.Size(648, 176);
			this.m_picbUpper.TabIndex = 0;
			this.m_picbUpper.TabStop = false;
			this.m_picbUpper.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintGraph);
			this.m_picbUpper.MouseHover += new System.EventHandler(this.HoverGraph);
			this.m_picbUpper.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
			// 
			// m_sbhUpper
			// 
			this.m_sbhUpper.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbhUpper.Location = new System.Drawing.Point(16, 392);
			this.m_sbhUpper.Name = "m_sbhUpper";
			this.m_sbhUpper.Size = new System.Drawing.Size(648, 17);
			this.m_sbhUpper.SmallChange = 10;
			this.m_sbhUpper.TabIndex = 1;
			this.m_sbhUpper.Visible = false;
			this.m_sbhUpper.ValueChanged += new System.EventHandler(this.ScrollPaint);
			// 
			// m_pbPrint
			// 
			this.m_pbPrint.Location = new System.Drawing.Point(600, 8);
			this.m_pbPrint.Name = "m_pbPrint";
			this.m_pbPrint.TabIndex = 2;
			this.m_pbPrint.Text = "Print";
			this.m_pbPrint.Click += new System.EventHandler(this.PrintGraph);
			this.m_pbPrint.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			// 
			// m_sbvUpper
			// 
			this.m_sbvUpper.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbvUpper.Location = new System.Drawing.Point(664, 32);
			this.m_sbvUpper.Name = "m_sbvUpper";
			this.m_sbvUpper.Size = new System.Drawing.Size(16, 360);
			this.m_sbvUpper.TabIndex = 3;
			this.m_sbvUpper.ValueChanged += new System.EventHandler(this.ScrollVertPaint);
			// 
			// m_picbLower
			// 
			this.m_picbLower.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_picbLower.BackColor = System.Drawing.SystemColors.Window;
			this.m_picbLower.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_picbLower.Location = new System.Drawing.Point(16, 416);
			this.m_picbLower.Name = "m_picbLower";
			this.m_picbLower.Size = new System.Drawing.Size(648, 144);
			this.m_picbLower.TabIndex = 4;
			this.m_picbLower.TabStop = false;
			this.m_picbLower.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintGraph);
			// 
			// m_sbhLower
			// 
			this.m_sbhLower.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbhLower.Location = new System.Drawing.Point(16, 560);
			this.m_sbhLower.Name = "m_sbhLower";
			this.m_sbhLower.Size = new System.Drawing.Size(648, 17);
			this.m_sbhLower.SmallChange = 10;
			this.m_sbhLower.TabIndex = 5;
			this.m_sbhLower.Visible = false;
			this.m_sbhLower.ValueChanged += new System.EventHandler(this.ScrollPaint);
			// 
			// m_sbvLower
			// 
			this.m_sbvLower.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbvLower.Location = new System.Drawing.Point(664, 416);
			this.m_sbvLower.Name = "m_sbvLower";
			this.m_sbvLower.Size = new System.Drawing.Size(16, 144);
			this.m_sbvLower.TabIndex = 6;
			this.m_sbvLower.ValueChanged += new System.EventHandler(this.ScrollVertPaint);
			// 
			// BgGraph
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(696, 590);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_sbvLower,
																		  this.m_sbhLower,
																		  this.m_picbLower,
																		  this.m_sbvUpper,
																		  this.m_pbPrint,
																		  this.m_sbhUpper,
																		  this.m_picbUpper});
			this.Name = "BgGraph";
			this.Text = "Form1";
			this.SizeChanged += new System.EventHandler(this.HandleSizeChange);
			this.ResumeLayout(false);

		}
		#endregion

		void SetPbDataPoints(PictureBox pb, VScrollBar sbv, HScrollBar sbh)
		{
			if (pb.Tag != null)
				((GraphicBox)pb.Tag).SetDataPoints(m_slbge, sbv, sbh);
		}


		public void SetDataPoints(SortedList slbge)
		{
			m_slbge = slbge;
			SetPbDataPoints(m_picbUpper, m_sbvUpper, m_sbhUpper);
			SetPbDataPoints(m_picbLower, m_sbvLower, m_sbhLower);
		}

		void SetPbBounds(PictureBox pb)
		{
			if (BvFromPb(pb) == BoxView.Log)
				((Reporter)pb.Tag).SetProps(m_gp);
			else if (BvFromPb(pb) == BoxView.Graph)
				((Grapher)pb.Tag).SetProps(m_gp);
		}

		/* S E T  B O U N D S */
		/*----------------------------------------------------------------------------
			%%Function: SetBounds
			%%Qualified: bg.BgGraph.SetBounds
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void SetBounds(double dLow, double dHigh, int nDays, int nBgIntervals, bool fShowMeals)
		{
			m_gp.dBgLow = dLow;
			m_gp.dBgHigh = dHigh;
			m_gp.nDays = nDays;
			m_gp.nIntervals = nBgIntervals;
			m_gp.fShowMeals = fShowMeals;
			SetPbBounds(m_picbUpper);
			SetPbBounds(m_picbLower);
		}

		public enum BoxView
		{
			None,
			Graph,
			Log
		};


		/* S E T  G R A P H I C  V I E W S */
		/*----------------------------------------------------------------------------
			%%Function: SetGraphicViews
			%%Qualified: bg.BgGraph.SetGraphicViews
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void SetGraphicViews(BoxView bvUpper, BoxView bvLower)
		{
			if (bvUpper == BoxView.None && bvLower == BoxView.None)
				throw(new Exception("Illegal BoxView parameters"));

			m_bvUpper = bvUpper;
			m_bvLower = bvLower;
			SetupViews(this.ClientSize.Height);

			Graphics gr = this.CreateGraphics();
			RectangleF rectfUpper = new RectangleF(Reporter.DxpFromDxa(gr, 100), 
												   Reporter.DypFromDya(gr, 100), 
												   m_picbUpper.Width - Reporter.DxpFromDxa(gr, 200), 
												   m_picbUpper.Height - Reporter.DypFromDya(gr, 200));

			RectangleF rectfLower = new RectangleF(Reporter.DxpFromDxa(gr, 100), 
												   Reporter.DypFromDya(gr, 100),
												   m_picbLower.Width - Reporter.DxpFromDxa(gr, 200), 
												   m_picbLower.Height - Reporter.DypFromDya(gr, 200));

			if (bvUpper == BoxView.Log)
				m_picbUpper.Tag = new Reporter(rectfUpper, gr);
			else if (bvUpper == BoxView.Graph)
				m_picbUpper.Tag = new Grapher(rectfUpper, gr);

			if (bvLower == BoxView.Log)
				m_picbLower.Tag = new Reporter(rectfLower, gr);
			else if (bvLower == BoxView.Graph)
				m_picbLower.Tag = new Grapher(rectfLower, gr);

		}

		BoxView m_bvUpper;
		BoxView m_bvLower;

		static public BoxView BvFromString(string s)
		{
			if (String.Compare(s, "None", true) == 0)
				return BoxView.None;
			else if (String.Compare(s, "Graph", true) == 0)
				return BoxView.Graph;
			else if (String.Compare(s, "Log", true) == 0)
				return BoxView.Log;

			return BoxView.None;
		}


		private void PaintGraph(object sender, System.Windows.Forms.PaintEventArgs e) 
		{
			PictureBox pb = (PictureBox)sender;

			e.Graphics.Clear(pb.BackColor);
			if (BvFromPb(pb) == BoxView.Log)
				{
				((Reporter)pb.Tag).Paint(e.Graphics);
				}
			else if (BvFromPb(pb) == BoxView.Graph)
				{
				((Grapher)pb.Tag).Paint(e.Graphics);
				}
		}


		bool m_fInPaint = false;
		private void ScrollPaint(object sender, System.EventArgs e) 
		{
			if (m_fInPaint)
				return;

			m_fInPaint = true;
			HScrollBar sbh = (HScrollBar)sender;

			PictureBox pb = (PictureBox)sbh.Tag;

			if (BvFromPb(pb) == BoxView.Graph)
				{
				// its a report
				Grapher grph = (Grapher)pb.Tag;
				int iFirstQuarter = -1;

				grph.SetFirstQuarter(iFirstQuarter = sbh.Value);

				DateTime dttm = grph.GetFirstDateTime();

				SetViewDateTimeScroll(m_picbUpper, m_sbhUpper, m_sbvUpper, dttm, iFirstQuarter);
				SetViewDateTimeScroll(m_picbLower, m_sbhLower, m_sbvLower, dttm, iFirstQuarter);

				pb.Invalidate();
				}
			m_fInPaint = false;
		}

		void PictureBoxSizeChange(PictureBox pb, HScrollBar sbh, VScrollBar sbv)
		{
			Graphics gr = this.CreateGraphics();
			RectangleF rcf = new RectangleF(Reporter.DxpFromDxa(gr, 100), 
											Reporter.DypFromDya(gr, 100), 
											pb.Width - Reporter.DxpFromDxa(gr, 200), 
											pb.Height - Reporter.DypFromDya(gr, 200));

			int iFirst = ((GraphicBox)pb.Tag).GetFirstForScroll();

			if (BvFromPb(pb) == BoxView.Graph)
				{
				Grapher grph = new Grapher(rcf, gr);

//				grph.SetProps(m_gp);
//				grph.SetDataPoints(m_slbge);
//				grph.SetFirstQuarter(sbh.Value);

//				grph.CalcGraph(sbh);

				pb.Tag = grph;
				}
			else if (BvFromPb(pb) == BoxView.Log)
				{
				Reporter rpt = new Reporter(rcf, gr); // pb.Width, pb.Height, 
//				rpt.SetProps(m_gp);
//				rpt.SetDataPoints(m_slbge, sbv);
//				rpt.CalcReport();
//				rpt.SetFirstLine(sbv.Value);

				pb.Tag = rpt;
				}
			((GraphicBox)pb.Tag).SetProps(m_gp);
			((GraphicBox)pb.Tag).SetDataPoints(m_slbge, sbv, sbh);
			((GraphicBox)pb.Tag).Calc();
			((GraphicBox)pb.Tag).SetFirstFromScroll(iFirst);

			pb.Invalidate();
		}

		private void HandleSizeChange(object sender, System.EventArgs e) 
		{
			SetupViews(this.ClientSize.Height);
			PictureBoxSizeChange(m_picbUpper, m_sbhUpper, m_sbvUpper);
			PictureBoxSizeChange(m_picbLower, m_sbhLower, m_sbvLower);
		}

		void CalcPictureBox(PictureBox pb, HScrollBar sbh)
		{
			if (pb.Tag != null)
				((GraphicBox)pb.Tag).Calc();
		}

		public void CalcGraph()
		{
			CalcPictureBox(m_picbUpper, m_sbhUpper);
			CalcPictureBox(m_picbLower, m_sbhLower);
		}

		private void HoverGraph(object sender, System.EventArgs e)
		{
			PictureBox pb = (PictureBox)sender;

			if (BvFromPb(pb) != BoxView.Graph)
				return;

			Grapher grph = (Grapher)pb.Tag;

			Point ptRaw = Cursor.Position;
			Point pt =  pb.PointToClient(ptRaw);

			PTFI ptfiHit = new PTFI();
			bool fHit = false;
			RectangleF rectfHit;
			object oHit;

			fHit = grph.FHitTest(pt, out oHit, out rectfHit);
			ptfiHit = (PTFI)oHit;
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

		void GraphPrintRegion(Grapher grphRef, RectangleF rcf, RectangleF rcfBanner, PrintPageEventArgs ev)
		{
			Grapher grph = new Grapher(rcf, ev.Graphics);

			grph.SetProps(m_gp);
			grph.DrawBanner(ev.Graphics, rcfBanner);
			grph.SetFirstQuarter(grphRef.GetFirstQuarter());
			grph.SetDataPoints(m_slbge, null, null);
			grph.Paint(ev.Graphics);
		}

		void LogPrintRegion(Reporter rptRef, RectangleF rcf, PrintPageEventArgs ev)
		{
			Reporter rpt = new Reporter(rcf, ev.Graphics);

			rpt.SetProps(m_gp);
			rpt.SetDataPoints(m_slbge, null, null);
			rpt.SetFirstLine(rptRef.GetFirstLine());
			rpt.Calc();
			rpt.Paint(ev.Graphics);
		}

		private void PrintPageHandler(object sender, PrintPageEventArgs ev)
		{
			Rectangle rectMargins = ev.MarginBounds;
			Rectangle rectPage = ev.PageBounds;

			// adjust the bottom margin...
			rectMargins.Height -= (int)Reporter.DypFromDya(ev.Graphics, 70);

//			ev.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Blue), 1.0F), rectMargins);
			int nPctUpper = 70;
			int nMarginTop = (int)Reporter.DypFromDya(ev.Graphics, 25);
			int nMarginBetween = nMarginTop;
			int nHeightTotal = rectMargins.Bottom - rectMargins.Top;
			int nHeightAvail = nHeightTotal - nMarginBetween - nMarginTop;
			int nWidth = rectMargins.Right - rectMargins.Left - (int)Reporter.DxpFromDxa(ev.Graphics, 10);

			if (m_bvUpper == BoxView.None)
				nPctUpper = 0;
			else if (m_bvLower == BoxView.None)
				nPctUpper = 100;

			// we have to apportion the regions...
			RectangleF rcfUpperBanner = new RectangleF(0, 0, nWidth, nMarginTop);
			RectangleF rcfUpper = new RectangleF(0, rcfUpperBanner.Bottom, nWidth, ((nHeightAvail) * nPctUpper) / 100);
			RectangleF rcfLowerBanner = new RectangleF(0, rcfUpper.Bottom, nWidth, nMarginBetween);
			RectangleF rcfLower = new RectangleF(0, rcfUpper.Bottom + nMarginBetween, nWidth, rectMargins.Bottom - (rcfUpper.Bottom + nMarginBetween));

			// paint the upper region
			switch (m_bvUpper)
				{
				case BoxView.Graph:
					{
					Grapher grph = (Grapher)m_picbUpper.Tag;
					GraphPrintRegion(grph, rcfUpper, rcfUpperBanner, ev);
					break;
					}
				case BoxView.Log:
					{
					Reporter rpt = (Reporter)m_picbUpper.Tag;
					LogPrintRegion(rpt, rcfUpper, ev);
					break;
					}
				}

			switch (m_bvLower)
				{
				case BoxView.Graph:
					{
					Grapher grph = (Grapher)m_picbLower.Tag;
					GraphPrintRegion(grph, rcfLower, rcfLowerBanner, ev);
					break;
					}
				case BoxView.Log:
					{
					Reporter rpt = (Reporter)m_picbLower.Tag;
					LogPrintRegion(rpt, rcfLower, ev);
					break;
					}
				}
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

//		int m_iFirstLine;

		BoxView BvFromPb(PictureBox pb)
		{
			if (pb.Tag == null)
				return BoxView.None;

			if (String.Compare(pb.Tag.GetType().ToString(), "bg.Grapher", true) == 0)
				return BoxView.Graph;
			else if (String.Compare(pb.Tag.GetType().ToString(), "bg.Reporter", true) == 0)
				return BoxView.Log;
			else
				return BoxView.None;
		}

		void SetViewDateTimeScroll(PictureBox pb, HScrollBar sbh, VScrollBar sbv, DateTime dttm, int iFirstQuarter)
		{
			if (BvFromPb(pb) == BoxView.Log)
				{
				Reporter rpt = (Reporter)pb.Tag;

				rpt.SetFirstDateTime(dttm);
				sbv.Value = rpt.GetFirstLine();
				}
			else if (BvFromPb(pb) == BoxView.Graph)
				{
				Grapher grph = (Grapher)pb.Tag;

				if (iFirstQuarter >= 0)
					{
					grph.SetFirstQuarter(iFirstQuarter);
					sbh.Value = iFirstQuarter;
					}
				else
					{
					grph.SetFirstDateTime(dttm.AddDays(-1.0));
					if (grph.GetFirstQuarter() > sbh.Maximum)	 // if we have exceeded the scrolling regions, then we want to act as if we've scrolled to the end
						grph.SetFirstQuarter(sbh.Value);
					if (grph.GetFirstQuarter() < 0)
						grph.SetFirstQuarter(0);
					sbh.Value = grph.GetFirstQuarter();
					}
				}
			pb.Invalidate();
		}

		private void ScrollVertPaint(object sender, System.EventArgs e)
		{
			if (m_fInPaint)
				return;

			m_fInPaint = true;

			VScrollBar sbv = (VScrollBar)sender;

			PictureBox pb = (PictureBox)sbv.Tag;

			if (BvFromPb(pb) == BoxView.Log)
				{
				// its a report.  scroll both views to this item.
				Reporter rpt = (Reporter)pb.Tag;

				rpt.SetFirstLine(sbv.Value);
				DateTime dttm = rpt.GetFirstDateTime();

				SetViewDateTimeScroll(m_picbUpper, m_sbhUpper, m_sbvUpper, dttm, -1);
				SetViewDateTimeScroll(m_picbLower, m_sbhLower, m_sbvLower, dttm, -1);

				pb.Invalidate();
				}
			m_fInPaint = false;
		}

	}
}
