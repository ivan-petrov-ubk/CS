using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Client.MarketData;
using System.Collections.Generic;
using System.IO;

namespace IPro.TradeSystems
{
    [TradeSystem("CandlePlus")]
    public class candleplus : TradeSystem
    {
		public struct SavedActive
		{
			public double open;
			public double close;
			public double high;
			public double low;
		}
        // Simple parameter example
 		
		[Parameter("Orders :", DefaultValue = 2, MinValue= 1, MaxValue = 100)]
		public int orderLimit { get;set; }

		[Parameter("Current Lots :", DefaultValue = 0.1, MinValue=0.01, MaxValue = 10.0)]
		public double vol { get;set; }

		[Parameter("SL :",DefaultValue = 700, MinValue = 10, MaxValue = 100000)]
		public int SL { get;set; }

		[Parameter("K :",DefaultValue = 0.6, MinValue = -10.0, MaxValue = 10.0)]
		public double k { get;set; }

		[Parameter("(H/L)/B :",DefaultValue = 0.3, MinValue = 0.0, MaxValue = 1.0)]
		public double hlc { get;set; }

		[Parameter("Fractal :",DefaultValue = 4, MinValue = 2, MaxValue = 100)]
		public int BarLimit { get;set; }

		[Parameter("UseStaticSL :",DefaultValue = true)]
		public bool useSL { get;set; }
		
		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"ZxZLog")]
		public string LogPath { get;set; }

		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 15);

		public static int magicNumber = 17987;

		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		
		
		public static int ActiveBuyCount = 0;
		public static int ActiveSellCount = 0;
		
		public ArrowUp cArrowUp;
		public ArrowDown cArrowDown;
		private string trueLogPath = "";

		public static StreamWriter LogSW = null;

		public static SavedActive svs =  new SavedActive(); 
		public static SavedActive svsBuy =  new SavedActive(); 
		public static SavedActive svsSell =  new SavedActive(); 
		
		public static int ci = 0;
		protected bool inFirst = true;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            // Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			#region	GetIndicatorsAndFrames // Get Indicators' references...           
			
			Print("Starting TS on account: {0} ", this.Account.Number);
         	
			InitLogFile();

			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
			
			svs.open = svs.close = svs.low = svs.high = 0.0;
			svsBuy.open = svsBuy.close = svsBuy.low = svsBuy.high = 0.0;
			svsSell.open = svsSell.close = svsSell.low = svsSell.high = 0.0;

			#endregion //GetIndicatorsAndFrames		
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
			// Event occurs on every new bar
            
			int ci = Bars.Range.To - 1;

			if(
				Instrument.Bid.Equals(double.NaN)
				||
				Bars[ci].Close.Equals(double.NaN)
				||
				Bars[ci-1].Close.Equals(double.NaN)
				||
				Bars[ci-2].Close.Equals(double.NaN)
				||
				Bars[ci-3].Close.Equals(double.NaN)
			  )
			{
				XXPrint("NaN in Data ...");
				SetUpArrows();
				return;	
			}			
			
			ActiveBuyCount = 0;
			ActiveSellCount = 0;
			IPosition[] activePositions;
			
			activePositions = null;
			activePositions = Trade.GetActivePositions(magicNumber);
			if(activePositions != null)
			{	
				for(int i = activePositions.GetLength(0) - 1; i >=0; i--)
				{
					if((int)activePositions[i].Type == (int)ExecutionRule.Buy)
					{
						ActiveBuyCount++;
						continue;
					}
					if((int)activePositions[i].Type == (int)ExecutionRule.Sell)
					{
						ActiveSellCount++;
						continue;
					}
				}
			}
			
			if(Bars[ci].Close > Bars[ci-1].High 
				&& ((Bars[ci-1].High - Bars[ci-1].Close) / 
				     Math.Abs(Bars[ci-1].Close - Bars[ci-1].Open) == 0 ? Instrument.Point : Math.Abs(Bars[ci-1].Close - Bars[ci-1].Open)) 
				     < hlc)
			{
				if(ActiveBuyCount < orderLimit)		
				{
					SetBuyOrder();
				}			
				svsBuy.close =  Bars[ci-1].Close;
				svsBuy.open  =  Bars[ci-1].Open;
				svsBuy.high  =  Bars[ci-1].High;
				svsBuy.low   =  Bars[ci-1].Low;
				//
				svs.close =  Bars[ci-1].Close;
				svs.open  =  Bars[ci-1].Open;
				svs.high  =  Bars[ci-1].High;
				svs.low   =  Bars[ci-1].Low;
			}

			if(Bars[ci].Close < Bars[ci-1].Low 
				&& ((Bars[ci-1].Close - Bars[ci-1].Low) / 
				     Math.Abs(Bars[ci-1].Close - Bars[ci-1].Open) == 0 ? Instrument.Point : Math.Abs(Bars[ci-1].Close - Bars[ci-1].Open)) 
				     < hlc)			
			{
				if(ActiveSellCount < orderLimit)		
				{
					SetSellOrder();
				}			
				svsSell.close =  Bars[ci-1].Close;
				svsSell.open  =  Bars[ci-1].Open;
				svsSell.high  =  Bars[ci-1].High;
				svsSell.low   =  Bars[ci-1].Low;
				//
				svs.close =  Bars[ci-1].Close;
				svs.open  =  Bars[ci-1].Open;
				svs.high  =  Bars[ci-1].High;
				svs.low   =  Bars[ci-1].Low;
			}
