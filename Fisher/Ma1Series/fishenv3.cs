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
    [TradeSystem("FishEnv")]
    public class fishenv : TradeSystem
    {
        // Simple parameter example
/// <summary>
/// 
/// </summary>
		[Parameter("Fisher_LR_Period", DefaultValue = 12, MinValue = 1)]
        public int FisherPeriod { get; set; }

        [Parameter("Fisher_LR_Ma1Period", DefaultValue = 6, MinValue = 1)]
        public int FisherMa1Period { get; set; }

        [Parameter("Fisher_LR_Ma1Method", DefaultValue = MaMethods.Ema)]
        public MaMethods FisherMa1Method { get; set; }

        [Parameter("Fisher_LR_Ma2Period", DefaultValue = 18, MinValue = 1)]
        public int FisherMa2Period { get; set; }

        [Parameter("Fisher_LR_Ma2Method", DefaultValue = MaMethods.Ema)]
        public MaMethods FisherMa2Method { get; set; }

/// <summary>
/// 
/// </summary>
		[Parameter("TP_Envelope_period", DefaultValue = 23, MinValue = 1, MaxValue = 100)]
		public int envelopesTPperiod { get;set; }
		
		[Parameter("TP_Envelope_deviation", DefaultValue = 0.300, MinValue = 0.000, MaxValue = 3.000)]
		public double envelopesTPdev { get;set; }

		[Parameter("TP_Envelope_Shift", DefaultValue = 0, MinValue = -100, MaxValue = 100)]
		public int envelopesTPshift { get;set; }

		[Parameter("TP_Envelope_Method", DefaultValue = MaMethods.Ema)]
		public MaMethods envelopesTPmethod { get;set; }

		[Parameter("TP_Envelope_Method", DefaultValue = PriceMode.Median)]
		public PriceMode envelopesTPprice { get;set; }

/// <summary>
/// 
/// </summary>		
		
//		[Parameter("Orders :", DefaultValue = 2, MinValue= 1, MaxValue = 100)]
//		public int orderLimit { get;set; }

		[Parameter("Current Lots :", DefaultValue = 0.1, MinValue = 0.01, MaxValue = 10.0)]
		public double vol { get;set; }
		
		[Parameter("Fractal :", DefaultValue = 7, MinValue = 2, MaxValue = 200)]
		public int frac { get;set; }

		
		
		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"ZxZLog")]
		public string LogPath { get;set; }

		
		
		
		
		
		
		private ISeries<Bar> _barSeries;
		private Period _period = new Period(PeriodType.Minute, 15);

		public static int magicNumber = 57557;

		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		
		
		public static int ActiveBuyCount = 0;
		public static int ActiveSellCount = 0;
		
		public ArrowUp cArrowUp;
		public ArrowDown cArrowDown;
		private string trueLogPath = "";

		public static StreamWriter LogSW = null;

		private FisherTransformOscillator FTOind;
		private Envelopes EnvindTP;
		
		public static int ci = 0;
		public static bool buyEnable = false;
		public static bool sellEnable = false;
		
		protected override void Init()
        {
            // Event occurs once at the start of the strategy
			#region	GetIndicatorsAndFrames // Get Indicators' references...           
			
			Print("Starting TS on account: {0} ", this.Account.Number);
         	
			InitLogFile();

			_period = Timeframe;
			_barSeries = GetCustomSeries(Instrument.Id, 
											_period);
			FTOind = GetIndicator<FisherTransformOscillator>(
												Instrument.Id,
												_period,
													FisherPeriod,
														FisherMa1Period,
														FisherMa1Method,
															FisherMa2Period,
															FisherMa2Method);
 			EnvindTP = GetIndicator<Envelopes>(
												Instrument.Id,
												_period,
													envelopesTPperiod,
														envelopesTPdev,
															envelopesTPshift,
																envelopesTPmethod,
																	envelopesTPprice);
			

			#endregion //GetIndicatorsAndFrames		
            // Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			ci = Bars.Range.To - 1;
			if(
				FTOind.Ma1Series[ci].Equals(double.NaN)
				||
				FTOind.Ma2Series[ci].Equals(double.NaN)
				||
				EnvindTP.SeriesDown[ci].Equals(double.NaN)
				||
				EnvindTP.SeriesUp[ci].Equals(double.NaN)
				||
				Instrument.Ask.Equals(double.NaN)
				||
				Instrument.Bid.Equals(double.NaN)
			   )
			{
				return;
			}
			
			TrailActiveOrders();

			if(	
				FTOind.Ma1Series[ci] < FTOind.Ma2Series[ci] 
				&&
				FTOind.Ma1Series[ci-1] >= FTOind.Ma2Series[ci-1]  
			  )
			{
				sellEnable = true;
				buyEnable = false;
			}
			
			if(	
				FTOind.Ma1Series[ci] > FTOind.Ma2Series[ci] 
				&&
				FTOind.Ma1Series[ci-1] <= FTOind.Ma2Series[ci-1]  
			  )
			{
				sellEnable = false;
				buyEnable = true;
			}
			
			if(
			  	FTOind.DownSeries[ci] != 0.0
				&&
				sellEnable
			   )
			{
				SetSellOrder();
				sellEnable = false;
			}
			
			if(
			  	FTOind.UpSeries[ci] != 0.0
				&&
				buyEnable
			   )
			{
				SetBuyOrder();
				buyEnable = false;
			}
			
        }
    

		protected void TrailActiveOrders()
		{
			
			IPosition[] activeposition = Trade.GetActivePositions(magicNumber);
			
			if(activeposition != null)
			{
				for(int p = 0; p < activeposition.Length; p++)
				{
					TradeResult tr = null;
					if(activeposition[p].Type == ExecutionRule.Buy)
					{
						tr = Trade.UpdateMarketPosition(
								activeposition[p].Id,
								getSL(1),
								getTP(1),
								" - update TP,SL"
														);
						if(tr == null)
						{
							XXPrint("Can't update  buy position {0}", activeposition[p].Id);
						}
					}
					if(activeposition[p].Type == ExecutionRule.Sell)
					{
						tr = Trade.UpdateMarketPosition(
								activeposition[p].Id,
								getSL(0),
								getTP(0),
								" - update TP,SL"
														);
						if(tr == null)
						{
							XXPrint("Can't update sell position {0}", activeposition[p].Id);
						}
					}
				}
			}
		}

		protected void SetBuyOrder()
		{
			double SL = getSL(1);
			double TP = getTP(1);
			
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
			double TP = getTP(0);

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
		
		protected double getSL(int type)
		{
			switch(type)
			{
				case 0:
						{
							double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX, Instrument.PriceScale);
						}
				case 1:
						{
							double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{
								if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN, Instrument.PriceScale);
						}
				default: 
					break;
			}
			return 0.0;
		}
		
		protected double getTP(int type)
		{
			switch(type)
			{
				case 0:
					return Math.Round(EnvindTP.SeriesDown[ci],Instrument.PriceScale);	
				case 1:
					return Math.Round(EnvindTP.SeriesUp[ci],Instrument.PriceScale);	
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