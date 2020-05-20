//using System;
//using IPro.Model.MarketData;
//using IPro.Model.Trade;
//using IPro.Model.Client.Trade;
//using IPro.Model.Programming;
//using IPro.Model.Programming.Indicators;
//using IPro.Model.Programming.Chart;
//using IPro.Model.Programming.TradeSystems;

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
//using System.IO;
//using System.Globalization;
//using System.Timers;

// using System.Data;

namespace IPro.TradeSystems
{
    [TradeSystem("PAVRYKNo1")]
    public class PAVRYKNo1 : TradeSystem
    {
		private ISeries<Bar> _barSeries;
		private ISeries<Bar> _PivotBarSeries;
		
/// <summary>
/// Time-frame for using in system...
/// </summary
		private Period _pivotPeriod = new Period(PeriodType.Minute, 15);
		private Period _period = new Period(PeriodType.Minute, 15);

/// <summary>
/// Fisher indicator initialization
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

       
		[Parameter("MA5_Period", DefaultValue = 5, MinValue = 2)]
        public int MA5period { get; set; }

		[Parameter("MA5_offset", DefaultValue = 1, MinValue = -1000, MaxValue = 1000)]
		public int MA5offset { get; set; }
		
		[Parameter("MA5_method", DefaultValue = MaMethods.Ema)]
		public MaMethods MA5method { get; set; }
		
		[Parameter("MA5_Calculated_Price", DefaultValue = PriceMode.Close)]
        public PriceMode MA5applyto { get; set; }

		
		[Parameter("MA24_Period", DefaultValue = 24, MinValue = 2)]
        public int MA24period { get; set; }

		[Parameter("MA24_offset", DefaultValue = 24, MinValue = -1000, MaxValue = 1000)]
		public int MA24offset { get; set; }
		
		[Parameter("MA24_method", DefaultValue = MaMethods.Ema)]
		public MaMethods MA24method { get; set; }
		
		[Parameter("MA24_Calculated_Price", DefaultValue = PriceMode.Close)]
        public PriceMode MA24applyto { get; set; }

		[Parameter("\"Fractal\"_steps ", DefaultValue = 2, MinValue = 2,MaxValue = 10)]
        public int fsteps;
		
		[Parameter("Start_Shift", DefaultValue = 0, MinValue = 0,MaxValue = 100)]
        public int barDelay;
	
		[Parameter("Use_Level_S1_R1)", DefaultValue = true)]
        public bool LevelSR1;
		[Parameter("Ordes_LOTS", DefaultValue = 0.5)]
        public double vol;

		public struct orderSL
		{
			public double _BuySL;
			public double _SellSL;
			public orderSL(double BuySL,double SellSL)
			{
				this._BuySL=BuySL;
				this._SellSL=SellSL;
			}
		
		}
	

		private /*public*/ FisherTransformOscillator FTOind;
		
		private /*public*/ Pivot Pivind;

		private /*public*/ MovingAverage MA5ind;
		private /*public*/ MovingAverage MA24ind;
		
//		public Fractal FRCind;
		
		
		private /*public*/ double FTOFsS;
		private /*public*/ double FTODown;
		private	/*public*/ double FTOUp;
		
		private	/*public*/ double FTOMA1;
		private	/*public*/ double FTOMA2;
		
		
		
		private /*public*/ double PivotVal;

		private /*public*/ double PivotS1;
		private /*public*/ double PivotS2;
		private /*public*/ double PivotS3;
		
		private /*public*/ double PivotR1;
		private /*public*/ double PivotR2;
		private /*public*/ double PivotR3;
		
		private /*public*/ orderSL currOrderSL=new orderSL(0.0,0.0);
		private /*public*/ orderSL prevOrderSL=new orderSL(0.0,0.0);
		
		private int _CIndex=-1; 
		private int _pivotIndex=-1; 

		private /*public*/ int  magicNumber = 777;

		private /*public*/ bool MA5UpDown = false;		
		private /*public*/ bool MA5DownUp = false;		
		
