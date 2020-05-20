using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Indicators.Standard;
using System.Collections.Generic;



namespace IPro.TradeSystems
{
    [TradeSystem("Elder2_1")]  //copy of "Elder2"
	
	public class Elder : TradeSystem
    {
		[Parameter("Время работы:", DefaultValue = "EUR=7-17  USD=12-22 AUD=21-7 JPY=23-9")]
        public string CommentText { get; set; }
		[Parameter("Начало:", DefaultValue = 0)]
        public int tm1 { get; set; }		
		[Parameter("Конец:", DefaultValue = 24)]
        public int tm2 { get; set; }	
		[Parameter("Через 0:", DefaultValue = false)]
        public bool tm0 { get; set; }	

		public ISeries<Bar> _barH4,_barH1,_barD1;
		public Period periodD1, periodH4;		
		private int _lastIndexD1 = -1,_lastIndexH4 = -1,_lastIndexH1 = -1;	
		
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		private MovingAverage _maInd,_maIndH1;
		private MovingAverageConvergenceDivergence _macdInd,_macdIndH1;

		private double mv1,md1,ss1,pr1,bc1,bd1,bu1,dmv,dmacd;
		private double mv2,md2,ss2,pr2,bc2,bd2,bu2,dbu,dbd,db;
		private bool ib=false,isl=false,torg=false;
		private bool MAH1U,MAH1D,MCH1U,MCH1D;
		
		public ISeries<Bar> _barM15;
		public Period periodM15;		
		private int _lastIndexM15 = -1;	
		public FisherTransformOscillator _ftoIndM15;
		public bool F15U,F15D;
     
		protected override void Init()
        {
			_maInd= GetIndicator<MovingAverage>(Instrument.Id, Timeframe);
			_maInd.Period=26;
			_macdInd= GetIndicator<MovingAverageConvergenceDivergence>(Instrument.Id, Timeframe);
			
			_barH1 = GetCustomSeries(Instrument.Id,Period.H1);
			_macdIndH1   = GetIndicator<MovingAverageConvergenceDivergence>(Instrument.Id, Period.H1); 
			_maIndH1   = GetIndicator<MovingAverage>(Instrument.Id, Period.H1); 
			_maIndH1.Period=26;		
			
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15);
        }        

		        protected override void NewQuote()
        {
				 if (_lastIndexH1 < _barH1.Range.To-1) {
					if(_lastIndexH1>0) {
					if (_macdIndH1.SeriesMacd[_barH1.Range.To]>_macdIndH1.SeriesMacd[_barH1.Range.To-1] && 
						_macdIndH1.SeriesMacd[_barH1.Range.To]>0.0003 ) MAH1U=true; else MAH1U=false;
					if (_macdIndH1.SeriesMacd[_barH1.Range.To]<_macdIndH1.SeriesMacd[_barH1.Range.To-1] && 
						_macdIndH1.SeriesMacd[_barH1.Range.To]<-0.0003 ) MAH1D=true; else MAH1D=false;
					} 	
     		    	_lastIndexH1 = _barH1.Range.To-1;  
				} 
				 if (_lastIndexM15 < _barM15.Range.To-1) {     		    	
					if(_lastIndexM15>0 && _ftoIndM15.FisherSeries[_barM15.Range.To]>_ftoIndM15.FisherSeries[_barM15.Range.To-1] && 
						_ftoIndM15.FisherSeries[_barM15.Range.To]>0.0003 )  F15U=true; else F15U=false;
					if(_lastIndexM15>0 && _ftoIndM15.FisherSeries[_barM15.Range.To]<_ftoIndM15.FisherSeries[_barM15.Range.To-1] &&
						_ftoIndM15.FisherSeries[_barM15.Range.To]<-0.0003)  F15D=true; else F15D=false;
					_lastIndexM15 = _barM15.Range.To-1;  
				}
		}
        
        protected override void NewBar()
        {   
			
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			// EUR - 7-17  USD-12-22 AUD-21-7 JPY-23-9
/*		if ( Bars[Bars.Range.To-1].Time.Hour>=tm2 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
	
		  }
			if(!tm0)
			if ( Bars[Bars.Range.To-1].Time.Hour>tm1 && Bars[Bars.Range.To-1].Time.Hour<tm2) torg=true; else torg=false;
			else
			if ( Bars[Bars.Range.To-1].Time.Hour>tm1 || Bars[Bars.Range.To-1].Time.Hour<tm2) torg=true; else torg=false;	*/
			torg=true;

			dmv=_maInd.SeriesMa[Bars.Range.To-1]-_maInd.SeriesMa[Bars.Range.To-2];
			// dmacd=_macdInd.SeriesMacd[Bars.Range.To-1]-_macdInd.SeriesMacd[Bars.Range.To-2];
			dmacd=_macdInd.SeriesSignal[Bars.Range.To-1]-_macdInd.SeriesSignal[Bars.Range.To-2];
		
			if( MAH1U && F15U) { var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time; }
			if( MAH1D && F15D) { var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time; }
			
			if(torg && dmv>0 && dmacd>0 && MAH1U && F15U && _macdInd.SeriesSignal[Bars.Range.To-1]>0) 
			{  //Buy
				var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;vr.Width=4;
				Buy1();				
			}
			
			if(dmv<=0 || dmacd<=0) CloseBuy1();	        
			
			if(torg && dmv<0 && dmacd<0  && MAH1D && F15D && _macdInd.SeriesSignal[Bars.Range.To-1]<0) 
			{ //Sell
				var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=4;
				Sell1();				
			}
			
			if(dmv>=0 || dmacd>=0) CloseSell1();
		}
		
protected void Buy1()
		    {
				if (posGuidBuy==Guid.Empty) { var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1,Stops.InPips(300,null), null, null);
				if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; }
			}

protected void Sell1()
		    {
				if (posGuidSell==Guid.Empty) {var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(300,null), null, null);
				if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;	} 
			}

protected void CloseBuy1()
		    {
				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}	
			}
protected void CloseSell1()
		    {
				if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				{ var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			}
			
			
    }
}
