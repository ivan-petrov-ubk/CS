using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using System.Collections.Generic;
using System.IO;

namespace IPro.TradeSystems
{
    [TradeSystem("AOsto")]
    public class AOsto : TradeSystem
    {
        // Simple parameter example
		[Parameter("Kp :", DefaultValue = 10)]
		public int Kp { get; set; }

		[Parameter("Dp :", DefaultValue = 8)]
		public int Dp { get; set; }
		
		[Parameter("Kslow :", DefaultValue = 6)]
		public int Kslow { get; set; }
		
		[Parameter("Method :", DefaultValue = MaMethods.Sma )]
		public MaMethods _method { get; set; }

		[Parameter("PricePair :", DefaultValue = PricePair.LowHigh)]
		public PricePair _pricePair { get; set; }

		
		
		
		// another parameters....
		[Parameter("Use Static SL", DefaultValue = false)]
		public bool UseStaticSL { get; set; }
		
		[Parameter("Period N:", DefaultValue = 10, MinValue = 2)]
		public int PeriodN { get; set; }
		
		[Parameter("Static SL :", DefaultValue = 800)]
		public int StaticStopLoss { get; set; }

		[Parameter("Lots :", DefaultValue = 0.1)]
		public double vol { get; set; }

		
		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"StohashLog")]
		public string LogPath { get;set; }


		
		private Period _period = new Period(PeriodType.Minute, 15);

		public static int magicNumber = 74589;

		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		
		
		public ArrowUp cArrowUp;
		public ArrowDown cArrowDown;
		private string trueLogPath = "";

		public static StreamWriter LogSW = null;

		private StochasticOscillator sto;
		private AcceleratorOscillator ao;

		private static int index = 0; 		
		
		private static bool setsell = false;
		private static bool setbuy = false;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            // Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        

			_period = Timeframe;
			index = Bars.Range.To - 1; 		

			sto = GetIndicator<StochasticOscillator>(
													Instrument.Id,
													_period ,
														Kp,
															Dp,
																Kslow,
																	_method,
																		_pricePair
													);
	
			ao = GetIndicator<AcceleratorOscillator>(
													Instrument.Id,
													_period 
													);
		
		}        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			index = Bars.Range.To - 1;
			
			if( sto.MainLine[index] >= 80.0 
				&& sto.SignalLine[index] >= 80.0 
				&& sto.MainLine[index - 1] >= sto.SignalLine[index - 1] 
				&& sto.MainLine[index] < sto.SignalLine[index])
			{
				//sell
				setsell = true;
				setbuy = false;
				
				CloseBuyOrders();
			}
			if(sto.MainLine[index] <= 20.0 
				&& sto.SignalLine[index] <= 20.0 
				&& sto.MainLine[index - 1] <= sto.SignalLine[index - 1] 
				&& sto.MainLine[index] > sto.SignalLine[index])
			{
				//buy
				setsell = false;
				setbuy = true;
				
				CloseSellOrders();
			}
			if(sto.MainLine[index] > 20.0 
				&& sto.SignalLine[index] > 20.0 
				&& sto.MainLine[index] < 80.0 
				&& sto.SignalLine[index] < 80.0)
			{
				setsell = false;
				setbuy = false;
			}	
			
			if(ao.SeriesDown[index] != double.NaN && setsell)			
			{
				SetSellOrder();
				setsell = false;
			}
			if(ao.SeriesUp[index]   != double.NaN && setbuy)
			{
				SetBuyOrder();
				setbuy = false;
			}
			
			SetUpArrows();
			
        }

		protected double getSL(int Ordertype)
		{
			switch(Ordertype)
			{	
				case 0:
				{
					if(UseStaticSL) return Math.Round(Instrument.Ask + StaticStopLoss * Instrument.Point, Instrument.PriceScale); 
					double MaxVal = double.MinValue;
					for(int i = index; i > index - PeriodN; i--)
					{
						if(Bars[i].High > MaxVal)
						{
							MaxVal = Bars[i].High;
						}
					}
					return Math.Round(MaxVal, Instrument.PriceScale);
				}
				case 1:
				{
					if(UseStaticSL) return Math.Round(Instrument.Bid - StaticStopLoss * Instrument.Point, Instrument.PriceScale); 
					double MinVal = double.MaxValue;
					for(int i = index; i > index - PeriodN; i--)
					{
						if(Bars[i].High < MinVal)
						{
							MinVal = Bars[i].High;
						}
					}
					return Math.Round(MinVal, Instrument.PriceScale);
				}
			}
			return 0.0;
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

 		protected void CloseBuyOrders()
		{
			IPosition[] activePositions = Trade.GetActivePositions(magicNumber);
			if(activePositions != null)
			{	
				for(int i = activePositions.GetLength(0) - 1; i >=0; i--)
				{
					TradeResult tR = null;
					if((int)activePositions[i].Type == (int)ExecutionRule.Buy)
						// && activePositions[i].State == OrderState.Executed)
						tR = Trade.CloseMarketPosition(	activePositions[i].Id,
      													activePositions[i].Lots,
														Instrument.Bid,
														-1,
														"Closed on changed TP,SL");
					if(tR==null)
					{
						XXPrint("*!* Can't close Buy position {1} at Price {0}",Instrument.Ask,activePositions[i].Id);
					}
				}
			}
		}

		protected void CloseSellOrders()
		{
			IPosition[] activePositions = Trade.GetActivePositions(magicNumber);
			if(activePositions != null)
			{	
				for(int i = activePositions.GetLength(0) - 1; i >=0; i--)
				{
					TradeResult tR = null;
					if((int)activePositions[i].Type == (int)ExecutionRule.Sell)
						// && activePositions[i].State == OrderState.Executed)
						tR = Trade.CloseMarketPosition(	activePositions[i].Id,
      													activePositions[i].Lots,
														Instrument.Ask,
														-1,
														"Closed on changed TP,SL");
					if(tR==null)
					{
						XXPrint("*!* Can't close Sell position {1} at Price {0}",Instrument.Ask,activePositions[i].Id);
					}
				}
			}
		}

		protected void CloseAllOrders()
		{
			IPosition[] activePositions = Trade.GetActivePositions(magicNumber);
			if(activePositions != null)
			{	
				for(int i = activePositions.GetLength(0) - 1; i >=0; i--)
				{
					TradeResult tR = null;
					tR = Trade.CloseMarketPosition(	activePositions[i].Id,
      													activePositions[i].Lots,
														(int)activePositions[i].Type == (int)ExecutionRule.Sell ? 
														Instrument.Ask : 
														Instrument.Bid,
														-1,
														"Closed on changed TP,SL");
					if(tR==null)
					{
						XXPrint("*!* Can't close position {1} at Price {0}",Instrument.Ask,activePositions[i].Id);
					}
				}
			}
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