		protected override void Init()
        {
            // Event occurs once at the start of the strategy
            
			Print("Starting {2} on account: {0}, comment: {1}, date: {3}", this.Account.Number, "Started!","PAVRYKNo1",DateTime.Now);
	
			try 
			{
			_barSeries = GetCustomSeries(Instrument.Id, _period);
			_PivotBarSeries = GetCustomSeries(Instrument.Id, _pivotPeriod);
			/// fisher activation
			FTOind  = GetIndicator<FisherTransformOscillator>(
												Instrument.Id,
												_period,
												FisherPeriod,
												FisherMa1Period,FisherMa1Method,
												FisherMa2Period,FisherMa2Method);
			
			/// MA5  activation
			MA5ind  = GetIndicator<MovingAverage>(
												Instrument.Id,
												_period,
												MA5period,
												MA5offset,
												MA5method,
												MA5applyto);
			
			/// MA24 activation
			MA24ind = GetIndicator<MovingAverage>(
												Instrument.Id,
												_period,
												MA24period,
												MA24offset,
												MA24method,
												MA24applyto);
		
			/// pivot activation
			Pivind  = GetIndicator<Pivot>(		
												Instrument.Id, 
									   			 _period);	 //_pivotPeriod,
												//96, //1440, quantity of bars in 1 day?
												//PivotType.Pivot);
			}
			catch(Exception ex)
			{
				Print("[*Init*]:"+ex.Message);
				Print("[*Init*]:"+ex.StackTrace);
			}
		
			CalculateSL(0);
		
		
		
		}        
	

        protected override void NewQuote()
        {
			// Event occurs on every new quote
		}
        
        protected override void NewBar()
        {
			try 
			{
				
				if (_CIndex >= _barSeries.Range.To - 1) return;
				_CIndex = _barSeries.Range.To - 1;//TransformIndex(_barSeries.Range.To/*-barDelay*/, _period, _period);
			
				if (_pivotIndex >= _barSeries.Range.To - 1) return;
			
				_pivotIndex = _barSeries.Range.To - 1;

				//_pivotIndex = TransformIndex(_PivotBarSeries.Range.To/*-barDelay*/,_period  /**/,_pivotPeriod); //_period);

//_CIndex = Bars.Range.To-barDelay;
//             
//			Print("[CB] PaVrYk - bars:{0}",_CIndex );
//			if(_CIndex<100) 
//			{
//				Print("[C<100] PaVrYk - bars:{0}",_CIndex );
//				return;
//			}
//------??			
				FTOFsS  = FTOind.FisherSeries[_CIndex];
	
				FTODown = FTOind.DownSeries[_CIndex];
				FTOUp   = FTOind.UpSeries[_CIndex];
            	FTOMA1  = FTOind.Ma1Series[_CIndex];
            	FTOMA2  = FTOind.Ma2Series[_CIndex];

			//1
				Print( " Index = {5}:: FTO >> {0}::{1}:{2}::{3}:{4}",
						FTOFsS, FTODown, FTOUp, FTOMA1, FTOMA2, _CIndex);
			 
				PivotVal = Pivind.PivotSeries[_pivotIndex];
				
				PivotS1 = Pivind.S1[_pivotIndex];
				PivotS2 = Pivind.S2[_pivotIndex];
				PivotS3 = Pivind.S3[_pivotIndex];
				PivotR1 = Pivind.R1[_pivotIndex];
				PivotR2 = Pivind.R2[_pivotIndex];
				PivotR3 = Pivind.R3[_pivotIndex];
				
				if(PivotVal.Equals(double.NaN)) return;
			//2				
				Print(	" Index = {5}:: PVT >> {0}::{1}:{2}::{3}:{4}",
						PivotVal, PivotS1, PivotS2, PivotR1, PivotR2, _pivotIndex);

			
			
			
				CalculateSL(1);
			
			
				MA5UpDown=	(
								(MA5ind.SeriesMa[_CIndex-1-barDelay]>MA24ind.SeriesMa[_CIndex-1-barDelay])
								&&
								(MA5ind.SeriesMa[_CIndex-barDelay]<MA24ind.SeriesMa[_CIndex-barDelay])
							);

				MA5DownUp=	(
								(MA5ind.SeriesMa[_CIndex-1-barDelay]<MA24ind.SeriesMa[_CIndex-1-barDelay])
								&&
								(MA5ind.SeriesMa[_CIndex-barDelay]>MA24ind.SeriesMa[_CIndex-barDelay])
							);
				
			
				
				if(MA5DownUp)
				{
			//3
					Print("MA5 UP : {0}", MA5DownUp);
					
					if((FTOUp.Equals(double.NaN) || FTOUp==null) && FTOMA1<FTOMA2)
					{
						Print("[S] Signal \"Sell\""+DateTime.Now.ToString("o"));
					setMarketSellOrder();
					}
				}
			
				if(MA5DownUp)
				{
			//4
					Print("MA5 Dn : {0}", MA5UpDown);
					if((FTODown.Equals(double.NaN) || FTODown==null) && FTOMA1>FTOMA2)
					{
						Print("[B] Signal \"Buy\""+DateTime.Now.ToString("o"));
					setMarketBuyOrder();
					}
				}
				Print("[-]bSL:{0},[+]sSL:{1}",currOrderSL._BuySL,currOrderSL._SellSL);
				
				if(currOrderSL._BuySL!=prevOrderSL._BuySL || currOrderSL._SellSL!=prevOrderSL._SellSL)
				{
				TrailActivePositions();
				prevOrderSL._BuySL=currOrderSL._BuySL;
				prevOrderSL._SellSL=currOrderSL._SellSL;
				}
			// Event occurs on every new bar
			}
			catch(Exception ex)
			{
				Print("[*NewBar*]:"+ex.Message);
				Print("[*NewBar*]:"+ex.StackTrace);
			}
			// Event occurs on every new bar
        }

