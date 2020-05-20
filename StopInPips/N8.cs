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
    [TradeSystem("NKZ1")]
    public class NKZ1 : TradeSystem
    {
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		public bool fUp=true, sigU=false, sigD=false;
		public double BarH,BarL,BarC;
		// =========== Вход ==============
		[Parameter("NKZ:", DefaultValue = 1680)]
        public int nkz1 { get; set; }	
		
		[Parameter("TendUp:", DefaultValue = false)]
        public bool trendU { get; set; }	
		[Parameter("TendDown:", DefaultValue = false)]
        public bool trendD { get; set; }			

		//public bool trendU=false; 
		//public bool trendD=true; 

		public double nkz=0.0168;
		// 16.02.2018 - 
		public double Hnkz=0;
		public double Lnkz=1000;
		public double nkz4,nkz2;
		
		public Fractals _frInd;
		public double frU1,frU2,frU3;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3;    // Значение Low - свечи с верхним фрактклом
		public DateTime tmU1,tmU2,tmD1,tmD2;
		public double frSU=0,frSD=0,dlU,dlD;
		public int SL,TP;
		public VerticalLine vr,vb,vy,vw;
		public HorizontalLine L1,L2,L3;

        protected override void Init()
        {
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			//nkz=Math.Round((double)nkz1/100000,5);
			vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; 
			vb=Tools.Create<VerticalLine>(); vb.Color=Color.Blue; 
			L1 = Tools.Create<HorizontalLine>();
			
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
		// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			
			
			frSU=_frInd.TopSeries[Bars.Range.To-5];
			frSD=_frInd.BottomSeries[Bars.Range.To-5];

			if(trendD && frSD<Lnkz) { Lnkz=frSD; nkz4=Lnkz+(nkz/4); nkz2=Lnkz+(nkz/2); sigD=false; vr.Time=Bars[Bars.Range.To-5].Time; L1.Price = nkz4;}
			if(trendU && frSU>Hnkz) { Hnkz=frSU; nkz4=Hnkz-(nkz/4); nkz2=Hnkz-(nkz/2); sigU=false; vb.Time=Bars[Bars.Range.To-5].Time; L1.Price = nkz4;}
				
			if(trendD && BarH>nkz4) sigD=true;
			if(trendU && BarL<nkz4) sigU=true;
			
			
			
// =========================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-5].Time;
			
			if (frSU>0 && fUp)     { fUp=false; frU3=frU2; frU2=frU1; frU1=Bars[Bars.Range.To-5].High; }
			if (frSU>0 && !fUp && Bars[Bars.Range.To-5].High>frU1) { frU1=Bars[Bars.Range.To-5].High; }
			if (frSD>0 && !fUp)    { fUp=true; frD3=frD2; frD2=frD1; frD1=Bars[Bars.Range.To-5].Low;  }
			if (frSD>0 && fUp && Bars[Bars.Range.To-5].Low<frD1)   { frD1=Bars[Bars.Range.To-5].Low; }
// =========================================================================================================	
			if (frSD>0)  dlD=(frU1-frD1)/(frU1-frD2);
			if (frSU>0)  dlU=(frU1-frD1)/(frU2-frD1);
			
			if(sigD && trendD && frSU>0 && frD2>frD3 && frD2>frD1 && frU2>frU1 && dlU<0.8 && dlU>0.4 ) { 
				SL=(int)Math.Round((frU2-Bars[Bars.Range.To-1].Close)*100000,0);
				
			if (posGuidSell!=Guid.Empty) { 	
			var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(SL,100), null, null);
								if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;	
			}				
			}
			if(sigU && trendU && frSD>0 && frU3>frU2 && frU1>frU2 && frD1>frD2 && dlD<0.8 && dlD>0.4 ) { 
				SL=(int)Math.Round((Bars[Bars.Range.To-1].Close-frD2)*100000,0);
				
			if (posGuidBuy!=Guid.Empty) { var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(SL,100), null, null);
								if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id; }
			}
        }
       	protected void Line() 
       	{
			var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-5].Time;
		}
    }
}