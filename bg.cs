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

namespace bg
{

//	___  ____ ____
//	|__] | __ |___
//	|__] |__] |___

public class BGE // BG Entry
{
	public enum ReadingType
	{
		Breakfast,
		Lunch,
		Dinner,
		Snack,
		SpotTest,
		Control,
		New
	};

	public enum CompareType
	{
		Date,
		Type,
		Reading,
		Carbs,
		Comment
	};

	DateTime m_dttm;
	int m_nBg;
	ReadingType m_type;
	int m_nCarbs;
	string m_sComment;
	int m_nMinutesSinceLastCarb;
	int m_nCarbsInLast4Hours;

	/* R E A D I N G  T Y P E  F R O M  S T R I N G */
	/*----------------------------------------------------------------------------
		%%Function: ReadingTypeFromString
		%%Qualified: bg.BGE.ReadingTypeFromString
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public static ReadingType ReadingTypeFromString(string s)
	{
		if (String.Compare(s, "breakfast", true) == 0)
			return ReadingType.Breakfast;
		else if (String.Compare(s, "lunch", true) == 0)
			return ReadingType.Lunch;
		else if (String.Compare(s, "dinner", true) == 0)
			return ReadingType.Dinner;
		else if (String.Compare(s, "snack", true) == 0)
			return ReadingType.Snack;
		else if (String.Compare(s, "new", true) == 0)
			return ReadingType.New;
		else if (String.Compare(s, "control", true) == 0)
			return ReadingType.Control;
		else
			return ReadingType.SpotTest;
	}

	/* S T R I N G  F R O M  R E A D I N G  T Y P E */
	/*----------------------------------------------------------------------------
		%%Function: StringFromReadingType
		%%Qualified: bg.BGE.StringFromReadingType
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public static string StringFromReadingType(ReadingType type)
	{
		switch (type)
			{
			case ReadingType.Breakfast:
				return "Breakfast";
			case ReadingType.Lunch:
				return "Lunch";
			case ReadingType.Snack:
				return "Snack";
			case ReadingType.Dinner:
				return "Dinner";
			case ReadingType.New:
				return "*NEW*";
			case ReadingType.Control:
				return "Control";
			default:
				return "SpotTest";
			}
	}


	/* B  G  E */
	/*----------------------------------------------------------------------------
		%%Function: BGE
		%%Qualified: bg.BGE.BGE
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public BGE(string sDate, string sTime, ReadingType type, int bg, int nCarbs, string sComment)
	{
		m_dttm = DateTime.Parse(sDate + " " + sTime);
		m_nBg = bg;
		m_type = type;
		m_nCarbs = nCarbs;
		m_sComment = sComment;
	}
	
	public int Carbs
	{
		get
		{
			return m_nCarbs;
		}

		set
		{
			m_nCarbs = value;
		}
	}

	public int MinutesSinceCarbs
	{
		get
		{
			return m_nMinutesSinceLastCarb;
		}

		set
		{
			m_nMinutesSinceLastCarb = value;
		}
	}

	public int CarbsIn4
	{
		get
		{
			return m_nCarbsInLast4Hours;
		}

		set
		{
			m_nCarbsInLast4Hours = value;
		}
	}

	public string Comment
	{
		get
		{
			return m_sComment;
		}

		set
		{
			m_sComment = value;
		}
	}

	public ReadingType Type
	{
		get
		{
			return m_type;
		}

		set
		{
			m_type = value;
		}
	}

	public DateTime Date
	{
		get
		{
			return m_dttm;
		}
	}

	public int Reading
	{
		get
		{
			return m_nBg;
		}
	}

	/* C O M P A R E  T O */
	/*----------------------------------------------------------------------------
		%%Function: CompareTo
		%%Qualified: bg.BGE.CompareTo
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public int CompareTo(BGE bge, CompareType type)
	{	
		return Compare(this, bge, type);
	}

	/* C O M P A R E */
	/*----------------------------------------------------------------------------
		%%Function: Compare
		%%Qualified: bg.BGE.Compare
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	static public int Compare(BGE bge1, BGE bge2, CompareType type)
	{
		switch (type)
			{
			case CompareType.Type:
				return (int)bge1.Type - (int)bge2.Type;
			case CompareType.Reading:
				return bge1.Reading - bge2.Reading;
			case CompareType.Date:
				return DateTime.Compare(bge1.Date, bge2.Date);
			default:
				return 0; 
			}
	}

	public void SetTo(BGE bge)
	{
		m_type = bge.Type;
		m_dttm = bge.Date;
		m_nBg = bge.Reading;
		m_nCarbs = bge.Carbs;
		m_sComment = bge.Comment;
	}

	
}

public struct PTFI
{
	public PointF ptf;
	public BGE bge;
};

public struct GrapherParams
{
	public int nDays;
	public int nIntervals;
	public bool fShowMeals;
	public double dBgLow;
	public double dBgHigh;
	public bool fLandscape;
}

//  ______  ______ _______  _____  _     _ _____ _______   ______   _____  _     _
// |  ____ |_____/ |_____| |_____] |_____|   |   |         |_____] |     |  \___/
// |_____| |    \_ |     | |       |     | __|__ |_____    |_____] |_____| _/   \_

public interface GraphicBox
{
	void Paint(Graphics gr);
	void SetFirstFromScroll(int i);
	void Calc();
	bool FHitTest(Point pt, out object oHit, out RectangleF rectfHit);
	void SetDataPoints(SortedList slbge, VScrollBar sbv, HScrollBar sbh);
	void SetProps(GrapherParams gp);
	int GetFirstForScroll();
	void SetColor(bool fColor);
	int GetDaysPerPage();
	void SetDaysPerPage(int nDaysPerPage);
	DateTime GetFirstDateTime();
	void SetFirstDateTime(DateTime dttm);
	bool FGetLastDateTimeOnPage(out DateTime dttm);

}

//	 ______ _______  _____   _____   ______ _______ _______  ______
//	|_____/ |______ |_____] |     | |_____/    |    |______ |_____/
//	|    \_ |______ |       |_____| |    \_    |    |______ |    \_

public class Reporter : GraphicBox
{
	//  _  _ ____ _  _ ___  ____ ____     _  _ ____ ____ _ ____ ___  _    ____ ____
	//  |\/| |___ |\/| |__] |___ |__/     |  | |__| |__/ | |__| |__] |    |___ [__
	//  |  | |___ |  | |__] |___ |  \      \/  |  | |  \ | |  | |__] |___ |___ ___]

	float m_nHeight;
	float m_nWidth;
	SortedList m_slbge;
	GrapherParams m_cgp;
	double m_dHoursBefore = 2.0;
	double m_dHoursAfter = 1.5;
	RectangleF m_rcfDrawing;
	bool m_fColor = true;

	COLD []m_mpicold;
	float []m_mpicolDxp;

	const int icolDay = 0;
	const int icolMealBreakfast = 1;
	const int icolMealLunch = 4;
	const int icolMealDinner = 7;
	const int icolBed = 10;
	const int icolComments = 11;

	const int iMealBreakfast = 0;
	const int iMealLunch = 1;
	const int iMealDinner = 2;

	struct PTB // PaintBox
	{
		public Font fontText;
		public SolidBrush brushText;
		public SolidBrush brushHeavy;
		public SolidBrush brushLight;
		public SolidBrush brushLightFill;
		public SolidBrush brushBorderFill;
		public Pen penHeavy;
		public Pen penLight;
	}

	struct MD // MealData
	{
		public int nBefore;
		public int nAfter;
		public int nCarbs;
	};

	PTB m_ptb;

	//	____ _    ___
	//	|__/ |    |  \
	//	|  \ |___ |__/

	class RLD	// ReportLineData
	{
		public string sDay;
		public int nBedReading;
		public string sComment;
		public string sDateHeader;
		public DateTime dttm;
		public MD []mpimd;

		/* R  L  D */
		/*----------------------------------------------------------------------------
			%%Function: RLD
			%%Qualified: bg.Reporter:RLD.RLD
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public RLD()
		{
			sDay = "";
			nBedReading = 0;
			sComment = "";
			sDateHeader = "";
			mpimd = new MD[3];
			mpimd[0].nBefore = mpimd[0].nAfter = mpimd[0].nCarbs;
			mpimd[1].nBefore = mpimd[1].nAfter = mpimd[1].nCarbs;
			mpimd[2].nBefore = mpimd[2].nAfter = mpimd[2].nCarbs;
		}

	};

	public enum BorderType
	{
		None,
		Solid,
		Double
	};

	//	____ ____ _    ___
	//	|    |  | |    |  \
	//	|___ |__| |___ |__/

	public class COLD	// COLumn Definition
	{
		public float xpLeft;
		public float dxpCol;
		public BorderType btLeft;
		public BorderType btRight;

		/* C  O  L  D */
		/*----------------------------------------------------------------------------
			%%Function: COLD
			%%Qualified: bg.Reporter:COLD.COLD
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		public COLD(COLD coldPrev, float dxpLeftSpace, float dxpColIn, BorderType btLeftIn, BorderType btRightIn)
		{
			float xpPrev = 0;
			if (coldPrev != null)
				{
				xpPrev = coldPrev.xpLeft + coldPrev.dxpCol;
				}
			xpLeft = xpPrev + dxpLeftSpace;
			dxpCol = dxpColIn;
			btLeft = btLeftIn;
			btRight = btRightIn;
		}

		/* D X P  R I G H T */
		/*----------------------------------------------------------------------------
			%%Function: dxpRight
			%%Qualified: bg.Reporter:COLD.dxpRight
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public float dxpRight
		{
			get
			{
				return xpLeft + dxpCol;
			}
		}

		/* R C F  F R O M  C O L U M N */
		/*----------------------------------------------------------------------------
			%%Function: RcfFromColumn
			%%Qualified: bg.Reporter:COLD.RcfFromColumn
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public RectangleF RcfFromColumn(float yp, float dyp)
		{
			return new RectangleF(xpLeft, yp, this.dxpRight - xpLeft, dyp);
		}

		/* D R A W  S I N G L E  B O R D E R */
		/*----------------------------------------------------------------------------
			%%Function: DrawSingleBorder
			%%Qualified: bg.Reporter:COLD.DrawSingleBorder
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		void DrawSingleBorder(Graphics gr, Brush brushFill, Pen pen, float xp, float yp, float dyp, BorderType bt)
		{
			switch (bt)
				{
				case BorderType.Solid:
					{
					float dxpAdjustForWidth = pen.Width / 2.0F;

					xp -= dxpAdjustForWidth;
					gr.DrawLine(pen, xp, yp, xp, yp + dyp);
					break;
					}
				case BorderType.Double:
					{
					float penWidth = (float)pen.Width;
					float dxpAdjustForWidth = (penWidth * 3.0F) / 2.0F;

					xp -= dxpAdjustForWidth;
					// clear the region for the border
					gr.FillRectangle(brushFill, xp + penWidth / 2.0F, yp, penWidth * 1.5F, dyp);

					gr.DrawLine(pen, xp, yp, xp, yp + dyp);

					gr.DrawLine(pen, xp + penWidth * 2.0F, yp, xp + penWidth * 2.0F, yp + dyp);
					break;
					}
				}
		}

		/* D R A W  B O R D E R S */
		/*----------------------------------------------------------------------------
			%%Function: DrawBorders
			%%Qualified: bg.Reporter:COLD.DrawBorders
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void DrawBorders(Graphics gr, float ypTop, float dyp, Pen pen, Brush brushFill)
		{
			DrawSingleBorder(gr, brushFill, pen, this.xpLeft, ypTop, dyp, this.btLeft);
			DrawSingleBorder(gr, brushFill, pen, this.xpLeft, ypTop, dyp, this.btRight);
		}
	}


	/* S E T  C O L O R */
	/*----------------------------------------------------------------------------
		%%Function: SetColor
		%%Qualified: bg.Reporter.SetColor
		%%Contact: rlittle
		
	----------------------------------------------------------------------------*/
	public void SetColor(bool fColor)
	{
		m_fColor = fColor;
	}

