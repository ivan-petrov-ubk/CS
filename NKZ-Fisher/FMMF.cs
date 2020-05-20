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
    [TradeSystem("FMMF")]   //copy of "FMM2"
    public class NKZ_Fisher_Max : TradeSystem
    {
        // Simple parameter example
        [Parameter("Ідея", DefaultValue = "Фишер через 0 - определение максимума/минимума")]
        public string CommentText { get; set; }

		[Parameter("Fisher MIN:", DefaultValue = 0.3)]
        public double kf1 { get; set; }			
		
		public FisherTransformOscillator _ftoInd;
		public int kl;
		public double mind, maxu, mindf, maxuf;
		public double mu1,mu2,mu3,mu4;
		public double md1,md2,md3,md4;
		public int mdi1,mdi2,mdi3,mdi4,mindi;
		public int mui1,mui2,mui3,mui4,maxui;
		
		public DateTime mdt1,mdt2,mdt3,mdt4;
		public DateTime mut1,mut2,mut3,mut4;
		
		public DateTime mindt, maxut;
		
        protected override void Init()
        {
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
        }        
       
        protected override void NewBar()
        {
//===================================================================================================================================			
			if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0 ) 
			{ kl=0;  mind=double.MaxValue; mindf=double.MaxValue; 
				do 
				{   kl++;         
					if(Bars[Bars.Range.To-kl].Low<mind) {mind=Bars[Bars.Range.To-kl].Low; mindt=Bars[Bars.Range.To-kl].Time; mindi=Bars.Range.To-kl; }
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]<mindf) mindf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]>0) && kl<1000);
					if(mindf<-kf1) 
					{ 	if(mui1>mdi1) 
						 {  md4=md3;   md3=md2;   md2=md1;   md1=mind;  
						    mdt4=mdt3; mdt3=mdt2; mdt2=mdt1; mdt1=mindt;
				            mdi4=mdi3; mdi3=mdi2; mdi2=mdi1; mdi1=mindi;
						 } else { if (mind<md1) { md1=mind; mdt1=mindt; mdi1=mindi; } }			 			 
					}  
			} 
			
			if( _ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0 ) 
				{ kl=0;  maxu=double.MinValue; maxuf=double.MinValue;
				do
				{ kl++; 
					if(Bars[Bars.Range.To-kl].High>maxu) {maxu=Bars[Bars.Range.To-kl].High; maxut=Bars[Bars.Range.To-kl].Time; maxui=Bars.Range.To-kl;}  
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]>maxuf) maxuf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}				
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]<0) && kl<1000);
					if(maxuf>kf1) 
				  {   if(mui1<mdi1)  {
				   			mu4=mu3;   mu3=mu2;   mu2=mu1;   mu1=maxu;  
				   			mut4=mut3; mut3=mut2; mut2=mut1; mut1=maxut;
							mui4=mui3; mui3=mui2; mui2=mui1; mui1=maxui;
					  } else { if (maxu>mu1) { mu1=maxu; mut1=maxut; mui1=maxui;} }				  			  
				  }  
       		 	}
//===================================================================================================================================       
    }

  }
}