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
    [TradeSystem("DIVER1")]  //copy of "Elder2"
	
	public class Elder : TradeSystem
    {
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public FisherTransformOscillator _ftoInd;
		private double maxu,mind;
		private double fsh1,fsh2,fsh3,fsh4,fsh5;
		private double max1,max2,max3,max4,min1,min2,min3,min4;
		public int kl=0;
		public bool flag1=true;
		private DateTime tmu1,tmu2,tmu3,tmu4;
		private DateTime tmd1,tmd2,tmd3,tmd4;
     
		protected override void Init()
        {_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); }        
        
        protected override void NewBar()
        {   
			fsh1 = _ftoInd.FisherSeries[Bars.Range.To-1];
			fsh2 = _ftoInd.FisherSeries[Bars.Range.To-2];
			fsh3 = _ftoInd.FisherSeries[Bars.Range.To-3];
			fsh4 = _ftoInd.FisherSeries[Bars.Range.To-4];
			fsh5 = _ftoInd.FisherSeries[Bars.Range.To-5];
			
		/*	if( fsh3>fsh2 && 
				fsh3>fsh1 &&
				fsh3>fsh4 &&
				fsh3>fsh5 &&
				fsh4>fsh5 &&
				fsh2>fsh1) */
			if( fsh2>fsh1 && fsh2>fsh3 )
			{
			var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-2].Time;
			}
			/*if( fsh3<fsh2 && 
				fsh3<fsh1 &&
				fsh3<fsh4 &&
				fsh3<fsh5 &&
				fsh4<fsh5 &&
				fsh2<fsh1) */
						if( fsh2<fsh1 && fsh2<fsh3 )

			{

					
			var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-2].Time;
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