/////////
			TrailActiveOrders();	
			//
//////////
		}
		
		protected void TrailActiveOrders()
		{
			double TP = GetTP();
			double SL = GetSL(0);

			if(svsBuy.open == 0.0 || svsSell.open == 0.0)
				return;
		    
			IPosition[] activePositions = null;
			activePositions = Trade.GetActivePositions(magicNumber);

			if(activePositions != null)
			{	
				for(int i = activePositions.GetLength(0) - 1; i >=0; i--)
				{
					TradeResult  tr = null;
					if((int)activePositions[i].Type == (int)ExecutionRule.Buy)
					{
						if(!useSL)
						{
							tr = Trade.UpdateMarketPosition(
													activePositions[i].Id,
													/*activePositions[i].OpenPrice -*/ GetSL(1),
													activePositions[i].TakeProfit,
													"Buy .. - change TP,SL"
														);
						}
						else
						{
							tr = Trade.UpdateMarketPosition(
													activePositions[i].Id,
													activePositions[i].OpenPrice - GetSL(1),
													activePositions[i].TakeProfit,
													"Buy .. - change TP,SL"
														);
						}
						if(tr == null)
						{
							XXPrint("[*!*] Can't update position {0} - set SL = {1}, TP = {2}", 
											activePositions[i].Id,
												activePositions[i].OpenPrice - SL,
													activePositions[i].OpenPrice + TP);
						}
					}
					if((int)activePositions[i].Type == (int)ExecutionRule.Sell)
					{
						if(!useSL)
						{
							tr = Trade.UpdateMarketPosition(
													activePositions[i].Id,
													/*activePositions[i].OpenPrice +*/ GetSL(0),
													activePositions[i].TakeProfit,
													"Sell.. - change TP,SL"
														); 
						}
						else
						{
							tr = Trade.UpdateMarketPosition(
													activePositions[i].Id,
													activePositions[i].OpenPrice + GetSL(0),
													activePositions[i].TakeProfit,
													"Sell.. - change TP,SL"
														); 
						}
						if(tr == null)
						{
							XXPrint("[*!*] Can't update position {0} - set SL = {1}, TP = {2}", 
											activePositions[i].Id,
												activePositions[i].OpenPrice + SL,
													activePositions[i].OpenPrice - TP);
						}
					}
				}
			}
		}
		
		protected void SetBuyOrder()
		{
			double SL = Math.Round(Instrument.Ask - GetSL(1), Instrument.PriceScale);
			//if(useStSL)
			double TP = Math.Round(Instrument.Ask + GetTP(), Instrument.PriceScale);
			
			Stops st = Stops.InPrice(SL,TP);
			
			TradeResult r = null;
			
			r = Trade.OpenMarketPosition(
											 Instrument.Id, 
											 ExecutionRule.Buy, 
											 vol, 
											 Instrument.Ask, 
											 -1,
											 st, 
											 "Buy..", 
											 magicNumber
										 );
			if(r==null)
			{
				XXPrint("*!* Can't open Buy position at Price {0}",Instrument.Ask);
			}
		}
		
		protected void SetSellOrder()
		{
			double SL = Math.Round(Instrument.Bid + GetSL(0), Instrument.PriceScale);
			//if(useStSL)
			double TP = Math.Round(Instrument.Bid - GetTP(), Instrument.PriceScale);

			Stops st = Stops.InPrice(SL,TP);

			TradeResult r = null;
			
			r = Trade.OpenMarketPosition(	 
											 Instrument.Id, 
											 ExecutionRule.Sell, 
											 vol, 
											 Instrument.Bid, 
											 -1,
											 st, 
											 "Sell.", 
											 magicNumber
										 );
			if(r==null)
			{
				XXPrint("*!* Can't open Sell position at Price {0}",Instrument.Bid);
			}
		}
 
		protected double GetTP()
		{
			double TP = 0.0;
			if((svs.high == 0.0) || (svs.low == 0.0))
			{
				TP = Math.Round(k * (Bars[ci-1].High - Bars[ci-1].Low),Instrument.PriceScale);
			}	
			else
			{
				TP = Math.Round(k * (svs.high - svs.low),Instrument.PriceScale);
			}	
			if(TP < (25.0 * Instrument.Point))
			{		
				TP = 25.0 * Instrument.Point;
			}
			return Math.Round(TP, Instrument.PriceScale);
		}
		
		protected double GetSL(int tradeType)
		{
			
			if(useSL)
			{
				return Math.Round(SL * Instrument.Point, Instrument.PriceScale);
			}
			else
			{
				if(inFirst)
				{	
					inFirst = false;
					return Math.Round(SL * Instrument.Point, Instrument.PriceScale);
				}
				else
				{	
					if(tradeType == 0)
					{
						double ret = 0.0;
						double MAX = 0.0;
						int Current = Bars.Range.To -1;
						for(int i=0; i < BarLimit; i++)
						{
							if(MAX < Bars[Current - i].High)
								MAX = Bars[Current - i].High;
						}	
						if(svsSell.high > MAX)
						{
							ret = svsSell.high;
						}
						else
						{	
							ret = MAX;
						}
						return Math.Round(ret + 10.0 * Instrument.Point, Instrument.PriceScale);
					}
					else
					{	
						double ret = 0.0;
						double MIN = 0.0;
						int Current = Bars.Range.To -1;
						for(int i=0; i < BarLimit; i++)
						{
							if(MIN < Bars[Current - i].Low)
								MIN = Bars[Current - i].Low;
						}	
						if(svsBuy.low < MIN)
						{
							ret = svsBuy.low;
						}
						else
						{	
							ret = MIN;
						}
						return Math.Round(ret - 10.0 * Instrument.Point, Instrument.PriceScale);
					}
				}
			}
		}
		
		
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            //if (type==ModificationType.Closed)
            //{
            //    Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            //}
        }
		protected void XXPrint(string xxformat, params object[] parameters)
		{
			if(LogSW!=null)
			{
				if(LogSW.BaseStream.CanSeek && LogSW.BaseStream.CanWrite)
				{
					LogSW.BaseStream.Seek(0,SeekOrigin.End);
					LogSW.WriteLine(string.Format(xxformat,parameters));
					LogSW.Flush();
				}
			}
			if(DebugMode)
			{
				Print(xxformat,parameters);
			}	
		}
		
		protected void InitLogFile()
		{
			if(LogMode)
			{
				trueLogPath = LogPath+DateTime.Now.Ticks.ToString().Trim() + ".LOG";
				if(File.Exists(trueLogPath))
				{ 
					File.Delete(trueLogPath);
				}
				LogSW = new StreamWriter(File.Open(trueLogPath, 
													FileMode.Create, 
														FileAccess.ReadWrite, 
															FileShare.ReadWrite)); 
			}
		}
		
		protected override void Deinit()
		{
			if(LogMode && LogSW!=null)
			{
				LogSW.Flush();
				LogSW.Close();
			}
		}

	
		protected void SetUpArrows()
		{
//
			if((cArrowUp != null) && Tools.GetById(cAUG).Equals(cArrowUp))
			{
				Tools.Remove(cArrowUp);
				cArrowUp = null; // Artifishial using... 
			}	
// 			
			if((cArrowDown != null) && Tools.GetById(cADG).Equals(cArrowDown))
			{
				Tools.Remove(cArrowDown);
				cArrowDown = null; // Artifishial using... 
			}
//
			if(Bars[Bars.Range.To-1].Open <= Bars[Bars.Range.To-1].Close)
			{
				cArrowUp = Tools.Create<ArrowUp>();   		
				cAUG = cArrowUp.Id;
					cArrowUp.Color 	 = Color.Blue;
					cArrowUp.Width 	 = 1 ;
					cArrowUp.Point 	 = new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low); 
			}
//			
			if(Bars[Bars.Range.To-1].Open >= Bars[Bars.Range.To-1].Close)
			{
				cArrowDown = Tools.Create<ArrowDown>();   		
				cADG = cArrowDown.Id;			
					cArrowDown.Color = Color.Red;
					cArrowDown.Width = 1 ;
					cArrowDown.Point = new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
			}
//
		}

	}
}