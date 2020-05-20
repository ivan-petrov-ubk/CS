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
    [TradeSystem("FMM1")]  //copy of "Fisher_MaxMin"
    public class NKZ_Fisher_Max : TradeSystem
    {
        // Simple parameter example
        [Parameter("Ідея", DefaultValue = "Фишер через 0 - определение максимума/минимума")]
        public string CommentText { get; set; }

		[Parameter("Fisher MIN:", DefaultValue = 0.3)]
        public double kf1 { get; set; }			
		
		public FisherTransformOscillator _ftoInd;
		public int kl;
		public double mind, maxu, mindf, maxuf,kf;
		public double mu1,mu2,mu3,mu4;
		public double md1,md2,md3,md4;
		public int mdi1,mdi2,mdi3,mdi4,mindi;
		public int mui1,mui2,mui3,mui4,maxui;
		
		public DateTime mdt1,mdt2,mdt3,mdt4;
		public DateTime mut1,mut2,mut3,mut4;
		
		public DateTime mindt, maxut;
		
		public TrendLine toolTrendLineU1,toolTrendLineU2,toolTrendLineU3,toolTrendLineD1,toolTrendLineD2,toolTrendLineD3;
		
        protected override void Init()
        {
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			mind=double.MaxValue;
			maxu=double.MinValue;
			kf=0.0;
			
			toolTrendLineU1 = Tools.Create<TrendLine>(); toolTrendLineU1.Color=Color.Aqua;toolTrendLineU1.Width=3;
			toolTrendLineU2 = Tools.Create<TrendLine>(); toolTrendLineU2.Color=Color.Aqua;toolTrendLineU2.Width=3;
			toolTrendLineU3 = Tools.Create<TrendLine>(); toolTrendLineU3.Color=Color.Aqua;toolTrendLineU3.Width=3;
			toolTrendLineD1 = Tools.Create<TrendLine>(); toolTrendLineD1.Color=Color.Aqua;toolTrendLineD1.Width=3;
			toolTrendLineD2 = Tools.Create<TrendLine>(); toolTrendLineD2.Color=Color.Aqua;toolTrendLineD2.Width=3;
			toolTrendLineD3 = Tools.Create<TrendLine>(); toolTrendLineD3.Color=Color.Aqua;toolTrendLineD3.Width=3;			
        }        
       
        protected override void NewBar()
        {
//=== ==========================================================================================================================
// MIN - ОПРЕДЕЛЯЕМ МИНИМУМ
			//  Fisher - пересекает 0 снизу вверх!
			if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0 ) 
			{ 
//==============================================================================================================================				
				kl=0;  mind=double.MaxValue; mindf=double.MaxValue; 
				do    //  MINIMUM
				{   kl++;         
					if(Bars[Bars.Range.To-kl].Low<mind) {mind=Bars[Bars.Range.To-kl].Low; mindt=Bars[Bars.Range.To-kl].Time; mindi=Bars.Range.To-kl; }
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]<mindf) mindf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]>0) && kl<1000);
//==============================================================================================================================				
					if(mindf<-kf1) 
					{ 	if(mui1>mdi1) 
						 {  md4=md3;   md3=md2;   md2=md1;   md1=mind;  
						    mdt4=mdt3; mdt3=mdt2; mdt2=mdt1; mdt1=mindt;
				            mdi4=mdi3; mdi3=mdi2; mdi2=mdi1; mdi1=mindi;
						 } else { if (mind<md1) { md1=mind; mdt1=mindt; mdi1=mindi; } }			 			 
if(mu1>0  && md1>0) { toolTrendLineD1.Point1= new ChartPoint(mdt1, md1); toolTrendLineD1.Point2= new ChartPoint(mut1, mu1); }
if(mu1>0  && md2>0) { toolTrendLineU1.Point1= new ChartPoint(mut1, mu1); toolTrendLineU1.Point2= new ChartPoint(mdt2, md2); } 
if(mu2>0  && md2>0) { toolTrendLineD2.Point1= new ChartPoint(mdt2, md2); toolTrendLineD2.Point2= new ChartPoint(mut2, mu2); }
if(mu2>0  && md3>0) { toolTrendLineU2.Point1= new ChartPoint(mut2, mu2); toolTrendLineU2.Point2= new ChartPoint(mdt3, md3); }
if(mu3>0  && md3>0) { toolTrendLineD3.Point1= new ChartPoint(mdt3, md3); toolTrendLineD3.Point2= new ChartPoint(mut3, mu3); }
if(mu3>0  && md4>0) { toolTrendLineU3.Point1= new ChartPoint(mut3, mu3); toolTrendLineU3.Point2= new ChartPoint(mdt4, md4); }
					}  
					/*
					var toolText = Tools.Create<Text>(); 
toolText.Point=new ChartPoint(mindt, mind);
toolText.Caption=string.Format("Fs={0}",Math.Round(mindf,3));*/
			} 
//==============================================================================================================================			
// MAX - ВИЗНАЧАЄМО МАКСИМУМ
			  //  Fisher - пересекает 0 сверху вниз!
	if( _ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0 ) 
		{   
//==============================================================================================================================				
				kl=0;  maxu=double.MinValue; maxuf=double.MinValue;
				do
				{ kl++; // ВИЗНАЧАЄМО МАКСИМУМ - при этом Fisher gthtctrftn 0 DOWN - ВНИЗ
					if(Bars[Bars.Range.To-kl].High>maxu) {maxu=Bars[Bars.Range.To-kl].High; maxut=Bars[Bars.Range.To-kl].Time; maxui=Bars.Range.To-kl;}  
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]>maxuf) maxuf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}				
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]<0) && kl<1000);
//==============================================================================================================================				
  				  if(maxuf>kf1) 
				  {   if(mui1<mdi1)  {
				   			mu4=mu3;   mu3=mu2;   mu2=mu1;   mu1=maxu;  
				   			mut4=mut3; mut3=mut2; mut2=mut1; mut1=maxut;
							mui4=mui3; mui3=mui2; mui2=mui1; mui1=maxui;
					  } else { if (maxu>mu1) { mu1=maxu; mut1=maxut; mui1=maxui;} }				  
if(mu1>0  && md1>0) { toolTrendLineU1.Point1= new ChartPoint(mut1, mu1); toolTrendLineU1.Point2= new ChartPoint(mdt1, md1); }				  
if(mu2>0  && md1>0) { toolTrendLineD1.Point1= new ChartPoint(mdt1, md1); toolTrendLineD1.Point2= new ChartPoint(mut2, mu2); } 
if(mu2>0  && md2>0) { toolTrendLineU2.Point1= new ChartPoint(mut2, mu2); toolTrendLineU2.Point2= new ChartPoint(mdt2, md2); }
if(mu3>0  && md2>0) { toolTrendLineD2.Point1= new ChartPoint(mdt2, md2); toolTrendLineD2.Point2= new ChartPoint(mut3, mu3); }
if(mu3>0  && md3>0) { toolTrendLineU3.Point1= new ChartPoint(mut3, mu3); toolTrendLineU3.Point2= new ChartPoint(mdt3, md3); }
if(mu4>0  && md3>0) { toolTrendLineD3.Point1= new ChartPoint(mdt3, md3); toolTrendLineD3.Point2= new ChartPoint(mut4, mu4); }				  
				  }  
	  /*
				  var toolText = Tools.Create<Text>(); 
toolText.Point=new ChartPoint(maxut, maxu+Instrument.Spread*2);
toolText.Caption=string.Format("Fs={0}",Math.Round(maxuf,3));	*/
        }
 //===================================================================================================================================       
    }

  }
}