		protected void TrailActivePositions()
		{

			int attempts=0;
			int count=0;
			
			IPosition[] lps = Trade.GetActivePositions(magicNumber,false);
			TradeResult tR = null;
			
			if(currOrderSL._BuySL!=prevOrderSL._BuySL || currOrderSL._SellSL!=prevOrderSL._SellSL)
			{
				
				if(lps.Length>0)
				for(int npos=lps.Length-1;npos>=0;npos--)
				{
					if((int)lps[npos].Type==(int)ExecutionRule.Buy)
					{
						if(lps[npos].StopLoss!=currOrderSL._BuySL)
						{
						do
							{ 
							attempts++;
							tR=Trade.UpdateMarketPosition(lps[npos].Id,currOrderSL._BuySL,null);
							} 
						while (!tR.IsSuccessful && attempts<1000);
						}
					}
					if((int)lps[npos].Type==(int)ExecutionRule.Sell)
					{
						if(lps[npos].StopLoss!=currOrderSL._SellSL)
						{
						do
							{ 
							attempts++;
							tR=Trade.UpdateMarketPosition(lps[npos].Id,currOrderSL._SellSL,null);
							} 
						while (!tR.IsSuccessful && attempts<1000);
						}
					}
					if(!tR.IsSuccessful)
					{
						Print("[*E*]{2} Error in modification of {1}-position {0}",
								lps[npos].Id,lps[npos].Type,DateTime.Now);
					}
					else
					{
						count++;
					}
				}
				if(count>0)
				{
						Print("[!]{1} Successfully modified {0} positions",
								count,DateTime.Now);
				}
			}

		}
		
