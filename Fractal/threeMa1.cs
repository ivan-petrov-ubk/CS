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
    [TradeSystem("threeMa")]
    public class threeMa : TradeSystem
    {
        // Simple parameter example
		[Parameter("MAHigh_Period", DefaultValue = 50, MinValue = 2)]
        public int MAHperiod { get; set; }

		[Parameter("MAHigh_offset", DefaultValue = 0, MinValue = -1000, MaxValue = 1000)]
		public int MAHoffset { get; set; }
		
		[Parameter("MAHigh_method", DefaultValue = MaMethods.Ema)]
		public MaMethods MAHmethod { get; set; }
		
		[Parameter("MAHigh_Calculated_Price", DefaultValue = PriceMode.Weighted)]
        public PriceMode MAHapplyto { get; set; }
		
		
		[Parameter("MAAver_Period", DefaultValue = 25, MinValue = 2)]
        public int MAAperiod { get; set; }

		[Parameter("MAAver_offset", DefaultValue = 0, MinValue = -1000, MaxValue = 1000)]
		public int MAAoffset { get; set; }
		
		[Parameter("MAAver_method", DefaultValue = MaMethods.Ema)]
		public MaMethods MAAmethod { get; set; }
		
		[Parameter("MAAver_Calculated_Price", DefaultValue = PriceMode.Median)]
        public PriceMode MAAapplyto { get; set; }
		
		
		[Parameter("MAFast_Period", DefaultValue = 10, MinValue = 2)]
        public int MAFperiod { get; set; }

		[Parameter("MAFast_offset", DefaultValue = 0, MinValue = -1000, MaxValue = 1000)]
		public int MAFoffset { get; set; }
		
		[Parameter("MAFast_method", DefaultValue = MaMethods.Ema)]
		public MaMethods MAFmethod { get; set; }
		
		[Parameter("MAFast_Calculated_Price", DefaultValue = PriceMode.Weighted)]
        public PriceMode MAFapplyto { get; set; }
		
		
		
		
		[Parameter("Current Lots :", DefaultValue = 0.1, MinValue = 0.01, MaxValue = 10.0)]
		public double vol { get;set; }
		
		[Parameter("Fractal :", DefaultValue = 10, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }

		
		
		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"ZxZLog")]
		public string LogPath { get;set; }

		
		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 15);

		public static int magicNumber = 54321;

		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		
		
		public static int ActiveBuyCount = 0;
		public static int ActiveSellCount = 0;
		
		public ArrowUp cArrowUp;
		public ArrowDown cArrowDown;
		private string trueLogPath = "";

		public static StreamWriter LogSW = null;

		
		private MovingAverage MAHa;
		private MovingAverage MAAa;
		private MovingAverage MAFa;
		private Fractals FRAC;
		
		private int ci = 0;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			#region	GetIndicatorsAndFrames // Get Indicators' references...           
			
			Print("Starting TS on account: {0} ", this.Account.Number);
         	
			InitLogFile();

			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
			
			MAHa = GetIndicator<MovingAverage>(
												Instrument.Id,
												_period,
													MAHperiod,
														MAHoffset,
															MAHmethod,
																MAHapplyto
															);
			MAAa = GetIndicator<MovingAverage>(
												Instrument.Id,
												_period,
													MAAperiod,
														MAAoffset,
															MAAmethod,
																MAAapplyto
															);
			MAFa = GetIndicator<MovingAverage>(
												Instrument.Id,
												_period,
													MAFperiod,
														MAFoffset,
															MAFmethod,
																MAFapplyto
															);
			FRAC = GetIndicator<Fractals>(
											Instrument.Id,
											_period,
													frac
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
			if(Bars.Range.To <= 0) 
			{
				return;
			}
			else
			{	
				ci = Bars.Range.To -1;
			}
			
			ActiveBuyCount = 0;
			ActiveSellCount = 0;
			
			IPosition[] activePositions = Trade.GetActivePositions(magicNumber);
			if(activePositions != null)
			{
				for(int p = 0; p < activePositions.Length; p++)
				{
					if((int)activePositions[p].Type == (int)ExecutionRule.Buy)
					{
						ActiveBuyCount++;
						continue;
					}
					if((int)activePositions[p].Type == (int)ExecutionRule.Sell)
					{
						ActiveSellCount++;
						continue;
					}
				}
			}
			
			
			
			
			if((MAFa.SeriesMa[ci] < MAAa.SeriesMa[ci]) && (MAFa.SeriesMa[ci] < MAHa.SeriesMa[ci]) && (ActiveSellCount == 0))
			{
				SetSellOrder();
			}
			if((MAFa.SeriesMa[ci] > MAAa.SeriesMa[ci]) && (MAFa.SeriesMa[ci] > MAHa.SeriesMa[ci]) && (ActiveBuyCount  == 0))			
			{
				SetBuyOrder();
			}
			
			TrailActiveOrders();
			
        }

		protected void SetBuyOrder()
		{
			double SL = getSL(1);
			
			double TP = 0.0;
			
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
			double SL = getSL(0);
			
			double TP = 0.0;

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
		
		protected void TrailActiveOrders()
		{
			IPosition[] activePosition = Trade.GetActivePositions(magicNumber);
			if(activePosition == null) return;
			
			for(int p = 0; p < activePosition.Length; p++)
			{
				if((int)activePosition[p].Type == (int)ExecutionRule.Buy)
				{
					TradeResult r = null;
			
					r = Trade.UpdateMarketPosition(	 
											 activePosition[p].Id, 
											 getSL(1), 
											 null, 
											 "- update Buy.."
										 );
					if(r==null)
					{
						XXPrint("*!* Can't modify  Buy position at Price {0}",Instrument.Bid);
					}
				}

				if((int)activePosition[p].Type == (int)ExecutionRule.Sell)
				{
					TradeResult r = null;
			
					r = Trade.UpdateMarketPosition(	 
											 activePosition[p].Id, 
											 getSL(0), 
											 null, 
											 "- update Sell."
										 );
					if(r==null)
					{
						XXPrint("*!* Can't modify Sell position at Price {0}",Instrument.Bid);
					}
				}
			}
		}
		
		protected double getSL(int type)
		{
			int cii = ci;
			switch(type)
			{
				case 0:
						{
							double MAX = 0.0;
							while(cii>=0)
							{
								if(FRAC.TopSeries[cii].Equals(double.NaN)) 
								{
									XXPrint("TopSeries = {0}",FRAC.TopSeries[cii]);
									cii--;
									continue;
								}
								MAX = FRAC.TopSeries[cii];
								break;
							}
							return Math.Round(MAX, Instrument.PriceScale);
/*							
							double MAX = double.MinValue;
							for(int i = 0; i < (frac + 100); i++)
							{
								if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX, Instrument.PriceScale);*/
						}
				case 1:
						{
							double MIN = 0.0;
							while(cii>=0)
							{
								if(FRAC.BottomSeries[cii].Equals(double.NaN)) 
								{
									XXPrint("BottomSeries = {0}",FRAC.BottomSeries[cii]);
									cii--;
									continue;
								}
								MIN = FRAC.BottomSeries[cii];
								break;
							}
							return Math.Round(MIN, Instrument.PriceScale);
							
/*							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN, Instrument.PriceScale);*/
						}
				default: 
					break;
			}
			return 0.0;
		}
			
		
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
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