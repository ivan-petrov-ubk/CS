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
    [TradeSystem("F02")]      //copy of "F01"
	
	public class Elder : TradeSystem
    {
		[Parameter("Начало:", DefaultValue = 4)]
        public int tm1 { get; set; }		
		[Parameter("Конец:", DefaultValue = 17)]
        public int tm2 { get; set; }	
		[Parameter("Buy:", DefaultValue = true)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = true)]
        public bool td { get; set; }	


 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		private bool torg=false;
				
		private double maxu,mind,adx1;
		private double max1,max2,max3;
		private double fmax1,fmax2,fmax3;
		private double min1,min2,min3;
		private double fmin1,fmin2,fmin3;
		private int kl=0;
		


		private FisherTransformOscillator _ftoInd;
		 private AverageDirectionalMovement _admInd;
		

     
		protected override void Init()
        {
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_admInd = GetIndicator<AverageDirectionalMovement>(Instrument.Id, Timeframe);
        }        

        
        protected override void NewBar()
        {   
		/*if ( Bars[Bars.Range.To-1].Time.Hour>=tm2 ) 
		  {  
			if (posGuidBuy!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
	
		  }
			*/
		adx1=_admInd.SeriesMain[Bars.Range.To-1];
			
		  if ( Bars[Bars.Range.To-1].Time.Hour>tm1 && Bars[Bars.Range.To-1].Time.Hour<tm2) torg=true; else torg=false;
			torg=true;
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
		
			//UP - ВВЕРХ - визанаємо МІНІМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]>0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0.3) 
			{  Print("BUY  {0} - adx={1}",Bars[Bars.Range.To-1].Time,adx1);
				if(torg && tu && adx1<0.3) { var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;
									Buy1();   }
				CloseSell1();
				
			} 
            // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]<-0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>-0.3) 
			{   Print("SELL  {0} - adx={1}",Bars[Bars.Range.To-1].Time,adx1);
				if( torg && td  && adx1<0.3) {  var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time;
				   					Sell1();  }
				CloseBuy1();
		     }
			// ВВЕРХУ
			if( _ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.FisherSeries[Bars.Range.To-1] && 
				_ftoInd.FisherSeries[Bars.Range.To-2]>_ftoInd.FisherSeries[Bars.Range.To-3] &&
				_ftoInd.FisherSeries[Bars.Range.To-1]>0) 
				{    fmax3=fmax2;fmax2=fmax1; fmax1=_ftoInd.FisherSeries[Bars.Range.To-2];
					 max3=max2; max2=max1; max1=Bars[Bars.Range.To-2].High;
					
					if(max1>max2 && fmax1<fmax2) {var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=4;}
					if (posGuidBuy!=Guid.Empty && max1>max2 && fmax1<fmax2) 
						{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;
						}
						
				}
			// ВНИЗУ
			if( _ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.FisherSeries[Bars.Range.To-1] && 
				_ftoInd.FisherSeries[Bars.Range.To-2]<_ftoInd.FisherSeries[Bars.Range.To-3] &&
				_ftoInd.FisherSeries[Bars.Range.To-1]<0)
				{   fmin3=fmin2;fmin2=fmin1; fmin1=_ftoInd.FisherSeries[Bars.Range.To-2];
					 min3=min2; min2=min1; min1=Bars[Bars.Range.To-2].Low;
					
					if(fmin1>fmin2 && min1<min2) { var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time; vr.Width=4;}
					
					if (posGuidSell!=Guid.Empty && fmin1>fmin2 && min1<min2) 
						{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;
						}	
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
												Stops.InPips(null,null), null, null);
						
				if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; }
			}

protected void Sell1()
		    {
				if (posGuidSell==Guid.Empty) {var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
						//Stops.InPrice(max3+0.0003,null), null, null);    						
					 Stops.InPips(null,null), null, null);
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
