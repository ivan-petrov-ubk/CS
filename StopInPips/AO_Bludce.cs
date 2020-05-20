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
    [TradeSystem("AO_Bludce")] //copy of "AO"
    public class AO : TradeSystem
    {
 		public AwesomeOscillator _awoInd;
		int ci;
		double aoUp1,aoDown1,aoUp2,aoDown2,aoUp3,aoDown3,aoUp4,aoDown4,aoUp5,aoDown5;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
        protected override void Init()
        {
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
        }        

       
        protected override void NewBar()
        {   ci = Bars.Range.To - 1;
            // Event occurs on every new bar
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
			aoUp1    = _awoInd.SeriesUp[Bars.Range.To-1];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown1  = _awoInd.SeriesDown[Bars.Range.To-1]; // Червоні лінії
			aoUp2    = _awoInd.SeriesUp[Bars.Range.To-2];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown2  = _awoInd.SeriesDown[Bars.Range.To-2];
			aoUp3    = _awoInd.SeriesUp[Bars.Range.To-3];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown3  = _awoInd.SeriesDown[Bars.Range.To-3];
			aoUp4    = _awoInd.SeriesUp[Bars.Range.To-4];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown4  = _awoInd.SeriesDown[Bars.Range.To-4];
				
			// // Вверху ВПАДИНА Зеленый на Красний
			 	 if(aoDown4>0.0002 && aoDown3>0.0002 && aoUp2>0.0002 && aoUp1>0.0002) 
     				{ LineR(ci-1);	
										if (posGuidBuy==Guid.Empty) { 		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(100,100),null,null);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
					}
        }
		
        protected void LineR(int c)
		{      var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[c].Time;
				toolVerticalLine.Color=Color.Red;        }

    }
}