	/* S E T  F I R S T  F R O M  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstFromScroll
		%%Qualified: bg.Reporter:COLD.SetFirstFromScroll
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstFromScroll(int i)
	{
		SetFirstLine(i);
	}

	/* D X P  F R O M  D X A */
	/*----------------------------------------------------------------------------
		%%Function: DxpFromDxa
		%%Qualified: bg.Reporter.DxpFromDxa
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	static public float DxpFromDxa(Graphics gr, float dxa)
	{
		return (float)(((double)dxa * gr.DpiX) / 1440.0);
	}

	static public float DxpFromDxaPrint(Graphics gr, float dxa)
	{
		return (float)(((double)dxa * 100.0) / 1440.0);
	}

	/* D Y P  F R O M  D Y A */
	/*----------------------------------------------------------------------------
		%%Function: DypFromDya
		%%Qualified: bg.Reporter.DypFromDya
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	static public float DypFromDya(Graphics gr, float dya)
	{
		return (float)(((double)dya * gr.DpiY) / 1440.0);
	}

	static public float DypFromDyaPrint(Graphics gr, float dya)
	{
		return (float)(((double)dya * 100.0) / 1440.0);
	}

	/* S E T  F I R S T  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstLine
		%%Qualified: bg.Reporter.SetFirstLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstLine(int iLine)
	{
		m_iLineFirst = iLine;
	}

	/* G E T  F I R S T  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstLine
		%%Qualified: bg.Reporter.GetFirstLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int GetFirstLine()
	{
		return m_iLineFirst;
	}

	/* G E T  F I R S T  F O R  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstForScroll
		%%Qualified: bg.Reporter.GetFirstForScroll
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int GetFirstForScroll()
	{
		return m_iLineFirst;
	}

	/* G E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstDateTime
		%%Qualified: bg.Reporter.GetFirstDateTime
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public DateTime GetFirstDateTime()
	{
		return ((RLD)m_plrld[m_iLineFirst]).dttm;
	}

	/* S E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstDateTime
		%%Qualified: bg.Reporter.SetFirstDateTime
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstDateTime(DateTime dttm)
	{
		// try to normalize around dttm
		int i, iMax;

		for (i = 0, iMax = m_plrld.Count; i < iMax; i++)
			{
			if (dttm <= ((RLD)m_plrld[i]).dttm)
				break;
			}

		if (i < iMax)
			m_iLineFirst = i;
	}



	/* S E T  C O L  W I D T H */
	/*----------------------------------------------------------------------------
		%%Function: SetColWidth
		%%Qualified: bg.Reporter.SetColWidth
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void SetColWidth(int iCol, int nPercent, BorderType btLeft, BorderType btRight)
	{
		float dPercent = (m_nWidth * (float)nPercent) / 100.0F;

		if (iCol > 0)
			{
			if (dPercent == 0)
				{
				dPercent = m_rcfDrawing.Right - m_mpicold[iCol - 1].dxpRight;
				}

			float dxpSpace = 0.0F;

			if (m_mpicold[iCol - 1].btRight != BorderType.None)
				{
				dxpSpace = 2.0F;
				}

			m_mpicold[iCol] = new COLD(m_mpicold[iCol - 1], dxpSpace, dPercent, btLeft, btRight);
			}
		else
			{
			m_mpicold[iCol] = new COLD(null, m_rcfDrawing.Left, dPercent, btLeft, btRight);
			}
	}

	/* R E P O R T E R */
	/*----------------------------------------------------------------------------
		%%Function: Reporter
		%%Qualified: bg.Reporter.Reporter
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public Reporter(RectangleF rcfDrawing, Graphics gr)	// int nWidth, int nHeight, 
	{
		// draw the first line
		m_ptb.fontText = new Font("Tahoma", 9);
		m_ptb.brushText = new SolidBrush(Color.Black);
		m_ptb.brushHeavy = new SolidBrush(Color.Blue);
		m_ptb.brushLight = new SolidBrush(Color.LightBlue);
		m_ptb.penHeavy = new Pen(m_ptb.brushHeavy, 1.0F);
		m_ptb.penLight = new Pen(m_ptb.brushLight, 0.5F);
		m_ptb.brushLightFill = new SolidBrush(Color.LightGray);
		m_ptb.brushBorderFill = new SolidBrush(Color.White);

		m_dyLine = gr.MeasureString("M", m_ptb.fontText).Height;
		m_mpicold = new COLD[13];

		m_rcfDrawing = rcfDrawing;
		m_nHeight = rcfDrawing.Height; // nHeight;
		m_nWidth = rcfDrawing.Width; // nWidth;
		m_cgp.dBgLow = 30.0;
		m_cgp.dBgHigh = 220.0;
		m_cgp.nDays = 7;
		m_cgp.nIntervals = 19;
		m_cgp.fShowMeals = false;

		float dy = YFromLine(2) - YFromLine(1);


		m_nLinesPerPage = (int)(m_nHeight / dy) - 1 ;

		Font font = new Font("Tahoma", 8);

		SetColWidth(icolDay, 5, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealBreakfast, 4, BorderType.Double, BorderType.None);
		SetColWidth(icolMealBreakfast + 1, 3, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealBreakfast + 2, 4, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealLunch, 4, BorderType.Double, BorderType.None);
		SetColWidth(icolMealLunch + 1, 3, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealLunch + 2, 4, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealDinner, 4, BorderType.Double, BorderType.None);
		SetColWidth(icolMealDinner + 1, 3, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealDinner + 2, 4, BorderType.Solid, BorderType.None);

		SetColWidth(icolBed, 4, BorderType.Double, BorderType.None);

		SetColWidth(icolComments, 0, BorderType.Double, BorderType.None);
	}

	//	___  ____ _ _  _ ___ _ _  _ ____
	//	|__] |__| | |\ |  |  | |\ | | __
	//	|    |  | | | \|  |  | | \| |__]

	/* P A I N T  G R I D L I N E S */
	/*----------------------------------------------------------------------------
		%%Function: PaintGridlines
		%%Qualified: bg.Reporter.PaintGridlines
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void PaintGridlines(Graphics gr)
	{

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealBreakfast].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));
		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealBreakfast + 2].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealLunch].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));
		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealLunch + 2].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealDinner].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));
		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealDinner + 2].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolBed].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.DrawRectangle(m_ptb.penHeavy, m_mpicold[0].xpLeft, m_rcfDrawing.Top, m_nWidth, m_nHeight);

		// draw the column borders
		m_mpicold[icolMealBreakfast].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolMealBreakfast + 1].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);
		m_mpicold[icolMealBreakfast + 2].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);

		m_mpicold[icolMealLunch].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolMealLunch + 1].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);
		m_mpicold[icolMealLunch + 2].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);

		m_mpicold[icolMealDinner].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolMealDinner + 1].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);
		m_mpicold[icolMealDinner + 2].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);

		m_mpicold[icolBed].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolComments].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);

		gr.DrawLine(m_ptb.penHeavy, m_mpicold[0].xpLeft, m_dyLine + m_rcfDrawing.Top, m_nWidth + m_mpicold[0].xpLeft, m_dyLine + m_rcfDrawing.Top);

	}

	/* P A I N T  H E A D E R */
	/*----------------------------------------------------------------------------
		%%Function: PaintHeader
		%%Qualified: bg.Reporter.PaintHeader
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void PaintHeader(Graphics gr)
	{
		float y = YFromLine(0);

		DrawTextInColumn(gr, "day", m_ptb.fontText, m_ptb.brushText, 0, y, 1, HorizontalAlignment.Left);
		DrawTextInColumn(gr, "Breakfast", m_ptb.fontText, m_ptb.brushText, icolMealBreakfast, y, 3, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Lunch", m_ptb.fontText, m_ptb.brushText, icolMealLunch, y, 3, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Dinner", m_ptb.fontText, m_ptb.brushText, icolMealDinner, y, 3, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Bed", m_ptb.fontText, m_ptb.brushText, icolBed, y, 1, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Comments", m_ptb.fontText, m_ptb.brushText, icolComments, y, 1, HorizontalAlignment.Left);
	}

	/* P A I N T  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: PaintLine
		%%Qualified: bg.Reporter.PaintLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void PaintLine(Graphics gr, RLD rld, int iLine)
	{
		int iMeal;

		DrawTextInColumn(gr, rld.sDay, m_ptb.fontText, m_ptb.brushText, 0, YFromLine(iLine), 1, HorizontalAlignment.Center);

		for (iMeal = 0; iMeal <= iMealDinner; iMeal++)
			{
			if (rld.mpimd[iMeal].nBefore > 0)
				DrawTextInColumn(gr, rld.mpimd[iMeal].nBefore.ToString(), m_ptb.fontText, m_ptb.brushText, 1 + iMeal * 3, YFromLine(iLine), 1, HorizontalAlignment.Right);
			if (rld.mpimd[iMeal].nCarbs > 0)
				DrawTextInColumn(gr, rld.mpimd[iMeal].nCarbs.ToString(), m_ptb.fontText, m_ptb.brushText, 2 + iMeal * 3, YFromLine(iLine), 1, HorizontalAlignment.Center);
			if (rld.mpimd[iMeal].nAfter > 0)
				DrawTextInColumn(gr, rld.mpimd[iMeal].nAfter.ToString(), m_ptb.fontText, m_ptb.brushText, 3 + iMeal * 3, YFromLine(iLine), 1, HorizontalAlignment.Right);
			}

		DrawTextInColumn(gr, rld.nBedReading.ToString(), m_ptb.fontText, m_ptb.brushText, icolBed, YFromLine(iLine), 1, HorizontalAlignment.Right);
		DrawTextInColumn(gr, /*rld.sDateHeader + "] " + */rld.sComment, m_ptb.fontText, m_ptb.brushText, icolComments, YFromLine(iLine), 1, HorizontalAlignment.Left);
	}

	int m_nLines;
	int m_iLineFirst;
	int m_nLinesPerPage;
	/* S E T  D A T A  P O I N T S */
	/*----------------------------------------------------------------------------
		%%Function: SetDataPoints
		%%Qualified: bg.Reporter.SetDataPoints
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetDataPoints(SortedList slbge, VScrollBar sbv, HScrollBar sbh)
	{
		m_slbge = slbge;

		// figure out how many lines we're going to have...
		// get the first entry and the last
		DateTime dttmFirst = ((BGE)m_slbge.GetByIndex(0)).Date;
		DateTime dttmLast = ((BGE)m_slbge.GetByIndex(m_slbge.Count - 1)).Date;

		dttmLast = dttmLast.AddMinutes(-241.0);	// subtract just over 4 hours to account for the fact that our day turns at 4am...  
		TimeSpan ts = dttmLast.Subtract(dttmFirst);
		// the number of days is the number of lines.
		m_nLines = ts.Days + 1;
		m_iLineFirst = 0;
		if (sbv != null)
			{
			if (m_nLines <= m_nLinesPerPage)
				sbv.Visible = false;
			else
				{
				sbv.Visible = true;
				sbv.Minimum = 0;
				sbv.Maximum = m_nLines + m_nLinesPerPage;
				}
			}
	}

	/* S E T  P R O P S */
	/*----------------------------------------------------------------------------
		%%Function: SetProps
		%%Qualified: bg.Reporter.SetProps
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetProps(GrapherParams cgp)
	{
		m_cgp = cgp;
	}

	float m_dyLine = 0.0F;

	/* Y  F R O M  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: YFromLine
		%%Qualified: bg.Reporter.YFromLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public float YFromLine(int nLine)
	{
		return (float)m_dyLine * nLine + m_rcfDrawing.Top;
	}

	/* D R A W  T E X T  I N  C O L U M N */
	/*----------------------------------------------------------------------------
		%%Function: DrawTextInColumn
		%%Qualified: bg.Reporter.DrawTextInColumn
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void DrawTextInColumn(Graphics gr, string s, Font font, SolidBrush br, int iCol, float y, int cColSpan, HorizontalAlignment jc)
	{
		int iColMax = iCol + cColSpan - 1;
		COLD cold = m_mpicold[iCol];
		float dxpRight = m_mpicold[iColMax].dxpRight;
		float dxpWidth = 0.0F;

		if (cColSpan == 1)
			dxpWidth = cold.dxpCol;
		else
			dxpWidth = dxpRight - cold.xpLeft;

		RectangleF rectfClip = new RectangleF(cold.xpLeft, y, dxpWidth, y + 20);
		gr.SetClip(rectfClip);

		switch (jc)
			{
			case HorizontalAlignment.Left:
				gr.DrawString(s, font, br, cold.xpLeft, y);
				break;
			case HorizontalAlignment.Center:
				gr.DrawString(s, font, br, cold.xpLeft + (dxpWidth - gr.MeasureString(s, font).Width) / 2, y);
				break;
			case HorizontalAlignment.Right:
				gr.DrawString(s, font, br, cold.xpLeft + (dxpWidth - gr.MeasureString(s, font).Width), y);
				break;
			}
		gr.ResetClip();
	}

	ArrayList m_plrld;

	/* C A L C  R E P O R T */
	/*----------------------------------------------------------------------------
		%%Function: CalcReport
		%%Qualified: bg.Reporter.CalcReport
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void Calc()
	{
		m_plrld = new ArrayList();

		// ok, let's walk through and figure out what gets reported and what doesn't
		//
		// first, draw the date we are working with.
		BGE bge = (BGE)m_slbge.GetByIndex(0);
		DateTime dttmFirst = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day);

		int ibge, ibgeMax;

		ibge = 0;
		ibgeMax = m_slbge.Values.Count;
		RLD rld = new RLD();

		while (ibge < ibgeMax)
			{
			float []dMeals = { 8.0F, 12.0F, 18.0F };	// our defaults for meals
			float dHours = 0.0F;
			string []rgsDay = { "S", "M", "T", "W", "T", "F", "S" };
			bool []rgfMeals = { false, false, false };


			bge = (BGE)m_slbge.GetByIndex(ibge);

			DateTime dttm = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day);
			DateTime dttmNextDay = dttm.AddHours(29.5);// next day is 4:30am the next day
			int ibgeT = ibge;
			rld.dttm = dttm;
			rld.sDay = rgsDay[(int)dttm.DayOfWeek];
			if (dttm.DayOfWeek == DayOfWeek.Sunday)
				{
				rld.sDay = dttm.ToString("M-dd");
				rld.sDateHeader = dttm.ToString("d");
				}

			// let's see what meals we have manually accounted for...
			while (bge.Date < dttmNextDay)
				{
				dHours = bge.Date.Hour + bge.Date.Minute / 60.0F;

				if (bge.Type == BGE.ReadingType.Breakfast)
					{
					dMeals[0] = dHours;
					rgfMeals[0] = true;
					}
				else if (bge.Type == BGE.ReadingType.Lunch)
					{
					dMeals[1] = dHours;
					rgfMeals[1] = true;
					}
				else if (bge.Type == BGE.ReadingType.Dinner)
					{
					dMeals[2] = dHours;
					rgfMeals[2] = true;
					}

				if (ibgeT + 1 >= ibgeMax)
					break;

				bge = (BGE)m_slbge.GetByIndex(++ibgeT);
				}

			// ok, we have figured out the meal boundaries.  now lets match the readings...
			// any reading within 1 hour of the meal time is considered a "before", and the first reading
			// between 2 hours and 3 hours after meal time is considered "after"
			bge = (BGE)m_slbge.GetByIndex(ibgeT = ibge);
			int iMealNext = 0;
			bool fBeforeMatched = false;
			int readingBefore = 0;
			int readingBed = 0;
			int readingFirst = 0; // the first reading of the day will serve as the "pre-breakfast" unless we get a better reading.
			float dBedMin = dMeals[iMealDinner] + 2.0F;
			bool fMealBumped = false;

			while (bge.Date < dttmNextDay)	
				{
				dHours = bge.Date.Hour + bge.Date.Minute / 60.0F;

				// don't duplicate comments...
				if (!fMealBumped && bge.Comment.Length > 0)
					{
					if (rld.sComment.Length > 0)
						rld.sComment += "; ";

					rld.sComment += bge.Comment;
					}

				fMealBumped = false;
				if (bge.Date.Day == dttmNextDay.Day)
					dHours += 24.0F;

				if (readingFirst == 0 && dHours <= dMeals[iMealBreakfast])
					readingFirst = bge.Reading;

				if (iMealNext <= iMealDinner)
					{
					// are we looking for a 'before' reading?

					if ((iMealNext == iMealBreakfast && bge.Type == BGE.ReadingType.Breakfast)
						|| (iMealNext == iMealLunch && bge.Type == BGE.ReadingType.Lunch)
						|| (iMealNext == iMealDinner && bge.Type == BGE.ReadingType.Dinner))
						{
						rld.mpimd[iMealNext].nCarbs = bge.Carbs;
						}

					if (!fBeforeMatched
						&& bge.Reading != 0
						&& dHours >= dMeals[iMealNext] - m_dHoursBefore && dHours <= dMeals[iMealNext])
						{
						// got a "before" match
						readingBefore = bge.Reading;
						}

					// did we find a "before" reading for the current meal and are looking
					// for that meal (or a reading post-meal) to confirm that this is the last
					// "before" reading?

					if (readingBefore != 0 && !fBeforeMatched && dHours > dMeals[iMealNext])
						{
						rld.mpimd[iMealNext].nBefore = readingBefore;
						fBeforeMatched = true;
						}

					// does this reading qualify as an 'after' reading?
					if (dHours >= dMeals[iMealNext] + m_dHoursAfter && dHours <= dMeals[iMealNext] + 4.0)
						{
						// got an "after" match
						rld.mpimd[iMealNext].nAfter = bge.Reading;
						iMealNext++;
						fBeforeMatched = false;
						if (readingBefore != 0)
							readingFirst = 0;
						readingBefore = 0;
						fMealBumped = true;
						}

					if (!fMealBumped)
						{
						// check to see if we are ready to bump to the next meal
						if (dHours > dMeals[iMealNext] + 4.0)
							{
							iMealNext++;
							fBeforeMatched = false;
							if (readingBefore != 0)
								readingFirst = 0;
							readingBefore = 0;
							fMealBumped = true;
							}
						}

					if (fMealBumped && iMealNext == iMealLunch)
						{
						if (readingFirst != 0)
							{
							// we still have a "first reading" which means that the breakfast
							// meal never posted a before reading.  use that first reading now.
							rld.mpimd[iMealBreakfast].nBefore = readingFirst;
							readingFirst = 0;
							}
						}

					if (!fMealBumped && ibgeT + 1 >= ibgeMax)
						{
						break;
						}

					}

				if (dHours >= dBedMin)
					readingBed = bge.Reading;

				// if we bumped the meal, then go through again considering this reading
				// for the next meal.
				if (!fMealBumped)
					{
					if (ibgeT + 1 >= ibgeMax)
						break;
					bge = (BGE)m_slbge.GetByIndex(++ibgeT);
					}
				}
			// need to flush everything here
			if (!fBeforeMatched && readingBefore != 0)
				{
				rld.mpimd[iMealNext].nBefore = readingBefore;
				}
			if (!fBeforeMatched && iMealNext == iMealBreakfast && readingFirst != 0)
				{
				rld.mpimd[iMealNext].nBefore = readingFirst;
				}

			rld.nBedReading = readingBed;
			m_plrld.Add(rld);
			rld = new RLD();

			// consume the rest of the day
			ibge = ibgeT;
			bge = (BGE)m_slbge.GetByIndex(ibge);
			while (bge.Date < dttmNextDay)
				{
				ibge++;
				if (ibge >= ibgeMax)
					break;

				bge = (BGE)m_slbge.GetByIndex(ibge);
				}
			}
		// at this point, we have all the data we'll ever want...

	}


	public int GetDaysPerPage()
	{
		return m_nLinesPerPage;
	}

	public void SetDaysPerPage(int nDaysPerPage)
	{
		throw new Exception("Cannot set days per page in a report!");
	}

	/* P A I N T */
	/*----------------------------------------------------------------------------
		%%Function: Paint
		%%Qualified: bg.Reporter.Paint
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void Paint(Graphics gr)
	{

		PaintHeader(gr);
		PaintGridlines(gr);

		int iLine, iLineMax;
		int iLinePainting = 1;

		for (iLine = m_iLineFirst, iLineMax = Math.Min(m_iLineFirst + m_nLinesPerPage, m_nLines); iLine < iLineMax; iLine++)
			{
			PaintLine(gr, (RLD)m_plrld[iLine], iLinePainting++);
			}
	}

	public bool FGetLastDateTimeOnPage(out DateTime dttm)
	{
		dttm = GetFirstDateTime();
		if (m_iLineFirst + m_nLinesPerPage > m_nLines)
			{
			return false;
			}

		dttm = dttm.AddDays(m_nLinesPerPage);
		return true;
	}

	/* F  H I T  T E S T */
	/*----------------------------------------------------------------------------
		%%Function: FHitTest
		%%Qualified: bg.Reporter.FHitTest
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public bool FHitTest(Point pt, out object oHit, out RectangleF rectfHit)
	{
		oHit = null;
		rectfHit = new RectangleF(0F,0F,0F,0F);
		return false;
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

	float m_nHeight;
	float m_nWidth;
	RectangleF m_rcfDrawing;
	SortedList m_slbge;
	GrapherParams m_cgp;
	double m_dyPerBgUnit;
	double m_dxQuarter;
	int m_iFirstQuarter;
	float m_dxAdjust = 0;
	bool m_fColor = true;

	ArrayList m_plptfi;

	float m_dxOffset = 30.0F;
	float m_dyOffset = 45.0F;

	HScrollBar m_sbh;

	/* G R A P H E R */
	/*----------------------------------------------------------------------------
		%%Function: Grapher
		%%Qualified: bg.Grapher.Grapher
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public Grapher(RectangleF rcf, Graphics gr) // int nWidth, int nHeight
	{
		m_rcfDrawing = rcf;
		m_nHeight = rcf.Bottom - rcf.Top; // nHeight;
		m_nWidth = rcf.Right - rcf.Left; // nWidth;
		m_cgp.dBgLow = 30.0;
		m_cgp.dBgHigh = 220.0;
		m_cgp.nDays = 7;
		m_cgp.nIntervals = 19;
		m_cgp.fShowMeals = false;
		Font font = new Font("Tahoma", 8);

		m_dxOffset = gr.MeasureString("0000", font).Width;
		m_dyOffset = gr.MeasureString("0\n0\n0\n0\n", font).Height;
	}

	public int GetDaysPerPage()
	{
		return m_cgp.nDays;
	}

	public void SetDaysPerPage(int nDaysPerPage)
	{
		m_cgp.nDays = nDaysPerPage;
	}

	/* S E T  F I R S T  F R O M  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstFromScroll
		%%Qualified: bg.Grapher.SetFirstFromScroll
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstFromScroll(int i)
	{
		SetFirstQuarter(i);
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

	/* S E T  C O L O R */
	/*----------------------------------------------------------------------------
		%%Function: SetColor
		%%Qualified: bg.Grapher.SetColor
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetColor(bool fColor)
	{
		m_fColor = fColor;
	}

	/* G E T  F I R S T  F O R  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstForScroll
		%%Qualified: bg.Grapher.GetFirstForScroll
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int GetFirstForScroll()
	{
		return GetFirstQuarter();
	}

	/* G E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstDateTime
		%%Qualified: bg.Grapher.GetFirstDateTime
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public DateTime GetFirstDateTime()
	{
		PTFI ptfi = (PTFI)m_plptfi[0];
		DateTime dttmFirst = new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);
		DateTime dttm = DateTime.Parse(dttmFirst.ToString("d"));
		double nDayFirst = ((double)m_iFirstQuarter + 95.0) / (4.0 * 24.0) - 1.0;
		dttm = dttm.AddDays(nDayFirst);

		return dttm;
	}

	/* S E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstDateTime
		%%Qualified: bg.Grapher.SetFirstDateTime
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstDateTime(DateTime dttm)
	{
		PTFI ptfi = (PTFI)m_plptfi[0];
		DateTime dttmFirst = new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);
		TimeSpan ts = dttm.Subtract(dttmFirst);
		m_iFirstQuarter = (int)(4.0 * ts.TotalHours) + 1;
	}


	/* P T F  F R O M  B G E */
	/*----------------------------------------------------------------------------
		%%Function: PtfFromBge
		%%Qualified: bg.Grapher.PtfFromBge
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	PointF PtfFromBge(DateTime dayFirst, BGE bge, double dxQuarter, double dyBg)
	{
		float x = XFromDate(dayFirst, bge.Date, dxQuarter);
		float y = YFromReading(bge.Reading, dyBg);

		// now we've got the number of quarters.  figure out the bge
		return new PointF(x,y);
	}

	/* S E T  D A T A  P O I N T S */
	/*----------------------------------------------------------------------------
		%%Function: SetDataPoints
		%%Qualified: bg.Grapher.SetDataPoints
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetDataPoints(SortedList slbge, VScrollBar sbv, HScrollBar sbh)
	{
		m_slbge = slbge;
		m_sbh = sbh;
	}

	/* S E T  P R O P S */
	/*----------------------------------------------------------------------------
		%%Function: SetProps
		%%Qualified: bg.Grapher.SetProps
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetProps(GrapherParams cgp)
	{
		m_cgp = cgp;
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

		return m_rcfDrawing.Top + m_nHeight - ((((float)nReading - (float)m_cgp.dBgLow) * (float)dyPerBgUnit)) - m_dyOffset;
	}

	/* X  F R O M  D A T E */
	/*----------------------------------------------------------------------------
		%%Function: XFromDate
		%%Qualified: bg.Grapher.XFromDate
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public float XFromDate(DateTime dayFirst, DateTime dttm, double dxQuarter)
	{
		// calculate how many quarter hours
		long ticks = dttm.Ticks - dayFirst.Ticks;
		long lQuarters = ticks / (36000000000 / 4);

		return (float)((lQuarters * dxQuarter) + m_rcfDrawing.Left); // (m_dxOffset + m_dxLeftMargin));
	}

	/* C A L C */
	/*----------------------------------------------------------------------------
		%%Function: Calc
		%%Qualified: bg.Grapher.Calc
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void Calc()
	{
		// graph the points in the dataset, m_cgp.nDays at a time
		BGE bge = (BGE)m_slbge.GetByIndex(0);
		DateTime dayFirst = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day);
   
		// split apart the graphing range into intervals, by the quarter hour

		double cxQuarters = m_cgp.nDays * 24 * 4;
		double dxQuarter = (m_nWidth - m_dxOffset / 2) / cxQuarters; // (m_nWidth - (m_dxOffset + m_dxLeftMargin + m_dxRightMargin) - m_dxOffset / 2) / cxQuarters;

		double cyBg = m_cgp.dBgHigh - m_cgp.dBgLow;
		double dyBg = (m_nHeight - m_dyOffset) / cyBg;// (m_nHeight - (m_dyOffset + m_dyTopMargin + m_dyBottomMargin)) / cyBg;

		// build up a set of points to graph
		int iValue = 0;
		m_plptfi = new ArrayList();

		float dxMax = 0;

		while (iValue < m_slbge.Values.Count)
			{
			bge = (BGE)m_slbge.GetByIndex(iValue);

			// set a point at bge
			
			PointF ptf = PtfFromBge(dayFirst, bge, dxQuarter, dyBg);
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

	----------------------------------------------------------------------------*/
	public void Paint(Graphics gr)
	{
		// there are several things that have to happen.

		// 1. Draw the shaded ranges
		// 2. Draw the grid lines
		// 3. Draw the actual points

		// ------------------------------
		// FIRST:  Draw the shaded ranges
		// ------------------------------
		int dxAdjust = (int)(m_iFirstQuarter * m_dxQuarter);	// how much should 0 based x-coordinates be adjusted?
		PTFI ptfi = (PTFI)m_plptfi[0];
		DateTime dttmFirst = new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);

