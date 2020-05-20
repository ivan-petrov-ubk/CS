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
    [TradeSystem("MAX2")] //copy of "Elder"
	
	public class Elder : TradeSystem
    {
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public FisherTransformOscillator _ftoInd;
		private double maxu,mind;
		private double max1,max2,max3,max4,min1,min2,min3,min4;
		public int kl=0;
		public bool flag1=true;
		private DateTime tmu1,tmu2,tmu3,tmu4;
		private DateTime tmd1,tmd2,tmd3,tmd4;
     
		protected override void Init()
        {_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); }        
        
        protected override void NewBar()
        {   
			
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  

			//UP - ВВЕРХ - визанаємо МІНІМУМ
			if(flag1 && _ftoInd.FisherSeries[Bars.Range.To-1]>0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0.3) 
			{  flag1=false;
				kl=0; mind=1000;tmu4=tmu3;tmu3=tmu2;tmu2=tmu1;
				do
				{ kl++; if(Bars[Bars.Range.To-kl].Low<mind) { mind=Bars[Bars.Range.To-kl].Low; 
				tmu1=Bars[Bars.Range.To-kl].Time;}}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]<0.3  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]>0.3) && kl<1000);
				min4=min3;min3=min2;min2=min1; min1=mind;
				
				

				
				
				UpMBuy();
				if(max3<max4 &&
				   max3<max2 &&
				   min3<min4 &&
				   min3<min1 &&	
				   max2<max1) {var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;
									Buy1();
				   							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=3;				
        					toolPolyLine.AddPoint(new ChartPoint(tmd4, max4));
   							toolPolyLine.AddPoint(new ChartPoint(tmu4, min4));
							toolPolyLine.AddPoint(new ChartPoint(tmd3, max3));
   							toolPolyLine.AddPoint(new ChartPoint(tmu3, min3));
							toolPolyLine.AddPoint(new ChartPoint(tmd2, max2));
							toolPolyLine.AddPoint(new ChartPoint(tmu2, min2));
							toolPolyLine.AddPoint(new ChartPoint(tmd1, max1));
							toolPolyLine.AddPoint(new ChartPoint(tmu1, min1));
							toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High));
				   }
			} 
            // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
			if(!flag1 && _ftoInd.FisherSeries[Bars.Range.To-1]<-0.3 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>-0.3) 
			{  
				flag1=true;
				kl=0;maxu=0;tmd4=tmd3;tmd3=tmd2;tmd2=tmd1;
				do
				{ kl++; if(Bars[Bars.Range.To-kl].High>maxu) { maxu=Bars[Bars.Range.To-kl].High;
												tmd1=Bars[Bars.Range.To-kl].Time;}}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]>-0.3  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]<-0.3) && kl<1000);
				max4=max3;max3=max2;max2=max1; max1=maxu;

				
				UpMSell();
				if(max3>max4 &&
				   max3>max2 &&
				   min3>min4 &&
				   min3>min1 &&	
				   max2>max1 ) {var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-1].Time;
				   					Sell1();
				   							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=3;				
   							toolPolyLine.AddPoint(new ChartPoint(tmu4, min4));
							toolPolyLine.AddPoint(new ChartPoint(tmd4, max4));
   							toolPolyLine.AddPoint(new ChartPoint(tmu3, min3));
							toolPolyLine.AddPoint(new ChartPoint(tmd3, max3));
							toolPolyLine.AddPoint(new ChartPoint(tmu2, min2));
							toolPolyLine.AddPoint(new ChartPoint(tmd2, max2));
							toolPolyLine.AddPoint(new ChartPoint(tmu1, min1));
							toolPolyLine.AddPoint(new ChartPoint(tmd1, max1));
							toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Low));
				   }
			} 

		}
protected void SellStop1()		
			{
				if(posGuidSell==Guid.Empty){ var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.SellStop, 0.1,  min1, 0, Stops.InPips(200,null), null, null, null);
					if (result1.IsSuccessful)   posGuidSell=result1.Position.Id; } 
			}

protected void BuyStop1()		
			{
				if(posGuidBuy==Guid.Empty){ var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.BuyStop, 0.1,  max1, 0, Stops.InPips(200,null), null, null, null);
					if (result1.IsSuccessful)   posGuidBuy=result1.Position.Id; } 
			}
			
protected void UpPBuy()	
			{	
				if(posGuidBuy!=Guid.Empty) { var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, max1,min1, null); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }			
			}
protected void UpPSell()	
			{	
				if(posGuidSell!=Guid.Empty) { var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, min1,max1, null); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }			
			}
protected void UpMSell()	
			{	
				if(posGuidSell!=Guid.Empty) { var result = Trade.UpdateMarketPosition(posGuidSell, max2, null, null); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }			
			}
protected void UpMBuy()	
			{	
				if(posGuidBuy!=Guid.Empty) { var result = Trade.UpdateMarketPosition(posGuidBuy, min2, null, null); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }			
			}
protected void Buy1()
		    {
				if (posGuidBuy==Guid.Empty) { var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
												Stops.InPrice(min3,null), null, null);
												// Stops.InPips(200,200), null, null);
						
				if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; }
			}

protected void Sell1()
		    {
				if (posGuidSell==Guid.Empty) {var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
						Stops.InPrice(max3,null), null, null);    						
						// Stops.InPips(200,200), null, null);
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
