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
    [TradeSystem("Stohash")]
    public class Stohash : TradeSystem
    {
        // Parameters for Stochastic Oscillator
		[Parameter("Kp", DefaultValue = 10)]
		public int Kp { get; set; }

		[Parameter("Dp", DefaultValue = 8)]
		public int Dp { get; set; }
		
		[Parameter("Kslow", DefaultValue = 6)]
		public int Kslow { get; set; }
		
		[Parameter("Method", DefaultValue = MaMethods.Sma )]
		public MaMethods _method { get; set; }

		[Parameter("PricePair", DefaultValue = PricePair.LowHigh)]
		public PricePair _pricePair { get; set; }

		// Trade parameters....
		[Parameter("Static SL", DefaultValue = 600)]
		public int StaticStopLoss { get; set; }

		[Parameter("Lot", DefaultValue = 0.1)]
		public double vol { get; set; }

		[Parameter("Debug_Mode", DefaultValue = true)]
		public bool DebugMode { get;set; }
		
		[Parameter("Log_Mode", DefaultValue = false)]
		public bool LogMode { get;set; }

		[Parameter("Log_Path", DefaultValue = @"StohashLog")]
		public string LogPath { get;set; }

		private StochasticOscillator sto;
		private HeikenAshi ash;
		private string trueLogPath = "";
		private static int index = 0; 		

		public static int magicNumber = 74551;
		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		
		public static StreamWriter LogSW = null;

		protected override void Init()
        {
			index = Bars.Range.To - 1;
			ash = GetIndicator<HeikenAshi>(Instrument.Id, Timeframe);
			sto = GetIndicator<StochasticOscillator>(Instrument.Id, Timeframe ,Kp,Dp,Kslow,_method, _pricePair);
		}        

        protected override void NewBar()
        {
			index = Bars.Range.To - 1;
			XXPrint("sto.ML[{0}] = {1}", index, sto.MainLine[index]);
			
			if(double.IsNaN(sto.MainLine[index]))
			{
				XXPrint("*!*  - NaN in main data !! [{0}]",sto.MainLine[index]);
				return;
			}
			
			if(sto.MainLine[index - 1] > 80.0 && sto.MainLine[index] < 80.0)
			{
				SetSellOrder();
			}

			if(sto.MainLine[index - 1] < 20.0 && sto.MainLine[index] > 20.0)
			{
				SetBuyOrder();
			}
			
			if(sto.MainLine[index] > 80.0 && sto.SignalLine[index] > 80.0)
			{
				if(ash.CloseSeries[index] < ash.OpenSeries[index]) 
				{
					CloseBuyOrders();
				}
			}
			if(sto.MainLine[index] < 20.0 && sto.SignalLine[index] < 20.0)
			{	
				if(ash.CloseSeries[index] > ash.OpenSeries[index]) 
				{
					CloseSellOrders();
				}
			}
		}

		protected void SetBuyOrder()
		{
			double SL = Math.Round(Instrument.Ask - StaticStopLoss * Instrument.Point, Instrument.PriceScale);
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
			
			if(r==null || !r.IsSuccessful)
			{
				XXPrint("*!* Can't open Buy position at Price {0}",Instrument.Ask);
			}
		
		}
		
		protected void SetSellOrder()
		{
			double SL = Math.Round(Instrument.Ask + StaticStopLoss * Instrument.Point, Instrument.PriceScale); 
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
			if(r==null || !r.IsSuccessful)
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
						tR = Trade.CloseMarketPosition(	activePositions[i].Id,
      													activePositions[i].Lots,
														Instrument.Bid,
														-1,
														"Closed on changed TP,SL");
					if(tR==null || !tR.IsSuccessful)
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
						tR = Trade.CloseMarketPosition(	activePositions[i].Id,
      													activePositions[i].Lots,
														Instrument.Ask,
														-1,
														"Closed on changed TP,SL");
					if(tR==null || !tR.IsSuccessful)
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
					if(tR==null || !tR.IsSuccessful)
					{
						XXPrint("*!* Can't close position {1} at Price {0}",Instrument.Ask,activePositions[i].Id);
					}
				}
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
    }
}