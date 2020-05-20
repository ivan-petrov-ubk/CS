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
    [TradeSystem("Fisher3v_1")]            //copy of "Fisher3v"
    public class ZZ_Ex1 : TradeSystem
    {

		public FisherTransformOscillator _ftoInd;


		public double Fs,Fs1,Fm,Fm1;
		private int ku,kd;
		private bool  FsU, FsD;
		
       protected override void Init()
        {  

			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
        }      


//===============================================================================================================================
        protected override void NewBar()
        {
				     Fs1=Fs; Fm1=Fm;
					Fs =_ftoInd.FisherSeries[Bars.Range.To-1];
				    Fm =_ftoInd.Ma1Series[Bars.Range.To-1];

					if ( Fm1<Fs1 && Fm>Fs && Fs>0)  { ku++; var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Red; }
					if ( Fm1>Fs1 && Fm<Fs && Fs<0)  { kd++; var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Blue; }
					if ( Fs1>0 && Fs<0 ) kd=0;
					if ( Fs1<0 && Fs>0 ) ku=0; 			

//====== Определяем пересечение нуля фишером========================================================================================
	if(_ftoInd.FisherSeries[Bars.Range.To-2]<0 && _ftoInd.FisherSeries[Bars.Range.To-1]>0) FsU=true; else FsU=false;
	if(_ftoInd.FisherSeries[Bars.Range.To-2]>0 && _ftoInd.FisherSeries[Bars.Range.To-1]<0) FsD=true; else FsD=false;

	if(kd>=3 && ku<=2 && ku>0 && Fs1>0 && Fs<0) { var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Red; vl1.Width=3; }
	if(ku>=3 && kd<=2 && kd>0 && Fs1<0 && Fs>0) { var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[Bars.Range.To-1].Time; vl1.Color=Color.Blue;vl1.Width=3; }
	
	 Print("{0} - ku={1} kd={2} Fs={3} Fs1={4} Fm={5} Fm1={6}",Bars[Bars.Range.To-1].Time,ku,kd,Math.Round(Fs,3),Math.Round(Fs1,3),Math.Round(Fm,3),Math.Round(Fm1,3));
		}
	}
}