//		RectangleF rectfGraphBodyClip = new RectangleF(m_dxOffset + m_dxLeftMargin, 
//													   m_dyTopMargin, // m_dyOffset + 
//													   ((float)m_nWidth) - m_dxOffset - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_nHeight) - m_dyOffset - m_dyTopMargin - m_dyBottomMargin);

		RectangleF rectfGraphBodyClip = new RectangleF(m_dxOffset + m_rcfDrawing.Left, 
													   m_rcfDrawing.Top, // m_dyOffset + 
													   m_rcfDrawing.Width - m_dxOffset,
													   m_rcfDrawing.Height - m_dyOffset); 
//													   ((float)m_nWidth) - m_dxOffset - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_nHeight) - m_dyOffset - m_dyTopMargin - m_dyBottomMargin);

		RectangleF rectfGraphFullClip = m_rcfDrawing; // new RectangleF(m_dxLeftMargin, 
//													   m_dyTopMargin, 
//													   ((float)m_nWidth) - m_dxLeftMargin - m_dxRightMargin, 
//													   ((float)m_nHeight) - m_dyTopMargin - m_dyBottomMargin);
//		
		gr.SetClip(rectfGraphBodyClip);
		ShadeCurvedRanges(gr, dxAdjust, dttmFirst);
		gr.ResetClip();

		// -----------------------------------------
		// SECOND: Draw the gridlines and the legend
		// -----------------------------------------

		gr.SetClip(rectfGraphFullClip);

		SolidBrush brushBlue = new SolidBrush(Color.Blue);
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
		float yDateLegend = m_rcfDrawing.Bottom - m_dyOffset + dyOffsetRegion * 1.3F; // m_nHeight - m_dyBottomMargin - m_dyOffset + dyOffsetRegion * 1.3F;
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
		gr.DrawLine(penBlue, m_dxOffset + (m_rcfDrawing.Left) - 1.0F, yDateLegend, m_nWidth + m_dxOffset, yDateLegend);
//		gr.DrawLine(penBlue, (m_dxOffset + m_dxLeftMargin) - 1.0F, m_dyTopMargin, (m_dxOffset + m_dxLeftMargin) - 1.0F, yDateLegend);
//		gr.DrawLine(penBlue, (m_dxOffset + m_dxLeftMargin) - 1.0F, yDateLegend, m_nWidth, yDateLegend);
		bool fLine = false;

		for (n = (int)m_cgp.dBgLow; n <= (int)m_cgp.dBgHigh; n += dn)
			{
			float x = m_rcfDrawing.Left /*m_dxLeftMargin*/ + 2.0F;
			float y = YFromReading(n, m_dyPerBgUnit);

			if (fLine)
				gr.DrawLine(penGrid, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/) - 4.0F, y, m_nWidth, y);
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
			PointF ptf = ((PTFI)m_plptfi[i]).ptf;

			// if its before our first point, skip it
			if (ptf.X < dxFirstQuarter)
				continue;

			ptfi = ((PTFI)m_plptfi[i]);
			ptf = ptfi.ptf;

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


	public bool FGetLastDateTimeOnPage(out DateTime dttm)
	{
		DateTime dttmLast = ((PTFI)m_plptfi[m_plptfi.Count - 1]).bge.Date;

		dttm = GetFirstDateTime();

		if (dttm.AddDays(m_cgp.nDays) > dttmLast)
			return false;

		dttm = dttm.AddDays(m_cgp.nDays);
		return true;
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
			hBrushTrans = new SolidBrush(Color.FromArgb(255, 255, 255, 167));
		else
			hBrushTrans = new SolidBrush(Color.FromArgb(255, 215, 215, 215));

		ShadeRanges2(gr, dxAdjust, dttmFirst, hBrushTrans, 10, 20);

		if (m_fColor)
			hBrushTrans = new SolidBrush(Color.FromArgb(255, 174, 255, 174));
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

		gr.FillRectangle(hBrushTrans, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/), yMax, m_nWidth, Math.Abs(yMax - yMin));

		yMin = YFromReading(120, m_dyPerBgUnit);
		yMax = YFromReading(140, m_dyPerBgUnit);

		hBrushTrans = new SolidBrush(Color.FromArgb(64, 255, 255, 0));

		gr.FillRectangle(hBrushTrans, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/), yMax, m_nWidth, Math.Abs(yMax - yMin));

		yMin = YFromReading(60, m_dyPerBgUnit);
		yMax = YFromReading(80, m_dyPerBgUnit);

		gr.FillRectangle(hBrushTrans, (m_dxOffset + m_rcfDrawing.Left/*m_dxLeftMargin*/), yMax, m_nWidth, Math.Abs(yMax - yMin));
	}

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

public class ListViewItemComparer : IComparer
{
	private int m_col;
	private bool m_fReverse;

