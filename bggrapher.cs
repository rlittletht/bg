using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using UWin32;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace bg
{
class SBGE // Set of BGE
{
	SortedList m_slbge;	// the dataset

	GrapherParams m_cgp;
	ArrayList m_plptfi;
	RectangleF m_rcfDrawing;
	double m_dyPerBgUnit;
	double m_dxQuarter;
	bool m_fWgtAvg;
	int m_iFirstQuarter;
	float m_dxAdjust = 0;
	HScrollBar m_sbh;
	bool m_fColor = true;

	float m_dxOffset;
	float m_dyOffset;

	public SBGE(GrapherParams cgp, Graphics gr, bool fWgtAvg)
	{
		m_cgp = cgp;
		m_fWgtAvg = fWgtAvg;

		Font font = new Font("Tahoma", 8);

		m_dxOffset = gr.MeasureString("0000", font).Width;
		m_dyOffset = gr.MeasureString("0\n0\n0\n0\n", font).Height;
	}

	public void SetColor(bool fColor)
	{
		m_fColor = fColor;
	}

	public void SetDataSet(SortedList slbge, HScrollBar sbh)
	{
		m_sbh = sbh;
		m_slbge = slbge;
	}

	/* S E T  F I R S T  Q U A R T E R */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstQuarter
		%%Qualified: bg.Grapher.SetFirstQuarter
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstQuarter(int i)
	{
		m_iFirstQuarter = i;
	}

	/* G E T  F I R S T  Q U A R T E R */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstQuarter
		%%Qualified: bg.Grapher.GetFirstQuarter
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int GetFirstQuarter()
	{
		return m_iFirstQuarter;
	}

	/* Y  F R O M  R E A D I N G */
	/*----------------------------------------------------------------------------
		%%Function: YFromReading
		%%Qualified: bg.Grapher.YFromReading
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public float YFromReading(int nReading, double dyPerBgUnit)
	{
		if (nReading == 0)
			return -1.0F;

		return m_rcfDrawing.Top + m_rcfDrawing.Height - ((((float)nReading - (float)m_cgp.dBgLow) * (float)dyPerBgUnit)) - m_dyOffset;
	}

	/* X  F R O M  D A T E */
	/*----------------------------------------------------------------------------
		%%Function: XFromDate
		%%Qualified: bg.SBGE.XFromDate
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public float XFromDate(DateTime dayFirst, DateTime dttm, double dxQuarter)
	{
		// calculate how many quarter hours
		long ticks = dttm.Ticks - dayFirst.Ticks;
		long lQuarters = ticks / (36000000000 / 4);

		return (float)((lQuarters * dxQuarter) + m_rcfDrawing.Left); // (m_dxOffset + m_dxLeftMargin));
	}


	/* P T F  F R O M  B G E */
	/*----------------------------------------------------------------------------
		%%Function: PtfFromBge
		%%Qualified: bg.SBGE.PtfFromBge
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	PointF PtfFromBge(DateTime dayFirst, BGE bge, double dxQuarter, double dyBg)
	{
		float x = XFromDate(dayFirst, bge.Date, dxQuarter);
		float y = YFromReading(bge.Reading, dyBg);

		// now we've got the number of quarters.  figure out the bge
		return new PointF(x,y);
	}

	/* P T F  F R O M  B G E  A V G */
	/*----------------------------------------------------------------------------
		%%Function: PtfFromBgeAvg
		%%Qualified: bg.SBGE.PtfFromBgeAvg
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	PointF PtfFromBgeAvg(DateTime dayFirst, BGE bge, double dxQuarter, double dyBg)
	{
		float x = XFromDate(dayFirst, bge.Date, dxQuarter);
		float y = YFromReading(bge.WgtAvg, dyBg);

		// now we've got the number of quarters.  figure out the bge
		return new PointF(x,y);
	}

	/* G E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstDateTime
		%%Qualified: bg.SBGE.GetFirstDateTime
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public DateTime GetFirstDateTime()
	{
		PTFI ptfi = (PTFI)m_plptfi[0];

		return new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);
	}

	public DateTime GetLastDateTime()
	{
		DateTime dttmLast = ((PTFI)m_plptfi[m_plptfi.Count - 1]).bge.Date;

		return dttmLast;
	}

	/* C A L C  G R A P H */
	/*----------------------------------------------------------------------------
		%%Function: CalcGraph
		%%Qualified: bg.SBGE.CalcGraph
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void CalcGraph(RectangleF rcf)
	{
		m_rcfDrawing = rcf;

		// graph the points in the dataset, m_cgp.nDays at a time
		BGE bge = (BGE)m_slbge.GetByIndex(0);
		DateTime dayFirst = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day);

		// split apart the graphing range into intervals, by the quarter hour

		double cxQuarters = m_cgp.nDays * 24 * 4;
		double dxQuarter = (rcf.Width - m_dxOffset / 2) / cxQuarters;

		double cyBg = m_cgp.dBgHigh - m_cgp.dBgLow;
		double dyBg = (rcf.Height - m_dyOffset) / cyBg;

		// build up a set of points to graph
		int iValue = 0;
		m_plptfi = new ArrayList();
		float dxMax = 0;

		while (iValue < m_slbge.Values.Count)
			{
			bge = (BGE)m_slbge.GetByIndex(iValue);

			// set a point at bge

			PointF ptf;

			if (m_fWgtAvg)
				ptf = PtfFromBgeAvg(dayFirst, bge, dxQuarter, dyBg);
			else
				ptf = PtfFromBge(dayFirst, bge, dxQuarter, dyBg);

			if (ptf.X > dxMax)
				dxMax = ptf.X;

			PTFI ptfi;

			ptfi.ptf = ptf;
			ptfi.bge = bge;

			m_plptfi.Add(ptfi);

			iValue++;
			}

		if (m_sbh != null)
			{
			if (dxMax > dxQuarter * cxQuarters)
				{
				m_sbh.Minimum = 0;
				m_sbh.Maximum = (int)(((dxMax / dxQuarter)) - (m_cgp.nDays * 24 * 4));
				m_sbh.Visible = true;
				}
			else
				{
				m_sbh.Visible = false;
				}
			}

		m_dyPerBgUnit = dyBg;
		m_dxQuarter = dxQuarter;
	}

	public void PaintGraphGridlines(Graphics gr)
	{
		// ------------------------------
		// FIRST:  Draw the shaded ranges
		// ------------------------------
		int dxAdjust = (int)(m_iFirstQuarter * m_dxQuarter);	// how much should 0 based x-coordinates be adjusted?

		PTFI ptfi = (PTFI)m_plptfi[0];
		DateTime dttmFirst = new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);

//		RectangleF rectfGraphBodyClip = new RectangleF(m_dxOffset + m_dxLeftMargin, 
//													   m_dyTopMargin, // m_dyOffset + 
//													   ((float)m_rcfDrawing.Width) - m_dxOffset - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_rcfDrawing.Height) - m_dyOffset - m_dyTopMargin - m_dyBottomMargin);

		RectangleF rectfGraphBodyClip = new RectangleF(m_dxOffset + m_rcfDrawing.Left, 
													   m_rcfDrawing.Top, // m_dyOffset + 
													   m_rcfDrawing.Width - m_dxOffset,
													   m_rcfDrawing.Height - m_dyOffset); 
//													   ((float)m_rcfDrawing.Width) - m_dxOffset - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_rcfDrawing.Height) - m_dyOffset - m_dyTopMargin - m_dyBottomMargin);

		RectangleF rectfGraphFullClip = m_rcfDrawing; // new RectangleF(m_dxLeftMargin, 
//													   m_dyTopMargin, 
//													   ((float)m_rcfDrawing.Width) - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_rcfDrawing.Height) - m_dyTopMargin - m_dyBottomMargin);
//		
		gr.SetClip(rectfGraphBodyClip);
		ShadeCurvedRanges(gr, dxAdjust, dttmFirst);
		gr.ResetClip();

		// -----------------------------------------
		// SECOND: Draw the gridlines and the legend
		// -----------------------------------------

		gr.SetClip(rectfGraphFullClip);

		SolidBrush brushBlue = new SolidBrush(Color.Blue);
		SolidBrush brushAvg = new SolidBrush(Color.Red);

		Pen penAvg = new Pen(Color.Red, (float)1);
		Pen penBlue = new Pen(Color.Blue, (float)1);
		Pen penGrid = new Pen(Color.DarkGray, (float)1);
		Pen penLightGrid = new Pen(Color.LightGray, (float)1);

		gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		// now we've found the first index we're going to draw
		DateTime dttm = DateTime.Parse(dttmFirst.ToString("d"));

		// figure out the number of quarters in the first date we want
		// to display

		int nDayFirst = (m_iFirstQuarter + 95) / (4 * 24) - 1;
		Font font = new Font("Tahoma", 8);
		// now we know (rounded up), what the first day will be
		// as a delta from the first day
		dttm = dttm.AddDays(nDayFirst);

		float dyOffsetRegion = m_dyOffset / 4;
		float yDateLegend = m_rcfDrawing.Bottom - m_dyOffset + dyOffsetRegion * 1.3F; // m_rcfDrawing.Height - m_dyBottomMargin - m_dyOffset + dyOffsetRegion * 1.3F;
		float xMacLastDrawn = 0.0F;
		float xMacNoonLastDrawn = 0.0F;

		for (int nDay = nDayFirst; nDay <= nDayFirst + m_cgp.nDays; nDay++)
			{
			dttm = dttm.AddDays(1);
			string s = dttm.ToString("MM/dd");
			SizeF szf = gr.MeasureString(s, font);
			float x = XFromDate(dttmFirst, dttm, m_dxQuarter) - dxAdjust;


			if (x > m_rcfDrawing.Left + m_dxOffset)
				gr.DrawLine(penLightGrid, x, yDateLegend + dyOffsetRegion, x, 0);

			if (x + (float)m_dxQuarter * (4.0F * 12.0F) > m_rcfDrawing.Left + m_dxOffset)
				gr.DrawLine(penGrid, x + (float)m_dxQuarter * (4.0F * 12.0F), yDateLegend + dyOffsetRegion, x + (float)m_dxQuarter * (4.0F * 12.0F), 0);

			Font fontSmall = new Font("Tahoma", 6);

			float dxNoon = gr.MeasureString("12:00pm", fontSmall).Width;
			if (xMacNoonLastDrawn < x + (float)m_dxQuarter * (4.0F * 12.0F) - dxNoon / 2.0F)
				{
				gr.DrawString("12:00pm", fontSmall, brushBlue, x + (float)m_dxQuarter * (4.0F * 12.0F) - dxNoon / 2.0F, yDateLegend + dyOffsetRegion / 2.2F);
				xMacNoonLastDrawn = x + (float)m_dxQuarter * (4.0F * 12.0F) + dxNoon / 2.0F;
				}
			x -= (szf.Width / 2.0F);
			if (xMacLastDrawn < x)
				{
				gr.DrawString(s, font, brushBlue, x, yDateLegend + dyOffsetRegion);
				xMacLastDrawn = x + gr.MeasureString(s, font).Width;
				}
			}

		// put legend up
		int n;
		int dn = ((int)m_cgp.dBgHigh - (int)m_cgp.dBgLow) / m_cgp.nIntervals;

		gr.DrawLine(penBlue, m_dxOffset + (m_rcfDrawing.Left) - 1.0F, m_rcfDrawing.Top, m_dxOffset + (m_rcfDrawing.Left) - 1.0F, yDateLegend);
		gr.DrawLine(penBlue, m_dxOffset + (m_rcfDrawing.Left) - 1.0F, yDateLegend, m_rcfDrawing.Width + m_dxOffset, yDateLegend);
//		gr.DrawLine(penBlue, (m_dxOffset + m_dxLeftMargin) - 1.0F, m_dyTopMargin, (m_dxOffset + m_dxLeftMargin) - 1.0F, yDateLegend);
//		gr.DrawLine(penBlue, (m_dxOffset + m_dxLeftMargin) - 1.0F, yDateLegend, m_rcfDrawing.Width, yDateLegend);
		bool fLine = false;

		for (n = (int)m_cgp.dBgLow; n <= (int)m_cgp.dBgHigh; n += dn)
			{
			float x = m_rcfDrawing.Left /*m_dxLeftMargin*/ + 2.0F;
			float y = YFromReading(n, m_dyPerBgUnit);

			if (fLine)
				gr.DrawLine(penGrid, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/) - 4.0F, y, m_rcfDrawing.Width, y);
			fLine = !fLine;

			y -= (gr.MeasureString(n.ToString(), font).Height / 2.0F);
			x = (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/) - 4.0F - gr.MeasureString(n.ToString(), font).Width;
			gr.DrawString(n.ToString(), font, brushBlue, x, y);
			}

		gr.ResetClip();

		m_dxAdjust = dxAdjust;
	}

	public void PaintGraph(Graphics gr)
	{
		int i, iLast;

		int dxAdjust = (int)(m_iFirstQuarter * m_dxQuarter);	// how much should 0 based x-coordinates be adjusted?

		Pen penMeal = new Pen(Color.Green, 1.0F);
		Pen penBlueThin = new Pen(Color.LightBlue, (float)0.5);
		Pen penBlue = new Pen(Color.Blue, (float)1);
		Pen penGrid = new Pen(Color.DarkGray, (float)1);
		Pen penLightGrid = new Pen(Color.LightGray, (float)1);

		SolidBrush brushBlue = new SolidBrush(Color.Blue);

		if (m_fWgtAvg)
			{
            brushBlue = new SolidBrush(Color.Red);
			penBlue = new Pen(Color.Red, (float)1);
			}

		double dxFirstQuarter = m_iFirstQuarter * m_dxQuarter + (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/);
		double dxLastQuarter = dxFirstQuarter + (m_cgp.nDays * 24 * 4) * m_dxQuarter;

		RectangleF rectfGraphBodyClip = new RectangleF(m_dxOffset + m_rcfDrawing.Left, 
													   m_rcfDrawing.Top, // m_dyOffset + 
													   m_rcfDrawing.Width - m_dxOffset,
													   m_rcfDrawing.Height - m_dyOffset); 

		gr.SetClip(rectfGraphBodyClip);
		// ------------------------------
		// THIRD: Graph the points
		// ------------------------------

		PTFI ptfiLastMeal = new PTFI();
		PTFI ptfi;

		ptfiLastMeal.bge = null;

		for (i = 0, iLast = m_plptfi.Count; i < iLast; i++)
			{
			PointF ptf;

			ptfi = ((PTFI)m_plptfi[i]);
			ptf = ptfi.ptf;

			// if its before our first point, skip it
			if (ptf.X < dxFirstQuarter)
				continue;

			if (ptf.Y == -1.0F && ptfi.bge.Reading == 0)
				{
				// lets get a real Y value for this by plotting a line on the curve
				if (i > 0)
					ptf.Y = ((PTFI)m_plptfi[i-1]).ptf.Y;
				else if (i < iLast)
					ptf.Y = ((PTFI)m_plptfi[i+1]).ptf.Y;
				else
					ptf.Y = 0;
				ptfi.ptf = ptf;
				m_plptfi[i] = ptfi;
				}

			if (ptfiLastMeal.bge != null && m_cgp.fShowMeals)
				{
				if (ptfiLastMeal.bge.Date.AddMinutes(90.0) <= ptfi.bge.Date
					&& ptfiLastMeal.bge.Date.AddMinutes(150.0) >= ptfi.bge.Date)
					{
					float yAdjust;
					float xLast = ptfiLastMeal.ptf.X - dxAdjust;
					float yLast = ptfiLastMeal.ptf.Y;

					yAdjust = ptf.Y - yLast;

					if (yAdjust < 0.0F)
						yAdjust -= 15.0F;
					else
						yAdjust += 15.0F;

					// we have a match
					gr.DrawLine(penMeal, xLast + 1, yLast, xLast + 1, yLast + yAdjust);
					gr.DrawLine(penMeal, xLast + 1, yLast + yAdjust, ptf.X + 1 - dxAdjust, yLast + yAdjust);
					gr.DrawLine(penMeal, ptf.X + 1 - dxAdjust, yLast + yAdjust, ptf.X + 1 - dxAdjust, ptf.Y + 1);
					ptfiLastMeal.bge = null;
					}
				else if (ptfiLastMeal.bge.Date.AddHours(150.0) < ptfi.bge.Date)
					ptfiLastMeal.bge = null;
				}

			if (ptfi.bge.Type != BGE.ReadingType.SpotTest)
				ptfiLastMeal = ptfi;

			if (ptf.X > dxLastQuarter)
				break;

			if (ptfi.bge.InterpReading)
				gr.DrawEllipse(penBlueThin, ptf.X - 1 - dxAdjust, ptf.Y - 1, 4, 4);
			else
				gr.FillEllipse(brushBlue, ptf.X - 1 - dxAdjust, ptf.Y - 1, 4, 4);

			if (i > 0)
				{
				PointF ptfLast = ((PTFI)m_plptfi[i - 1]).ptf;

				if (ptfLast.X >= dxFirstQuarter)
					gr.DrawLine(penBlue,  ptfLast.X + 1 - dxAdjust, ptfLast.Y + 1, ptf.X + 1 - dxAdjust, ptf.Y + 1);

				}
			}
		// if we were doing the translucent range shading, we'd do it here.
//			ShadeRanges(pb, gr);
		gr.ResetClip();
		m_dxAdjust = dxAdjust;
	}
	void AddCurvePoint(double dNext, double dCur, ref double dCum, ref ArrayList plptf, int dxAdjust, DateTime dttmFirst, ref DateTime dttm, double dHours, int nReading, int nPctSlop, int nPctAbove)
	{
		if (dNext - dCur - dCum < dHours)
			return;

		dCum += dHours;
		DateTime dttmForRender;
		dttmForRender = dttm.AddHours(dHours + (((double)dHours * (double)nPctSlop) / 100.0));
		dttm = dttm.AddHours(dHours);
		float x = XFromDate(dttmFirst, dttmForRender, m_dxQuarter) - dxAdjust;

		PointF ptf = new PointF(x, YFromReading(nReading + (nReading * nPctAbove) / 100, m_dyPerBgUnit));
		plptf.Add(ptf);
	}

	void ShadeRanges2(Graphics gr, int dxAdjust, DateTime dttmFirst, SolidBrush hBrushTrans, int nPctSlop, int nPctAbove)
	{
		// ok
		// shade the regions
		float yMin = YFromReading(80, m_dyPerBgUnit);
		float yMax = YFromReading(120, m_dyPerBgUnit);


		// ok, there are 7 days, starting at dttmFirst
		int iDay = 0;

		DateTime dttmCur;

		dttmCur = dttmFirst.AddMinutes(m_iFirstQuarter * 15.0);
		dttmCur = new DateTime(dttmCur.Year, dttmCur.Month, dttmCur.Day);

		ArrayList plptf = new ArrayList();

		double []dMeals = { 8.0, 12.0, 18.0 };
		double dHours = 0.0;
		int iplptfi = 0;

		while (iplptfi < m_plptfi.Count)
			{
			PTFI ptfi = (PTFI)m_plptfi[iplptfi];

			if (ptfi.bge.Date >= dttmCur)
				break;
			iplptfi++;
			}

		AddCurvePoint(24.0, 0, ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 0, 130, 0, nPctAbove);
		for (; iDay < m_cgp.nDays + 2; iDay++)
			{
			// now, first analyze the day and determine when the meals are...otherwise, use the default meal times
			dMeals[0] = 8.0;
			dMeals[1] = 12.0;
			dMeals[2] = 18.0;

			// now, see if we can find meals in our day
			DateTime dttmNext = dttmCur.AddDays(1);

			while (iplptfi < m_plptfi.Count)
				{
				PTFI ptfi = (PTFI)m_plptfi[iplptfi];

				if (ptfi.bge.Date >= dttmNext)
					break;

				if (ptfi.bge.Type == BGE.ReadingType.Dinner)
					dMeals[2] = ptfi.bge.Date.Hour;
				else if (ptfi.bge.Type == BGE.ReadingType.Breakfast)
					dMeals[0] = ptfi.bge.Date.Hour;
				else if (ptfi.bge.Type == BGE.ReadingType.Lunch)
					dMeals[1] = ptfi.bge.Date.Hour;
				iplptfi++;
				}

			dHours = 0;
			AddCurvePoint(dMeals[0], 0.0, ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, dMeals[0], 120, -nPctSlop, nPctAbove);	// 0800

			dHours = 0;
			AddCurvePoint(dMeals[1], dMeals[0], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 0.5, 180, -nPctSlop, nPctAbove);	// 0830
			AddCurvePoint(dMeals[1], dMeals[0], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 1.0, 180, 0, nPctAbove);	// 0930
			AddCurvePoint(dMeals[1], dMeals[0], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 0.5, 160, nPctSlop, nPctAbove);	// 1000
			AddCurvePoint(dMeals[1], dMeals[0], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 1.5, 120, nPctSlop, nPctAbove);	// 1130

			AddCurvePoint(dMeals[1], dMeals[0], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, dMeals[1] - dMeals[0] - dHours, 120, 0, nPctAbove);	// 1200

			dHours = 0;
			AddCurvePoint(dMeals[2], dMeals[1], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 0.5, 180, -nPctSlop, nPctAbove);	// 1230
			AddCurvePoint(dMeals[2], dMeals[1], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 1.0, 180, 0, nPctAbove);	// 1330
			AddCurvePoint(dMeals[2], dMeals[1], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 0.5, 160, nPctSlop, nPctAbove);	// 1400
			AddCurvePoint(dMeals[2], dMeals[1], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 1.5, 120, nPctSlop, nPctAbove);	// 1530

			AddCurvePoint(dMeals[2], dMeals[1], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, dMeals[2] - dMeals[1] - dHours, 120, 0, nPctAbove);	// 1800

			dHours = 0;
			AddCurvePoint(24.00, dMeals[2], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 0.5, 180, -nPctSlop, nPctAbove);	// 1830
			AddCurvePoint(24.00, dMeals[2], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 1.0, 180, 0, nPctAbove);	// 1930
			AddCurvePoint(24.00, dMeals[2], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 0.5, 160, nPctSlop, nPctAbove);	// 2000
			AddCurvePoint(24.00, dMeals[2], ref dHours, ref plptf, dxAdjust, dttmFirst, ref dttmCur, 24.0 - dMeals[2] - dHours, 140, nPctSlop, nPctAbove);	// 2400
			// repeat for 7 days worth
			}

		// ok, now just fill a line back to the beginning
		while (iDay >= 0)
			{
			float x = XFromDate(dttmFirst, dttmCur, m_dxQuarter) - dxAdjust;
			float y = YFromReading(80 - (80 * nPctAbove) / 100, m_dyPerBgUnit);

			PointF ptf = new PointF(x, y);
			plptf.Add(ptf);
			dttmCur = dttmCur.AddDays(-1);
			iDay--;
			}

		PointF[] points;

		points = new PointF[plptf.Count];

		int iptf = 0;

		foreach (PointF ptf in plptf)
			{
			points[iptf] = ptf;
			iptf++;
			}

		FillMode fm = FillMode.Winding;

		gr.FillClosedCurve(hBrushTrans, points, fm, 0.2F);
	}

	void ShadeCurvedRanges(Graphics gr, int dxAdjust, DateTime dttmFirst)
	{
		SolidBrush hBrushTrans;

		if (m_fColor)
			hBrushTrans = new SolidBrush(Color.FromArgb(255, 255, 255, 200/*167*/));
		else
			hBrushTrans = new SolidBrush(Color.FromArgb(255, 215, 215, 215));

		ShadeRanges2(gr, dxAdjust, dttmFirst, hBrushTrans, 10, 20);

		if (m_fColor)
			hBrushTrans = new SolidBrush(Color.FromArgb(255, 200/*174*/, 255, 200/*174*/));
		else
			hBrushTrans = new SolidBrush(Color.FromArgb(255, 192, 192, 192));
		ShadeRanges2(gr, dxAdjust, dttmFirst, hBrushTrans, 0, 0);

	}

	void ShadeRanges(Graphics gr)
	{
		// shade the regions
		float yMin = YFromReading(80, m_dyPerBgUnit);
		float yMax = YFromReading(120, m_dyPerBgUnit);

		SolidBrush hBrushTrans = new SolidBrush(Color.FromArgb(64, 0, 255, 0));

		gr.FillRectangle(hBrushTrans, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/), yMax, m_rcfDrawing.Width, Math.Abs(yMax - yMin));

		yMin = YFromReading(120, m_dyPerBgUnit);
		yMax = YFromReading(140, m_dyPerBgUnit);

		hBrushTrans = new SolidBrush(Color.FromArgb(64, 255, 255, 0));

		gr.FillRectangle(hBrushTrans, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/), yMax, m_rcfDrawing.Width, Math.Abs(yMax - yMin));

		yMin = YFromReading(60, m_dyPerBgUnit);
		yMax = YFromReading(80, m_dyPerBgUnit);

		gr.FillRectangle(hBrushTrans, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/), yMax, m_rcfDrawing.Width, Math.Abs(yMax - yMin));
	}

	/* F  H I T  T E S T */
	/*----------------------------------------------------------------------------
		%%Function: FHitTest
		%%Qualified: bg.SBGE.FHitTest
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public bool FHitTest(Point ptClient, out object oHit, out RectangleF rectfHit)
	{
		rectfHit = new RectangleF();;
		PTFI ptfiHit;

		// figure out what we hit.

		// convert the pt into a raw point compatible with our array
		PointF ptfArray = new PointF(((float)ptClient.X) + m_dxAdjust, (float)ptClient.Y);

		ptfiHit = new PTFI();
		bool fHit = false;

		// now we can go searching for a point this corresponds to
		foreach (PTFI ptfi in m_plptfi)
			{
			rectfHit = new RectangleF(ptfi.ptf.X - 4.0F, ptfi.ptf.Y - 4.0F, 8.0F, 8.0F);

			if (!rectfHit.Contains(ptfArray))
				continue;

			fHit = true;
			ptfiHit = ptfi;

			break;
			}

		oHit = ptfiHit;
		return fHit;
	}

}

//  ______  ______ _______  _____  _     _ _______  ______
// |  ____ |_____/ |_____| |_____] |_____| |______ |_____/
// |_____| |    \_ |     | |       |     | |______ |    \_

public class Grapher : GraphicBox
{
//  _  _ ____ _  _ ___  ____ ____     _  _ ____ ____ _ ____ ___  _    ____ ____
//	|\/| |___ |\/| |__] |___ |__/     |  | |__| |__/ | |__| |__] |    |___ [__
//	|  | |___ |  | |__] |___ |  \      \/  |  | |  \ | |  | |__] |___ |___ ___]

	RectangleF m_rcfDrawing;
//	SortedList m_slbge;
	GrapherParams m_cgp;

//	ArrayList m_plptfi;
//	ArrayList m_plptfiAvg;

	float m_dxOffset = 30.0F;
	float m_dyOffset = 45.0F;

	SBGE m_sbge;
	SBGE m_sbgeAvg;

	/* G R A P H E R */
	/*----------------------------------------------------------------------------
		%%Function: Grapher
		%%Qualified: bg.Grapher.Grapher
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public Grapher(RectangleF rcf, Graphics gr) // int nWidth, int nHeight
	{
		m_rcfDrawing = rcf;
		m_cgp.dBgLow = 30.0;
		m_cgp.dBgHigh = 220.0;
		m_cgp.nDays = 7;
		m_cgp.nIntervals = 19;
		m_cgp.fShowMeals = false;
		Font font = new Font("Tahoma", 8);

		m_dxOffset = gr.MeasureString("0000", font).Width;
		m_dyOffset = gr.MeasureString("0\n0\n0\n0\n", font).Height;

		m_sbge = new SBGE(m_cgp, gr, false);
		m_sbgeAvg = new SBGE(m_cgp, gr, true);
	}

	/* G E T  D A Y S  P E R  P A G E */
	/*----------------------------------------------------------------------------
		%%Function: GetDaysPerPage
		%%Qualified: bg.Grapher.GetDaysPerPage
		%%Contact: rlittle

		
		GraphicBox interface
	----------------------------------------------------------------------------*/
	public int GetDaysPerPage()
	{
		return m_cgp.nDays;
	}

	/* S E T  D A Y S  P E R  P A G E */
	/*----------------------------------------------------------------------------
		%%Function: SetDaysPerPage
		%%Qualified: bg.Grapher.SetDaysPerPage
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void SetDaysPerPage(int nDaysPerPage)
	{
		m_cgp.nDays = nDaysPerPage;
	}

	/* S E T  F I R S T  F R O M  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstFromScroll
		%%Qualified: bg.Grapher.SetFirstFromScroll
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void SetFirstFromScroll(int i)
	{
		m_sbge.SetFirstQuarter(i);
		m_sbgeAvg.SetFirstQuarter(i);
	}

	/* S E T  C O L O R */
	/*----------------------------------------------------------------------------
		%%Function: SetColor
		%%Qualified: bg.Grapher.SetColor
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void SetColor(bool fColor)
	{
		m_sbge.SetColor(fColor);
		m_sbgeAvg.SetColor(fColor);
	}

	/* G E T  F I R S T  F O R  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstForScroll
		%%Qualified: bg.Grapher.GetFirstForScroll
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public int GetFirstForScroll()
	{
		return m_sbge.GetFirstQuarter();
	}

	/* G E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstDateTime
		%%Qualified: bg.Grapher.GetFirstDateTime
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public DateTime GetFirstDateTime()
	{
		DateTime dttmFirst = m_sbge.GetFirstDateTime();
		DateTime dttm = DateTime.Parse(dttmFirst.ToString("d"));
		double nDayFirst = ((double)m_sbge.GetFirstQuarter() + 95.0) / (4.0 * 24.0) - 1.0;
		dttm = dttm.AddDays(nDayFirst);

		return dttm;
	}

	/* S E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstDateTime
		%%Qualified: bg.Grapher.SetFirstDateTime
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void SetFirstDateTime(DateTime dttm)
	{
		DateTime dttmFirst = m_sbge.GetFirstDateTime();
		TimeSpan ts = dttm.Subtract(dttmFirst);
		m_sbge.SetFirstQuarter((int)(4.0 * ts.TotalHours) + 1);
		m_sbgeAvg.SetFirstQuarter((int)(4.0 * ts.TotalHours) + 1);
	}


	/* S E T  D A T A  P O I N T S */
	/*----------------------------------------------------------------------------
		%%Function: SetDataPoints
		%%Qualified: bg.Grapher.SetDataPoints
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void SetDataPoints(SortedList slbge, VScrollBar sbv, HScrollBar sbh)
	{
		m_sbge.SetDataSet(slbge, sbh);
		m_sbgeAvg.SetDataSet(slbge, sbh);
	}

	/* S E T  P R O P S */
	/*----------------------------------------------------------------------------
		%%Function: SetProps
		%%Qualified: bg.Grapher.SetProps
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void SetProps(GrapherParams cgp)
	{
		m_cgp = cgp;
	}

	/* C A L C */
	/*----------------------------------------------------------------------------
		%%Function: Calc
		%%Qualified: bg.Grapher.Calc
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void Calc()
	{
		m_sbge.CalcGraph(m_rcfDrawing);
		m_sbgeAvg.CalcGraph(m_rcfDrawing);
#if NEVER
		// graph the points in the dataset, m_cgp.nDays at a time
		BGE bge = (BGE)m_slbge.GetByIndex(0);
		DateTime dayFirst = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day);

		// split apart the graphing range into intervals, by the quarter hour

		double cxQuarters = m_cgp.nDays * 24 * 4;
		double dxQuarter = (m_rcfDrawing.Width - m_dxOffset / 2) / cxQuarters; // (m_rcfDrawing.Width - (m_dxOffset + m_dxLeftMargin + m_dxRightMargin) - m_dxOffset / 2) / cxQuarters;

		double cyBg = m_cgp.dBgHigh - m_cgp.dBgLow;
		double dyBg = (m_rcfDrawing.Height - m_dyOffset) / cyBg;// (m_rcfDrawing.Height - (m_dyOffset + m_dyTopMargin + m_dyBottomMargin)) / cyBg;

		// build up a set of points to graph
		int iValue = 0;
		m_plptfi = new ArrayList();
		m_plptfiAvg = new ArrayList();
		float dxMax = 0;

		while (iValue < m_slbge.Values.Count)
			{
			bge = (BGE)m_slbge.GetByIndex(iValue);

			// set a point at bge

			PointF ptf = PtfFromBge(dayFirst, bge, dxQuarter, dyBg);
			PointF ptfAvg = PtfFromBgeAvg(dayFirst, bge, dxQuarter, dyBg);

			if (ptf.X > dxMax)
				dxMax = ptf.X;

			PTFI ptfi;
			PTFI ptfiAvg;

			ptfi.ptf = ptf;
			ptfi.bge = bge;

			ptfiAvg.ptf = ptfAvg;
			ptfiAvg.bge = bge;

			m_plptfi.Add(ptfi);
			m_plptfiAvg.Add(ptfiAvg);

			iValue++;
			}

		if (m_sbh != null)
			{
			if (dxMax > dxQuarter * cxQuarters)
				{
				m_sbh.Minimum = 0;
				m_sbh.Maximum = (int)(((dxMax / dxQuarter)) - (m_cgp.nDays * 24 * 4));
				m_sbh.Visible = true;
				}
			else
				{
				m_sbh.Visible = false;
				}
			}

		m_dyPerBgUnit = dyBg;
		m_dxQuarter = dxQuarter;
#endif // NEVER
	}

	//	___  ____ _ _  _ ___ _ _  _ ____
	//	|__] |__| | |\ |  |  | |\ | | __
	//	|    |  | | | \|  |  | | \| |__]

	/* D R A W  B A N N E R */
	/*----------------------------------------------------------------------------
		%%Function: DrawBanner
		%%Qualified: bg.Grapher.DrawBanner
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void DrawBanner(Graphics gr, RectangleF rcf)
	{
		Font font = new Font("Tahoma", 12);

//		gr.DrawString("Graph", font, new SolidBrush(Color.Black), rcf);
	}

	/* P A I N T */
	/*----------------------------------------------------------------------------
		%%Function: Paint
		%%Qualified: bg.Grapher.Paint
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public void Paint(Graphics gr)
	{
		// there are several things that have to happen.

		// 1. Draw the shaded ranges
		// 2. Draw the grid lines
		// 3. Draw the actual points

		m_sbge.PaintGraphGridlines(gr);
		m_sbge.PaintGraph(gr);
		m_sbgeAvg.PaintGraph(gr);

#if NEVER
		// ------------------------------
		// FIRST:  Draw the shaded ranges
		// ------------------------------
		int dxAdjust = (int)(m_iFirstQuarter * m_dxQuarter);	// how much should 0 based x-coordinates be adjusted?
		PTFI ptfi = (PTFI)m_plptfi[0];
		PTFI ptfiAvg = (PTFI)m_plptfiAvg[0];
		DateTime dttmFirst = new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);

//		RectangleF rectfGraphBodyClip = new RectangleF(m_dxOffset + m_dxLeftMargin, 
//													   m_dyTopMargin, // m_dyOffset + 
//													   ((float)m_rcfDrawing.Width) - m_dxOffset - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_rcfDrawing.Height) - m_dyOffset - m_dyTopMargin - m_dyBottomMargin);

		RectangleF rectfGraphBodyClip = new RectangleF(m_dxOffset + m_rcfDrawing.Left, 
													   m_rcfDrawing.Top, // m_dyOffset + 
													   m_rcfDrawing.Width - m_dxOffset,
													   m_rcfDrawing.Height - m_dyOffset); 
//													   ((float)m_rcfDrawing.Width) - m_dxOffset - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_rcfDrawing.Height) - m_dyOffset - m_dyTopMargin - m_dyBottomMargin);

		RectangleF rectfGraphFullClip = m_rcfDrawing; // new RectangleF(m_dxLeftMargin, 
//													   m_dyTopMargin, 
//													   ((float)m_rcfDrawing.Width) - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_rcfDrawing.Height) - m_dyTopMargin - m_dyBottomMargin);
//		
		gr.SetClip(rectfGraphBodyClip);
		ShadeCurvedRanges(gr, dxAdjust, dttmFirst);
		gr.ResetClip();

		// -----------------------------------------
		// SECOND: Draw the gridlines and the legend
		// -----------------------------------------

		gr.SetClip(rectfGraphFullClip);

		SolidBrush brushBlue = new SolidBrush(Color.Blue);
		SolidBrush brushAvg = new SolidBrush(Color.Red);

		Pen penAvg = new Pen(Color.Red, (float)1);
		Pen penBlueThin = new Pen(Color.LightBlue, (float)0.5);
		Pen penBlue = new Pen(Color.Blue, (float)1);
		Pen penGrid = new Pen(Color.DarkGray, (float)1);
		Pen penLightGrid = new Pen(Color.LightGray, (float)1);
		Pen penMeal = new Pen(Color.Green, 1.0F);

		double dxFirstQuarter = m_iFirstQuarter * m_dxQuarter + (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/);
		double dxLastQuarter = dxFirstQuarter + (m_cgp.nDays * 24 * 4) * m_dxQuarter;

		gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		// now we've found the first index we're going to draw
		DateTime dttm = DateTime.Parse(dttmFirst.ToString("d"));

		// figure out the number of quarters in the first date we want
		// to display

		int nDayFirst = (m_iFirstQuarter + 95) / (4 * 24) - 1;
		Font font = new Font("Tahoma", 8);
		// now we know (rounded up), what the first day will be
		// as a delta from the first day
		dttm = dttm.AddDays(nDayFirst);
		
		float dyOffsetRegion = m_dyOffset / 4;
		float yDateLegend = m_rcfDrawing.Bottom - m_dyOffset + dyOffsetRegion * 1.3F; // m_rcfDrawing.Height - m_dyBottomMargin - m_dyOffset + dyOffsetRegion * 1.3F;
		float xMacLastDrawn = 0.0F;
		float xMacNoonLastDrawn = 0.0F;

		for (int nDay = nDayFirst; nDay <= nDayFirst + m_cgp.nDays; nDay++)
			{
			dttm = dttm.AddDays(1);
			string s = dttm.ToString("MM/dd");
			SizeF szf = gr.MeasureString(s, font);
			float x = XFromDate(dttmFirst, dttm, m_dxQuarter) - dxAdjust;


			if (x > m_rcfDrawing.Left + m_dxOffset)
				gr.DrawLine(penLightGrid, x, yDateLegend + dyOffsetRegion, x, 0);

			if (x + (float)m_dxQuarter * (4.0F * 12.0F) > m_rcfDrawing.Left + m_dxOffset)
				gr.DrawLine(penGrid, x + (float)m_dxQuarter * (4.0F * 12.0F), yDateLegend + dyOffsetRegion, x + (float)m_dxQuarter * (4.0F * 12.0F), 0);

			Font fontSmall = new Font("Tahoma", 6);

			float dxNoon = gr.MeasureString("12:00pm", fontSmall).Width;
			if (xMacNoonLastDrawn < x + (float)m_dxQuarter * (4.0F * 12.0F) - dxNoon / 2.0F)
				{
				gr.DrawString("12:00pm", fontSmall, brushBlue, x + (float)m_dxQuarter * (4.0F * 12.0F) - dxNoon / 2.0F, yDateLegend + dyOffsetRegion / 2.2F);
				xMacNoonLastDrawn = x + (float)m_dxQuarter * (4.0F * 12.0F) + dxNoon / 2.0F;
				}
			x -= (szf.Width / 2.0F);
			if (xMacLastDrawn < x)
				{
				gr.DrawString(s, font, brushBlue, x, yDateLegend + dyOffsetRegion);
				xMacLastDrawn = x + gr.MeasureString(s, font).Width;
				}
			}

		int i, iLast;

		// put legend up
		int n;
		int dn = ((int)m_cgp.dBgHigh - (int)m_cgp.dBgLow) / m_cgp.nIntervals;

		gr.DrawLine(penBlue, m_dxOffset + (m_rcfDrawing.Left) - 1.0F, m_rcfDrawing.Top, m_dxOffset + (m_rcfDrawing.Left) - 1.0F, yDateLegend);
		gr.DrawLine(penBlue, m_dxOffset + (m_rcfDrawing.Left) - 1.0F, yDateLegend, m_rcfDrawing.Width + m_dxOffset, yDateLegend);
//		gr.DrawLine(penBlue, (m_dxOffset + m_dxLeftMargin) - 1.0F, m_dyTopMargin, (m_dxOffset + m_dxLeftMargin) - 1.0F, yDateLegend);
//		gr.DrawLine(penBlue, (m_dxOffset + m_dxLeftMargin) - 1.0F, yDateLegend, m_rcfDrawing.Width, yDateLegend);
		bool fLine = false;

		for (n = (int)m_cgp.dBgLow; n <= (int)m_cgp.dBgHigh; n += dn)
			{
			float x = m_rcfDrawing.Left /*m_dxLeftMargin*/ + 2.0F;
			float y = YFromReading(n, m_dyPerBgUnit);

			if (fLine)
				gr.DrawLine(penGrid, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/) - 4.0F, y, m_rcfDrawing.Width, y);
			fLine = !fLine;

			y -= (gr.MeasureString(n.ToString(), font).Height / 2.0F);
			x = (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/) - 4.0F - gr.MeasureString(n.ToString(), font).Width;
			gr.DrawString(n.ToString(), font, brushBlue, x, y);
			}

		gr.ResetClip();

		gr.SetClip(rectfGraphBodyClip);
		// ------------------------------
		// THIRD: Graph the points
		// ------------------------------

		PTFI ptfiLastMeal = new PTFI();

		ptfiLastMeal.bge = null;

		for (i = 0, iLast = m_plptfi.Count; i < iLast; i++)
			{
			PointF ptf;
			PointF ptfAvg;

			ptfi = ((PTFI)m_plptfi[i]);
			ptf = ptfi.ptf;

			ptfiAvg = ((PTFI)m_plptfiAvg[i]);
			ptfAvg = ptfiAvg.ptf;

			// if its before our first point, skip it
			if (ptf.X < dxFirstQuarter)
				continue;

			if (ptf.Y == -1.0F && ptfi.bge.Reading == 0)
				{
				// lets get a real Y value for this by plotting a line on the curve
				if (i > 0)
					ptf.Y = ((PTFI)m_plptfi[i-1]).ptf.Y;
				else if (i < iLast)
					ptf.Y = ((PTFI)m_plptfi[i+1]).ptf.Y;
				else
					ptf.Y = 0;
				ptfi.ptf = ptf;
				m_plptfi[i] = ptfi;
				}

			if (ptfAvg.Y == -1.0F && ptfiAvg.bge.WgtAvg == 0)
				{
				// lets get a real Y value for this by plotting a line on the curve
				if (i > 0)
					ptfAvg.Y = ((PTFI)m_plptfiAvg[i-1]).ptf.Y;
				else if (i < iLast)
					ptfAvg.Y = ((PTFI)m_plptfiAvg[i+1]).ptf.Y;
				else
					ptfAvg.Y = 0;
				ptfiAvg.ptf = ptf;
				m_plptfiAvg[i] = ptfiAvg;
				}

			if (ptfiLastMeal.bge != null && m_cgp.fShowMeals)
				{
				if (ptfiLastMeal.bge.Date.AddMinutes(90.0) <= ptfi.bge.Date
					&& ptfiLastMeal.bge.Date.AddMinutes(150.0) >= ptfi.bge.Date)
					{
					float yAdjust;
					float xLast = ptfiLastMeal.ptf.X - dxAdjust;
					float yLast = ptfiLastMeal.ptf.Y;

					yAdjust = ptf.Y - yLast;

					if (yAdjust < 0.0F)
						yAdjust -= 15.0F;
					else
						yAdjust += 15.0F;

					// we have a match
					gr.DrawLine(penMeal, xLast + 1, yLast, xLast + 1, yLast + yAdjust);
					gr.DrawLine(penMeal, xLast + 1, yLast + yAdjust, ptf.X + 1 - dxAdjust, yLast + yAdjust);
					gr.DrawLine(penMeal, ptf.X + 1 - dxAdjust, yLast + yAdjust, ptf.X + 1 - dxAdjust, ptf.Y + 1);
					ptfiLastMeal.bge = null;
					}
				else if (ptfiLastMeal.bge.Date.AddHours(150.0) < ptfi.bge.Date)
					ptfiLastMeal.bge = null;
				}

			if (ptfi.bge.Type != BGE.ReadingType.SpotTest)
				ptfiLastMeal = ptfi;

			if (ptf.X > dxLastQuarter)
				break;

			if (ptfi.bge.InterpReading)
                gr.DrawEllipse(penBlueThin, ptf.X - 1 - dxAdjust, ptf.Y - 1, 4, 4);
			else
				gr.FillEllipse(brushBlue, ptf.X - 1 - dxAdjust, ptf.Y - 1, 4, 4);

			if (m_cgp.fGraphAvg)
				gr.FillEllipse(brushAvg, ptfAvg.X - 1 - dxAdjust, ptfAvg.Y - 1, 4, 4);

			if (i > 0)
				{
				PointF ptfLast = ((PTFI)m_plptfi[i - 1]).ptf;

				if (ptfLast.X >= dxFirstQuarter)
					gr.DrawLine(penBlue,  ptfLast.X + 1 - dxAdjust, ptfLast.Y + 1, ptf.X + 1 - dxAdjust, ptf.Y + 1);

				if (m_cgp.fGraphAvg)
					{
					ptfLast = ((PTFI)m_plptfiAvg[i - 1]).ptf;

					if (ptfLast.X >= dxFirstQuarter)
						gr.DrawLine(penAvg,  ptfLast.X + 1 - dxAdjust, ptfLast.Y + 1, ptfAvg.X + 1 - dxAdjust, ptfAvg.Y + 1);
					}
				}
			}
		// if we were doing the translucent range shading, we'd do it here.
//			ShadeRanges(pb, gr);
		gr.ResetClip();
#endif // NEVER
	}


	/* F  G E T  L A S T  D A T E  T I M E  O N  P A G E */
	/*----------------------------------------------------------------------------
		%%Function: FGetLastDateTimeOnPage
		%%Qualified: bg.Grapher.FGetLastDateTimeOnPage
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public bool FGetLastDateTimeOnPage(out DateTime dttm)
	{
		DateTime dttmLast = m_sbge.GetLastDateTime();

		dttm = GetFirstDateTime();

		if (dttm.AddDays(m_cgp.nDays) > dttmLast)
			return false;

		dttm = dttm.AddDays(m_cgp.nDays);
		return true;
	}


	/* F  H I T  T E S T */
	/*----------------------------------------------------------------------------
		%%Function: FHitTest
		%%Qualified: bg.Grapher.FHitTest
		%%Contact: rlittle

		GraphicBox interface
	----------------------------------------------------------------------------*/
	public bool FHitTest(Point ptClient, out object oHit, out RectangleF rectfHit)
	{
		return m_sbge.FHitTest(ptClient, out oHit, out rectfHit);
	}

}
}
