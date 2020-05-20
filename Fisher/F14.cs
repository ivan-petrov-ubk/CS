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
    [TradeSystem("F1")]
    public class F1 : TradeSystem
    {		
		[Parameter("NKZ :")]
        public int dt1 { get; set; }
	/*	[Parameter("frU1 :", DefaultValue = 0)]
        public int fu1 { get; set; }		
		[Parameter("frU2 :", DefaultValue = 0)]
        public int fu2 { get; set; }
		[Parameter("frD1 :", DefaultValue = 0)]
        public int fd1 { get; set; }
		[Parameter("frD2 :", DefaultValue = 0)]
        public int fd2 { get; set; } */
		[Parameter("Fractal", DefaultValue = 5)]
		public int frac { get;set; }	
		
		public DateTime DTime; // Время
		private int ci = 0 ,i=0;		
		public Fractals _frInd;			
		public double frU1,frU2,frU3;    
		public double frD1,frD2,frD3;	
		public bool frU,frD;
		
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3;		
		
        protected override void Init()
        {
	/*		frU1 = fu1*Instrument.Point;
			frU2 = fu2*Instrument.Point;
			frD1 = fd1*Instrument.Point;
			frD2 = fd2*Instrument.Point;  */
			frU1 = 0;
			frU2 = 0;
			frD1 = 0;
			frD2 = 0;  
			
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;			
        }        

        protected override void NewBar()
        {
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
//====  Fractal ====================================================================================
        i=0;     Print("S - {0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6}",Bars[Bars.Range.To-1].Time,frU1,frU2,frU3,frD1,frD2,frD3);

			while (!(frU1>0 && frU2>0 && frU3>0 && frD1>0 && frD2>0 && frD3>0 ) && i<100 ) {
		    Print("-------------   i={0} fracTop={1} {2} fracBot={3} {4}",i,_frInd.TopSeries[Bars.Range.To-frac-i],_frInd.TopSeries[Bars.Range.To-frac-i]>0,_frInd.BottomSeries[Bars.Range.To-frac-i],_frInd.BottomSeries[Bars.Range.To-frac-i]>0);
			  if (_frInd.TopSeries[Bars.Range.To-frac-i]>0) 		{	
				  
				  if(frU) { frU=false;		
				  frU1=frU2; frU2=frU3; frU3=_frInd.TopSeries[Bars.Range.To-frac-i];
				  tmU1=tmU2; tmU2=tmU3; tmU3=Bars[Bars.Range.To-frac-i].Time;  
				  } else {
					  if(_frInd.TopSeries[Bars.Range.To-frac-i]>frU1) {
						  frU3=_frInd.TopSeries[Bars.Range.To-frac-i];
					  	  tmU3=Bars[Bars.Range.To-frac-i].Time; }
				  }
Print("U - {0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6}", Bars[Bars.Range.To-frac-i].Time,frU1,frU2,frU3,frD1,frD2,frD3);
				  }
			  if (_frInd.BottomSeries[Bars.Range.To-frac-i]>0)   { 
				  if(!frU) { frU=true;
				  frD1=frD2; frD2=frD3; frD3=_frInd.BottomSeries[Bars.Range.To-frac-i];
				     tmD1=tmD2; tmD2=tmD3; tmD3=Bars[Bars.Range.To-frac-i].Time;
				  } else 
				  {
					 if (_frInd.BottomSeries[Bars.Range.To-frac-i]<frD1) {
						 frD3=_frInd.BottomSeries[Bars.Range.To-frac-i];
					     tmD3=Bars[Bars.Range.To-frac-i].Time;
					 } 
				  }
Print("D - {0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} ", Bars[Bars.Range.To-frac-i].Time,frU1,frU2,frU3,frD1,frD2,frD3);				  
			  } 			
            i++;
        }
 Print("=================================================================================================================================================");       
		}
    }
}