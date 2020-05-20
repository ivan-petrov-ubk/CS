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
    [TradeSystem("patternplus")]
    public class patternplus : TradeSystem
    {
//        // Simple parameter example
//        [Parameter("Some comment", DefaultValue = "Hello, world!")]
//        public string CommentText { get; set; }

		[Parameter("Coefficient :", DefaultValue = 0.618, MinValue=0.001, MaxValue = 10.000)]
		public double coef { get;set; }

		[Parameter("Use Coefficient :", DefaultValue = true)]
		public bool UseCoef { get;set; }

		[Parameter("Current Lots :", DefaultValue = 0.1, MinValue=0.01, MaxValue = 10.0)]
		public double vol { get;set; }

		[Parameter("Static SL :", DefaultValue = 550, MinValue=10, MaxValue = 10000)]
		public int staticSL { get;set; }

		[Parameter("Static TP :", DefaultValue = 350, MinValue=10, MaxValue = 10000)]
		public int staticTP { get;set; }
	
		
		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"PPLog")]
		public string LogPath { get;set; }


		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 15);
		private string trueLogPath = "";
		
		public static int magicNumber = 97198;

		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		

		public static int Cm0 = 0;
		public static int Cm1 = 0;
		public static int Cm2 = 0;
		
		public ArrowUp cArrowUp;
		public ArrowDown cArrowDown;

		public static StreamWriter LogSW = null;

        protected override void Init()
        {
			// Event occurs once at the start of the strategy
            Print("Starting TS on account: {0} ", this.Account.Number);
         	
			InitLogFile();

			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
		}        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			Cm0 = Bars.Range.To - 1;         	
			Cm1 = Bars.Range.To - 2; 		
			Cm2 = Bars.Range.To - 3;
		
			if((Cm0 >0) && (Cm1 >0) && (Cm2 >0))
			{
				double body0 = Math.Round(Bars[Cm0].Close -Bars[Cm0].Open, Instrument.PriceScale);
				double body1 = Math.Round(Bars[Cm1].Close -Bars[Cm1].Open, Instrument.PriceScale);
				double body2 = Math.Round(Bars[Cm2].Close -Bars[Cm2].Open, Instrument.PriceScale);
				if((body0>0) && (body1>0) && (body2>0))
				{
					SetBuyOrder();
				}
				if((body0<0) && (body1<0) && (body2<0))
				{
					SetSellOrder();
				}
			}
		}

		protected double GetProfit(double Ask, double Bid, int OrderType) 
		{
			
			if(UseCoef)
			{	
				double HighLevel = Bars[Cm0].High;
				if(Bars[Cm1].High > HighLevel)
					HighLevel= Bars[Cm1].High;
				if(Bars[Cm2].High > HighLevel)
					HighLevel = Bars[Cm2].High;
			
				double LowLevel = Bars[Cm0].Low;
				if(Bars[Cm1].Low < LowLevel)
					LowLevel = Bars[Cm1].Low;
				if(Bars[Cm2].Low < LowLevel)
					LowLevel = Bars[Cm2].Low;
			
				double Profit = Math.Round((HighLevel - LowLevel)*coef, Instrument.PriceScale);
			
			
				if(OrderType == 0)
					return Math.Round(Bid + Profit, Instrument.PriceScale);
				if(OrderType == 1)
					return Math.Round(Ask - Profit, Instrument.PriceScale);
			}
			else
			{	
				if(OrderType == 0)
					return Math.Round(Bid + staticTP * Instrument.Point, Instrument.PriceScale); 
				if(OrderType == 1)
					return Math.Round(Ask - staticTP * Instrument.Point, Instrument.PriceScale);
			}
			return 0.0;
		}
		protected double GetSL(double Ask, double Bid, int OrderType)		
		{
			if(UseCoef)
			{	
				double HighLevel = Bars[Cm0].High;
				if(Bars[Cm1].High > HighLevel)
					HighLevel= Bars[Cm1].High;
				if(Bars[Cm2].High > HighLevel)
					HighLevel = Bars[Cm2].High;
			
				double LowLevel = Bars[Cm0].Low;
				if(Bars[Cm1].Low < LowLevel)
					LowLevel = Bars[Cm1].Low;
				if(Bars[Cm2].Low < LowLevel)
					LowLevel = Bars[Cm2].Low;
			
			
				if(OrderType == 0)
					return Math.Round(LowLevel, Instrument.PriceScale);
				if(OrderType == 1)
					return Math.Round(HighLevel, Instrument.PriceScale);
			}
			else
			{
				if(OrderType == 0)
					return Math.Round(Ask - staticSL * Instrument.Point, Instrument.PriceScale);
				if(OrderType == 1)
					return Math.Round(Bid + staticSL * Instrument.Point, Instrument.PriceScale);
			}
			return 0.0;
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
//            if (type==ModificationType.Closed)
//            {
//                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
//            }
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
