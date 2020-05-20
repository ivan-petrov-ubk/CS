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
    [TradeSystem("TC_50pr")]  //copy of "Zona_Sr"
    public class Zona1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		[Parameter("LR_Period", DefaultValue = 3, MinValue = 1)]
        public int k1 { get; set; }
				// Fractal
		public Fractals _frInd;
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frDownL = 0.0;  
		public double frSU=0,frSD=0;
	    bool frUp=false;
		bool frDown=true,Up;
		public int k,srU,srD,ind,otkU1,otkD1,otkU2,otkD2;
		public int U1,U2,U3,U4,U5,U6,U7,U8,U9,U10;
		public int D1,D2,D3,D4,D5,D6,D7,D8,D9,D10;
		public DateTime DTimeU1,DTimeU2,DTimeD1,DTimeD2;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			 _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			k=k1;
			Print("1 - k={0}",k);
			_frInd.Range=k;
			ind=0;
			k=k+2;
			Print("2 - k={0}",k);
        }        

        protected override void NewBar()
        {
            // Event occurs on every new bar
			ind++;
			frSU=_frInd.TopSeries[Bars.Range.To-k];
			frSD=_frInd.BottomSeries[Bars.Range.To-k];
			
			//if(frSU>0 && Up) { 
			if(frSU>0) { 				  
				 U2=U1;U1=Bars.Range.To-k; 
				 srU=Math.Abs(U1-D1);
				 frUp=true; 
				 Print("UP - otkU={0} {1} -- {2}",otkU1,Bars.Range.To,(U1+(U1-U2)));
			if(srU>k && Up) 
			{
				var vW = Tools.Create<VerticalLine>();
			    vW.Color=Color.Red;
				vW.Time = Bars[Bars.Range.To-1].Time; 	
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(200,200), null, null);
			} Up=false;
            } else { frUp=false;   }
			
			//if(frSD>0 && !Up) {
			if(frSD>0) {	
				D2=D1;D1=Bars.Range.To-k;
				frDown=true; 
				srD=Math.Abs(U1-D1);
			if(srD>k && !Up) 
			{
				var vW = Tools.Create<VerticalLine>();
			    vW.Color=Color.Blue;
				vW.Time = Bars[Bars.Range.To-1].Time;
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPips(200,200), null, null);	
			}	Up=true;
			} else { frDown=false; }
				
		//if(srU>5  && Bars.Range.To==(D1+srD)) Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(150,100), null, null);
        //if(srD>5  && Bars.Range.To==(U1+srU)) Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPips(150,100), null, null);			

        }
        
  
    }
}