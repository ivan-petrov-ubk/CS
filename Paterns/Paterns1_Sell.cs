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
    [TradeSystem("Paterns1_Sell")]  //copy of "Paterns1"
    public class NKZ_Fisher_Max : TradeSystem
    {
        // Simple parameter example
        [Parameter("Ідея", DefaultValue = "Фишер через 0 - определение максимума")]
        public string CommentText { get; set; }
		[Parameter("TP:", DefaultValue = 200)]
        public int TP1 { get; set; }			
		[Parameter("StopLoss:", DefaultValue = 200)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }		
				private int ci = 0;
				private double dlt,n1;
		public FisherTransformOscillator _ftoInd;
		public VerticalLine toolVerticalLine ;
		public int kl;
		public string pt="",pb,ps;
		public bool fish,torg;
		public double mind, maxu, mindf, maxuf,kf,fst=0;
		public double m1,m2,m3,m4,m5,m6,m7,m8,m9;
		public DateTime mt1,mt2,mt3,mt4,mt5,mt6,mt7,mt8,mt9;
		public DateTime mindt, maxut, DTime;
		public PolyLine toolPolyLine;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;		
		
        protected override void Init()
        {
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
        }        

       
        protected override void NewBar()
        {   if(torg) torg=false;
			DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
  //=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  		    
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
				//TrailActiveOrders();
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
				//TrailActiveOrders();
            }

//===================================================================================================================================			
		//  Патерн 5-0
/*			if( (m4-m3)/(m4-m5)>=0.13  && (m4-m3)/(m4-m5)<=0.618 && 
				(m2-m3)/(m4-m3)>=1.618 && (m2-m3)/(m4-m3)>=2.24  &&
				(m2-m1)/(m2-m3)>=0.5 && (m2-m1)/(m2-m3)>=0.886 && fst!=m1) 
				{	    Print("Патерн 5-0 - {0} - {1}",DTime,Instrument.Name); fst=m1; torg=true; pt="Патерн 5-0";
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
				*/
		//  Летучая мышь №2 :
			if( (m4-m3)/(m4-m5)>=0.382  && (m4-m3)/(m4-m5)<=0.5  && 
				(m4-m3)/(m2-m3)>=0.382 && (m4-m3)/(m2-m3)>=0.886  &&
				(m2-m1)/(m2-m3)>=2.0 && (m2-m1)/(m2-m3)>=3.618   &&
				(m4-m1)/(m4-m5)>=1.13  && fst!=m1) 
				{	    Print("Летучая мышь №2 - {0} - {1}",DTime,Instrument.Name);fst=m1;torg=true;pt="Летучая мышь №2";
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
				{	    Print("Глубокий краб: - {0} - {1}",DTime,Instrument.Name);fst=m1;torg=true;pt="Глубокий краб";
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
				{	    Print("Идеальный краб : - {0} - {1}",DTime,Instrument.Name);fst=m1;torg=true;pt="Идеальный краб";
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
				{	    Print("Бабочка Гартли: - {0} - {1}",DTime,Instrument.Name);fst=m1;torg=true;pt="Бабочка Гартли";
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
				{	    Print("Бабочка Песавенто : - {0} - {1}",DTime,Instrument.Name);  fst=m1;torg=true;pt="Бабочка Песавенто";
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
				{	    Print("3 движения : - {0} - {1}",DTime,Instrument.Name);fst=m1;torg=true;pt="3 движения";
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
				if(torg && fish) // Buy
				{   if (posGuidBuy==Guid.Empty) {  	pb=pt;	torg=false;
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								            Stops.InPips(SL1,TP1),pb,null);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id; } 
				}
				if(torg && !fish) // Sell
				{ if (posGuidSell==Guid.Empty) {   ps=pt;  torg=false;
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(SL1,TP1),ps,null); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id; } 
				}
    }
	protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { 
			  if(fish && m1!=n1) { n1=m1;
			  var sl2 = Math.Round(m1-dlt-Instrument.Spread, Instrument.PriceScale);
			  var tr = Trade.UpdateMarketPosition(posGuidBuy, sl2,null,pb); 
			  }
		  }
		  
		  if(posGuidSell!=Guid.Empty) { 
			  if(!fish && m1!=n1) { n1=m1;
			  var sl2 = Math.Round(m1+dlt+Instrument.Spread, Instrument.PriceScale);
			  var tr = Trade.UpdateMarketPosition(posGuidSell, sl2 ,null,ps);  }
		  }
		  	  
		} 	
	
protected void TrailActiveOrders2()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null,pb); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null,ps); }
		} 	
			protected double getSL(int type)
		{
			switch(type)
			{   case 0: {   double MAX = double.MinValue;
							for(int i = 0; i < frac; i++)
							{ if(Bars[ci - i].High > MAX)
									MAX = Bars[ci - i].High; 
							}	
							return Math.Round(MAX+dlt+Instrument.Spread, Instrument.PriceScale);
						}
				case 1: {   double MIN = double.MaxValue;
							for(int i = 0; i < frac; i++)
							{  if(Bars[ci - i].Low < MIN)
									MIN = Bars[ci - i].Low; 
							}	
							return Math.Round(MIN-dlt-Instrument.Spread, Instrument.PriceScale);
						}
				default:  break;
			}
			return 0.0;
		}	
  }
}