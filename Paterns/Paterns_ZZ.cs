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
    [TradeSystem("Paterns_ZZ")]  //copy of "Paterns1"
    public class NKZ_Fisher_Max : TradeSystem
    {
        // Simple parameter example
        [Parameter("Ідея", DefaultValue = "Фишер через 0 - определение максимума")]
        public string CommentText { get; set; }
	
				private int ci = 0;
				private double dlt;
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
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=12;
			_wprInd.ExtDeviation=0;
			_wprInd.ExtBackStep=3;
        }        

       
        protected override void NewBar()
        {   _wprInd.ReInit();
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ			
			  	   m9=m8; m8=m7; m7=m6; m6=m5; m5=m4; m4=m3; m3=m2; m2=m1; m1=zz2;
				   mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=Bars[zzi2].Time;
				}
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
			  	   m9=m8; m8=m7; m7=m6; m6=m5; m5=m4; m4=m3; m3=m2; m2=m1; m1=zz2 ;
				   mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=Bars[zzi2].Time;
				}
			}
//===================================================================================================================================			
		//  Патерн 5-0
			if( (m4-m3)/(m4-m5)>=0.13  && (m4-m3)/(m4-m5)<=0.618 && 
				(m2-m3)/(m4-m3)>=1.618 && (m2-m3)/(m4-m3)>=2.24  &&
				(m2-m1)/(m2-m3)>=0.5 && (m2-m1)/(m2-m3)>=0.886 && fst!=m1) 
				{	    Print("Патерн 5-0 - {0} - {1}",DTime,Instrument.Name); fst=m1; 
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
		//  Летучая мышь №2 :
			if( (m4-m3)/(m4-m5)>=0.382  && (m4-m3)/(m4-m5)<=0.5  && 
				(m4-m3)/(m2-m3)>=0.382 && (m4-m3)/(m2-m3)>=0.886  &&
				(m2-m1)/(m2-m3)>=2.0 && (m2-m1)/(m2-m3)>=3.618   &&
				(m4-m1)/(m4-m5)>=1.13  && fst!=m1) 
				{	    Print("Летучая мышь №2 - {0} - {1}",DTime,Instrument.Name);fst=m1;
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(DTime, Bars[Bars.Range.To-1].Close));
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));	
							var toolText = Tools.Create<Text>(); toolText.Color=Color.White;	toolText.FontSize=14; toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High);
        					toolText.Caption="Летучая мышь №2 ";
					//var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=DTime;	
				}
		//  Глубокий краб:
			if( (m4-m3)/(m4-m5)>=0.382  && (m4-m3)/(m4-m5)<=0.886  && 
				(m4-m3)/(m2-m3)>=0.382 && (m4-m3)/(m2-m3)>=0.886  &&
				(m2-m1)/(m2-m3)>=2.240 && (m2-m1)/(m2-m3)>=3.618   &&
				(m4-m1)/(m4-m5)>=1.618  && fst!=m1) 
				{	    Print("Глубокий краб: - {0} - {1}",DTime,Instrument.Name);fst=m1;
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.SeaGreen;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(DTime, Bars[Bars.Range.To-1].Close));
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));	
							var toolText = Tools.Create<Text>(); toolText.Color=Color.SeaGreen;	toolText.FontSize=14; toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High);
        					toolText.Caption="Глубокий краб";	
					//var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=DTime;	
				}

		// Идеальный краб :
			if( (m4-m3)/(m4-m5)>=0.618  && (m4-m3)/(m4-m5)<=0.886  && 
				(m4-m3)/(m2-m3)>=0.500 && (m4-m3)/(m2-m3)>=0.618  &&
				(m2-m1)/(m2-m3)>=2.00 && (m2-m1)/(m2-m3)>=3.140   &&
				(m4-m1)/(m4-m5)>=1.618  && fst!=m1 ) 
				{	    Print("Идеальный краб : - {0} - {1}",DTime,Instrument.Name);fst=m1;
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Yellow;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(DTime, Bars[Bars.Range.To-1].Close));
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));	
							var toolText = Tools.Create<Text>(); toolText.Color=Color.Yellow;	toolText.FontSize=14; toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High);
       						toolText.Caption="Идеальный краб";	
					//var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=DTime;	
				}
		// Бабочка Гартли
			if( (m4-m3)/(m4-m5)>=0.618  && (m4-m3)/(m4-m5)<=0.886  && 
				(m4-m3)/(m2-m3)>=0.382 && (m4-m3)/(m2-m3)>=0.886  &&
				(m2-m1)/(m2-m3)>=1.270 && (m2-m1)/(m2-m3)>=1.618   &&
				(m4-m1)/(m4-m5)>=0.786  && fst!=m1) 
				{	    Print("Бабочка Гартли: - {0} - {1}",DTime,Instrument.Name);fst=m1;
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Violet;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(DTime, Bars[Bars.Range.To-1].Close));
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));		
							var toolText = Tools.Create<Text>(); toolText.Color=Color.Violet;	toolText.FontSize=14; toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High);
        					toolText.Caption="Бабочка Гартли";	
					//var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=DTime;	
				}	
		// Бабочка Песавенто
			if( (m4-m3)/(m4-m5)>=0.786  && (m4-m3)/(m4-m5)<=0.886  && 
				(m4-m3)/(m2-m3)>=0.382 && (m4-m3)/(m2-m3)>=0.886  &&
				(m2-m1)/(m2-m3)>=1.618 && (m2-m1)/(m2-m3)>=2.618   &&
				(m4-m1)/(m4-m5)>=1.270 && (m4-m1)/(m4-m5)>=1.618 && fst!=m1) 
				{	    Print("Бабочка Песавенто : - {0} - {1}",DTime,Instrument.Name);  fst=m1;
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.DeepPink;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(DTime, Bars[Bars.Range.To-1].Close));
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));	
							var toolText = Tools.Create<Text>(); toolText.Color=Color.DeepPink;	toolText.FontSize=14; toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High);
        					toolText.Caption="Бабочка Песавенто";	
					//var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=DTime;	
				}					
        // 3 движния
			if( (m1-m2)/(m3-m2)>=1.270  && (m1-m2)/(m3-m2)<=1.618  && 
				(m3-m4)/(m5-m4)>=1.270  && (m3-m4)/(m5-m4)<=1.618  && fst!=m1) 
				{	    Print("3 движения : - {0} - {1}",DTime,Instrument.Name);fst=m1;
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.DarkRed;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(DTime, Bars[Bars.Range.To-1].Close));
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));
							var toolText = Tools.Create<Text>(); toolText.Color=Color.DarkRed;	toolText.FontSize=14; toolText.Point=new ChartPoint(DTime, Bars[Bars.Range.To-1].High);
        					toolText.Caption="3 движения :";	
					//var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=DTime;	
				}	

    }

  }
}