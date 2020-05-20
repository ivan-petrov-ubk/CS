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
    [TradeSystem("MA-H1")]
    public class MAH1 : TradeSystem
    {
		private static int index = 0; 
		
		private MovingAverage _ma1B;		
		private MovingAverage _ma2B;

		private MovingAverage _ma1R;		
		private MovingAverage _ma2R;
		
		// MAB1
		private int _maPeriod1B = 9;
        private int _maShift1B = 0;
        private MaMethods _maMethod1B = MaMethods.Ema;
        private PriceMode _priceMode1B = PriceMode.Close;

		// MAB2
		private int _maPeriod2B = 30;
        private int _maShift2B = 0;
        private MaMethods _maMethod2B = MaMethods.Ema;
        private PriceMode _priceMode2B = PriceMode.Close;

		// MAR1
		private int _maPeriod1R = 200;
        private int _maShift1R = 0;
        private MaMethods _maMethod1R = MaMethods.Sma;
        private PriceMode _priceMode1R = PriceMode.Close;

		// MAR2
		private int _maPeriod2R = 8;
        private int _maShift2R = 0;
        
		private MaMethods _maMethod2R = MaMethods.Lwma;
        private PriceMode _priceMode2R = PriceMode.Close;
		
		// another parameters....
		[Parameter("Use Static SL", DefaultValue = false)]
		public bool UseStaticSL { get; set; }
		
		[Parameter("Period N:", DefaultValue = 10, MinValue = 2)]
		public int PeriodN { get; set; }
		
		[Parameter("Static SL :", DefaultValue = 250)]
		public int StaticStopLoss { get; set; }

		[Parameter("Lots :", DefaultValue = 0.1)]
		public double vol { get; set; }

		

		public static int magicNumber = 74589;

		public static Guid cAUG = Guid.Empty; 		
		public static Guid cADG = Guid.Empty; 		

		private static bool setsell = false;
		private static bool setbuy = false;

		private Momentum _momentInd;
		
        protected override void Init()
        {

          _ma1B = GetIndicator<MovingAverage>(Instrument.Id, Timeframe,_maPeriod1B, _maShift1B, _maMethod1B, _priceMode1B);
          _ma2B = GetIndicator<MovingAverage>(Instrument.Id, Timeframe,_maPeriod2B, _maShift2B, _maMethod2B, _priceMode2B);
			
		 _momentInd= GetIndicator<Momentum>(Instrument.Id, Timeframe);
			_momentInd.Period=100;
         _ma1R = GetIndicator<MovingAverage>(Instrument.Id, Timeframe,_maPeriod1R, _maShift1R, _maMethod1R, _priceMode1R);
          //_ma2R = GetIndicator<MovingAverage>(Instrument.Id, Timeframe,_maPeriod2R, _maShift2R, _maMethod2R, _priceMode2R);
		}        

       
        protected override void NewBar()
        {	
			index = Bars.Range.To - 1;
		
           if(_ma1B.SeriesMa[index-1]>_ma2B.SeriesMa[index-1] && 
			   _ma1B.SeriesMa[index]<_ma2B.SeriesMa[index] && 
			   //_momentInd.SeriesMomentum[index]>100 && 
			   Bars[index].Close<_ma1R.SeriesMa[index]) 
          	 SetSellOrder();

           if(_ma1B.SeriesMa[index-1]<_ma2B.SeriesMa[index-1] && 
			   _ma1B.SeriesMa[index]>_ma2B.SeriesMa[index] && 
			   //_momentInd.SeriesMomentum[index]>100 && 
			   Bars[index].Close>_ma1R.SeriesMa[index]) 
          	 SetBuyOrder(); 
		   
		   if(_ma1B.SeriesMa[index]<_ma2B.SeriesMa[index]) CloseBuyOrders();
		   if(_ma1B.SeriesMa[index]>_ma2B.SeriesMa[index]) CloseSellOrders();	   
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
			if(!setbuy) { setbuy=true; 
			r = Trade.OpenMarketPosition(Instrument.Id,ExecutionRule.Buy,vol,Instrument.Ask,-1,st,"Buy..",magicNumber);
			}
		}
		
		protected void SetSellOrder()
		{
			double SL = getSL(0);
			double TP = 0.0;
			Stops st = Stops.InPrice(SL,TP);
			TradeResult r = null;
			if(!setsell) {
			r = Trade.OpenMarketPosition(Instrument.Id,ExecutionRule.Sell,vol,Instrument.Bid,-1,st,"Sell.",magicNumber);
			setsell=true; }
		}

 		protected void CloseBuyOrders()
		{    setbuy=false;
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
		{    setsell=false;
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