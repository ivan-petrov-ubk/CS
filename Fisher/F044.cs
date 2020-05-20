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
    [TradeSystem("F0")]    //copy of "Fisher0"
	
	public class Elder : TradeSystem
    {
		[Parameter("Время работы:", DefaultValue = "EUR=7-17  USD=12-22 AUD=21-7 JPY=23-9")]
        private string CommentText { get; set; }
		[Parameter("Начало:", DefaultValue = 4)]
        private int tm1 { get; set; }		
		[Parameter("Конец:", DefaultValue = 17)]
        private int tm2 { get; set; }	

 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		private bool torg=false,flag=true;
				
		private double maxu,mind;
		private double max1,max2,max3,max4,min1,min2,min3,min4;
		private int kl=0;
		
		private ISeries<Bar> _barM15;
		private Period periodM15;		
		private int _lastIndexM15 = -1;	
		
		private ISeries<Bar> _barM5;
		private Period periodM5;		
		private int _lastIndexM5 = -1;	

		private FisherTransformOscillator _ftoInd,_ftoIndM5,_ftoIndM15;
		private bool F5U,F5D,F15U,F15D,F1U,F1D,F4U,F4D;
		
		private int klu=0,kld=0;
     
		protected override void Init()
        {
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15);
			periodM5 = new Period(PeriodType.Minute, 5);
			_barM5 = GetCustomSeries(Instrument.Id,periodM5);
			_ftoIndM5   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM5);

        }        

		        protected override void NewQuote()
        {
 
				 if (_lastIndexM15 < _barM15.Range.To-1) {   
					 if(_lastIndexM15>0) {
							if( _ftoIndM15.FisherSeries[_barM15.Range.To]>-0.3 )  F15U=true; else F15U=false;
							if( _ftoIndM15.FisherSeries[_barM15.Range.To]<0.3)  F15D=true; else F15D=false;
					 }
					_lastIndexM15 = _barM15.Range.To-1;  }

				 if (_lastIndexM5 < _barM5.Range.To-1) {   
					 if(_lastIndexM5>0) {
			if( _ftoIndM5.FisherSeries[_barM5.Range.To]>0 && _ftoIndM5.FisherSeries[_barM5.Range.To-1]<0 ) klu=0;
			if( _ftoIndM5.FisherSeries[_barM5.Range.To]<0 && _ftoIndM5.FisherSeries[_barM5.Range.To-1]>0 ) klu=0;
							if( _ftoIndM5.FisherSeries[_barM5.Range.To]>-0.3) F5U=true; else F5U=false;
							if( _ftoIndM5.FisherSeries[_barM5.Range.To]<0.3)  F5D=true; else F5D=false;
					 }
					_lastIndexM5 = _barM5.Range.To-1;  }

		}
        
        protected override void NewBar()
        {   
		if ( Bars[Bars.Range.To-1].Time.Hour>=tm2 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
	
		  }
			
		  if ( Bars[Bars.Range.To-1].Time.Hour>tm1 && Bars[Bars.Range.To-1].Time.Hour<tm2) torg=true; else torg=false;
			
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
			//if( F1U && F15U) { var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time; }
			//if( F1D && F15D) { var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time; }
			
			//UP - ВВЕРХ - визанаємо МІНІМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]>0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0.3) 
			{   kl=0; mind=1000; 
				do
				{ kl++; if(Bars[Bars.Range.To-kl].Low<mind) mind=Bars[Bars.Range.To-kl].Low;}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]<0.3  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]>0.3) && kl<1000);
				min4=min3;min3=min2;min2=min1; min1=mind;
				UpMBuy();
				if(F15U && F5U && klu==0 && torg ) {var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;
									Buy1();  klu++;  }
			} 
            // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]<-0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>-0.3) 
			{   kl=0;maxu=0;
				do
				{ kl++; if(Bars[Bars.Range.To-kl].High>maxu) maxu=Bars[Bars.Range.To-kl].High;  }
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]>-0.3  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]<-0.3) && kl<1000);
				max4=max3;max3=max2;max2=max1; max1=maxu;
				UpMSell();
				if( F15D && F5D  && klu==0 && torg) {  var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time;
				   					Sell1(); klu++;  }
		     }
		}		

protected void UpMSell()	
			{	
				if(posGuidSell!=Guid.Empty) { var result = Trade.UpdateMarketPosition(posGuidSell, max1, null, null); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }			
			}
protected void UpMBuy()	
			{	
				if(posGuidBuy!=Guid.Empty) { var result = Trade.UpdateMarketPosition(posGuidBuy, min1, null, null); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }			
			}
protected void Buy1()
		    {
				if (posGuidBuy==Guid.Empty) { var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												//Stops.InPrice(min3-0.0003,null), null, null);
												Stops.InPips(100,null), null, null);
						
				if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; }
			}

protected void Sell1()
		    {
				if (posGuidSell==Guid.Empty) {var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
						//Stops.InPrice(max3+0.0003,null), null, null);    						
					 Stops.InPips(100,null), null, null);
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
