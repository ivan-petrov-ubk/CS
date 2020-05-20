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
    [TradeSystem("NKZ_Fisher_Max")]
    public class NKZ_Fisher_Max : TradeSystem
    {
        // Simple parameter example
        [Parameter("Ідея", DefaultValue = "Фишер через 0 - определение максимума/минимума")]
        public string CommentText { get; set; }
		[Parameter("StopLoss:", DefaultValue = 250)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 30)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 7)]
		public int frac { get;set; }	
				private int ci = 0;
				private double dlt;
		public FisherTransformOscillator _ftoInd;
		public VerticalLine toolVerticalLine ;
		public int kl;
		public bool fish;
		public double mind, maxu, mindf, maxuf,kf;
		public double m1,m2,m3,m4,m5,m6,m7,m8,m9;
		public DateTime mt1,mt2,mt3,mt4,mt5,mt6,mt7,mt8,mt9;
		public DateTime mindt, maxut;
		public PolyLine toolPolyLine;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;		
		
        protected override void Init()
        {
			 _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			mind=double.MaxValue;
			maxu=double.MinValue;
			fish=false;
			toolPolyLine = Tools.Create<PolyLine>();
			toolPolyLine.Color=Color.Aqua;
			toolPolyLine.Width=2;
			kf=0.0;
					
			dlt=dl*Instrument.Point; 
        }        

       
        protected override void NewBar()
        {
			//DTime = Bars[Bars.Range.To-1].Time;
			ci = Bars.Range.To - 1;
  //=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
//=== Трелинг  =======================================================================================================================	
			TrailActiveOrders();
			
			// переход через 0 UP - ВВЕРХ - 
					// определяем минимум пред тренда вниз - Up=false
			if(_ftoInd.FisherSeries[Bars.Range.To-1]>0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]<0) 
			{ 
				kl=0;  mind=double.MaxValue; mindf=double.MaxValue; 
				do
				{   kl++;          //Переход фишера 0 UP - ВВЕРХ - определяем минимум свечи и фишера между 0 фишера
					if(Bars[Bars.Range.To-kl].Low<mind) { mind=Bars[Bars.Range.To-kl].Low; mindt=Bars[Bars.Range.To-kl].Time; }
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]<mindf) mindf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]>0) && kl<1000);
		
						m9=m8;   m8=m7;   m7=m6;   m6=m5;   m5=m4;   m4=m3;   m3=m2;   m2=m1;   m1=mind;  
						mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=mindt;
				//Print("BUY {0} - {1} {2} {3} {4} {5} ",Bars[Bars.Range.To-1].Time,m4<m6,m4<m2,m6<m8,m3<m1,m5<m7);	
				if(m4<m6 && m4<m2 &&  m3<m1 && m5<m7 && (m1-m2)/(m3-m2)<0.7 && (m1-m2)/(m3-m2)>0.38 && (m3-m2)/(m3-m4)>1.38 ) 
				{	Print("BUY {0} -(1-2)/(3-2)={1} (3-2)/(3-4)={2}",Bars[Bars.Range.To-1].Time,(m1-m2)/(m3-m2),(m3-m2)/(m3-m4));
					   if (posGuidBuy==Guid.Empty) {  		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
							   				null,null,null); 
								            // Stops.InPips(SL1,null),null,null);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id; }  //BUY
					   
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.White;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));				
				}
			} 
//===================================================================================================================================			
            // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
			if(_ftoInd.FisherSeries[Bars.Range.To-1]<0 &&  _ftoInd.FisherSeries[Bars.Range.To-2]>0) 
			{   
				kl=0;  maxu=double.MinValue; maxuf=double.MinValue;
				do
				{ kl++; // DOWN - ВНИЗ - ВИЗНАЧАЄМО МАКСИМУМ
					if(Bars[Bars.Range.To-kl].High>maxu) {maxu=Bars[Bars.Range.To-kl].High; maxut=Bars[Bars.Range.To-kl].Time;}  
					if(_ftoInd.FisherSeries[Bars.Range.To-kl]>maxuf) maxuf=_ftoInd.FisherSeries[Bars.Range.To-kl];
				}
				while(!(_ftoInd.FisherSeries[Bars.Range.To-kl]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-kl-1]<0) && kl<1000);
  				   m9=m8; m8=m7; m7=m6; m6=m5; m5=m4; m4=m3; m3=m2; m2=m1; m1=maxu;  
				   mt8=mt7; mt7=mt6; mt6=mt5; mt5=mt4; mt4=mt3; mt3=mt2; mt2=mt1; mt1=maxut;
				//Print("SELL {0} - {1} {2} {3} {4} {5} ",Bars[Bars.Range.To-1].Time,m4>m6,m4>m2,m6>m8,m3>m1,m5>m7);
				if(m4>m6 && m4>m2 && m3>m1 && m5>m7 && (m1-m2)/(m3-m2)<0.7 && (m1-m2)/(m3-m2)>0.38 && (m3-m2)/(m3-m4)>1.38) 
				{	 Print("SELL {0} -(1-2)/3-2)={1} (3-2)/(3-4)={2}",Bars[Bars.Range.To-1].Time,(m1-m2)/(m3-m2), (m3-m2)/(m3-m4));
					   if (posGuidSell==Guid.Empty) {
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
							   				null,null,null); 
					 		                // Stops.InPips(SL1,null),null,null); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;	} //SELL
					
						var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.White;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(mt1, m1));
							toolPolyLine.AddPoint(new ChartPoint(mt2, m2));
							toolPolyLine.AddPoint(new ChartPoint(mt3, m3)); 
							toolPolyLine.AddPoint(new ChartPoint(mt4, m4));				
							toolPolyLine.AddPoint(new ChartPoint(mt5, m5));				
							toolPolyLine.AddPoint(new ChartPoint(mt6, m6));					
				}
		      
            }
        
    }
	protected void TrailActiveOrders()
		{		
		  if(posGuidBuy!=Guid.Empty)  { var tr = Trade.UpdateMarketPosition(posGuidBuy,	  getSL(1),null," - update TP,SL"); }
		  if(posGuidSell!=Guid.Empty) { var tr = Trade.UpdateMarketPosition(posGuidSell,  getSL(0),null," - update TP,SL"); }
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