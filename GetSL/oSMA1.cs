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
    [TradeSystem("OSMA")]
    public class osma : TradeSystem
    {
        // Simple parameter example

		[Parameter("OSMA-FastEMA", DefaultValue = 12, MinValue = 1, MaxValue = 100)]
		public int OSMAfastEMA { get; set; }

		[Parameter("OSMA-SlowEMA", DefaultValue = 26, MinValue = 1, MaxValue = 100)]
		public int OSMAslowEMA { get; set; }
		
		[Parameter("OSMA-MACDSMA", DefaultValue = 9, MinValue = 1, MaxValue = 100)]
		public int OSMAMACDSMA { get; set; }

		[Parameter("OSMA-Price", DefaultValue = PriceMode.Close)]
		public PriceMode OSMAprice { get; set; }

		
		[Parameter("Current Lots :", DefaultValue = 0.1, MinValue=0.01, MaxValue = 10.0)]
		public double vol { get;set; }

		[Parameter("Static SL :", DefaultValue = 550, MinValue=10, MaxValue = 10000)]
		public int staticSL { get;set; }

		[Parameter("Static TP :", DefaultValue = 350, MinValue=50, MaxValue = 10000)]
		public int staticTP { get;set; }
	
		
		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"OSMALog")]
		public string LogPath { get;set; }
		
/// <summary>
/// go...go...
/// </summary>		
		
		private ISeries<Bar> _barSeries;
		//private BollingerBands BolaInd;
		private Period _period = new Period(PeriodType.Minute, 15);
		private string trueLogPath = "";

		public static int magicNumber = 96696;


		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		

		public ArrowUp cArrowUp;
		public ArrowDown cArrowDown;

	
		public static StreamWriter LogSW = null;

		public static int ActiveBuyCount  = 0; 
		public static int ActiveSellCount = 0; 
		public static int AttemptLimit = 3;

		public static double Vbuy  = 0.0;
		public static double Vsell = 0.0;
		
		public static bool TrailSLTP = true;
		
		private MovingAverageOfOscillator OSMAInd;

		
		protected override void Init()
        {
            // Event occurs once at the start of the strategy
			//                
#region	GetIndicatorsAndFrames // Get Indicators' references...           

			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
			 
			OSMAInd = GetIndicator<MovingAverageOfOscillator>(
												Instrument.Id,
												_period,
												OSMAfastEMA,
												OSMAslowEMA,
												OSMAMACDSMA,
												OSMAprice
																	 );
#endregion //GetIndicatorsAndFrames		

					
		}

		
		protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        
		protected override void NewBar()
        {
		    // Event occurs on every new bar
        	 
			double Curr = OSMAInd.SeriesMain[Bars.Range.To-1];
			double Prev = OSMAInd.SeriesMain[Bars.Range.To-2];
		
			if(Curr > 0)
			{
				if(Prev < 0)
				{
					SetBuyOrder();
				}
			}
			if(Curr < 0)
			{
				if(Prev > 0)
				{
					SetSellOrder();
				}
			}
			SetUpArrows();
		}
        
        
		protected double GetProfit(double CurrentBid, double CurrentAsk, int OrderType)
		{
			if(OrderType == 0)
			{
				return (CurrentAsk + staticTP*Instrument.Point);
			}
			else
			{
				return (CurrentBid - staticTP*Instrument.Point);
			}
		}

		protected double GetSL(double CurrentBid,double CurrentAsk, int OrderType)
		{
			if(OrderType == 0)
			{
				return (CurrentBid - staticSL*Instrument.Point);
			}
			else
			{
				return (CurrentAsk + staticSL*Instrument.Point);
			}	
		}
		
		protected void SetBuyOrder()
		{
			
			double TP = GetProfit(Instrument.Bid,Instrument.Ask, 0);
			double SL = GetSL(Instrument.Bid,Instrument.Ask, 0);
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

		}
		
		protected void SetSellOrder()
		{
			double TP = GetProfit(Instrument.Bid,Instrument.Ask, 1);
			double SL = GetSL(Instrument.Bid,Instrument.Ask, 1);
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
		}
		
		protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
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