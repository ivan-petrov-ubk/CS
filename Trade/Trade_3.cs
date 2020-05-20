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
using System.IO;

namespace IPro.TradeSystems
{
    [TradeSystem("TC_M1")]
    public class TC_M1 : TradeSystem
    {
        [Parameter("Торговая Система :", DefaultValue = "ТС")]
        public string CommentText { get; set; }
		[Parameter("Lots", DefaultValue = 0.1, MinValue=0.01, MaxValue = 2.0)]
		public double vol { get;set; }
		[Parameter("Интервал мин.:", DefaultValue = 10)]
        public int In { get; set; }
		[Parameter("Пункти инт.:", DefaultValue = 200)]
        public int Pt { get; set; }
		[Parameter("Take Profit :", DefaultValue = 1000)]
        public int TP { get; set; }
        [Parameter("Stop Loss :", DefaultValue = 200)]
        public int SL { get; set; }
	    [Parameter("Мин. до Stop :", DefaultValue = 200)]
        public int DS { get; set; }
	
		private Guid posGuid=Guid.Empty;
		public double C1,O10,x;
		public int ind=0;
		public Fractals _frInd;
						// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frDownL = 0.0;  // Значение текущего нижнего Fractal
		public bool frUp=false;
		public bool frDown=true;
		
		        protected override void Init()
        {  _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_frInd.Range=9;
		}
        
       protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
		
        protected override void NewBar()
        {

			O10=Bars[Bars.Range.To-In].Open;
			C1=Bars[Bars.Range.To-1].Close;
			
//=== Определение уровня STOP ===========================================================================================================				
		 	if(_frInd.TopSeries[Bars.Range.To-11]>0) { frUp=true; frUpH=Bars[Bars.Range.To-11].High; } else frUp=false;
   		 	if(_frInd.BottomSeries[Bars.Range.To-11]>0) { frDown=true; frDownL=Bars[Bars.Range.To-11].Low; } else frDown=false;
//=======================================================================================================================================
			if(Math.Abs(Math.Round((O10-C1)*100000,0))>Pt) 
				Print("{0} - {1}",Math.Abs(Math.Round((O10-C1)*100000,0)),Bars[Bars.Range.To-10].Time);
			
			if (posGuid!=Guid.Empty && Trade.GetPosition(posGuid).State==PositionState.Closed) posGuid=Guid.Empty; 

			var pos = Trade.GetPosition(posGuid);
			//if(frUp && posGuid!=Guid.Empty && pos.Type==ExecutionRule.Sell && frUpH<(pos.OpenPrice+0.0001))   
			if(frUp && posGuid!=Guid.Empty && pos.Type==ExecutionRule.Sell)   
				        Trade.UpdateMarketPosition(posGuid, frUpH+Instrument.Spread, null, null); 
			//if(frDown && posGuid!=Guid.Empty && pos.Type==ExecutionRule.Buy && frUpH<(pos.OpenPrice+0.0001)) 
			if(frDown && posGuid!=Guid.Empty && pos.Type==ExecutionRule.Buy) 
				        Trade.UpdateMarketPosition(posGuid, frDownL-Instrument.Spread, null, null); 
			
			if(Instrument.Name.EndsWith("JPY")) x=0.001*Pt; else x=0.00001*Pt;		
			
			if (C1-O10>x) { 
				if (posGuid==Guid.Empty) {
				var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, vol, Instrument.Bid, -1, Stops.InPips(SL,TP), null, null);	
			    if (result1.IsSuccessful)  posGuid = result1.Position.Id; }
			}
			
			if (O10-C1>x) { 
				if (posGuid==Guid.Empty) {
		          var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, vol, Instrument.Ask, -1, Stops.InPips(SL,TP), null, null);	
				  if (result.IsSuccessful)  posGuid = result.Position.Id;   } 
			}

			if(DS>0 && ind==DS && posGuid!=Guid.Empty) {      
				var res = Trade.CloseMarketPosition(posGuid);
        		if (res.IsSuccessful) posGuid = Guid.Empty;}
			
			if (posGuid!=Guid.Empty) ind++; else ind=0;
			
        }
        
    }
}