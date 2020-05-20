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
    [TradeSystem("Parabolic")]
    public class Parabolic : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
        private ParabolicSar _psInd;
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public double pr1;
		
        protected override void Init()
        {
  			_psInd= GetIndicator<ParabolicSar>(Instrument.Id, Timeframe);
			_psInd.CoefStep=0.01;
        }        

        
        protected override void NewBar()
        {
            _psInd.ReInit();
			pr1 = _psInd.SarSeries[Bars.Range.To-1];
			pr1=Math.Round(pr1,Instrument.PriceScale); 
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty; 

			// Parabolic выше цены
			if(Bars[Bars.Range.To-1].High<pr1) {	
			if(posGuidBuy==Guid.Empty){ var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.BuyStop, 0.1,  pr1, 0, Stops.InPips(200,null), null, null, null);
					if (result1.IsSuccessful)   posGuidBuy=result1.Position.Id; }
			
			if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
				      { var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, pr1,null, null); 
						if (result.IsSuccessful)  posGuidBuy = result.Position.Id; }
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}						  
	  
			}
			
			// Parabolic Ниже цены
			if(Bars[Bars.Range.To-1].Low>pr1) {	
			if(posGuidSell==Guid.Empty) { var result1 = Trade.OpenPendingPosition(Instrument.Id, 
					ExecutionRule.SellStop, 0.1,  pr1, 0, Stops.InPips(200,null), null, null, null);
					if (result1.IsSuccessful)   posGuidSell=result1.Position.Id; }
			
			if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			         { var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, pr1, null, null); 
						if (result.IsSuccessful)  posGuidSell = result.Position.Id; }
			

						if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				{ var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}		
			
			}
			//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time;
			
			Print("{0} --  parabolic={1}",Bars[Bars.Range.To-1].Time,pr1);
        }
        
 
    }
}