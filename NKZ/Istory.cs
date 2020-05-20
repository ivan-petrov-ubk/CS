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
    [TradeSystem("Istory")]
    public class Istory : TradeSystem
    {
		[Parameter("N-Gist", DefaultValue = 100)]
		public int kol { get;set; }	
		
		public double frU1,frU2,frU3;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3;    // Значение Low - свечи с верхним фрактклом
		public double fsU1,fsU2,fsU3;   
		public double fsD1,fsD2,fsD3;    				
		public double min2,max2;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3,tmin,tmax;
		private int k,NKZ;
						public Fractals _frInd;		
						public FisherTransformOscillator _ftoInd;
		public bool frU;		
		public bool tu,td,nu,nd,n4;
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin;
		
        protected override void Init()
        {   k=0;NKZ=560;
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 			
			 Print("Init"); k=0;
			frU1=0;frU2=0;frU3=0;frD1=0;frD2=0;frD3=0;
	       // InitFr();
		}        

 
        protected override void NewBar()
        { if(k==0)  InitFr(kol);
		    
				
			if(k==1) 
		    { 

			if(tu) {  zmax = max2;  
				     nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 					
			}
			if(td) { zmin = min2;
				     nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				     nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					 nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				     nkz2v= zmin+(((NKZ*2))*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					 nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					 nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					 nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  				
			}
            Print(" UP ИТОРИЯ- {0} - frU1={1}({4}) frU2={2}({5}) frU3={3}({6}) zmax={7}({8})", 
			Bars[Bars.Range.To-1].Time,frU1,frU2,frU3,tmU1,tmU2,tmU3,zmax,tmax);
            Print(" DW ИТОРИЯ- {0} - frD1={4}({1}) frD2={5}({2}) frD3={6}({3})  zmin={7}({8})", 
			Bars[Bars.Range.To-1].Time,tmD1,tmD2,tmD3,frD1,frD2,frD3,zmin,tmin);
		    }
		    k++;
        }
//====== History =================================================================================================		
 		protected void InitFr(int kl)
		{
				var i=0; 
			    var kfd=0; 
			    var kfu=0;
			zmin=double.MaxValue;
			zmax=0;
			Print("Start InitFr");
			while ( i<kl ) 
			{ 
		Print("frU={0}-kfu={1} frD={2}-kfd={3}",_frInd.TopSeries[Bars.Range.To-i],kfu,_frInd.BottomSeries[Bars.Range.To-i],kfd);
			  if (_frInd.TopSeries[Bars.Range.To-i]>0  && kfu<3)	{  kfu++;
				  frU1=frU2; frU2=frU3; frU3=_frInd.TopSeries[Bars.Range.To-i];
				  fsU3=fsU2; fsU2=fsU1; fsU1=_ftoInd.FisherSeries[Bars.Range.To-i];
				  tmU1=tmU2; tmU2=tmU3; tmU3=Bars[Bars.Range.To-i].Time;  
					  }

			  if (_frInd.BottomSeries[Bars.Range.To-i]>0 && kfd<3)   { kfd++; 
				  frD1=frD2; frD2=frD3; frD3=_frInd.BottomSeries[Bars.Range.To-i];
				  fsU3=fsU2; fsU2=fsU1; fsU1=_ftoInd.FisherSeries[Bars.Range.To-i];
				  tmD1=tmD2; tmD2=tmD3; tmD3=Bars[Bars.Range.To-i].Time;
					  } 
			  
			  if(Bars[Bars.Range.To-i].High>zmax) { zmax=Bars[Bars.Range.To-i].High; tmax=Bars[Bars.Range.To-i].Time; }
			  if(Bars[Bars.Range.To-i].Low<zmin)  { zmin=Bars[Bars.Range.To-i].Low;  tmin=Bars[Bars.Range.To-i].Time;  }
		
			  	  i++;
        	}			
		 Print("OK!!! ---- {0} frU1={1} frU2={2} frU3={3} frD1={4} frD2={5} frD3={6} kfu={7} kfd={8}  zmax={9} zmin={10}",i,Bars[Bars.Range.To-i].Time,frU1,frU2,frU3,frD1,frD2,frD3,kfu,kfd,zmax,zmin);
			
		} 
//===============  END History ===============================================================================
		
    }
}