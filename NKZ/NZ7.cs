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
    [TradeSystem("NZ7")]            //copy of "NZ"
    public class ZZ_Ex1 : TradeSystem
    {

		[Parameter("Fractal", DefaultValue = 9)]
		public int frac { get;set; }	
		public DateTime DTime; // Время
		private int ci = 0,ku,kd,i;
		public Fractals _frInd;	
		private FisherTransformOscillator _ftoInd;			
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		public double frU1,frU2,frU3,frU4,frU5;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3,frD4,frD5;    // Значение Low - свечи с верхним фрактклом
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3;
		public bool frU,frD;
		private VerticalLine vy,vb;		
		
        protected override void Init()
        {	
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;
				_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);						
        }        
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
//====  Fractal ====================================================================================
	if(_frInd.TopSeries[Bars.Range.To-frac]>0 || _frInd.BottomSeries[Bars.Range.To-frac]>0)	{
		i=1; ku=0; kd=0; frU3=0; Print("{0}  Fractal!!",Bars[Bars.Range.To-frac].Time);
		while(frU3==0 && i<100) {  Print("{0} - {1} Fractal!!",_frInd.TopSeries[Bars.Range.To-frac-i],i);
			  if (_frInd.TopSeries[Bars.Range.To-frac-i]>0) 		
			  {	
				  if(frU) { frU=false;	
				  frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-frac-i];
				  tmU3=tmU2; tmU2=tmU1; tmU1=Bars[Bars.Range.To-frac-i].Time;  
				  } else {
					  if(_frInd.TopSeries[Bars.Range.To-frac-i]>frU1) {
						  frU1=_frInd.TopSeries[Bars.Range.To-frac-i];
					  	  tmU1=Bars[Bars.Range.To-frac-i].Time; }
				  }
			  }
			  
			  
			  if (_frInd.BottomSeries[Bars.Range.To-frac-i]>0)   
			  { 
				  if(!frU) { frU=true; 
				  frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-frac-i];
				     tmD3=tmD2; tmD2=tmD1; tmD1=Bars[Bars.Range.To-frac].Time;
				  } else  {	 
					  if (_frInd.BottomSeries[Bars.Range.To-frac-i]<frD1) {
						 frD1=_frInd.BottomSeries[Bars.Range.To-frac-i];
					     tmD1=Bars[Bars.Range.To-frac-i].Time; } 
				  }
			  } 
			  i++;
		}  ;
		Print("{0} - ku={7} kd={8} i={9} tmU1={1} tmU2={2} tmU3={3} tmD1={4} tmD2={5} tmD3={6}",Bars[Bars.Range.To-frac].Time,tmU1,tmU2,tmU3,tmD1,tmD2,tmD3,ku,kd,i);
	}
      }
//===============================================================================================================================   

    }
}