		protected int intCalculateTP(AdvisorOrderType _ordertype,int mode)
		{
			if(mode==0)
			{
				if((int)_ordertype == (int)AdvisorOrderType.Sell)
				{	
					if(LevelSR1) 
					{
						if(Instrument.Bid<PivotS1)
						{return (int)Math.Round((Instrument.Bid-PivotS2)/Instrument.Point,0);}
						else
						{return (int)Math.Round((Instrument.Bid-PivotS1)/Instrument.Point,0);}
					}
					else 
					{return (int)Math.Round((Instrument.Bid-PivotS2)/Instrument.Point,0);}
				}
				if((int)_ordertype == (int)AdvisorOrderType.Buy)
				{	
					if(LevelSR1) 
					{
					if(Instrument.Ask>PivotR1)
					{return (int)Math.Round((PivotR2-Instrument.Ask)/Instrument.Point,0);}
					else
					{return (int)Math.Round((PivotR1-Instrument.Ask)/Instrument.Point,0);}
				}
				else 
					{return (int)Math.Round((PivotR2-Instrument.Ask)/Instrument.Point,0);}
				}
			}
			else
			{
				if((int)_ordertype == (int)AdvisorOrderType.Sell)
				{	return (int)Math.Round((PivotS2-PivotS3)/Instrument.Point,0);}
				if((int)_ordertype == (int)AdvisorOrderType.Buy)
				{	return (int)Math.Round((PivotR3-PivotR2)/Instrument.Point,0);}
			}
			return 0;
		}
		protected double dblCalculateTP(AdvisorOrderType _ordertype,int mode)
		{
			if(mode==0)
			{
				if((int)_ordertype == (int)AdvisorOrderType.Sell)
				{	
					if(LevelSR1) 
					{
						if(Instrument.Bid<PivotS1)
						{return (double)Math.Round(PivotS2,Instrument.PriceScale);}
						else
						{return (double)Math.Round(PivotS1,Instrument.PriceScale);}
					}
					else 
					{return (double)Math.Round(PivotS2,Instrument.PriceScale);}
				}
				if((int)_ordertype == (int)AdvisorOrderType.Buy)
				{	
					if(LevelSR1) 
					{
						if(Instrument.Ask>PivotR1)
						{return (double)Math.Round(PivotR2,Instrument.PriceScale);}
						else
						{return (double)Math.Round(PivotR1,Instrument.PriceScale);}
					}
					else 
					{return (double)Math.Round(PivotR2,Instrument.PriceScale);}
				}
			}
			else
			{
				if((int)_ordertype == (int)AdvisorOrderType.Sell)
				{return (double)Math.Round(Instrument.Bid-(PivotS2-PivotS3),Instrument.PriceScale);}
				if((int)_ordertype == (int)AdvisorOrderType.Buy)
				{return (double)Math.Round(Instrument.Ask+(PivotR3-PivotR2),Instrument.PriceScale);}
			}
			return (double)0.0;
		}
		protected void CalculateSL(int mode)
		{
			double HMax1 = 0.0; 
			double HMax2 = 0.0; 
			double MaxSL = 0.0;
			
			bool Found = false;
			bool isMM = true;	
			int cB =_CIndex-fsteps;
			int cBB =0;

			while(cB>=fsteps)
			{
				isMM=true;	
				for(int i=1; i<=fsteps;i++)
				{
					isMM = (isMM &&  (Bars[cB].High>=Bars[cB+i].High));
					isMM = (isMM && (Bars[cB].High>=Bars[cB-i].High));				
				}
				if(!isMM)
				{
					cB--;
				}
				else
				{
					Found=true;
					HMax1=Bars[cB].High;
					break;
				}
			}				
			if(HMax1>0 && cB>0 && Found)
			{
				cBB = cB-fsteps;
				Found = false;
				
				while(cBB>=fsteps)
				{
					isMM=true;	
					for(int i=1; i<=fsteps;i++)
					{
						isMM = (isMM &&  (Bars[cBB].High>=Bars[cBB+i].High));
						isMM = (isMM && (Bars[cBB].High>=Bars[cBB-i].High));				
					}
					if(!isMM)
					{
						cBB--;
					}
					else
					{
						Found=true;
						HMax2=Bars[cBB].High;
						break;
					}
				}				
			}
				
				
			if(HMax1>HMax2) 
			{
				MaxSL=HMax1;
			}
			else
			{
				if(HMax2>=HMax1) 
				{
					MaxSL=HMax2;
				}
			}		

			MaxSL+=Instrument.Spread;
			
			double HMin1 = 0.0; 
			double HMin2 = 0.0; 
			double MinSL = 0.0;
			Found = false;
		
			cB =_CIndex-fsteps;
			cBB =0;

			while(cB>=fsteps)
			{
				isMM=true;	
				for(int i=1; i<=fsteps;i++)
				{
					isMM = (isMM &&  (Bars[cB].Low<=Bars[cB+i].Low));
					isMM = (isMM && (Bars[cB].Low<=Bars[cB-i].Low));				
				}
				if(!isMM)
				{
					cB--;
				}
				else
				{
					Found=true;
					HMin1=Bars[cB].Low;
					break;
				}
			}				
			if(HMin1>0 && cB>=fsteps && Found)
			{
				cBB = cB-fsteps;
				Found = false;
				while(cBB>=fsteps)
				{
					isMM=true;	
					for(int i=1; i<=fsteps;i++)
					{
						isMM = (isMM &&  (Bars[cBB].Low<=Bars[cBB+i].Low));
						isMM = (isMM && (Bars[cBB].Low<=Bars[cBB-i].Low));				
					}
					if(!isMM)
					{
						cBB--;
					}
					else
					{
						Found=true;
						HMin2=Bars[cBB].Low;
						break;
					}
				}				
			}
			if(HMin1<HMin2) 
			{
				MinSL=HMin1;
			}
			else
			{
				if(HMin2<HMin1) 
				{
					MinSL=HMin2;
				}
			}		
			
			MinSL-=Instrument.Spread;
			
			if(mode==0)
			{
				prevOrderSL._BuySL=MinSL;
				prevOrderSL._SellSL=MaxSL;
				currOrderSL._BuySL=MinSL;
				currOrderSL._SellSL=MaxSL;
			}
			else	
			{
				currOrderSL._BuySL=MinSL;
				currOrderSL._SellSL=MaxSL;
			}
		
		}
		protected void setMarketSellOrder()
		{
			Stops SLTP=Stops.InPrice(
									currOrderSL._SellSL,
									dblCalculateTP(AdvisorOrderType.Sell,0)
									);
			int attempt=0;
			TradeResult tR=null;
			do 
			{
				attempt++;
				tR=Trade.OpenMarketPosition(Instrument.Id,ExecutionRule.Sell,vol,Instrument.Bid,-1,SLTP,"sell on bar signal",magicNumber);	
			}
			while (!tR.IsSuccessful && attempt < 100);
			if(tR.IsSuccessful)
			{
				Print("[{3}>>]{2} Sended successfully Sell order for position {0} at Bid: {1} ",
					tR.Position.Id,tR.Position.OpenPrice,tR.Position.OpenTime,Instrument.Name);
			}
			else
			{
				Print("[{2}>>]{1} Cann't Send  Sell order  at Bid: {0} ",
					Instrument.Bid,DateTime.Now,Instrument.Name);
			}
		}    
    	
		protected void setMarketBuyOrder()
		{
			
			Stops TPSL=Stops.InPrice(
									currOrderSL._BuySL,
									dblCalculateTP(AdvisorOrderType.Buy,0)
									);
			int attempt=0;
			TradeResult tR=null;
			do 
			{
				attempt++;
				tR=Trade.OpenMarketPosition(Instrument.Id,ExecutionRule.Buy,vol,Instrument.Ask,-1,TPSL,"buy on bar signal",magicNumber);	
			}
			while (!tR.IsSuccessful && attempt < 100);
			if(tR.IsSuccessful)
			{
				Print("[{3}>>]{2} Sended successfully Buy order for position {0} at Ask: {1} ",
					tR.Position.Id,tR.Position.OpenPrice,tR.Position.OpenTime,Instrument.Name);
			}
			else
			{
				Print("[{2}>>]{1} Cann't Send Buy order at Ask: {0} ",
					Instrument.Bid,DateTime.Now,Instrument.Name);
			}
			
		}    

		protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                if(position.MagicNumber==magicNumber)
				{
					// do nothing now...	
					
				}
				
				Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
    }
}