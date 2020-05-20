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
    [TradeSystem("AOsto4")]   //copy of "AOsto3"
    public class AOsto : TradeSystem
    {
        // Simple parameter example
		[Parameter("Buy", DefaultValue = true)]
		public bool TBuy { get;set; }
		[Parameter("Sell", DefaultValue = true)]
		public bool TSell { get;set; }
		
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
		
		[Parameter("Static SL :", DefaultValue = 250)]
		public int StaticStopLoss { get; set; }

		[Parameter("Lots :", DefaultValue = 0.1)]
		public double vol { get; set; }	
		
		private static int ci = 0; 	
		public static int magicNumber = 74589;
		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		
		private double sF1,sF2,mF1,mF2;
		private StochasticOscillator sto;
		public FisherTransformOscillator _ftoInd;
		private static bool setsell = false;
		private static bool setbuy = false;
		
        protected override void Init()
        {	ci = Bars.Range.To - 1; 		
			sto = GetIndicator<StochasticOscillator>(Instrument.Id,Timeframe ,Kp,Dp,Kslow,_method,_pricePair);
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
		}        

        protected override void NewBar()
        {
			ci = Bars.Range.To - 1;
			if( sto.MainLine[ci] >= 80.0 || (_ftoInd.FisherSeries[ci-1]>0 && _ftoInd.FisherSeries[ci]<0))
			{
				CloseBuyOrders();  // 1
				//CloseSellOrders(); // 2
			}
			
			if(sto.MainLine[ci] <= 20.0 || (_ftoInd.FisherSeries[ci-1]<0 && _ftoInd.FisherSeries[ci]>0))
			{

				CloseSellOrders(); //1
				//CloseBuyOrders();  //2
			}
	
			if(sto.MainLine[ci] <= 80.0 && sto.MainLine[ci-1] >= 80.0 && _ftoInd.FisherSeries[ci]<0)			
			{
				SetSellOrder(); //1
				//SetBuyOrder(); //2
			}
			
			if(sto.MainLine[ci] >= 20.0 && sto.MainLine[ci-1] <= 20.0 && _ftoInd.FisherSeries[ci]>0)
			{
				SetBuyOrder(); //1
				//SetSellOrder(); //2
			}
			Print("{0}  sto={1}  fs={2}",Bars[ci].Time,sto.MainLine[ci],_ftoInd.FisherSeries[ci]);
        }

		protected double getSL(int Ordertype)
		{
			switch(Ordertype)
			{	
				case 0:
				{
					if(UseStaticSL) return Math.Round(Instrument.Ask + StaticStopLoss * Instrument.Point, Instrument.PriceScale); 
					double MaxVal = double.MinValue;
					for(int i = ci; i > ci - PeriodN; i--)
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
					for(int i = ci; i > ci - PeriodN; i--)
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
			r = Trade.OpenMarketPosition(Instrument.Id,ExecutionRule.Buy,vol,Instrument.Ask,-1,st,"Buy..",magicNumber);
			
		}
		
		protected void SetSellOrder()
		{
			double SL = getSL(0);
			double TP = 0.0;
			Stops st = Stops.InPrice(SL,TP);
			TradeResult r = null;
			r = Trade.OpenMarketPosition(Instrument.Id,ExecutionRule.Sell,vol,Instrument.Bid,-1,st,"Sell.",magicNumber);
			
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
					
				}
			}
		}



	}
}