	/* L I S T  V I E W  I T E M  C O M P A R E R */
	/*----------------------------------------------------------------------------
		%%Function: ListViewItemComparer
		%%Qualified: bg.ListViewItemComparer.ListViewItemComparer
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public ListViewItemComparer()
	{
		m_col = 0;
		m_fReverse = true;
	}

	/* L I S T  V I E W  I T E M  C O M P A R E R */
	/*----------------------------------------------------------------------------
		%%Function: ListViewItemComparer
		%%Qualified: bg.ListViewItemComparer.ListViewItemComparer
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public ListViewItemComparer(int col)
	{
		m_col = col;
		m_fReverse = true;
	}

	/* S E T  C O L U M N */
	/*----------------------------------------------------------------------------
		%%Function: SetColumn
		%%Qualified: bg.ListViewItemComparer.SetColumn
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public void SetColumn(int col)
	{
		if (m_col == col)
			m_fReverse = !m_fReverse;
		else
			m_fReverse = true;
		m_col = col;
	}

	/* C O M P A R E */
	/*----------------------------------------------------------------------------
		%%Function: Compare
		%%Qualified: bg.ListViewItemComparer.Compare
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public int Compare(object x, object y)
	{
		ListViewItem lvi1 = (ListViewItem)x;
		ListViewItem lvi2 = (ListViewItem)y;
		BGE bge1 = (BGE)lvi1.Tag;
		BGE bge2 = (BGE)lvi2.Tag;
		
		if (bge1 == null)
			return -1;

		if (bge2 == null)
			return 1;

		int n = 0;

		switch (m_col)
			{
			case 0:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Date);
				break;
			case 1:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Type);
				break;
			case 2:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Reading);
				break;
			case 3:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Carbs);
				break;
			case 4:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Comment);
				break;
			}
		return m_fReverse ? -n : n;
	}

}

/// <summary>
/// Summary description for Form1.
/// </summary>
public class _bg : System.Windows.Forms.Form
{

	private System.Windows.Forms.TabControl m_tabc;
	private System.Windows.Forms.TabPage m_tabEntry;
	private System.Windows.Forms.TabPage m_tabAnalysis;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.Label label3;
	private System.Windows.Forms.TextBox m_ebDate;
	private System.Windows.Forms.Label label4;
	private System.Windows.Forms.TextBox m_ebTime;
	private System.Windows.Forms.Label label5;
	private System.Windows.Forms.TextBox m_ebReading;
	private System.Windows.Forms.Label label6;
	private System.Windows.Forms.Label label7;
	private System.Windows.Forms.ListView m_lvHistory;
	private SortedList m_slbge;
	private System.Windows.Forms.Button m_pbAdd;
	private System.Windows.Forms.ComboBox m_cbxType;
	private XmlDocument m_dom;
	private XmlNamespaceManager m_nsmgr;
	private System.Windows.Forms.Label label8;
	private System.Windows.Forms.TextBox m_ebCarbs;
	private System.Windows.Forms.Label label9;
	private System.Windows.Forms.TextBox m_ebComment;
	private BGE m_bgeCurrent;
	private System.Windows.Forms.Button m_pbGraph;
	private System.Windows.Forms.CheckBox m_cbSpot;
	private System.Windows.Forms.CheckBox m_cbBreakfast;
	private System.Windows.Forms.CheckBox m_cbLunch;
	private System.Windows.Forms.CheckBox m_cbDinner;
	private System.Windows.Forms.CheckBox m_cbSnack;
	private System.Windows.Forms.Label label12;
	private System.Windows.Forms.Label label14;
	private System.Windows.Forms.TextBox m_ebFastLength;
	private System.Windows.Forms.GroupBox groupBox1;
	private System.Windows.Forms.Label label21;
	private System.Windows.Forms.Label label22;
	private System.Windows.Forms.TextBox m_ebLifetimeAvg;
	private System.Windows.Forms.GroupBox groupBox2;
	private System.Windows.Forms.TextBox m_ebAvg30;
	private System.Windows.Forms.TextBox m_ebFastingAvg;
	private System.Windows.Forms.TextBox m_ebFastingAvg30;
	private System.Windows.Forms.GroupBox groupBox3;
	private System.Windows.Forms.TextBox m_ebFastingAvg15;
	private System.Windows.Forms.TextBox m_ebAvg15;
	private System.Windows.Forms.GroupBox groupBox4;
	private System.Windows.Forms.TextBox m_ebFastingAvg7;
	private System.Windows.Forms.TextBox m_ebAvg7;
	private System.Windows.Forms.Label label28;
	private System.Windows.Forms.Label label29;
	private System.Windows.Forms.CheckBox m_cbShowMeals;
	private System.Windows.Forms.TextBox m_ebIntervals;
	private System.Windows.Forms.TextBox m_ebHigh;
	private System.Windows.Forms.TextBox m_ebLow;
	private System.Windows.Forms.Label label19;
	private System.Windows.Forms.Label label18;
	private System.Windows.Forms.Label label17;
	private System.Windows.Forms.TextBox m_ebDays;
	private System.Windows.Forms.Label label16;
	private System.Windows.Forms.Label label15;
	private System.Windows.Forms.Label label11;
	private System.Windows.Forms.Label label10;
	private System.Windows.Forms.TextBox m_ebLast;
	private System.Windows.Forms.TextBox m_ebFirst;
	private System.Windows.Forms.Label label30;
	private System.Windows.Forms.Label label31;
	private System.Windows.Forms.ComboBox m_cbxUpper;
	private System.Windows.Forms.ComboBox m_cbxLower;
	private System.Windows.Forms.ComboBox m_cbxOrient;
	private System.Windows.Forms.ComboBox m_cbxDateRange;
	private System.Windows.Forms.ComboBox m_cbxFilterType;
	private System.Windows.Forms.Label label13;
	private System.Windows.Forms.Button button1;
	private System.Windows.Forms.Label label34;
	private System.Windows.Forms.Label label35;
	private System.Windows.Forms.Label label20;
	private System.Windows.Forms.Label label23;
	private System.Windows.Forms.Label label36;
	private System.Windows.Forms.Label label37;
	private System.Windows.Forms.Label label24;
	private System.Windows.Forms.Label label25;
	private System.Windows.Forms.Label label32;
	private System.Windows.Forms.Label label33;
	private System.Windows.Forms.Label label26;
	private System.Windows.Forms.Label label27;
	private System.Windows.Forms.Label label38;
	private System.Windows.Forms.Label label39;
	private System.Windows.Forms.TextBox m_ebA1c;
	private System.Windows.Forms.TextBox m_ebAvgWgt;
	private System.Windows.Forms.GroupBox groupBox5;
	private System.Windows.Forms.Label label40;
	private System.Windows.Forms.Label label41;
	private System.Windows.Forms.Label label42;
	private System.Windows.Forms.Label label43;
	private System.Windows.Forms.TextBox m_ebA1c90;
	private System.Windows.Forms.TextBox m_ebWgtAvg90;
	private System.Windows.Forms.TextBox m_ebA1c30;
	private System.Windows.Forms.TextBox m_ebWgtAvg30;
	private System.Windows.Forms.TextBox m_ebA1c15;
	private System.Windows.Forms.TextBox m_ebWgtAvg15;
	private System.Windows.Forms.TextBox m_ebA1c7;
	private System.Windows.Forms.TextBox m_ebWgtAvg7;
	private System.Windows.Forms.TextBox m_ebAvg90;
	private System.Windows.Forms.TextBox m_ebFastingAvg90; 

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public _bg()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		//
		// TODO: Add any constructor code after InitializeComponent call
		//
		m_cbxUpper.SelectedIndex = 1;
		m_cbxLower.SelectedIndex = 0;
		SetupListView(m_lvHistory);
		LoadBgData();

		m_cbxOrient.SelectedIndex = 1;
		m_cbxDateRange.SelectedIndex = 3;
		m_cbxFilterType.SelectedIndex = 0;
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if (components != null) 
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
		this.m_tabc = new System.Windows.Forms.TabControl();
		this.m_tabEntry = new System.Windows.Forms.TabPage();
		this.button1 = new System.Windows.Forms.Button();
		this.m_ebComment = new System.Windows.Forms.TextBox();
		this.label9 = new System.Windows.Forms.Label();
		this.m_ebCarbs = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.m_pbAdd = new System.Windows.Forms.Button();
		this.m_lvHistory = new System.Windows.Forms.ListView();
		this.label7 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.m_ebReading = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_ebTime = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_ebDate = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_cbxType = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.m_tabAnalysis = new System.Windows.Forms.TabPage();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.m_ebA1c90 = new System.Windows.Forms.TextBox();
		this.m_ebWgtAvg90 = new System.Windows.Forms.TextBox();
		this.label40 = new System.Windows.Forms.Label();
		this.label41 = new System.Windows.Forms.Label();
		this.label42 = new System.Windows.Forms.Label();
		this.label43 = new System.Windows.Forms.Label();
		this.m_ebFastingAvg90 = new System.Windows.Forms.TextBox();
		this.m_ebAvg90 = new System.Windows.Forms.TextBox();
		this.label13 = new System.Windows.Forms.Label();
		this.m_cbxDateRange = new System.Windows.Forms.ComboBox();
		this.m_cbxFilterType = new System.Windows.Forms.ComboBox();
		this.m_cbxOrient = new System.Windows.Forms.ComboBox();
		this.m_cbxLower = new System.Windows.Forms.ComboBox();
		this.m_cbxUpper = new System.Windows.Forms.ComboBox();
		this.label31 = new System.Windows.Forms.Label();
		this.label30 = new System.Windows.Forms.Label();
		this.m_cbShowMeals = new System.Windows.Forms.CheckBox();
		this.m_ebIntervals = new System.Windows.Forms.TextBox();
		this.m_ebHigh = new System.Windows.Forms.TextBox();
		this.m_ebLow = new System.Windows.Forms.TextBox();
		this.label19 = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.m_ebDays = new System.Windows.Forms.TextBox();
		this.label16 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.m_ebLast = new System.Windows.Forms.TextBox();
		this.m_ebFirst = new System.Windows.Forms.TextBox();
		this.label29 = new System.Windows.Forms.Label();
		this.label28 = new System.Windows.Forms.Label();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.m_ebA1c7 = new System.Windows.Forms.TextBox();
		this.m_ebWgtAvg7 = new System.Windows.Forms.TextBox();
		this.label26 = new System.Windows.Forms.Label();
		this.label27 = new System.Windows.Forms.Label();
		this.label38 = new System.Windows.Forms.Label();
		this.label39 = new System.Windows.Forms.Label();
		this.m_ebFastingAvg7 = new System.Windows.Forms.TextBox();
		this.m_ebAvg7 = new System.Windows.Forms.TextBox();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.m_ebA1c15 = new System.Windows.Forms.TextBox();
		this.m_ebWgtAvg15 = new System.Windows.Forms.TextBox();
		this.label24 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.label32 = new System.Windows.Forms.Label();
		this.label33 = new System.Windows.Forms.Label();
		this.m_ebFastingAvg15 = new System.Windows.Forms.TextBox();
		this.m_ebAvg15 = new System.Windows.Forms.TextBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.m_ebA1c30 = new System.Windows.Forms.TextBox();
		this.m_ebWgtAvg30 = new System.Windows.Forms.TextBox();
		this.label20 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.label36 = new System.Windows.Forms.Label();
		this.label37 = new System.Windows.Forms.Label();
		this.m_ebFastingAvg30 = new System.Windows.Forms.TextBox();
		this.m_ebAvg30 = new System.Windows.Forms.TextBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.m_ebA1c = new System.Windows.Forms.TextBox();
		this.label35 = new System.Windows.Forms.Label();
		this.m_ebAvgWgt = new System.Windows.Forms.TextBox();
		this.label34 = new System.Windows.Forms.Label();
		this.m_ebFastingAvg = new System.Windows.Forms.TextBox();
		this.m_ebLifetimeAvg = new System.Windows.Forms.TextBox();
		this.label22 = new System.Windows.Forms.Label();
		this.label21 = new System.Windows.Forms.Label();
		this.m_ebFastLength = new System.Windows.Forms.TextBox();
		this.label14 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.m_cbSnack = new System.Windows.Forms.CheckBox();
		this.m_cbDinner = new System.Windows.Forms.CheckBox();
		this.m_cbLunch = new System.Windows.Forms.CheckBox();
		this.m_cbBreakfast = new System.Windows.Forms.CheckBox();
		this.m_cbSpot = new System.Windows.Forms.CheckBox();
		this.m_pbGraph = new System.Windows.Forms.Button();
		this.m_tabc.SuspendLayout();
		this.m_tabEntry.SuspendLayout();
		this.m_tabAnalysis.SuspendLayout();
		this.groupBox5.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.SuspendLayout();
		// 
		// m_tabc
		// 
		this.m_tabc.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right);
		this.m_tabc.Controls.AddRange(new System.Windows.Forms.Control[] {
																			 this.m_tabEntry,
																			 this.m_tabAnalysis});
		this.m_tabc.Location = new System.Drawing.Point(16, 24);
		this.m_tabc.Name = "m_tabc";
		this.m_tabc.SelectedIndex = 0;
		this.m_tabc.Size = new System.Drawing.Size(528, 472);
		this.m_tabc.TabIndex = 0;
		this.m_tabc.SelectedIndexChanged += new System.EventHandler(this.ChangeTabs);
		// 
		// m_tabEntry
		// 
		this.m_tabEntry.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.button1,
																				 this.m_ebComment,
																				 this.label9,
																				 this.m_ebCarbs,
																				 this.label8,
																				 this.m_pbAdd,
																				 this.m_lvHistory,
																				 this.label7,
																				 this.label6,
																				 this.m_ebReading,
																				 this.label5,
																				 this.m_ebTime,
																				 this.label4,
																				 this.m_ebDate,
																				 this.label3,
																				 this.m_cbxType,
																				 this.label2,
																				 this.label1});
		this.m_tabEntry.Location = new System.Drawing.Point(4, 22);
		this.m_tabEntry.Name = "m_tabEntry";
		this.m_tabEntry.Size = new System.Drawing.Size(520, 446);
		this.m_tabEntry.TabIndex = 0;
		this.m_tabEntry.Text = "Data Entry";
		// 
		// button1
		// 
		this.button1.Location = new System.Drawing.Point(416, 56);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(72, 24);
		this.button1.TabIndex = 17;
		this.button1.Text = "Read";
		this.button1.Click += new System.EventHandler(this.ReadFromDevice);
		// 
		// m_ebComment
		// 
		this.m_ebComment.Location = new System.Drawing.Point(72, 101);
		this.m_ebComment.Name = "m_ebComment";
		this.m_ebComment.Size = new System.Drawing.Size(424, 20);
		this.m_ebComment.TabIndex = 13;
		this.m_ebComment.Text = "";
		// 
		// label9
		// 
		this.label9.Location = new System.Drawing.Point(16, 104);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(56, 23);
		this.label9.TabIndex = 12;
		this.label9.Text = "Comment";
		// 
		// m_ebCarbs
		// 
		this.m_ebCarbs.Location = new System.Drawing.Point(200, 37);
		this.m_ebCarbs.Name = "m_ebCarbs";
		this.m_ebCarbs.Size = new System.Drawing.Size(56, 20);
		this.m_ebCarbs.TabIndex = 4;
		this.m_ebCarbs.Text = "";
		// 
		// label8
		// 
		this.label8.Location = new System.Drawing.Point(152, 40);
		this.label8.Name = "label8";
		this.label8.TabIndex = 3;
		this.label8.Text = "Carbs";
		// 
		// m_pbAdd
		// 
		this.m_pbAdd.Location = new System.Drawing.Point(416, 24);
		this.m_pbAdd.Name = "m_pbAdd";
		this.m_pbAdd.TabIndex = 15;
		this.m_pbAdd.Text = "Add";
		this.m_pbAdd.Click += new System.EventHandler(this.AddEntry);
		// 
		// m_lvHistory
		// 
		this.m_lvHistory.LabelEdit = true;
		this.m_lvHistory.Location = new System.Drawing.Point(16, 152);
		this.m_lvHistory.Name = "m_lvHistory";
		this.m_lvHistory.Size = new System.Drawing.Size(488, 288);
		this.m_lvHistory.TabIndex = 16;
		this.m_lvHistory.Click += new System.EventHandler(this.HandleHistoryClick);
		// 
		// label7
		// 
		this.label7.Location = new System.Drawing.Point(8, 128);
		this.label7.Name = "label7";
		this.label7.TabIndex = 14;
		this.label7.Text = "History";
		// 
		// label6
		// 
		this.label6.Location = new System.Drawing.Point(360, 72);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(32, 23);
		this.label6.TabIndex = 11;
		this.label6.Text = "mg/dl";
		// 
		// m_ebReading
		// 
		this.m_ebReading.Location = new System.Drawing.Point(320, 67);
		this.m_ebReading.Name = "m_ebReading";
		this.m_ebReading.Size = new System.Drawing.Size(40, 20);
		this.m_ebReading.TabIndex = 10;
		this.m_ebReading.Text = "";
		// 
		// label5
		// 
		this.label5.Location = new System.Drawing.Point(264, 72);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(48, 23);
		this.label5.TabIndex = 9;
		this.label5.Text = "Reading";
		// 
		// m_ebTime
		// 
		this.m_ebTime.Location = new System.Drawing.Point(200, 69);
		this.m_ebTime.Name = "m_ebTime";
		this.m_ebTime.Size = new System.Drawing.Size(56, 20);
		this.m_ebTime.TabIndex = 8;
		this.m_ebTime.Text = "";
		// 
		// label4
		// 
		this.label4.Location = new System.Drawing.Point(152, 72);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(32, 23);
		this.label4.TabIndex = 7;
		this.label4.Text = "Time";
		// 
		// m_ebDate
		// 
		this.m_ebDate.Location = new System.Drawing.Point(56, 69);
		this.m_ebDate.Name = "m_ebDate";
		this.m_ebDate.Size = new System.Drawing.Size(80, 20);
		this.m_ebDate.TabIndex = 6;
		this.m_ebDate.Text = "";
		// 
		// label3
		// 
		this.label3.Location = new System.Drawing.Point(16, 72);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(32, 23);
		this.label3.TabIndex = 5;
		this.label3.Text = "Date";
		// 
		// m_cbxType
		// 
		this.m_cbxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxType.Items.AddRange(new object[] {
													   "Breakfast",
													   "Lunch",
													   "Dinner",
													   "Snack",
													   "SpotTest"});
		this.m_cbxType.Location = new System.Drawing.Point(56, 37);
		this.m_cbxType.Name = "m_cbxType";
		this.m_cbxType.Size = new System.Drawing.Size(80, 21);
		this.m_cbxType.TabIndex = 2;
		// 
		// label2
		// 
		this.label2.Location = new System.Drawing.Point(16, 40);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(40, 16);
		this.label2.TabIndex = 1;
		this.label2.Text = "Type";
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(8, 8);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(88, 16);
		this.label1.TabIndex = 0;
		this.label1.Text = "Reading Entry";
		// 
		// m_tabAnalysis
		// 
		this.m_tabAnalysis.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.groupBox5,
																					this.label13,
																					this.m_cbxDateRange,
																					this.m_cbxFilterType,
																					this.m_cbxOrient,
																					this.m_cbxLower,
																					this.m_cbxUpper,
																					this.label31,
																					this.label30,
																					this.m_cbShowMeals,
																					this.m_ebIntervals,
																					this.m_ebHigh,
																					this.m_ebLow,
																					this.label19,
																					this.label18,
																					this.label17,
																					this.m_ebDays,
																					this.label16,
																					this.label15,
																					this.label11,
																					this.label10,
																					this.m_ebLast,
																					this.m_ebFirst,
																					this.label29,
																					this.label28,
																					this.groupBox4,
																					this.groupBox3,
																					this.groupBox2,
																					this.groupBox1,
																					this.m_ebFastLength,
																					this.label14,
																					this.label12,
																					this.m_cbSnack,
																					this.m_cbDinner,
																					this.m_cbLunch,
																					this.m_cbBreakfast,
																					this.m_cbSpot,
																					this.m_pbGraph});
		this.m_tabAnalysis.Location = new System.Drawing.Point(4, 22);
		this.m_tabAnalysis.Name = "m_tabAnalysis";
		this.m_tabAnalysis.Size = new System.Drawing.Size(520, 446);
		this.m_tabAnalysis.TabIndex = 1;
		this.m_tabAnalysis.Text = "Analysis";
		// 
		// groupBox5
		// 
		this.groupBox5.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebA1c90,
																				this.m_ebWgtAvg90,
																				this.label40,
																				this.label41,
																				this.label42,
																				this.label43,
																				this.m_ebFastingAvg90,
																				this.m_ebAvg90});
		this.groupBox5.Location = new System.Drawing.Point(152, 304);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(136, 64);
		this.groupBox5.TabIndex = 43;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "90 Day Averages";
		this.groupBox5.Enter += new System.EventHandler(this.groupBox5_Enter);
		// 
		// m_ebA1c90
		// 
		this.m_ebA1c90.Location = new System.Drawing.Point(104, 32);
		this.m_ebA1c90.Name = "m_ebA1c90";
		this.m_ebA1c90.Size = new System.Drawing.Size(24, 20);
		this.m_ebA1c90.TabIndex = 12;
		this.m_ebA1c90.Text = "5.5";
		// 
		// m_ebWgtAvg90
		// 
		this.m_ebWgtAvg90.Location = new System.Drawing.Point(72, 32);
		this.m_ebWgtAvg90.Name = "m_ebWgtAvg90";
		this.m_ebWgtAvg90.Size = new System.Drawing.Size(24, 20);
		this.m_ebWgtAvg90.TabIndex = 11;
		this.m_ebWgtAvg90.Text = "444";
		// 
		// label40
		// 
		this.label40.Location = new System.Drawing.Point(104, 16);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(24, 16);
		this.label40.TabIndex = 10;
		this.label40.Text = "a1c";
		// 
		// label41
		// 
		this.label41.Location = new System.Drawing.Point(72, 16);
		this.label41.Name = "label41";
		this.label41.Size = new System.Drawing.Size(24, 16);
		this.label41.TabIndex = 9;
		this.label41.Text = "Wgt";
		// 
		// label42
		// 
		this.label42.Location = new System.Drawing.Point(40, 16);
		this.label42.Name = "label42";
		this.label42.Size = new System.Drawing.Size(32, 16);
		this.label42.TabIndex = 8;
		this.label42.Text = "Fast";
		// 
		// label43
		// 
		this.label43.Location = new System.Drawing.Point(16, 16);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(24, 16);
		this.label43.TabIndex = 7;
		this.label43.Text = "All";
		// 
		// m_ebFastingAvg90
		// 
		this.m_ebFastingAvg90.Location = new System.Drawing.Point(40, 32);
		this.m_ebFastingAvg90.Name = "m_ebFastingAvg90";
		this.m_ebFastingAvg90.Size = new System.Drawing.Size(24, 20);
		this.m_ebFastingAvg90.TabIndex = 3;
		this.m_ebFastingAvg90.Text = "444";
		// 
		// m_ebAvg90
		// 
		this.m_ebAvg90.Location = new System.Drawing.Point(8, 32);
		this.m_ebAvg90.Name = "m_ebAvg90";
		this.m_ebAvg90.Size = new System.Drawing.Size(24, 20);
		this.m_ebAvg90.TabIndex = 1;
		this.m_ebAvg90.Text = "444";
		// 
		// label13
		// 
		this.label13.Location = new System.Drawing.Point(8, 200);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(496, 16);
		this.label13.TabIndex = 42;
		this.label13.Text = "Graph Options";
		this.label13.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// m_cbxDateRange
		// 
		this.m_cbxDateRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxDateRange.Items.AddRange(new object[] {
															"7 Days",
															"15 Days",
															"30 Days",
															"Custom"});
		this.m_cbxDateRange.Location = new System.Drawing.Point(232, 104);
		this.m_cbxDateRange.Name = "m_cbxDateRange";
		this.m_cbxDateRange.Size = new System.Drawing.Size(88, 21);
		this.m_cbxDateRange.TabIndex = 41;
		this.m_cbxDateRange.SelectedIndexChanged += new System.EventHandler(this.SelectDateRange);
		// 
		// m_cbxFilterType
		// 
		this.m_cbxFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxFilterType.Items.AddRange(new object[] {
															 "Custom",
															 "Fasting"});
		this.m_cbxFilterType.Location = new System.Drawing.Point(384, 56);
		this.m_cbxFilterType.Name = "m_cbxFilterType";
		this.m_cbxFilterType.Size = new System.Drawing.Size(128, 21);
		this.m_cbxFilterType.TabIndex = 40;
		// 
		// m_cbxOrient
		// 
		this.m_cbxOrient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxOrient.Items.AddRange(new object[] {
														 "Portrait",
														 "Landscape"});
		this.m_cbxOrient.Location = new System.Drawing.Point(16, 160);
		this.m_cbxOrient.Name = "m_cbxOrient";
		this.m_cbxOrient.Size = new System.Drawing.Size(128, 21);
		this.m_cbxOrient.TabIndex = 39;
		// 
		// m_cbxLower
		// 
		this.m_cbxLower.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxLower.Items.AddRange(new object[] {
														"Empty",
														"Graph",
														"Log"});
		this.m_cbxLower.Location = new System.Drawing.Point(352, 160);
		this.m_cbxLower.Name = "m_cbxLower";
		this.m_cbxLower.Size = new System.Drawing.Size(72, 21);
		this.m_cbxLower.TabIndex = 38;
		// 
		// m_cbxUpper
		// 
		this.m_cbxUpper.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxUpper.Items.AddRange(new object[] {
														"Empty",
														"Graph",
														"Log"});
		this.m_cbxUpper.Location = new System.Drawing.Point(216, 160);
		this.m_cbxUpper.Name = "m_cbxUpper";
		this.m_cbxUpper.Size = new System.Drawing.Size(72, 21);
		this.m_cbxUpper.TabIndex = 37;
		// 
		// label31
		// 
		this.label31.Location = new System.Drawing.Point(288, 164);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(64, 23);
		this.label31.TabIndex = 36;
		this.label31.Text = "Lower Box";
		// 
		// label30
		// 
		this.label30.Location = new System.Drawing.Point(152, 164);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(64, 23);
		this.label30.TabIndex = 35;
		this.label30.Text = "Upper Box";
		// 
		// m_cbShowMeals
		// 
		this.m_cbShowMeals.Checked = true;
		this.m_cbShowMeals.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbShowMeals.Location = new System.Drawing.Point(16, 248);
		this.m_cbShowMeals.Name = "m_cbShowMeals";
		this.m_cbShowMeals.Size = new System.Drawing.Size(128, 24);
		this.m_cbShowMeals.TabIndex = 19;
		this.m_cbShowMeals.Text = "Connect meal points";
		// 
		// m_ebIntervals
		// 
		this.m_ebIntervals.Location = new System.Drawing.Point(304, 224);
		this.m_ebIntervals.Name = "m_ebIntervals";
		this.m_ebIntervals.Size = new System.Drawing.Size(48, 20);
		this.m_ebIntervals.TabIndex = 29;
		this.m_ebIntervals.Text = "19";
		// 
		// m_ebHigh
		// 
		this.m_ebHigh.Location = new System.Drawing.Point(192, 224);
		this.m_ebHigh.Name = "m_ebHigh";
		this.m_ebHigh.Size = new System.Drawing.Size(48, 20);
		this.m_ebHigh.TabIndex = 27;
		this.m_ebHigh.Text = "220";
		// 
		// m_ebLow
		// 
		this.m_ebLow.Location = new System.Drawing.Point(72, 224);
		this.m_ebLow.Name = "m_ebLow";
		this.m_ebLow.Size = new System.Drawing.Size(48, 20);
		this.m_ebLow.TabIndex = 25;
		this.m_ebLow.Text = "30";
		// 
		// label19
		// 
		this.label19.Location = new System.Drawing.Point(248, 227);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(48, 23);
		this.label19.TabIndex = 28;
		this.label19.Text = "Intervals";
		// 
		// label18
		// 
		this.label18.Location = new System.Drawing.Point(136, 227);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(48, 23);
		this.label18.TabIndex = 26;
		this.label18.Text = "High bg";
		// 
		// label17
		// 
		this.label17.Location = new System.Drawing.Point(16, 227);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(48, 23);
		this.label17.TabIndex = 24;
		this.label17.Text = "Low bg";
		// 
		// m_ebDays
		// 
		this.m_ebDays.Location = new System.Drawing.Point(448, 224);
		this.m_ebDays.Name = "m_ebDays";
		this.m_ebDays.Size = new System.Drawing.Size(24, 20);
		this.m_ebDays.TabIndex = 18;
		this.m_ebDays.Text = "7";
		// 
		// label16
		// 
		this.label16.Location = new System.Drawing.Point(360, 227);
		this.label16.Name = "label16";
		this.label16.TabIndex = 17;
		this.label16.Text = "Days per Page";
		// 
		// label15
		// 
		this.label15.Location = new System.Drawing.Point(8, 136);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(496, 16);
		this.label15.TabIndex = 12;
		this.label15.Text = "Report Options";
		this.label15.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// label11
		// 
		this.label11.Location = new System.Drawing.Point(128, 104);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(32, 23);
		this.label11.TabIndex = 15;
		this.label11.Text = "To";
		// 
		// label10
		// 
		this.label10.Location = new System.Drawing.Point(16, 104);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(40, 23);
		this.label10.TabIndex = 13;
		this.label10.Text = "From";
		// 
		// m_ebLast
		// 
		this.m_ebLast.Location = new System.Drawing.Point(160, 104);
		this.m_ebLast.Name = "m_ebLast";
		this.m_ebLast.Size = new System.Drawing.Size(56, 20);
		this.m_ebLast.TabIndex = 16;
		this.m_ebLast.Text = "";
		// 
		// m_ebFirst
		// 
		this.m_ebFirst.Location = new System.Drawing.Point(56, 104);
		this.m_ebFirst.Name = "m_ebFirst";
		this.m_ebFirst.Size = new System.Drawing.Size(48, 20);
		this.m_ebFirst.TabIndex = 14;
		this.m_ebFirst.Text = "";
		// 
		// label29
		// 
		this.label29.Location = new System.Drawing.Point(472, 86);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(40, 16);
		this.label29.TabIndex = 10;
		this.label29.Text = "hours";
		// 
		// label28
		// 
		this.label28.Location = new System.Drawing.Point(8, 280);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(496, 16);
		this.label28.TabIndex = 30;
		this.label28.Text = "Statistics";
		this.label28.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// groupBox4
		// 
		this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebA1c7,
																				this.m_ebWgtAvg7,
																				this.label26,
																				this.label27,
																				this.label38,
																				this.label39,
																				this.m_ebFastingAvg7,
																				this.m_ebAvg7});
		this.groupBox4.Location = new System.Drawing.Point(152, 376);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(136, 64);
		this.groupBox4.TabIndex = 0;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "7 Day Averages";
		// 
		// m_ebA1c7
		// 
		this.m_ebA1c7.Location = new System.Drawing.Point(104, 32);
		this.m_ebA1c7.Name = "m_ebA1c7";
		this.m_ebA1c7.Size = new System.Drawing.Size(24, 20);
		this.m_ebA1c7.TabIndex = 16;
		this.m_ebA1c7.Text = "5.5";
		// 
		// m_ebWgtAvg7
		// 
		this.m_ebWgtAvg7.Location = new System.Drawing.Point(72, 32);
		this.m_ebWgtAvg7.Name = "m_ebWgtAvg7";
		this.m_ebWgtAvg7.Size = new System.Drawing.Size(24, 20);
		this.m_ebWgtAvg7.TabIndex = 15;
		this.m_ebWgtAvg7.Text = "444";
		// 
		// label26
		// 
		this.label26.Location = new System.Drawing.Point(104, 16);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(24, 16);
		this.label26.TabIndex = 14;
		this.label26.Text = "a1c";
		// 
		// label27
		// 
		this.label27.Location = new System.Drawing.Point(72, 16);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(24, 16);
		this.label27.TabIndex = 13;
		this.label27.Text = "Wgt";
		// 
		// label38
		// 
		this.label38.Location = new System.Drawing.Point(40, 16);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(32, 16);
		this.label38.TabIndex = 12;
		this.label38.Text = "Fast";
		// 
		// label39
		// 
		this.label39.Location = new System.Drawing.Point(16, 16);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(24, 16);
		this.label39.TabIndex = 11;
		this.label39.Text = "All";
		// 
		// m_ebFastingAvg7
		// 
		this.m_ebFastingAvg7.Location = new System.Drawing.Point(40, 32);
		this.m_ebFastingAvg7.Name = "m_ebFastingAvg7";
		this.m_ebFastingAvg7.Size = new System.Drawing.Size(24, 20);
		this.m_ebFastingAvg7.TabIndex = 2;
		this.m_ebFastingAvg7.Text = "textBox2";
		// 
		// m_ebAvg7
		// 
		this.m_ebAvg7.Location = new System.Drawing.Point(8, 32);
		this.m_ebAvg7.Name = "m_ebAvg7";
		this.m_ebAvg7.Size = new System.Drawing.Size(24, 20);
		this.m_ebAvg7.TabIndex = 0;
		this.m_ebAvg7.Text = "m_ebAvg30";
		// 
		// groupBox3
		// 
		this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebA1c15,
																				this.m_ebWgtAvg15,
																				this.label24,
																				this.label25,
																				this.label32,
																				this.label33,
																				this.m_ebFastingAvg15,
																				this.m_ebAvg15});
		this.groupBox3.Location = new System.Drawing.Point(8, 376);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(136, 64);
		this.groupBox3.TabIndex = 33;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "15 Day Averages";
		// 
		// m_ebA1c15
		// 
		this.m_ebA1c15.Location = new System.Drawing.Point(104, 32);
		this.m_ebA1c15.Name = "m_ebA1c15";
		this.m_ebA1c15.Size = new System.Drawing.Size(24, 20);
		this.m_ebA1c15.TabIndex = 16;
		this.m_ebA1c15.Text = "5.5";
		// 
		// m_ebWgtAvg15
		// 
		this.m_ebWgtAvg15.Location = new System.Drawing.Point(72, 32);
		this.m_ebWgtAvg15.Name = "m_ebWgtAvg15";
		this.m_ebWgtAvg15.Size = new System.Drawing.Size(24, 20);
		this.m_ebWgtAvg15.TabIndex = 15;
		this.m_ebWgtAvg15.Text = "444";
		// 
		// label24
		// 
		this.label24.Location = new System.Drawing.Point(104, 16);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(24, 16);
		this.label24.TabIndex = 14;
		this.label24.Text = "a1c";
		// 
		// label25
		// 
		this.label25.Location = new System.Drawing.Point(72, 16);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(24, 16);
		this.label25.TabIndex = 13;
		this.label25.Text = "Wgt";
		// 
		// label32
		// 
		this.label32.Location = new System.Drawing.Point(40, 16);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(32, 16);
		this.label32.TabIndex = 12;
		this.label32.Text = "Fast";
		// 
		// label33
		// 
		this.label33.Location = new System.Drawing.Point(16, 16);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(24, 16);
		this.label33.TabIndex = 11;
		this.label33.Text = "All";
		// 
		// m_ebFastingAvg15
		// 
		this.m_ebFastingAvg15.Location = new System.Drawing.Point(40, 32);
		this.m_ebFastingAvg15.Name = "m_ebFastingAvg15";
		this.m_ebFastingAvg15.Size = new System.Drawing.Size(24, 20);
		this.m_ebFastingAvg15.TabIndex = 3;
		this.m_ebFastingAvg15.Text = "444";
		// 
		// m_ebAvg15
		// 
		this.m_ebAvg15.Location = new System.Drawing.Point(8, 32);
		this.m_ebAvg15.Name = "m_ebAvg15";
		this.m_ebAvg15.Size = new System.Drawing.Size(24, 20);
		this.m_ebAvg15.TabIndex = 1;
		this.m_ebAvg15.Text = "444";
		// 
		// groupBox2
		// 
		this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebA1c30,
																				this.m_ebWgtAvg30,
																				this.label20,
																				this.label23,
																				this.label36,
																				this.label37,
																				this.m_ebFastingAvg30,
																				this.m_ebAvg30});
		this.groupBox2.Location = new System.Drawing.Point(304, 304);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(136, 64);
		this.groupBox2.TabIndex = 32;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "30 Day Averages";
		// 
		// m_ebA1c30
		// 
		this.m_ebA1c30.Location = new System.Drawing.Point(104, 32);
		this.m_ebA1c30.Name = "m_ebA1c30";
		this.m_ebA1c30.Size = new System.Drawing.Size(24, 20);
		this.m_ebA1c30.TabIndex = 12;
		this.m_ebA1c30.Text = "5.5";
		// 
		// m_ebWgtAvg30
		// 
		this.m_ebWgtAvg30.Location = new System.Drawing.Point(72, 32);
		this.m_ebWgtAvg30.Name = "m_ebWgtAvg30";
		this.m_ebWgtAvg30.Size = new System.Drawing.Size(24, 20);
		this.m_ebWgtAvg30.TabIndex = 11;
		this.m_ebWgtAvg30.Text = "444";
		// 
		// label20
		// 
		this.label20.Location = new System.Drawing.Point(104, 16);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(24, 16);
		this.label20.TabIndex = 10;
		this.label20.Text = "a1c";
		// 
		// label23
		// 
		this.label23.Location = new System.Drawing.Point(72, 16);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(24, 16);
		this.label23.TabIndex = 9;
		this.label23.Text = "Wgt";
		// 
		// label36
		// 
		this.label36.Location = new System.Drawing.Point(40, 16);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(32, 16);
		this.label36.TabIndex = 8;
		this.label36.Text = "Fast";
		// 
		// label37
		// 
		this.label37.Location = new System.Drawing.Point(16, 16);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(24, 16);
		this.label37.TabIndex = 7;
		this.label37.Text = "All";
		// 
		// m_ebFastingAvg30
		// 
		this.m_ebFastingAvg30.Location = new System.Drawing.Point(40, 32);
		this.m_ebFastingAvg30.Name = "m_ebFastingAvg30";
		this.m_ebFastingAvg30.Size = new System.Drawing.Size(24, 20);
		this.m_ebFastingAvg30.TabIndex = 3;
		this.m_ebFastingAvg30.Text = "444";
		// 
		// m_ebAvg30
		// 
		this.m_ebAvg30.Location = new System.Drawing.Point(8, 32);
		this.m_ebAvg30.Name = "m_ebAvg30";
		this.m_ebAvg30.Size = new System.Drawing.Size(24, 20);
		this.m_ebAvg30.TabIndex = 1;
		this.m_ebAvg30.Text = "444";
		// 
		// groupBox1
		// 
		this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebA1c,
																				this.label35,
																				this.m_ebAvgWgt,
																				this.label34,
																				this.m_ebFastingAvg,
																				this.m_ebLifetimeAvg,
																				this.label22,
																				this.label21});
		this.groupBox1.Location = new System.Drawing.Point(8, 304);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(136, 64);
		this.groupBox1.TabIndex = 31;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Lifetime Averages";
		// 
		// m_ebA1c
		// 
		this.m_ebA1c.Location = new System.Drawing.Point(104, 32);
		this.m_ebA1c.Name = "m_ebA1c";
		this.m_ebA1c.Size = new System.Drawing.Size(24, 20);
		this.m_ebA1c.TabIndex = 7;
		this.m_ebA1c.Text = "5.5";
		// 
		// label35
		// 
		this.label35.Location = new System.Drawing.Point(104, 16);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(24, 16);
		this.label35.TabIndex = 6;
		this.label35.Text = "a1c";
		// 
		// m_ebAvgWgt
		// 
		this.m_ebAvgWgt.Location = new System.Drawing.Point(72, 32);
		this.m_ebAvgWgt.Name = "m_ebAvgWgt";
		this.m_ebAvgWgt.Size = new System.Drawing.Size(24, 20);
		this.m_ebAvgWgt.TabIndex = 5;
		this.m_ebAvgWgt.Text = "444";
		// 
		// label34
		// 
		this.label34.Location = new System.Drawing.Point(72, 16);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(24, 16);
		this.label34.TabIndex = 4;
		this.label34.Text = "Wgt";
		// 
		// m_ebFastingAvg
		// 
		this.m_ebFastingAvg.Location = new System.Drawing.Point(40, 32);
		this.m_ebFastingAvg.Name = "m_ebFastingAvg";
		this.m_ebFastingAvg.Size = new System.Drawing.Size(24, 20);
		this.m_ebFastingAvg.TabIndex = 3;
		this.m_ebFastingAvg.Text = "444";
		// 
		// m_ebLifetimeAvg
		// 
		this.m_ebLifetimeAvg.Location = new System.Drawing.Point(8, 32);
		this.m_ebLifetimeAvg.Name = "m_ebLifetimeAvg";
		this.m_ebLifetimeAvg.Size = new System.Drawing.Size(24, 20);
		this.m_ebLifetimeAvg.TabIndex = 1;
		this.m_ebLifetimeAvg.Text = "444";
		// 
		// label22
		// 
		this.label22.Location = new System.Drawing.Point(40, 16);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(32, 16);
		this.label22.TabIndex = 2;
		this.label22.Text = "Fast";
		// 
		// label21
		// 
		this.label21.Location = new System.Drawing.Point(16, 16);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(24, 16);
		this.label21.TabIndex = 0;
		this.label21.Text = "All";
		// 
		// m_ebFastLength
		// 
		this.m_ebFastLength.Location = new System.Drawing.Point(432, 83);
		this.m_ebFastLength.Name = "m_ebFastLength";
		this.m_ebFastLength.Size = new System.Drawing.Size(32, 20);
		this.m_ebFastLength.TabIndex = 9;
		this.m_ebFastLength.Text = "7";
		// 
		// label14
		// 
		this.label14.Location = new System.Drawing.Point(360, 86);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(96, 24);
		this.label14.TabIndex = 8;
		this.label14.Text = "Fast Length:";
		// 
		// label12
		// 
		this.label12.Location = new System.Drawing.Point(8, 32);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(496, 23);
		this.label12.TabIndex = 0;
		this.label12.Text = "Filter Options";
		this.label12.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// m_cbSnack
		// 
		this.m_cbSnack.Checked = true;
		this.m_cbSnack.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbSnack.Location = new System.Drawing.Point(200, 56);
		this.m_cbSnack.Name = "m_cbSnack";
		this.m_cbSnack.TabIndex = 3;
		this.m_cbSnack.Text = "Snack";
		// 
		// m_cbDinner
		// 
		this.m_cbDinner.Checked = true;
		this.m_cbDinner.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbDinner.Location = new System.Drawing.Point(128, 72);
		this.m_cbDinner.Name = "m_cbDinner";
		this.m_cbDinner.TabIndex = 5;
		this.m_cbDinner.Text = "Dinner";
		// 
		// m_cbLunch
		// 
		this.m_cbLunch.Checked = true;
		this.m_cbLunch.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbLunch.Location = new System.Drawing.Point(128, 56);
		this.m_cbLunch.Name = "m_cbLunch";
		this.m_cbLunch.TabIndex = 2;
		this.m_cbLunch.Text = "Lunch";
		// 
		// m_cbBreakfast
		// 
		this.m_cbBreakfast.Checked = true;
		this.m_cbBreakfast.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbBreakfast.Location = new System.Drawing.Point(16, 72);
		this.m_cbBreakfast.Name = "m_cbBreakfast";
		this.m_cbBreakfast.TabIndex = 4;
		this.m_cbBreakfast.Text = "Breakfast";
		// 
		// m_cbSpot
		// 
		this.m_cbSpot.Checked = true;
		this.m_cbSpot.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbSpot.Location = new System.Drawing.Point(16, 56);
		this.m_cbSpot.Name = "m_cbSpot";
		this.m_cbSpot.TabIndex = 1;
		this.m_cbSpot.Text = "SpotTests";
		// 
		// m_pbGraph
		// 
		this.m_pbGraph.Location = new System.Drawing.Point(440, 8);
		this.m_pbGraph.Name = "m_pbGraph";
		this.m_pbGraph.Size = new System.Drawing.Size(72, 24);
		this.m_pbGraph.TabIndex = 6;
		this.m_pbGraph.Text = "&Generate";
		this.m_pbGraph.Click += new System.EventHandler(this.DoGraph);
		// 
		// _bg
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(552, 518);
		this.Controls.AddRange(new System.Windows.Forms.Control[] {
																	  this.m_tabc});
		this.Name = "_bg";
		this.Text = "bgGraph";
		this.m_tabc.ResumeLayout(false);
		this.m_tabEntry.ResumeLayout(false);
		this.m_tabAnalysis.ResumeLayout(false);
		this.groupBox5.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox3.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		this.ResumeLayout(false);

	}
	#endregion

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new _bg());
	}

	/* S E T U P  L I S T  V I E W */
	/*----------------------------------------------------------------------------
		%%Function: SetupListView
		%%Qualified: bg._bg.SetupListView
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void SetupListView(ListView lv)
	{
		lv.Columns.Add(new ColumnHeader());
		lv.Columns[0].Text = "Date";
		lv.Columns[0].Width = 128;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[1].Text = "Type";
		lv.Columns[1].Width = 128;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[2].Text = "bg (mg/dl)";
		lv.Columns[2].Width = 32;
		lv.Columns[2].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[3].Text = "Carbs";
		lv.Columns[3].Width = 48;
		lv.Columns[3].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[4].Text = "Comment";
		lv.Columns[4].Width = 256;


		lv.FullRowSelect = true;
		lv.MultiSelect = false;
		lv.View = View.Details;

		m_lvHistory.ListViewItemSorter = new ListViewItemComparer(0);
		m_lvHistory.ColumnClick += new ColumnClickEventHandler(HandleColumn);
		AddBge(null);
	}

	/* I N I T  R E A D I N G S */
	/*----------------------------------------------------------------------------
		%%Function: InitReadings
		%%Qualified: bg._bg.InitReadings
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void InitReadings()
	{
		m_slbge = new SortedList();
	}

	/* L O A D  B G  D A T A */
	/*----------------------------------------------------------------------------
		%%Function: LoadBgData
		%%Qualified: bg._bg.LoadBgData
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void LoadBgData()
	{
		XmlDocument dom = new XmlDocument();

		InitReadings();

		dom.Load("c:\\docs\\bg.xml");

		XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);

		nsmgr.AddNamespace("b", "http://www.thetasoft.com/schemas/bg");

		XmlNodeList nodes = dom.SelectNodes("//b:reading", nsmgr);

		if (nodes != null && nodes.Count > 0)
			{
			foreach (XmlNode nodeT in nodes)
				{
				BGE bge;

				string sDate, sTime, sType, sComment;
				int nBg, nCarbs;
				BGE.ReadingType type;

				sDate = nodeT.SelectSingleNode("b:date", nsmgr).InnerText;
				sTime = nodeT.SelectSingleNode("b:time", nsmgr).InnerText;
				sType = nodeT.SelectSingleNode("@type").Value;
				try
				{
					sComment = nodeT.SelectSingleNode("b:comment", nsmgr).InnerText;
				} catch { sComment = ""; };

				try
				{
					nCarbs = Int32.Parse(nodeT.SelectSingleNode("b:carbs", nsmgr).InnerText);
				} catch { nCarbs = 0; };

				try
				{
					nBg = Int32.Parse(nodeT.SelectSingleNode("b:bg", nsmgr).InnerText);
				} catch { nBg = 0; }
				type = BGE.ReadingTypeFromString(sType);
				
				bge = new BGE(sDate, sTime, type, nBg, nCarbs, sComment);
				m_slbge.Add(bge.Date.ToString("s"), bge);
				}
			}

		foreach (BGE bge in m_slbge.Values)
			{
			AddBge(bge);
			}

		m_dom = dom;
		m_nsmgr = nsmgr;
	}

	void UpdateBge(BGE bge, ListViewItem lvi)
	{
		lvi.Tag = bge;

		if (bge == null)
			{
			lvi.SubItems[0].Text = "* (New Entry)";
			}
		else
			{
			lvi.SubItems[4].Text = bge.Comment;
			if (bge.Carbs != 0)
				lvi.SubItems[3].Text = bge.Carbs.ToString();
			lvi.SubItems[2].Text = bge.Reading.ToString();
			lvi.SubItems[1].Text = BGE.StringFromReadingType(bge.Type);
			lvi.SubItems[0].Text = bge.Date.ToString();
			if (bge.Type == BGE.ReadingType.New)
				lvi.BackColor = Color.Yellow;
			else
				lvi.BackColor = m_lvHistory.BackColor;
			}
	}

	/* A D D  B G E */
	/*----------------------------------------------------------------------------
		%%Function: AddBge
		%%Qualified: bg._bg.AddBge
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void AddBge(BGE bge)
	{
		ListViewItem lvi = new ListViewItem();

		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		if (bge != null && bge.Type == BGE.ReadingType.New)
			lvi.BackColor = Color.Yellow;
		m_lvHistory.Items.Add(lvi);
		UpdateBge(bge, lvi);
	}

	void AddEntryCore(BGE bge, bool fFromDevice)
	{
		BGE bgeCurrent = m_bgeCurrent;

		if (fFromDevice)
			{
			int iKey = m_slbge.IndexOfKey(bge.Date.ToString("s"));

			if (iKey >= 0)
				{
				bgeCurrent = (BGE)m_slbge.GetByIndex(iKey);

				if (bgeCurrent.Reading == bge.Reading)
					// readings match as does the date; nothing to do
					return;

				MessageBox.Show("Conflicting entry read from device!  bgeOld.Reading = " + bgeCurrent.Reading.ToString() + ", bgeNew.Reading = " + bge.Reading.ToString());
				return;
				}
			// no match yet...look for a "close" match
			DateTime dttm = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day, bge.Date.Hour, bge.Date.Minute, 0);
			iKey = m_slbge.IndexOfKey(dttm.ToString("s"));
			if (iKey >= 0)
				{
				//ooh a match.  update this one if the readings match
				bgeCurrent = (BGE)m_slbge.GetByIndex(iKey);

				if (bgeCurrent.Reading != bge.Reading)
					{
					MessageBox.Show("Conflicting entry read from device!  bgeOld.Reading = " + bgeCurrent.Reading.ToString() + ", bgeNew.Reading = " + bge.Reading.ToString());
					return;
					}
				// otherwise, fallthrough and do the update
				bge.Comment = bgeCurrent.Comment;
				bge.Type = bgeCurrent.Type;
				}
			}

		XmlNode nodeReading = m_dom.CreateElement("", "reading", "http://www.thetasoft.com/schemas/bg");
		XmlNode nodeBg = m_dom.CreateElement("", "bg", "http://www.thetasoft.com/schemas/bg");
		XmlNode nodeDate = m_dom.CreateElement("", "date", "http://www.thetasoft.com/schemas/bg");
		XmlNode nodeTime = m_dom.CreateElement("", "time", "http://www.thetasoft.com/schemas/bg");

		XmlNode nodeComment = null;
		XmlNode nodeCarbs = null;

		if (m_ebCarbs.Text.Length > 0)
			nodeCarbs = m_dom.CreateElement("", "carbs", "http://www.thetasoft.com/schemas/bg");

		if (m_ebComment.Text.Length > 0)
			nodeComment = m_dom.CreateElement("", "comment", "http://www.thetasoft.com/schemas/bg");

		nodeBg.InnerText = bge.Reading.ToString();
		nodeDate.InnerText = bge.Date.ToString("d");
		nodeTime.InnerText = bge.Date.ToString("T", DateTimeFormatInfo.InvariantInfo);
		nodeReading.AppendChild(nodeDate);
		nodeReading.AppendChild(nodeTime);
		nodeReading.AppendChild(nodeBg);
		if (nodeCarbs != null)
			{
			nodeCarbs.InnerText = bge.Carbs.ToString();
			nodeReading.AppendChild(nodeCarbs);
			}

		if (nodeComment != null)
			{
			nodeComment.InnerText = bge.Comment;
			nodeReading.AppendChild(nodeComment);
			}

		nodeReading.Attributes.Append(m_dom.CreateAttribute("type"));
		nodeReading.Attributes["type"].Value = BGE.StringFromReadingType(bge.Type);

		if (bgeCurrent == null)
			{
			int iKey;
			if ((iKey = m_slbge.IndexOfKey(bge.Date.ToString("s"))) >= 0)
				{
				if (!fFromDevice)
					{
					MessageBox.Show("Duplicate entry detected!");
					return;
					}

				BGE bgeOld = (BGE)m_slbge.GetByIndex(iKey);
				if (bgeOld.Reading == bge.Reading)
					{
					// readings match, so does date, do nothing
					return;
					}

				MessageBox.Show("Conflicting entry read from device!  bgeOld.Reading = " + bgeOld.Reading.ToString() + ", bgeNew.Reading = " + bge.Reading.ToString());
				return;
				}

			AddBge(bge);
			m_slbge.Add(bge.Date.ToString("s"), bge);
			m_dom.SelectSingleNode("/b:bg", m_nsmgr).AppendChild(nodeReading);
			m_dom.Save("c:\\docs\\bg.xml");
			}
		else
			{
			XmlNode node = m_dom.SelectSingleNode("/b:bg/b:reading[b:date='"+bgeCurrent.Date.ToString("d")+"' and b:time='"+bgeCurrent.Date.ToString("T", DateTimeFormatInfo.InvariantInfo)+"']", m_nsmgr);
			XmlNode nodeRoot = m_dom.SelectSingleNode("/b:bg", m_nsmgr);

			nodeRoot.RemoveChild(node);
			nodeRoot.AppendChild(nodeReading);

			// we need to edit the current item.
			bgeCurrent.SetTo(bge);	// this takes care of m_slbge
			ListViewItem lvi = null;

			if (bgeCurrent != m_bgeCurrent)
				{
				foreach (ListViewItem lviT in m_lvHistory.Items)
					{
					if (((BGE)lviT.Tag) == bgeCurrent)
						{
						lvi = lviT;
						break;
						}
					}
				if (lvi == null)
					MessageBox.Show("Couldn't find matching LVI for BGE");
				}
			else
				lvi = m_lvHistory.SelectedItems[0];

			UpdateBge(bgeCurrent, lvi);				// this handles updating the listbox
			if (fFromDevice)
				lvi.BackColor = Color.LightBlue;
			m_dom.Save("c:\\docs\\bg.xml");
			}

		m_fDirtyStats = true;
	}

	/* A D D  E N T R Y */
	/*----------------------------------------------------------------------------
		%%Function: AddEntry
		%%Qualified: bg._bg.AddEntry
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void AddEntry(object sender, System.EventArgs e) 
	{  
		int nCarbs = m_ebCarbs.Text.Length > 0 ? Int32.Parse(m_ebCarbs.Text) : 0;
		BGE bge = new BGE(m_ebDate.Text, m_ebTime.Text, BGE.ReadingTypeFromString(m_cbxType.Text), Int32.Parse(m_ebReading.Text), nCarbs, m_ebComment.Text);

		AddEntryCore(bge, false);
	}

	/* H A N D L E  C O L U M N */
	/*----------------------------------------------------------------------------
		%%Function: HandleColumn
		%%Qualified: bg._bg.HandleColumn
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void HandleColumn(object sender, System.Windows.Forms.ColumnClickEventArgs e) 
	{
		if (((ListView)sender).ListViewItemSorter == null)
			((ListView)sender).ListViewItemSorter = new ListViewItemComparer(e.Column);
		else
			((ListViewItemComparer)(((ListView)sender).ListViewItemSorter)).SetColumn(e.Column);
		((ListView)sender).Sort();
	}

	/* D I S P L A Y  B G E */
	/*----------------------------------------------------------------------------
		%%Function: DisplayBge
		%%Qualified: bg._bg.DisplayBge
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void DisplayBge(BGE bge)
	{
		if (bge == null)
			{
			m_ebDate.Text = System.DateTime.Now.ToString("d");
			m_ebTime.Text = System.DateTime.Now.ToString("T", DateTimeFormatInfo.InvariantInfo);
			m_ebComment.Text = "";
			m_cbxType.Text = "SpotTest";
			m_ebReading.Text = "";
			m_ebCarbs.Text = "";
			m_pbAdd.Text = "Add";
			m_bgeCurrent = null;
			}
		else
			{
			m_ebDate.Text = bge.Date.ToString("d");
			m_ebTime.Text = bge.Date.ToString("T", DateTimeFormatInfo.InvariantInfo);
			m_ebComment.Text = bge.Comment;
			m_ebReading.Text = bge.Reading.ToString();
			if (bge.Carbs != 0)
				m_ebCarbs.Text = bge.Carbs.ToString(); 
			else
				m_ebCarbs.Text = "";
			m_cbxType.Text = BGE.StringFromReadingType(bge.Type);
			m_pbAdd.Text = "Update";
			m_bgeCurrent = bge;
			}
	}

	/* H A N D L E  H I S T O R Y  C L I C K */
	/*----------------------------------------------------------------------------
		%%Function: HandleHistoryClick
		%%Qualified: bg._bg.HandleHistoryClick
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void HandleHistoryClick(object sender, System.EventArgs e) 
	{
		ListView lv = (ListView)sender;
		ListViewItem lvi = lv.SelectedItems[0];
		BGE bge = (BGE)lvi.Tag;

		DisplayBge(bge);
	}

	private void DoReport(out string sReport, SortedList slbge)
	{
		sReport = "c:\\temp\\tempreport.htm";
		StreamWriter sw = new StreamWriter(sReport);
		sw.WriteLine("<html><body><table border=0>");

		sw.WriteLine("<html><body><p><b>Hello world!</b></p></body></html>");
		sw.Close();


	}

	SortedList SlbgeCalcCustom()
	{
		SortedList slbge = new SortedList();
		DateTime dttmFirst = DateTime.Parse(m_ebFirst.Text);
		DateTime dttmLast = DateTime.Parse(m_ebLast.Text).AddDays(1);

		foreach (BGE bge in m_slbge.Values)
			{
			if (bge.Type == BGE.ReadingType.Control)
				continue;

			if (bge.Date < dttmFirst)
				continue;

			if (bge.Date >= dttmLast)
				continue;

			switch (bge.Type)
				{
				case BGE.ReadingType.Snack:
					if (m_cbSnack.Checked != true)
						continue;
					break;
				case BGE.ReadingType.Lunch:
					if (m_cbLunch.Checked != true)
						continue;
					break;
				case BGE.ReadingType.Dinner:
					if (m_cbDinner.Checked != true)
						continue;
					break;
				case BGE.ReadingType.Breakfast:
					if (m_cbBreakfast.Checked != true)
						continue;
					break;
				case BGE.ReadingType.SpotTest:
					if (m_cbSpot.Checked != true)
						continue;
					break;
				}
			slbge.Add(bge.Date.ToString("s"), bge);
			}
		return slbge;
	}

	/* D O  G R A P H */
	/*----------------------------------------------------------------------------
		%%Function: DoGraph
		%%Qualified: bg._bg.DoGraph
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void DoGraph(object sender, System.EventArgs e) 
	{
		SortedList slbge;

		if (m_cbxFilterType.Text == "Fasting")
            slbge = SlbgeCalcFasting();
		else
			slbge = SlbgeCalcCustom();

		BgGraph bgg = new BgGraph();
		BgGraph.BoxView bvUpper, bvLower;

		bvUpper = BgGraph.BvFromString(m_cbxUpper.Text);
		bvLower = BgGraph.BvFromString(m_cbxLower.Text);

		bgg.SetGraphicViews(bvUpper, bvLower);
		SetGraphBounds(bgg);
		bgg.SetDataPoints(slbge);
		bgg.CalcGraph();
		bgg.ShowDialog();
	}

	void SetGraphBounds(BgGraph bgg)
	{
		int nDays = 7;
		if (m_ebDays.Text.Length > 0)
			nDays = Int32.Parse(m_ebDays.Text);

		int nIntervals = 19;
		if (m_ebIntervals.Text.Length > 0)
			nIntervals = Int32.Parse(m_ebIntervals.Text);

		int nLow = 30;
		if (m_ebLow.Text.Length > 0)
			nLow = Int32.Parse(m_ebLow.Text);

		int nHigh = 30;
		if (m_ebHigh.Text.Length > 0)
			nHigh = Int32.Parse(m_ebHigh.Text);

		bool fLandscape = true;

		if (m_cbxOrient.Text == "Portrait")
			fLandscape = false;

		bgg.SetBounds(nLow, nHigh, nDays, nIntervals, m_cbShowMeals.Checked, fLandscape);
	}

	SortedList SlbgeCalcFasting()
	{
		DateTime dttmFirst = DateTime.Parse(m_ebFirst.Text);
		DateTime dttmLast = DateTime.Parse(m_ebLast.Text).AddDays(1);
		SortedList slbge = new SortedList();

		int nFastLength = 8;

		if (m_ebFastLength.Text.Length > 0)
			{
			nFastLength = Int32.Parse(m_ebFastLength.Text);
			}

		DateTime dttmNextFast = DateTime.Parse("1/1/1900 12:00 AM");

		foreach (BGE bge in m_slbge.Values)
			{
			if (bge.Type == BGE.ReadingType.Control)
				continue;

			// see if this one is a fasting
			if (bge.Date > dttmNextFast)
				{
				if (bge.Reading != 0)
					{
					if (bge.Date >= dttmFirst && bge.Date < dttmLast)
						slbge.Add(bge.Date.ToString("s"), bge);
					}
				}

			// now see if this one should reset the fasting counter
			if (bge.Type != BGE.ReadingType.SpotTest)
				{
				dttmNextFast = bge.Date.AddHours((double)nFastLength);
				}
			}
		return slbge;
	}

	/* R E N D E R  W I T H  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: RenderWithLine
		%%Qualified: bg._bg.RenderWithLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void RenderWithLine(object sender, System.Windows.Forms.PaintEventArgs e)
	{
		Label lbl = (Label)sender;
		Font hFont = lbl.Font; // new Font("Tahoma", 8);
		SolidBrush hBrush = new SolidBrush(lbl.ForeColor);
		Pen pen = new Pen(Color.DarkGray, (float)1.0);
		Pen pen2 = new Pen(Color.LightGray, (float)1.0);

		e.Graphics.Clear(lbl.BackColor);
		e.Graphics.DrawString(lbl.Text, hFont, hBrush, 0, 0);
		SizeF szf =  e.Graphics.MeasureString(lbl.Text, hFont);

		e.Graphics.DrawLine(pen, szf.Width + (float)2.0, (szf.Height) / 2, lbl.Width, (szf.Height) / 2);
		e.Graphics.DrawLine(pen2, szf.Width + (float)1.0, (szf.Height) / 2 - (float)1.0, lbl.Width - (float)1.0, (szf.Height) / 2 - (float)1.0);

	}

	private bool m_fDirtyStats = true;
	private int m_nLifetimeAverage = 0;
	private int m_nLifetimeFastingAverage = 0;
	private int m_nLifetimeAverageWgt = 0;

	private int m_n30DayAverage = 0;
	private int m_n30DayFastingAverage = 0;
	private int m_n15DayAverage = 0;
	private int m_n15DayFastingAverage = 0;
	private int m_n7DayAverage = 0;
	private int m_n7DayFastingAverage = 0;

	private int UpdateCarbList(BGE bgeCur, ref SortedList slbge)
	{
		// retire all the items at the beginning of the list
		while (slbge.Count > 0)
			{
			BGE bgeFirst = (BGE)slbge.GetByIndex(0);

			if (bgeFirst.Date.AddHours(4.0) > bgeCur.Date)
				break; // nothing left to retire

			slbge.RemoveAt(0);
			}

		// now, if bgeCur has carbs, then add it to the list
		if (bgeCur.Carbs > 0)
			slbge.Add(bgeCur.Date.ToString("s"), bgeCur);

		int nCarbs = 0;

		for (int i = 0, iLast = slbge.Count; i < iLast; i++)
			{
			BGE bge = (BGE) slbge.GetByIndex(i);

			if (bge.Date != bgeCur.Date)
				nCarbs += bge.Carbs;
			}

		return nCarbs;
	}

	class STN
	{
		public int nTotal;
		public int cTotal;

		public int nFastTotal;
		public int cFastTotal;

		public Int64 nWgtTotal;
		public Int64 cWgtTotal;

		public TextBox ebAvg;
		public TextBox ebFast;
		public TextBox ebWgt;
		public TextBox ebA1c;

		public DateTime dttmCutoff;

		public STN(TextBox ebAvgIn, TextBox ebFastIn, TextBox ebWgtIn, TextBox ebA1cIn, DateTime dttmCutoffIn)
		{
			nTotal = cTotal = nFastTotal = cFastTotal = 0;
			nWgtTotal = cWgtTotal = 0;

			ebAvg = ebAvgIn;
			ebFast = ebFastIn;
			ebWgt = ebWgtIn;
			ebA1c = ebA1cIn;
			dttmCutoff = dttmCutoffIn;
		}

		public float A1c
		{
			get
			{
				int nAvg = (int)(nWgtTotal / cWgtTotal);

				float dA1c = (nAvg + 77.3f) / 35.6f;
				return dA1c;
			}
		}

		public int Avg
		{
			get
			{
				return  nTotal / cTotal;
			}
		}

		public int FastAvg
		{
			get
			{
				return nFastTotal / cFastTotal;
			}
		}

		public int WgtAvg
		{
			get
			{
				return (int)(nWgtTotal / cWgtTotal);
			}
		}

		public void SetText()
		{
			ebAvg.Text = Avg.ToString();
			ebFast.Text = FastAvg.ToString();
			ebWgt.Text = WgtAvg.ToString();
			ebA1c.Text = A1c.ToString();
		}

	};

	/* C A L C  S T A T S */
	/*----------------------------------------------------------------------------
		%%Function: CalcStats
		%%Qualified: bg._bg.CalcStats
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void CalcStats()
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);

		DateTime dttm90 = bgeLast.Date.AddDays(-90);
		DateTime dttm30 = bgeLast.Date.AddDays(-30);
		DateTime dttm15 = bgeLast.Date.AddDays(-15);
		DateTime dttm7 = bgeLast.Date.AddDays(-7);

		// switching to analysis.  If anything is dirty, then recalc
		// the stats
		STN []rgstn = new STN[] 
			{ new STN(m_ebLifetimeAvg, m_ebFastingAvg, m_ebAvgWgt, m_ebA1c, DateTime.Parse("1/1/1900")),
			  new STN(m_ebAvg90, m_ebFastingAvg90, m_ebWgtAvg90, m_ebA1c90, dttm90),
			  new STN(m_ebAvg30, m_ebFastingAvg30, m_ebWgtAvg30, m_ebA1c30, dttm30),
			  new STN(m_ebAvg15, m_ebFastingAvg15, m_ebWgtAvg15, m_ebA1c15, dttm15),
			  new STN(m_ebAvg7, m_ebFastingAvg7, m_ebWgtAvg7, m_ebA1c7, dttm7) };

		int nFastLength = 8;

		if (m_ebFastLength.Text.Length > 0)
			nFastLength = Int32.Parse(m_ebFastLength.Text);

		DateTime dttmNextFast = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmNextMealCheckLow = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmNextMealCheckHigh = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmLastWgt = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmStop = DateTime.Parse("9/9/2004");

		int nLastWgt = 0;
		BGE bgePrev = null;

		int nTotalFastingLifetime = 0;
		int cFastingLifetime = 0;

		int nTotalLifetime = 0;
		int cLifetime = 0;

		Int64 nWgtCur = 0;
		Int64 cWgtCur = 0;

		Int64 nTotalLifetimeWgt = 0;
		Int64 cTotalLifetimeWgt = 0;

		int nTotalFasting30 = 0;
		int cFasting30 = 0;

		int nTotal30 = 0;
		int c30 = 0;

		int nTotalFasting15 = 0;
		int cFasting15 = 0;

		int nTotal15 = 0;
		int c15 = 0;

		int nTotalFasting7 = 0;
		int cFasting7 = 0;

		int nTotal7 = 0;
		int c7 = 0;


		// we are interesting in keeping track of the last 4 hours of carb
		// intake.
		SortedList slbgeCarbs = new SortedList();
		DateTime dttmLastCarb = new DateTime(1900, 1, 1);

		foreach (BGE bge in m_slbge.Values)
			{
			if (bge.Type == BGE.ReadingType.Control)
				continue;

			int nCarbs = UpdateCarbList(bge, ref slbgeCarbs);
			
			if (dttmLastCarb.Year != 1900)
				{
				bge.MinutesSinceCarbs = (int)((bge.Date.Ticks - dttmLastCarb.Ticks) / (36000000000 / 60)); // number of hours
				}
			else
				bge.MinutesSinceCarbs = -1;

			bge.CarbsIn4 = nCarbs;

			if (bge.Reading != 0)
				{
				nTotalLifetime += bge.Reading;
				cLifetime++;

				nWgtCur = cWgtCur = 0;

				if (nLastWgt != 0)//  && bge.Date < dttmStop)
					{
					TimeSpan ts = bge.Date.Subtract(dttmLastWgt);
					Int64 nHours = ((Int64)(ts.TotalMinutes * 100) / 60);
					Int64 nWgtEstCur = 0;
					Int64 cWgtEstCur = 0;

					if (bgePrev.Carbs > 0 && ts.TotalMinutes > 90)
						{
						// if the readings since the carb were > 1.5 hours, we can 
						// assume we didn't capture the spike.  use a guess of 20bgc rise
						// for each carb

						Int64 nRise;

						if (ts.TotalMinutes > 90)
							{
							nRise = nLastWgt + bgePrev.Carbs * 40;
							nWgtEstCur = (Int64)nRise * (cWgtEstCur = (Int64)(90 * 100 / 60));

							nHours -= cWgtEstCur;
							}

						// assume that a greater reading immediately following a "carbs" entry
						// will be higher previous to this reading
						if (bge.Reading > bgeLast.Reading)
							{
							// don't let the previous reading (which was pre-meal) get weighted
							// as such; treat the larger reading (post-meal) as the average)
							nLastWgt = bge.Reading;
							}
						}
					else
						{
						// if the current reading is higher than the previous, assume its
						// been that way for 1/2 of the time
						if (bge.Reading > bgeLast.Reading)
							{
							cWgtEstCur = (Int64)((ts.TotalMinutes * 100) / 60 / 2);
							nHours -= cWgtEstCur;

							nWgtEstCur = (bge.Reading * cWgtEstCur);
							}
						}

					nWgtCur = ((Int64)nLastWgt) * nHours;
					cWgtCur = nHours;

					nWgtCur += nWgtEstCur;
					cWgtCur += cWgtEstCur;
					}
				bgePrev = bge;
				nLastWgt = bge.Reading;
				dttmLastWgt = bge.Date;

				nTotalLifetimeWgt += nWgtCur;
				cTotalLifetimeWgt += cWgtCur;

				int nFast = 0;
				int cFast = 0;

				// see if this one is a fasting
				if (bge.Date > dttmNextFast)
					{
					nFast = bge.Reading;
					cFast = 1;
					}

				foreach (STN stn in rgstn)
					{
					if (bge.Date >= stn.dttmCutoff)
						{
						stn.nTotal += bge.Reading;
						stn.cTotal++;
						stn.nFastTotal += nFast;
						stn.cFastTotal += cFast;
						stn.nWgtTotal += nWgtCur;
						stn.cWgtTotal += cWgtCur;
						}
					}

				if (bge.Date >= dttm30)
					{
					nTotal30 += bge.Reading;
					c30++;
					}
	
				if (bge.Date >= dttm15)
					{
					nTotal15 += bge.Reading;
					c15++;
					}
	
				if (bge.Date >= dttm7)
					{
					nTotal7 += bge.Reading;
					c7++;
					}

				// see if this one is a fasting
				if (bge.Date > dttmNextFast)
					{
					nTotalFastingLifetime += bge.Reading;
					cFastingLifetime++;
	
					if (bge.Date >= dttm30)
						{
						nTotalFasting30 += bge.Reading;
						cFasting30++;
						}
					if (bge.Date >= dttm15)
						{
						nTotalFasting15 += bge.Reading;
						cFasting15++;
						}
					if (bge.Date >= dttm7)
						{
						nTotalFasting7 += bge.Reading;
						cFasting7++;
						}
					}
				}

			// now see if this one should reset the fasting counter
			if (bge.Type != BGE.ReadingType.SpotTest)
				{
				dttmNextFast = bge.Date.AddHours((double)nFastLength);
				}
			if (bge.Carbs > 0)
				dttmLastCarb = bge.Date;
			}

		m_nLifetimeAverage = nTotalLifetime/ cLifetime;
		m_nLifetimeFastingAverage = nTotalFastingLifetime / cFastingLifetime;
		m_nLifetimeAverageWgt = (int)(nTotalLifetimeWgt / cTotalLifetimeWgt);
		float dA1c = (m_nLifetimeAverageWgt + 77.3f) / 35.6f;

		m_n30DayAverage = nTotal30 / c30;
		m_n30DayFastingAverage = nTotalFasting30 / cFasting30;
		m_n15DayAverage = nTotal15 / c15;
		m_n15DayFastingAverage = nTotalFasting15 / cFasting15;
		m_n7DayAverage = nTotal7 / c7;
		m_n7DayFastingAverage = nTotalFasting7 / cFasting7;

		if (rgstn[0].Avg != m_nLifetimeAverage) throw(new Exception("mismatch!"));
		if (rgstn[0].FastAvg != m_nLifetimeFastingAverage) throw(new Exception("mismatch!"));

		if (rgstn[2].Avg != m_n30DayAverage) throw(new Exception("mismatch!"));
		if (rgstn[2].FastAvg != m_n30DayFastingAverage) throw(new Exception("mismatch!"));
		if (rgstn[3].Avg != m_n15DayAverage) throw(new Exception("mismatch!"));
		if (rgstn[3].FastAvg != m_n15DayFastingAverage) throw(new Exception("mismatch!"));
		if (rgstn[4].Avg != m_n7DayAverage) throw(new Exception("mismatch!"));
		if (rgstn[4].FastAvg != m_n7DayFastingAverage) throw(new Exception("mismatch!"));

		m_ebLifetimeAvg.Text = m_nLifetimeAverage.ToString();
		m_ebFastingAvg.Text = m_nLifetimeFastingAverage.ToString();
		m_ebAvgWgt.Text = m_nLifetimeAverageWgt.ToString();
		m_ebA1c.Text = dA1c.ToString();
		m_ebAvg30.Text = m_n30DayAverage.ToString();
		m_ebFastingAvg30.Text = m_n30DayFastingAverage.ToString();
		m_ebAvg15.Text = m_n15DayAverage.ToString();
		m_ebFastingAvg15.Text = m_n15DayFastingAverage.ToString();
		m_ebAvg7.Text = m_n7DayAverage.ToString();
		m_ebFastingAvg7.Text = m_n7DayFastingAverage.ToString();
		m_fDirtyStats = false;
	}

	private void ChangeTabs(object sender, System.EventArgs e) 
	{
		TabControl tabc = (TabControl)sender;

		if (tabc.SelectedIndex == 1 && m_fDirtyStats)
			{
			CalcStats();
			}
	}

	private void Setup7DayGraph(object sender, System.EventArgs e)
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);

		m_ebLast.Text = bgeLast.Date.ToString("d");
		m_ebFirst.Text = bgeLast.Date.AddDays(-7).ToString("d");

		m_ebFirst.Enabled = false;
		m_ebLast.Enabled = false;
	}

	private void Setup15DayGraph(object sender, System.EventArgs e)
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);
	
		m_ebLast.Text = bgeLast.Date.ToString("d");
		m_ebFirst.Text = bgeLast.Date.AddDays(-15).ToString("d");
	
		m_ebFirst.Enabled = false;
		m_ebLast.Enabled = false;
	}

	private void Setup30DayGraph(object sender, System.EventArgs e)
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);
	
		m_ebLast.Text = bgeLast.Date.ToString("d");
		m_ebFirst.Text = bgeLast.Date.AddDays(-30).ToString("d");
	
		m_ebFirst.Enabled = false;
		m_ebLast.Enabled = false;
	}

	private void EnableCustomDates(object sender, System.EventArgs e) 
	{
		if (m_ebLast.Text.Length <= 0)
			{
			BGE bge = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);
			m_ebLast.Text = bge.Date.ToString("d");
			}

		if (m_ebFirst.Text.Length <= 0)
			{
			BGE bge = (BGE)m_slbge.GetByIndex(0);
			m_ebFirst.Text = bge.Date.ToString("d");
			}
		m_ebFirst.Enabled = true;
		m_ebLast.Enabled = true;
	}

	private void SelectDateRange(object sender, System.EventArgs e) 
	{
		ComboBox cbx = (ComboBox)sender;
		
		if (cbx.Text == "7 Days")
			Setup7DayGraph(sender, e);
		else if (cbx.Text == "15 Days")
			Setup15DayGraph(sender, e);
		else if (cbx.Text == "30 Days")
			Setup30DayGraph(sender, e);
		else
			EnableCustomDates(sender, e);
	}

	public class DevComm : CommBase
	{
		CommBaseSettings m_cbs;

		protected override CommBaseSettings CommSettings()
		{
			return m_cbs;
		}

		public void Init()
		{
			m_cbs = new CommBaseSettings();

			m_cbs.SetStandard("COM3:", 9600, CommBase.Handshake.none);
			rgbRxBuffer = new byte[512];
		}

		void SendCommand(string s)
		{
			byte []rgb = new byte[Encoding.ASCII.GetByteCount(s)];
			byte []rgbString = Encoding.ASCII.GetBytes(s);

			Buffer.BlockCopy(rgbString, 0, rgb, 0, rgbString.Length);
			
			int iMac = rgbString.Length;
			int i;

			for (i = 0; i < iMac; i++)
				{
				Send(rgb[i]);
				Sleep(150);
				}
//			Send(rgb);
		}

		private byte[] rgbRxBuffer;
		private uint ibRxBuffer = 0;
		private ASCII[] rgbRxTerm = { ASCII.CR, ASCII.LF };
		private ASCII[] TxTerm;
		private ASCII[] RxFilter;
		private string RxString = "";
		private ManualResetEvent TransFlag = new ManualResetEvent(true);

		private uint TransTimeout;
		int ibRxTermWaiting = 0;
		ArrayList pls = new ArrayList();

		void OnRxLine(string s)
		{
			lock(pls)
				{
				pls.Add(s);
				}
		}

		protected override void OnRxChar(byte ch) 
		{
			ASCII ca = (ASCII)ch;
			if (ibRxTermWaiting < rgbRxTerm.Length)
				{
				if (ca == rgbRxTerm[ibRxTermWaiting])
					ibRxTermWaiting++;
				else
					ibRxTermWaiting = 0;	// bail out if we didn't get the next ASCII char in the sequence
				}

			if (ibRxTermWaiting >= rgbRxTerm.Length
				|| (ibRxBuffer > rgbRxBuffer.GetUpperBound(0)))
				{
				//JH 1.1: Use static encoder for efficiency. Thanks to Prof. Dr. Peter Jesorsky!
				lock(RxString) 
					{
					RxString = Encoding.ASCII.GetString(rgbRxBuffer, 0, (int)ibRxBuffer - ibRxTermWaiting);
					}
				ibRxBuffer = 0;
				ibRxTermWaiting = 0;
				if (TransFlag.WaitOne(0,false)) 
					{
//					ThrowException("Received line when noone was looking!");
					OnRxLine(RxString);
					}
				else 
					{
					TransFlag.Set();
					}
				}
			else
				{
				bool wr = true;
//				if (RxFilter != null) 
//					{
//					for (int i=0; i <= RxFilter.GetUpperBound(0); i++) 
//						if (RxFilter[i] == ca) 
//							wr = false;
//					}

				if (wr)
					{
					rgbRxBuffer[ibRxBuffer] = ch;
					ibRxBuffer++;
					}
				}
		}

		public ArrayList GetReadings()
		{
			// actually read the data
			lock(pls)
				{
				pls.Clear();
				}

			TransFlag.Reset();
			SendCommand("DMP");
			// the first line we get back will tell us how many lines to expect
			if (!TransFlag.WaitOne((int)5000, false))
				ThrowException("Timeout");

			// more lines are coming in, they are being collected in pls
			string s;
			lock(RxString)
				{
				s = RxString;
				}

			// find the leading "P"
			int ibp = s.IndexOf('P');
			if (ibp < 0)
				return null;

			int cLines = Int32.Parse(s.Substring(ibp + 2, 3));

			// and now we wait for that many lines to show up in pls
			int cMsecsTimeout = 20000;
			ArrayList plsFinal = null;
			int count = 0;

			while (cMsecsTimeout > 0 && plsFinal == null)
				{
				lock (pls)
					{
					if (pls.Count >= cLines)
						plsFinal = pls;
					count = pls.Count;
					}
				Sleep(2000);
				cMsecsTimeout -= 2000;
				}

			if (plsFinal == null)
				{
				MessageBox.Show("Only got " + count.ToString()+" readings, expected " + cLines.ToString());
				return null;
				}

			return plsFinal;
		}

		public bool CheckDevice()
		{
			SendCommand("DM");
			Sleep(500);
			TransFlag.Reset();
			SendCommand("DM?");

			// expecting one line back
			if (!TransFlag.WaitOne((int)500, false))
				ThrowException("Timeout");
			string s;
			lock(RxString)
				{
				s = RxString;
				}
			if (s.Substring(0,1) != "?")
				return false;

			return true;
		}

		public void CloseDevice()
		{
			this.Close();
		}
	}

	ArrayList GetReadingsFromDevice()
	{
		ArrayList pls = new ArrayList();

		StreamReader sr = new StreamReader("c:\\temp\\dmp1.txt");
		string s;

		s = sr.ReadLine();	// consume the first one
		while ((s = sr.ReadLine()) != null)
			pls.Add(s);

		sr.Close();
		return pls;
	}

	string SGetNextQuotedField(string s, int iFirst, out int iNext)
	{
		int ib;
		iNext = 0;
		ib = s.IndexOf('"', iFirst);
		if (ib < iFirst)
			return null;

		int ib2 = s.IndexOf('"', ib + 1);
		if (ib2 < ib)
			return null;

		string sRet = s.Substring(ib + 1, ib2 - ib - 1);
		if (ib2 + 1 >= s.Length)
			iNext = -1;
		else
			iNext = ib2 + 1;

		return sRet;
	}

	private void ReadFromDevice(object sender, System.EventArgs e) 
	{
		ArrayList pls = GetReadingsFromDeviceReal();

		foreach (string s in pls)
			{
			int iFirst = 0, iNext;

			if (s.Substring(0, 2) != "P ")
				continue;

			string sDay = SGetNextQuotedField(s, iFirst, out iNext);
			string sDate = SGetNextQuotedField(s, iFirst = iNext, out iNext);
			string sTime = SGetNextQuotedField(s, iFirst = iNext, out iNext);
			string sReading = SGetNextQuotedField(s, iFirst = iNext, out iNext);

			BGE bge = new BGE(sDate, sTime, BGE.ReadingType.New, Int32.Parse(sReading), 0, "");

			AddEntryCore(bge, true);
			}
	}

	private ArrayList GetReadingsFromDeviceReal() 
	{
		DevComm devComm = new DevComm();

		devComm.Init();
		CommBase.PortStatus ps = devComm.IsPortAvailable("COM3:");

		if (ps != CommBase.PortStatus.available)
			{
			MessageBox.Show("COM3 unavailable");
			return null;
			}

		if (!devComm.Open())
			{
			MessageBox.Show("COM3 unable to open");
			return null;
			}

		if (!devComm.CheckDevice())
			{
			MessageBox.Show("OTU did not respond appropriately to DM?");
			return null;
			}

		ArrayList pls = devComm.GetReadings();

		if (pls == null)
			{
			MessageBox.Show("OTU did not provide readings");
			return null;
			}

		devComm.CloseDevice();
		return pls;
	}

	private void groupBox5_Enter(object sender, System.EventArgs e) {
	
	}



}}
