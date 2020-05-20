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
    [TradeSystem("TC_Uroven_Fractal1")]   //copy of "TC_50pr"
    public class Zona1 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		[Parameter("LR_Period", DefaultValue = 9, MinValue = 1)]
        public int k1 { get; set; }
				// Fractal
		public Fractals _frInd;
		//public Fractals _frInd3;		
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frDownL = 0.0;  
		public double frSU=0,frSD=0;
	    bool frUp=false;
		bool frDown=true,Up;
		public int k,srU,srD,indH,indL,otkU1,otkD1,otkU2,otkD2;
		public double U1,D1;
		public DateTime DTimeU1,DTimeU2,DTimeD1,DTimeD2;
		private HorizontalLine hline;
		private HorizontalLine lline;
		double H1,L1,C1,O1;
		double H2,L2,C2,O2;
		double H3,L3,C3,O3;
		double H4,L4,C4,O4;
		public DateTime DTime;


		
        protected override void Init()
        {
			 //_frInd3 = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			 _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			k=k1;
			_frInd.Range=k;
			k=k+2;
			hline = Tools.Create<HorizontalLine>();
			lline = Tools.Create<HorizontalLine>();
			indH=10;
			indL=10;

        }        

       
        protected override void NewBar()
        {
// =================================================================================================================================				 
			frSU=_frInd.TopSeries[Bars.Range.To-k];
			frSD=_frInd.BottomSeries[Bars.Range.To-k];
			
			//if(frSU>0 && Up) { 
			if(frSU>0) {  U1=Bars[Bars.Range.To-k].High; hline.Price = U1; }
			if(frSD>0) {  D1=Bars[Bars.Range.To-k].Low;  lline.Price = D1; }

//========================================================================================			
			// Значения текущего Бара
H1=Bars[Bars.Range.To-1].High;  H2=Bars[Bars.Range.To-2].High;  H3=Bars[Bars.Range.To-3].High; H4=Bars[Bars.Range.To-4].High;
L1=Bars[Bars.Range.To-1].Low;   L2=Bars[Bars.Range.To-2].Low;   L3=Bars[Bars.Range.To-3].Low;  L4=Bars[Bars.Range.To-4].Low;
C1=Bars[Bars.Range.To-1].Close; C2=Bars[Bars.Range.To-2].Close; C3=Bars[Bars.Range.To-3].Close;C4=Bars[Bars.Range.To-4].Close;
O1=Bars[Bars.Range.To-1].Open;  O2=Bars[Bars.Range.To-2].Open;  O3=Bars[Bars.Range.To-3].Open; O4=Bars[Bars.Range.To-4].Open;
DTime = Bars[Bars.Range.To-1].Time;
			
			if(H1>U1 && L1<U1) indH=0;
			if(L1<D1 && H1>D1) indL=0;
			indH++; indL++;

// 2 ----------------------------------------------------------------------------------------------------------------------------
/*			if(O4<U1 && C4>U1 && O3>U1 && C3<U1 && O2<U1 && O2>C2 && C1<U1 && O1<C1 && indH==4) 
			{   
				var vW = Tools.Create<VerticalLine>();
			    vW.Color=Color.Red;
				vW.Time = Bars[Bars.Range.To-1].Time; 	
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(300,300), null, null);
			} 

			if(O4>D1 && C4<D1 && O3<D1 && C3>D1 && O2>D1 && O2<C2 && C1>D1 && O1>C1 && indL==4) 
			{   
				var vW1 = Tools.Create<VerticalLine>();
			    vW1.Color=Color.Blue;
				vW1.Time = Bars[Bars.Range.To-1].Time;
				
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPips(300,300), null, null);	
			}	

			
*/			
// 2 ----------------------------------------------------------------------------------------------------------------------------
			if(H3>U1 && L3<U1 && O3<U1 && C3<U1  && Math.Abs(O2-C2)<150 && Math.Abs(O1-C1)<150 && H2<U1+10 && H1<U1+10  && indH==3) 
			{   
				var vW = Tools.Create<VerticalLine>();
			    vW.Color=Color.Red;
				vW.Time = Bars[Bars.Range.To-1].Time; 	
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(300,300), null, null);
			} 

			if(L3<D1 && H3>D1 && O3>D1 && C3>D1 && L2>D1 && L1>D1 && Math.Abs(O2-C2)<150 && Math.Abs(O1-C1)<150 && indL==3) 
			{   
				var vW1 = Tools.Create<VerticalLine>();
			    vW1.Color=Color.Blue;
				vW1.Time = Bars[Bars.Range.To-1].Time;
				Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPips(300,300), null, null);	
			}	

// ===============================================================================================================================
			
				
		//if(srU>5  && Bars.Range.To==(D1+srD)) Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Instrument.Ask, -1, Stops.InPips(150,100), null, null);
        //if(srD>5  && Bars.Range.To==(U1+srU)) Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Instrument.Bid, -1, Stops.InPips(150,100), null, null);			
        
        }
    }
}