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
		/*	frU1 = fu1*Instrument.Point;
			frU2 = fu2*Instrument.Point;
			frD1 = fd1*Instrument.Point;
			frD2 = fd2*Instrument.Point;  */
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;			
        }        

        protected override void NewBar()
        {
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
//====  Fractal ====================================================================================
        i=0;     
			while (frU3==0 || frU2==0 || frU1==0 || i<100) {
		
			  if (_frInd.TopSeries[Bars.Range.To-frac-i]>0) 		{	
				  
				  if(frU) { frU=false;		
				  frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-frac-i];
				  tmU3=tmU2; tmU2=tmU1; tmU1=Bars[Bars.Range.To-frac-i].Time;  
				  } else {
					  if(_frInd.TopSeries[Bars.Range.To-frac-i]>frU1) {
						  frU1=_frInd.TopSeries[Bars.Range.To-frac-i];
					  	  tmU1=Bars[Bars.Range.To-frac-i].Time; }
				  }
Print("{0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6}", Bars[Bars.Range.To-frac].Time,frU1,frU2,frU3,frD1,frD2,frD3);
				  }
			  if (_frInd.BottomSeries[Bars.Range.To-frac-i]>0)   { 
				  if(!frU) { frU=true;
				  frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-frac-i];
				     tmD3=tmD2; tmD2=tmD1; tmD1=Bars[Bars.Range.To-frac-i].Time;
				  } else 
				  {
					 if (_frInd.BottomSeries[Bars.Range.To-frac-i]<frD1) {
						 frD1=_frInd.BottomSeries[Bars.Range.To-frac-i];
					     tmD1=Bars[Bars.Range.To-frac-i].Time;
					 } 
				  }
Print("{0} - frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} ", Bars[Bars.Range.To-frac].Time,frU1,frU2,frU3,frD1,frD2,frD3);				  
			  } 			
            i++;
        }
        
		}
    }
}