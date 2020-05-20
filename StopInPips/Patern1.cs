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
    [TradeSystem("Patern1")]
    public class Patern1 : TradeSystem
    {
        // Simple parameter example
		[Parameter("Тренд ВВЕРХ =", DefaultValue = true)]
		public bool tUp { get; set; }
		[Parameter("Тренд ВНИЗ =", DefaultValue = true)]
        public bool tDw { get; set; }
		
				public Fractals _frInd;
				public double frSU=0,frSD=0;
				public double U1,D1;
				public int kl=1;
				public DateTime tm1,tm2,tm3,tm4,tm5;
				private double d1,d2,d3,d4,d5;
				private int i1,i2,i3,i4,i5;
				private int t1,t2,t3,sl1,dl1;
				private double Ur1,Ur2,C1;
				private bool kBuy,kSell;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;	
		public FisherTransformOscillator _ftoInd;
		
		
        protected override void Init()
        {
          _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			Ur1=10.0;
			Ur2=0.0;

		}        

        protected override void NewBar()
        {
			//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  }
			//=== Закрытие  ордеров если уровень ==================================================================
			if(Bars[Bars.Range.To-1].High>Ur1 && posGuidBuy!=Guid.Empty)  {var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if(Bars[Bars.Range.To-1].Low<Ur2 && posGuidSell!=Guid.Empty ) {var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			frSU=_frInd.TopSeries[Bars.Range.To-5];
			frSD=_frInd.BottomSeries[Bars.Range.To-5];
			
			if(frSU>0 && kl==2 && frSU>d1) { d1=frSU; i1=Bars.Range.To-5;}
			
			if(frSU>0 && kl==1) {  	kl=2; d5=d4; d4=d3; d4=d3; d3=d2; d2=d1; d1=frSU;
									i5=i4; i4=i3; i4=i3; i3=i2; i2=i1; i1=Bars.Range.To-5; 
									C1=Bars[Bars.Range.To-1].Close;
									dl1=(int)(Math.Round(d1-C1,5)*100000);
					//Print("{0} - {1} {2} ",Bars[i1].Time,dl1,Math.Round(C1-d1,5));
									t1=(int)(Math.Round(Math.Abs(d2-d3),5)*100000); 	 // 100
									t2=(int)(Math.Round(1.618*Math.Abs(d2-d3),5)*100000); // 161.8
									t3=(int)(Math.Round(2.618*Math.Abs(d2-d3),5)*100000); // 261.8
									sl1=(int)(Math.Round(Math.Abs(C1-d3),5)*100000); 
										t1=t1-dl1;	t2=t2-dl1;	t3=t3-dl1;		
					
				
						if(d5<d3 && d1<d3 &&  C1<d1 &&
							Math.Abs((d2-d1)/(d3-d2))>0.5
							/* Math.Abs((d2-d1)/(d3-d2))<0.81 &&
							Bars[Bars.Range.To-4].Close<Bars[Bars.Range.To-4].Open &&
							Bars[Bars.Range.To-3].Close<Bars[Bars.Range.To-3].Open &&
							Bars[Bars.Range.To-2].Close<Bars[Bars.Range.To-2].Open*/ ) {
							Print("SELL {0} t1(100)={1} t2(161)={2} dl={3} - sl={4} / {5} {6} {7} {8} {9} /",Bars[i1].Time,t1,t3,dl1,sl1,d1,d2,d3,d4,d5);
							if(posGuidSell==Guid.Empty && tDw){
	 						var result1 = Trade.OpenMarketPosition(Instrument.Id, 
					 										ExecutionRule.Sell, 0.1,
															Instrument.Ask, -1,
															Stops.InPips(sl1+20,100), null, null);
						    if (result1.IsSuccessful)  posGuidSell=result1.Position.Id;		}							
															
							
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(Bars[i5].Time, d5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i4].Time, d4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i3].Time, d3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i2].Time, d2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i1].Time, d1));
				}
								}
			if(frSD>0 && kl==1 && frSD<d1) { d1=frSD; i1=Bars.Range.To-5;}
			if(frSD>0 && kl==2) {   kl=1; d5=d4; d4=d3; d4=d3; d3=d2; d2=d1; d1=frSD;  
									i5=i4; i4=i3; i4=i3; i3=i2; i2=i1; i1=Bars.Range.To-5;
									C1=Bars[Bars.Range.To-1].Close;
									dl1=(int)(Math.Round(C1-d1,5)*100000);
					//Print("{0} - {1} {2} ",Bars[i1].Time,dl1,Math.Round(C1-d1,5));
									t1=(int)(Math.Round(Math.Abs(d2-d3),5)*100000); 		 // 100
									t2=(int)(Math.Round(1.618*Math.Abs(d2-d3),5)*100000); // 161.8
									t3=(int)(Math.Round(2.618*Math.Abs(d2-d3),5)*100000); // 261.8
									sl1=(int)(Math.Round(Math.Abs(C1-d3),5)*100000); 
										t1=t1-dl1;	t2=t2-dl1;	t3=t3-dl1;		
					//Print("{0} {1} {2} {3} - sl={4}",Bars[i1].Time,dl1,t1,t2,sl1);
				                
						if(d5>d3 && d1>d3 &&  C1>d1 &&
							Math.Abs((d2-d1)/(d3-d2))>0.5 
							/* Bars[Bars.Range.To-4].Close>Bars[Bars.Range.To-4].Open &&
							   Bars[Bars.Range.To-3].Close>Bars[Bars.Range.To-3].Open &&
							   Bars[Bars.Range.To-2].Close>Bars[Bars.Range.To-2].Open*/ ) {
					 	Print("BUY {0} t1(100)={1} t2(161)={2} dl1={3} - sl={4} / {5} {6} {7} {8} {9} /",Bars[i1].Time,t1,t3,dl1,sl1,d1,d2,d3,d4,d5);
									
							if(posGuidBuy==Guid.Empty && tUp){
	 						var result2 = Trade.OpenMarketPosition(Instrument.Id, 
					 										ExecutionRule.Buy, 0.1,
															Instrument.Bid, -1,
															Stops.InPips(sl1+20,100), null, null);
						    if (result2.IsSuccessful)  posGuidBuy=result2.Position.Id;		}							
								
								var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(Bars[i5].Time, d5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i4].Time, d4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i3].Time, d3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i2].Time, d2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[i1].Time, d1)); 
				}
								}
			
			
		}
        
    }
}