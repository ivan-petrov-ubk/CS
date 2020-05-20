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
    [TradeSystem("TC_M1_4")] //copy of "TC_M1_3"
    public class TC_M1 : TradeSystem
    {
        [Parameter("Торговая Система :", DefaultValue = "ТС")]
        public string CommentText { get; set; }
		[Parameter("Lots", DefaultValue = 0.1, MinValue=0.01, MaxValue = 2.0)]
		public double vol { get;set; }
		[Parameter("Тики :", DefaultValue = 600)]
        public int In { get; set; }
		[Parameter("Пункти инт.:", DefaultValue = 300)]
        public int Pt { get; set; }
		[Parameter("Take Profit :", DefaultValue = 3000)]
        public int TP { get; set; }
        [Parameter("Stop Loss :", DefaultValue = 180)]
        public int SL { get; set; }
	    [Parameter("Мин. до Stop :", DefaultValue = 600, MinValue=1, MaxValue = 1200)]
        public int DS { get; set; }
	
		private Guid posGuid=Guid.Empty;
		public double C1,O10,x;
		public int ind=0,iTik=0,kTik=0;
		public double[] aTik = new double[1200];
		public bool uTik=false;
	
		protected override void Init()
		{  for (int i=0; i<In; i++) aTik[i]=0.0; 
			if(Instrument.Name.EndsWith("JPY")) x=0.001*Pt; else x=0.00001*Pt;
			Print("Если На протяжении {0} тиков цена пршла {1} пуктов - будет открыт ордер с ТP={3} SL={4}",SL,TP);
		}
		
		       protected override void NewQuote()
        {
            // Event occurs on every new quote
			
			for (int i=1; i<In; i++) { aTik[In-i]=aTik[In-1-i]; }
				aTik[0]=Instrument.Ask; 
			
				if(aTik[In-1]>0 && aTik[In-1]-aTik[0]>x) { 
					Print("Ask={0} Bid={1} -- {2}",Instrument.Ask,Instrument.Bid,aTik[In-1]-aTik[0]);
				if (posGuid==Guid.Empty) {
				var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, vol, Instrument.Ask, -1, Stops.InPips(SL,TP), null, null);	
			    if (result1.IsSuccessful)  {posGuid = result1.Position.Id;  uTik=true;} }
				}
				if(aTik[In-1]>0 && aTik[0]-aTik[In-1]>x) { 
					Print("Ask={0} Bid={1} -- {2}",Instrument.Ask,Instrument.Bid,aTik[In-1]-aTik[0]);
				if (posGuid==Guid.Empty) {
				var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, vol, Instrument.Ask, -1, Stops.InPips(SL,TP), null, null);	
			    if (result1.IsSuccessful)  { posGuid = result1.Position.Id; uTik=true;} 
				} 
				}
				
      
			
		}
        
        protected override void NewBar()
        {

			if(uTik) 
			{   uTik=false;
				var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Red;	
			}
			
			if(DS>0 && ind==DS && posGuid!=Guid.Empty) {      
				var res = Trade.CloseMarketPosition(posGuid);
        		if (res.IsSuccessful) posGuid = Guid.Empty;}
			
			if (posGuid!=Guid.Empty) ind++; else ind=0;
			
			//Print("{0} - {1}",Bars[Bars.Range.To - 1].TickCount,Bars[Bars.Range.To - 1].Time);
        }
        
    }
}