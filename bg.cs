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

namespace bg
{

public class BGE // BG Entry
{
	public enum ReadingType
	{
		Breakfast,
		Lunch,
		Dinner,
		Snack,
		SpotTest
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
}


public interface GraphicBox
{
	void Paint(Graphics gr);
	void SetFirstFromScroll(int i);
	void Calc();
	bool FHitTest(Point pt, out object oHit, out RectangleF rectfHit);
	void SetDataPoints(SortedList slbge, VScrollBar sbv, HScrollBar sbh);
	void SetProps(GrapherParams gp);
	int GetFirstForScroll();
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
		return (dxa * (float)gr.DpiX) / 1440.0F;
	}

	/* D Y P  F R O M  D Y A */
	/*----------------------------------------------------------------------------
		%%Function: DypFromDya
		%%Qualified: bg.Reporter.DypFromDya
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	static public float DypFromDya(Graphics gr, float dya)
	{
		return (dya * (float)gr.DpiY) / 1440.0F;
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
		Font font = new Font("Tahoma", 8);

		SetColWidth(icolDay, 5, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealBreakfast, 5, BorderType.Double, BorderType.None);
		SetColWidth(icolMealBreakfast + 1, 5, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealBreakfast + 2, 5, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealLunch, 5, BorderType.Double, BorderType.None);
		SetColWidth(icolMealLunch + 1, 5, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealLunch + 2, 5, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealDinner, 5, BorderType.Double, BorderType.None);
		SetColWidth(icolMealDinner + 1, 5, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealDinner + 2, 5, BorderType.Solid, BorderType.None);

		SetColWidth(icolBed, 5, BorderType.Double, BorderType.None);

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
		float dy = YFromLine(2) - YFromLine(1);


		m_nLinesPerPage = (int)(m_nHeight / dy) - 1 ;
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

				if (readingFirst == 0)
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

public class Grapher : GraphicBox
{
	float m_nHeight;
	float m_nWidth;
	RectangleF m_rcfDrawing;
	SortedList m_slbge;
	GrapherParams m_cgp;
	double m_dyPerBgUnit;
	double m_dxQuarter;
	int m_iFirstQuarter;
	float m_dxAdjust = 0;
	
	ArrayList m_plptfi;

	float m_dxOffset = 30.0F;
	float m_dyOffset = 45.0F;

//	float m_dxLeftMargin = 15.0F;
//	float m_dxRightMargin = 0;
//	float m_dyTopMargin = 0;
//	float m_dyBottomMargin = 0;

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

//	public void SetMargins(float dxMarginLeft, float dxMarginRight, float dyMarginTop, float dyMarginBottom)
//	{
//		m_dxLeftMargin = dxMarginLeft;
//		m_dxRightMargin = dxMarginRight;
//		m_dyTopMargin = dyMarginTop;
//		m_dyBottomMargin = dyMarginBottom;
//	}

	public void SetFirstFromScroll(int i)
	{
		SetFirstQuarter(i);
	}

	public void SetFirstQuarter(int i)
	{
		m_iFirstQuarter = i;
	}

	public int GetFirstQuarter()
	{
		return m_iFirstQuarter;
	}

	public int GetFirstForScroll()
	{
		return GetFirstQuarter();
	}

	public DateTime GetFirstDateTime()
	{
		PTFI ptfi = (PTFI)m_plptfi[0];
		DateTime dttmFirst = new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);
		DateTime dttm = DateTime.Parse(dttmFirst.ToString("d"));
		double nDayFirst = ((double)m_iFirstQuarter + 95.0) / (4.0 * 24.0) - 1.0;
		dttm = dttm.AddDays(nDayFirst);

		return dttm;
	}

	public void SetFirstDateTime(DateTime dttm)
	{
		PTFI ptfi = (PTFI)m_plptfi[0];
		DateTime dttmFirst = new DateTime(ptfi.bge.Date.Year, ptfi.bge.Date.Month, ptfi.bge.Date.Day);
		TimeSpan ts = dttm.Subtract(dttmFirst);
		m_iFirstQuarter = (int)(4.0 * ts.TotalHours);
	}


	PointF PtfFromBge(DateTime dayFirst, BGE bge, double dxQuarter, double dyBg)
	{
		float x = XFromDate(dayFirst, bge.Date, dxQuarter);
		float y = YFromReading(bge.Reading, dyBg);

		// now we've got the number of quarters.  figure out the bge
		return new PointF(x,y);
	}

	public void SetDataPoints(SortedList slbge, VScrollBar sbv, HScrollBar sbh)
	{
		m_slbge = slbge;
		m_sbh = sbh;
	}

	public void SetProps(GrapherParams cgp)
	{
		m_cgp = cgp;
	}

	public float YFromReading(int nReading, double dyPerBgUnit)
	{
		if (nReading == 0)
			return -1.0F;

//		return (float)(m_nHeight - ((nReading - m_cgp.dBgLow) * dyPerBgUnit) - m_dyOffset - m_dyBottomMargin);
		return m_rcfDrawing.Top + m_nHeight - ((((float)nReading - (float)m_cgp.dBgLow) * (float)dyPerBgUnit)) - m_dyOffset;
	}

	public float XFromDate(DateTime dayFirst, DateTime dttm, double dxQuarter)
	{
		// calculate how many quarter hours
		long ticks = dttm.Ticks - dayFirst.Ticks;
//			long lQuarters = (ticks / DateTime.TicksPerHour) / 4;
		long lQuarters = ticks / (36000000000 / 4);

		return (float)((lQuarters * dxQuarter) + m_rcfDrawing.Left); // (m_dxOffset + m_dxLeftMargin));
	}

	HScrollBar m_sbh;
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

	public void DrawBanner(Graphics gr, RectangleF rcf)
	{
		Font font = new Font("Tahoma", 12);

//		gr.DrawString("Graph", font, new SolidBrush(Color.Black), rcf);
	}

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

		hBrushTrans = new SolidBrush(Color.FromArgb(255,255, 255, 167));
		ShadeRanges2(gr, dxAdjust, dttmFirst, hBrushTrans, 10, 20);

		hBrushTrans = new SolidBrush(Color.FromArgb(255, 174, 255, 174));
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
	private System.Windows.Forms.Label label13;
	private System.Windows.Forms.Button m_pbGraphFast;
	private System.Windows.Forms.Label label14;
	private System.Windows.Forms.TextBox m_ebFastLength;
	private System.Windows.Forms.GroupBox groupBox1;
	private System.Windows.Forms.Label label21;
	private System.Windows.Forms.Label label22;
	private System.Windows.Forms.TextBox m_ebLifetimeAvg;
	private System.Windows.Forms.GroupBox groupBox2;
	private System.Windows.Forms.TextBox m_ebAvg30;
	private System.Windows.Forms.Label label20;
	private System.Windows.Forms.Label label23;
	private System.Windows.Forms.TextBox m_ebFastingAvg;
	private System.Windows.Forms.TextBox m_ebFastingAvg30;
	private System.Windows.Forms.GroupBox groupBox3;
	private System.Windows.Forms.TextBox m_ebFastingAvg15;
	private System.Windows.Forms.TextBox m_ebAvg15;
	private System.Windows.Forms.Label label24;
	private System.Windows.Forms.Label label25;
	private System.Windows.Forms.GroupBox groupBox4;
	private System.Windows.Forms.TextBox m_ebFastingAvg7;
	private System.Windows.Forms.TextBox m_ebAvg7;
	private System.Windows.Forms.Label label26;
	private System.Windows.Forms.Label label27;
	private System.Windows.Forms.Label label28;
	private System.Windows.Forms.Label label29;
	private System.Windows.Forms.CheckBox m_cbShowMeals;
	private System.Windows.Forms.RadioButton m_rbCustom;
	private System.Windows.Forms.RadioButton m_rb30Days;
	private System.Windows.Forms.RadioButton m_rb15Days;
	private System.Windows.Forms.RadioButton m_rb7Days;
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
		this.m_cbxLower = new System.Windows.Forms.ComboBox();
		this.m_cbxUpper = new System.Windows.Forms.ComboBox();
		this.label31 = new System.Windows.Forms.Label();
		this.label30 = new System.Windows.Forms.Label();
		this.m_cbShowMeals = new System.Windows.Forms.CheckBox();
		this.m_rbCustom = new System.Windows.Forms.RadioButton();
		this.m_rb30Days = new System.Windows.Forms.RadioButton();
		this.m_rb15Days = new System.Windows.Forms.RadioButton();
		this.m_rb7Days = new System.Windows.Forms.RadioButton();
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
		this.m_ebFastingAvg7 = new System.Windows.Forms.TextBox();
		this.m_ebAvg7 = new System.Windows.Forms.TextBox();
		this.label26 = new System.Windows.Forms.Label();
		this.label27 = new System.Windows.Forms.Label();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.m_ebFastingAvg15 = new System.Windows.Forms.TextBox();
		this.m_ebAvg15 = new System.Windows.Forms.TextBox();
		this.label24 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.m_ebFastingAvg30 = new System.Windows.Forms.TextBox();
		this.m_ebAvg30 = new System.Windows.Forms.TextBox();
		this.label20 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.m_ebFastingAvg = new System.Windows.Forms.TextBox();
		this.m_ebLifetimeAvg = new System.Windows.Forms.TextBox();
		this.label22 = new System.Windows.Forms.Label();
		this.label21 = new System.Windows.Forms.Label();
		this.m_ebFastLength = new System.Windows.Forms.TextBox();
		this.label14 = new System.Windows.Forms.Label();
		this.m_pbGraphFast = new System.Windows.Forms.Button();
		this.label13 = new System.Windows.Forms.Label();
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
		this.m_tabc.Size = new System.Drawing.Size(528, 480);
		this.m_tabc.TabIndex = 0;
		this.m_tabc.SelectedIndexChanged += new System.EventHandler(this.ChangeTabs);
		// 
		// m_tabEntry
		// 
		this.m_tabEntry.Controls.AddRange(new System.Windows.Forms.Control[] {
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
		this.m_tabEntry.Size = new System.Drawing.Size(520, 454);
		this.m_tabEntry.TabIndex = 0;
		this.m_tabEntry.Text = "Data Entry";
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
																					this.m_cbxLower,
																					this.m_cbxUpper,
																					this.label31,
																					this.label30,
																					this.m_cbShowMeals,
																					this.m_rbCustom,
																					this.m_rb30Days,
																					this.m_rb15Days,
																					this.m_rb7Days,
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
																					this.m_pbGraphFast,
																					this.label13,
																					this.label12,
																					this.m_cbSnack,
																					this.m_cbDinner,
																					this.m_cbLunch,
																					this.m_cbBreakfast,
																					this.m_cbSpot,
																					this.m_pbGraph});
		this.m_tabAnalysis.Location = new System.Drawing.Point(4, 22);
		this.m_tabAnalysis.Name = "m_tabAnalysis";
		this.m_tabAnalysis.Size = new System.Drawing.Size(520, 454);
		this.m_tabAnalysis.TabIndex = 1;
		this.m_tabAnalysis.Text = "Analysis";
		// 
		// m_cbxLower
		// 
		this.m_cbxLower.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxLower.Items.AddRange(new object[] {
														"Empty",
														"Graph",
														"Log"});
		this.m_cbxLower.Location = new System.Drawing.Point(432, 224);
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
		this.m_cbxUpper.Location = new System.Drawing.Point(432, 200);
		this.m_cbxUpper.Name = "m_cbxUpper";
		this.m_cbxUpper.Size = new System.Drawing.Size(72, 21);
		this.m_cbxUpper.TabIndex = 37;
		// 
		// label31
		// 
		this.label31.Location = new System.Drawing.Point(368, 232);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(64, 23);
		this.label31.TabIndex = 36;
		this.label31.Text = "Lower Box";
		// 
		// label30
		// 
		this.label30.Location = new System.Drawing.Point(368, 208);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(64, 23);
		this.label30.TabIndex = 35;
		this.label30.Text = "Upper Box";
		// 
		// m_cbShowMeals
		// 
		this.m_cbShowMeals.Checked = true;
		this.m_cbShowMeals.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbShowMeals.Location = new System.Drawing.Point(352, 168);
		this.m_cbShowMeals.Name = "m_cbShowMeals";
		this.m_cbShowMeals.TabIndex = 19;
		this.m_cbShowMeals.Text = "Show Meals";
		// 
		// m_rbCustom
		// 
		this.m_rbCustom.Location = new System.Drawing.Point(240, 192);
		this.m_rbCustom.Name = "m_rbCustom";
		this.m_rbCustom.Size = new System.Drawing.Size(64, 24);
		this.m_rbCustom.TabIndex = 23;
		this.m_rbCustom.Text = "Custom";
		this.m_rbCustom.Click += new System.EventHandler(this.EnableCustomDates);
		// 
		// m_rb30Days
		// 
		this.m_rb30Days.Location = new System.Drawing.Point(160, 192);
		this.m_rb30Days.Name = "m_rb30Days";
		this.m_rb30Days.Size = new System.Drawing.Size(64, 24);
		this.m_rb30Days.TabIndex = 22;
		this.m_rb30Days.Text = "30 Days";
		this.m_rb30Days.Click += new System.EventHandler(this.Setup30DayGraph);
		// 
		// m_rb15Days
		// 
		this.m_rb15Days.Location = new System.Drawing.Point(88, 192);
		this.m_rb15Days.Name = "m_rb15Days";
		this.m_rb15Days.Size = new System.Drawing.Size(64, 24);
		this.m_rb15Days.TabIndex = 21;
		this.m_rb15Days.Text = "15 Days";
		this.m_rb15Days.Click += new System.EventHandler(this.Setup15DayGraph);
		// 
		// m_rb7Days
		// 
		this.m_rb7Days.Location = new System.Drawing.Point(16, 192);
		this.m_rb7Days.Name = "m_rb7Days";
		this.m_rb7Days.Size = new System.Drawing.Size(72, 24);
		this.m_rb7Days.TabIndex = 20;
		this.m_rb7Days.Text = "&7 Days";
		this.m_rb7Days.Click += new System.EventHandler(this.Setup7DayGraph);
		// 
		// m_ebIntervals
		// 
		this.m_ebIntervals.Location = new System.Drawing.Point(304, 229);
		this.m_ebIntervals.Name = "m_ebIntervals";
		this.m_ebIntervals.Size = new System.Drawing.Size(48, 20);
		this.m_ebIntervals.TabIndex = 29;
		this.m_ebIntervals.Text = "19";
		// 
		// m_ebHigh
		// 
		this.m_ebHigh.Location = new System.Drawing.Point(192, 229);
		this.m_ebHigh.Name = "m_ebHigh";
		this.m_ebHigh.Size = new System.Drawing.Size(48, 20);
		this.m_ebHigh.TabIndex = 27;
		this.m_ebHigh.Text = "220";
		// 
		// m_ebLow
		// 
		this.m_ebLow.Location = new System.Drawing.Point(72, 229);
		this.m_ebLow.Name = "m_ebLow";
		this.m_ebLow.Size = new System.Drawing.Size(48, 20);
		this.m_ebLow.TabIndex = 25;
		this.m_ebLow.Text = "30";
		// 
		// label19
		// 
		this.label19.Location = new System.Drawing.Point(248, 232);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(48, 23);
		this.label19.TabIndex = 28;
		this.label19.Text = "Intervals";
		// 
		// label18
		// 
		this.label18.Location = new System.Drawing.Point(136, 232);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(48, 23);
		this.label18.TabIndex = 26;
		this.label18.Text = "High bg";
		// 
		// label17
		// 
		this.label17.Location = new System.Drawing.Point(16, 232);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(48, 23);
		this.label17.TabIndex = 24;
		this.label17.Text = "Low bg";
		// 
		// m_ebDays
		// 
		this.m_ebDays.Location = new System.Drawing.Point(312, 165);
		this.m_ebDays.Name = "m_ebDays";
		this.m_ebDays.Size = new System.Drawing.Size(24, 20);
		this.m_ebDays.TabIndex = 18;
		this.m_ebDays.Text = "7";
		// 
		// label16
		// 
		this.label16.Location = new System.Drawing.Point(232, 168);
		this.label16.Name = "label16";
		this.label16.TabIndex = 17;
		this.label16.Text = "Days per Page";
		// 
		// label15
		// 
		this.label15.Location = new System.Drawing.Point(8, 144);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(496, 16);
		this.label15.TabIndex = 12;
		this.label15.Text = "All Report Options";
		this.label15.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// label11
		// 
		this.label11.Location = new System.Drawing.Point(128, 168);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(32, 23);
		this.label11.TabIndex = 15;
		this.label11.Text = "To";
		// 
		// label10
		// 
		this.label10.Location = new System.Drawing.Point(16, 168);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(40, 23);
		this.label10.TabIndex = 13;
		this.label10.Text = "From";
		// 
		// m_ebLast
		// 
		this.m_ebLast.Location = new System.Drawing.Point(160, 165);
		this.m_ebLast.Name = "m_ebLast";
		this.m_ebLast.Size = new System.Drawing.Size(56, 20);
		this.m_ebLast.TabIndex = 16;
		this.m_ebLast.Text = "";
		// 
		// m_ebFirst
		// 
		this.m_ebFirst.Location = new System.Drawing.Point(56, 165);
		this.m_ebFirst.Name = "m_ebFirst";
		this.m_ebFirst.Size = new System.Drawing.Size(48, 20);
		this.m_ebFirst.TabIndex = 14;
		this.m_ebFirst.Text = "";
		// 
		// label29
		// 
		this.label29.Location = new System.Drawing.Point(128, 112);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(40, 16);
		this.label29.TabIndex = 10;
		this.label29.Text = "hours";
		// 
		// label28
		// 
		this.label28.Location = new System.Drawing.Point(8, 272);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(496, 23);
		this.label28.TabIndex = 30;
		this.label28.Text = "Statistics";
		this.label28.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// groupBox4
		// 
		this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebFastingAvg7,
																				this.m_ebAvg7,
																				this.label26,
																				this.label27});
		this.groupBox4.Location = new System.Drawing.Point(256, 376);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(240, 64);
		this.groupBox4.TabIndex = 0;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "7 Day Averages";
		// 
		// m_ebFastingAvg7
		// 
		this.m_ebFastingAvg7.Location = new System.Drawing.Point(104, 32);
		this.m_ebFastingAvg7.Name = "m_ebFastingAvg7";
		this.m_ebFastingAvg7.Size = new System.Drawing.Size(64, 20);
		this.m_ebFastingAvg7.TabIndex = 2;
		this.m_ebFastingAvg7.Text = "textBox2";
		// 
		// m_ebAvg7
		// 
		this.m_ebAvg7.Location = new System.Drawing.Point(104, 8);
		this.m_ebAvg7.Name = "m_ebAvg7";
		this.m_ebAvg7.Size = new System.Drawing.Size(64, 20);
		this.m_ebAvg7.TabIndex = 0;
		this.m_ebAvg7.Text = "m_ebAvg30";
		// 
		// label26
		// 
		this.label26.Location = new System.Drawing.Point(8, 32);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(48, 16);
		this.label26.TabIndex = 1;
		this.label26.Text = "Fasting";
		// 
		// label27
		// 
		this.label27.Location = new System.Drawing.Point(8, 16);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(72, 23);
		this.label27.TabIndex = 0;
		this.label27.Text = "All Readings";
		// 
		// groupBox3
		// 
		this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebFastingAvg15,
																				this.m_ebAvg15,
																				this.label24,
																				this.label25});
		this.groupBox3.Location = new System.Drawing.Point(8, 376);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(240, 64);
		this.groupBox3.TabIndex = 33;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "15 Day Averages";
		// 
		// m_ebFastingAvg15
		// 
		this.m_ebFastingAvg15.Location = new System.Drawing.Point(104, 32);
		this.m_ebFastingAvg15.Name = "m_ebFastingAvg15";
		this.m_ebFastingAvg15.Size = new System.Drawing.Size(64, 20);
		this.m_ebFastingAvg15.TabIndex = 3;
		this.m_ebFastingAvg15.Text = "textBox2";
		// 
		// m_ebAvg15
		// 
		this.m_ebAvg15.Location = new System.Drawing.Point(104, 8);
		this.m_ebAvg15.Name = "m_ebAvg15";
		this.m_ebAvg15.Size = new System.Drawing.Size(64, 20);
		this.m_ebAvg15.TabIndex = 1;
		this.m_ebAvg15.Text = "m_ebAvg30";
		// 
		// label24
		// 
		this.label24.Location = new System.Drawing.Point(8, 32);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(48, 16);
		this.label24.TabIndex = 2;
		this.label24.Text = "Fasting";
		// 
		// label25
		// 
		this.label25.Location = new System.Drawing.Point(8, 16);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(72, 23);
		this.label25.TabIndex = 0;
		this.label25.Text = "All Readings";
		// 
		// groupBox2
		// 
		this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebFastingAvg30,
																				this.m_ebAvg30,
																				this.label20,
																				this.label23});
		this.groupBox2.Location = new System.Drawing.Point(256, 304);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(240, 64);
		this.groupBox2.TabIndex = 32;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "30 Day Averages";
		// 
		// m_ebFastingAvg30
		// 
		this.m_ebFastingAvg30.Location = new System.Drawing.Point(104, 32);
		this.m_ebFastingAvg30.Name = "m_ebFastingAvg30";
		this.m_ebFastingAvg30.Size = new System.Drawing.Size(64, 20);
		this.m_ebFastingAvg30.TabIndex = 3;
		this.m_ebFastingAvg30.Text = "textBox2";
		// 
		// m_ebAvg30
		// 
		this.m_ebAvg30.Location = new System.Drawing.Point(104, 8);
		this.m_ebAvg30.Name = "m_ebAvg30";
		this.m_ebAvg30.Size = new System.Drawing.Size(64, 20);
		this.m_ebAvg30.TabIndex = 1;
		this.m_ebAvg30.Text = "m_ebAvg30";
		// 
		// label20
		// 
		this.label20.Location = new System.Drawing.Point(8, 32);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(48, 16);
		this.label20.TabIndex = 2;
		this.label20.Text = "Fasting";
		// 
		// label23
		// 
		this.label23.Location = new System.Drawing.Point(8, 16);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(72, 23);
		this.label23.TabIndex = 0;
		this.label23.Text = "All Readings";
		// 
		// groupBox1
		// 
		this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				this.m_ebFastingAvg,
																				this.m_ebLifetimeAvg,
																				this.label22,
																				this.label21});
		this.groupBox1.Location = new System.Drawing.Point(8, 304);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(240, 64);
		this.groupBox1.TabIndex = 31;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Lifetime Averages";
		// 
		// m_ebFastingAvg
		// 
		this.m_ebFastingAvg.Location = new System.Drawing.Point(104, 32);
		this.m_ebFastingAvg.Name = "m_ebFastingAvg";
		this.m_ebFastingAvg.Size = new System.Drawing.Size(64, 20);
		this.m_ebFastingAvg.TabIndex = 3;
		this.m_ebFastingAvg.Text = "textBox2";
		// 
		// m_ebLifetimeAvg
		// 
		this.m_ebLifetimeAvg.Location = new System.Drawing.Point(104, 8);
		this.m_ebLifetimeAvg.Name = "m_ebLifetimeAvg";
		this.m_ebLifetimeAvg.Size = new System.Drawing.Size(64, 20);
		this.m_ebLifetimeAvg.TabIndex = 1;
		this.m_ebLifetimeAvg.Text = "textBox1";
		// 
		// label22
		// 
		this.label22.Location = new System.Drawing.Point(8, 32);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(48, 16);
		this.label22.TabIndex = 2;
		this.label22.Text = "Fasting";
		// 
		// label21
		// 
		this.label21.Location = new System.Drawing.Point(8, 16);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(72, 23);
		this.label21.TabIndex = 0;
		this.label21.Text = "All Readings";
		// 
		// m_ebFastLength
		// 
		this.m_ebFastLength.Location = new System.Drawing.Point(88, 108);
		this.m_ebFastLength.Name = "m_ebFastLength";
		this.m_ebFastLength.Size = new System.Drawing.Size(32, 20);
		this.m_ebFastLength.TabIndex = 9;
		this.m_ebFastLength.Text = "7";
		// 
		// label14
		// 
		this.label14.Location = new System.Drawing.Point(16, 112);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(96, 24);
		this.label14.TabIndex = 8;
		this.label14.Text = "Fast Length:";
		// 
		// m_pbGraphFast
		// 
		this.m_pbGraphFast.Location = new System.Drawing.Point(432, 112);
		this.m_pbGraphFast.Name = "m_pbGraphFast";
		this.m_pbGraphFast.Size = new System.Drawing.Size(72, 23);
		this.m_pbGraphFast.TabIndex = 11;
		this.m_pbGraphFast.Text = "Generate";
		this.m_pbGraphFast.Click += new System.EventHandler(this.GraphFast);
		// 
		// label13
		// 
		this.label13.Location = new System.Drawing.Point(8, 88);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(496, 23);
		this.label13.TabIndex = 7;
		this.label13.Text = "Fasting Reports";
		this.label13.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// label12
		// 
		this.label12.Location = new System.Drawing.Point(8, 8);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(496, 23);
		this.label12.TabIndex = 0;
		this.label12.Text = "Custom Reports";
		this.label12.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// m_cbSnack
		// 
		this.m_cbSnack.Checked = true;
		this.m_cbSnack.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbSnack.Location = new System.Drawing.Point(200, 32);
		this.m_cbSnack.Name = "m_cbSnack";
		this.m_cbSnack.TabIndex = 3;
		this.m_cbSnack.Text = "Snack";
		// 
		// m_cbDinner
		// 
		this.m_cbDinner.Checked = true;
		this.m_cbDinner.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbDinner.Location = new System.Drawing.Point(128, 48);
		this.m_cbDinner.Name = "m_cbDinner";
		this.m_cbDinner.TabIndex = 5;
		this.m_cbDinner.Text = "Dinner";
		// 
		// m_cbLunch
		// 
		this.m_cbLunch.Checked = true;
		this.m_cbLunch.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbLunch.Location = new System.Drawing.Point(128, 32);
		this.m_cbLunch.Name = "m_cbLunch";
		this.m_cbLunch.TabIndex = 2;
		this.m_cbLunch.Text = "Lunch";
		// 
		// m_cbBreakfast
		// 
		this.m_cbBreakfast.Checked = true;
		this.m_cbBreakfast.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbBreakfast.Location = new System.Drawing.Point(16, 48);
		this.m_cbBreakfast.Name = "m_cbBreakfast";
		this.m_cbBreakfast.TabIndex = 4;
		this.m_cbBreakfast.Text = "Breakfast";
		// 
		// m_cbSpot
		// 
		this.m_cbSpot.Checked = true;
		this.m_cbSpot.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbSpot.Location = new System.Drawing.Point(16, 32);
		this.m_cbSpot.Name = "m_cbSpot";
		this.m_cbSpot.TabIndex = 1;
		this.m_cbSpot.Text = "SpotTests";
		// 
		// m_pbGraph
		// 
		this.m_pbGraph.Location = new System.Drawing.Point(432, 32);
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
		m_lvHistory.Items.Add(lvi);
		UpdateBge(bge, lvi);
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
		nodeTime.InnerText = bge.Date.ToString("t", DateTimeFormatInfo.InvariantInfo);
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

		if (m_bgeCurrent == null)
			{
			m_slbge.Add(bge.Date.ToString("s"), bge);
			AddBge(bge);
	
			m_dom.SelectSingleNode("/b:bg", m_nsmgr).AppendChild(nodeReading);
			m_dom.Save("c:\\docs\\bg.xml");
			}
		else
			{
			XmlNode node = m_dom.SelectSingleNode("/b:bg/b:reading[b:date='"+m_bgeCurrent.Date.ToString("d")+"' and b:time='"+m_bgeCurrent.Date.ToString("t", DateTimeFormatInfo.InvariantInfo)+"']", m_nsmgr);
			XmlNode nodeRoot = m_dom.SelectSingleNode("/b:bg", m_nsmgr);

			nodeRoot.RemoveChild(node);
			nodeRoot.AppendChild(nodeReading);
			
			// we need to edit the current item.
			m_bgeCurrent.SetTo(bge);	// this takes care of m_slbge
			UpdateBge(m_bgeCurrent, m_lvHistory.SelectedItems[0]);				// this handles updating the listbox
			m_dom.Save("c:\\docs\\bg.xml");
			}
		m_fDirtyStats = true;
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
			m_ebTime.Text = System.DateTime.Now.ToString("t", DateTimeFormatInfo.InvariantInfo);
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
			m_ebTime.Text = bge.Date.ToString("t", DateTimeFormatInfo.InvariantInfo);
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
		
	private void DoGraph(object sender, System.EventArgs e) 
	{
		BgGraph bgg = new BgGraph();
		SortedList slbge = new SortedList();
		DateTime dttmFirst = DateTime.Parse(m_ebFirst.Text);
		DateTime dttmLast = DateTime.Parse(m_ebLast.Text).AddDays(1);

		foreach (BGE bge in m_slbge.Values)
			{
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

		bgg.SetBounds(nLow, nHigh, nDays, nIntervals, m_cbShowMeals.Checked);
	}

	private void GraphFast(object sender, System.EventArgs e) 
	{
		DateTime dttmFirst = DateTime.Parse(m_ebFirst.Text);
		DateTime dttmLast = DateTime.Parse(m_ebLast.Text).AddDays(1);

		BgGraph bgg = new BgGraph();
		SortedList slbge = new SortedList();

		int nFastLength = 8;

		if (m_ebFastLength.Text.Length > 0)
			{
			nFastLength = Int32.Parse(m_ebFastLength.Text);
			}

		DateTime dttmNextFast = DateTime.Parse("1/1/1900 12:00 AM");

		foreach (BGE bge in m_slbge.Values)
			{
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
		BgGraph.BoxView bvUpper, bvLower;

		bvUpper = BgGraph.BvFromString(m_cbxUpper.Text);
		bvLower = BgGraph.BvFromString(m_cbxLower.Text);

		bgg.SetGraphicViews(bvUpper, bvLower);
		SetGraphBounds(bgg);
		bgg.SetDataPoints(slbge);
		bgg.CalcGraph();
		bgg.ShowDialog();
	}

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

	private void CalcStats()
	{
		// switching to analysis.  If anything is dirty, then recalc
		// the stats

		int nFastLength = 8;

		if (m_ebFastLength.Text.Length > 0)
			nFastLength = Int32.Parse(m_ebFastLength.Text);

		DateTime dttmNextFast = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmNextMealCheckLow = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmNextMealCheckHigh = DateTime.Parse("1/1/1900 12:00 AM");

		int nTotalFastingLifetime = 0;
		int cFastingLifetime = 0;

		int nTotalLifetime = 0;
		int cLifetime = 0;

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


		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);
		DateTime dttm30 = bgeLast.Date.AddDays(-30);
		DateTime dttm15 = bgeLast.Date.AddDays(-15);
		DateTime dttm7 = bgeLast.Date.AddDays(-7);

		// we are interesting in keeping track of the last 4 hours of carb
		// intake.
		SortedList slbgeCarbs = new SortedList();
		DateTime dttmLastCarb = new DateTime(1900, 1, 1);

		foreach (BGE bge in m_slbge.Values)
			{
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
		m_n30DayAverage = nTotal30 / c30;
		m_n30DayFastingAverage = nTotalFasting30 / cFasting30;
		m_n15DayAverage = nTotal15 / c15;
		m_n15DayFastingAverage = nTotalFasting15 / cFasting15;
		m_n7DayAverage = nTotal7 / c7;
		m_n7DayFastingAverage = nTotalFasting7 / cFasting7;

		m_ebLifetimeAvg.Text = m_nLifetimeAverage.ToString();
		m_ebFastingAvg.Text = m_nLifetimeFastingAverage.ToString();
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
		if (m_ebFirst.Text.Length <= 0)
			{
			m_rbCustom.Checked = true;
			EnableCustomDates(null, null);
			}

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


}}
