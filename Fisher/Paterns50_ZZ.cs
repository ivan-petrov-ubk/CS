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
    [TradeSystem("Paterns50_ZZ")]   //copy of "Paterns_ZZ"
    public class NKZ_Fisher_Max : TradeSystem
    {
        // Simple parameter example
        [Parameter("Ідея", DefaultValue = "Фишер через 0 - определение максимума")]
        public string CommentText { get; set; }
	
				private int ci = 0;
				private double dlt;
		public FisherTransformOscillator _ftoInd;
		private ZigZag _wprInd;
		public VerticalLine toolVerticalLine ;
		public int kl;
		public bool fish;
		public double mind, maxu, mindf, maxuf,kf,fst=0;
		public double m1,m2,m3,m4,m5,m6,m7,m8,m9;
		public DateTime mt1,mt2,mt3,mt4,mt5,mt6,mt7,mt8,mt9;
		public DateTime mindt, maxut, DTime;
		public PolyLine toolPolyLine;
		private double zz1=2,zz2=2,zz3=2;	
		private int zzi1,zzi2,zzi3;
			
        protected override void Init()
        {
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			//_wprInd.ExtDepth=5;
			//_wprInd.ExtDeviation=0;
			//_wprInd.ExtBackStep=3;
        }        

       
        protected override void NewBar()
        {   _wprInd.ReInit();
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
//===================================================================================================================================			
			// переход через 0 UP - ВВЕРХ - 
					// определяем минимум пред тренда вниз - Up=false
			if(_ftoInd.FisherSeries[Bars.Range.To-1]>0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0) 
			{   fish=true; 
				kl=0;   mind=double.MaxValue; mindf=double.MaxValue; 
				do
				{   kl++;          //Переход фишера 0 UP - ВВЕРХ - определяем минимум свечи и фишера между 0 фишера
					if(Bars[Bars.Range.To-kl].Low<mind) { mind=Bars[Bars.Range.To-kl].Low; mindt=Bars[Bars.Range.To-kl].Time; }
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]<mindf) mindf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]>0) && kl<1000);
		
						m9=m8;   m8=m7;   m7=m6;   m6=m5;   m5=m4;   m4=m3;   m3=m2;   m2=m1;   m1=mind;  
						mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=mindt;
					/*				Print("U - {0} - {1}  {2}  {3} {4}",DTime,
					Math.Round((m3-m4)/(m5-m4),2),    //  1.13 - 1.618
					Math.Round((m3-m2)/(m3-m4),2),    //  1.618 - 2.24
					Math.Round((m1-m2)/(m3-m2),2),    //  0.5 - 0.618
					Math.Round((m1-m2)/(m3-m4),2));   // 1  */
			} 
//===================================================================================================================================			
            // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]<0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>0) 
			{   fish=false;
				kl=0;   maxu=double.MinValue; maxuf=double.MinValue;
				do
				{ kl++; // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
					if(Bars[Bars.Range.To-kl].High>maxu) {maxu=Bars[Bars.Range.To-kl].High; maxut=Bars[Bars.Range.To-kl].Time;}  
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]>maxuf) maxuf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]<0) && kl<1000);
  				   m9=m8; m8=m7; m7=m6; m6=m5; m5=m4; m4=m3; m3=m2; m2=m1; m1=maxu;  
				   mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=maxut;
					/*				Print("D - {0} - {1}  {2}  {3} {4}",DTime,
					Math.Round((m3-m4)/(m5-m4),2),    //  1.13 - 1.618
					Math.Round((m3-m2)/(m3-m4),2),    //  1.618 - 2.24
					Math.Round((m1-m2)/(m3-m2),2),    //  0.5 - 0.618
					Math.Round((m1-m2)/(m3-m4),2));   // 1 */
            }

//======================================================================================================================================
/*			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ			
			  	   m9=m8; m8=m7; m7=m6; m6=m5; m5=m4; m4=m3; m3=m2; m2=m1; m1=zz2;
				   mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=Bars[zzi2].Time;
					Print("U - {0} - {1}  {2}  {3} {4}",DTime,
					Math.Round((m3-m4)/(m5-m4),2),
					Math.Round((m3-m2)/(m3-m4),2),
					Math.Round((m1-m2)/(m3-m2),2),
					Math.Round((m1-m2)/(m3-m4),2));
				}
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
			  	   m9=m8; m8=m7; m7=m6; m6=m5; m5=m4; m4=m3; m3=m2; m2=m1; m1=zz2 ;
				   mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=Bars[zzi2].Time;
					Print("D - {0} - {1}  {2}  {3} {4}",DTime,
					Math.Round((m3-m4)/(m5-m4),2),    //  1.13 - 1.618
					Math.Round((m3-m2)/(m3-m4),2),    //  1.618 - 2.24
					Math.Round((m1-m2)/(m3-m2),2),    //  0.5 - 0.618
					Math.Round((m1-m2)/(m3-m4),2));   // 1
				}
			}
			*/
//===================================================================================================================================			
		//  Патерн 5-0
			if( //(m1-m2)/(m3-m4)>=0.9   && (m1-m2)/(m3-m4)<=1.1 && 
				(m3-m4)/(m5-m4)>=1.1  && (m3-m4)/(m5-m4)<=1.7  &&
				(m3-m2)/(m3-m4)>=1.7 && (m3-m2)/(m3-m4)<=2.3 && 
				(m1-m2)/(m3-m2)>=0.4   && (m1-m2)/(m3-m2)<=0.8 &&
				fst!=m1) 
				{	    //Print("Патерн 5-0 - {0} - {1}",DTime,Instrument.Name); 
						fst=m1; 
					Print("D - {0} - {1}  {2}  {3} {4}",DTime,
					Math.Round((m3-m4)/(m5-m4),2),    //  1.13 - 1.618
					Math.Round((m3-m2)/(m3-m4),2),    //  1.618 - 2.24
					Math.Round((m1-m2)/(m3-m2),2),    //  0.5 - 0.618
					Math.Round((m1-m2)/(m3-m4),2));   // 1
					
					var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(DTime, Bars[Bars.Range.To-1].Close));
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));		
							var toolText = Tools.Create<Text>(); toolText.Color=Color.White;	toolText.FontSize=14; toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High);
        					toolText.Caption="Патерн 5-0 ";	
					//var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=DTime;				
				}

    }

  }
}