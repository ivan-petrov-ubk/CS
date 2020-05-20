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
    [TradeSystem("ZigZag6_Test")]    //copy of "ZigZag5"
    public class ZigZag2 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "TC ZigZag")]
        public string CommentText { get; set; }
		[Parameter("TakeProfit=", DefaultValue = 100)]
        public int TP { get; set; }
		[Parameter("StopLoss=", DefaultValue = 200)]
        public int SL { get; set; }
		[Parameter("Spred=", DefaultValue = 0)]
        public int SP { get; set; }	
		[Parameter("ExtDepth=", DefaultValue = 12)]
        public int ED { get; set; }		
		
		private ZigZag _wprInd;
		private double zz1,zz2,zz3,zzU,zzD;
		private int zzd1,zzd2,zzd3;
		private int zzi1,zzi2,zzi3;
		private bool up;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double kor,cnU,cnD,x;
		public Fractals _frInd;
		public VerticalLine vr,vb,vy,vw;
		
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			if(Instrument.Name.EndsWith("JPY")) kor=0.001*SP; else kor=0.00001*SP;		
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
		
			
			vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; 
			vb=Tools.Create<VerticalLine>(); vb.Color=Color.Blue; 
			vy=Tools.Create<VerticalLine>(); vy.Color=Color.Yellow;
			vw=Tools.Create<VerticalLine>(); vw.Color=Color.White; 
			
			if(Instrument.Name.EndsWith("JPY")) x=0.3; else x=0.003;		
        }        
        
        protected override void NewBar()
        {
			_wprInd.ReInit();
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-2]>0) 
			{ // Всі точки перегину зиззага
				
				 zz3=zz2;	 zz2=zz1;    zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-2];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1=Bars.Range.To-2;
				 Print("{0} {1} {2} -- {3}",zz3,zz2,zz1, Bars[Bars.Range.To-2].Time);
			if(zz3>zz2 && zz2<zz1)  { zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;}
			if(zz3<zz2 && zz2>zz1)  { zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;}
			
			
			if(Bars[zzd2].Low>Bars[zzi1].Low &&  Bars[zzd2].High<Bars[zzd1].High && Bars[zzi1].High<Bars[zzd1].High) 
			  { Print("Ввверху Максимум"); 
				vr.Time = Bars[zzd2].Time;
				vb.Time = Bars[zzd1].Time;
				vy.Time = Bars[zzi1].Time;
				if(Bars[zzd1].Low!=zzU) { zzU=Bars[zzd1].Low;	
					if(Bars[zzd1].Close>Bars[zzd1].Open) cnU=(Bars[zzd1].High-Bars[zzd1].Close)/2; else cnU=(Bars[zzd1].High-Bars[zzd1].Open)/2;
					if(x<(Bars[zzd1].High-Bars[zzd2].Low))   Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  Bars[zzd1].High-cnU-Instrument.Spread-kor, 0, Stops.InPips(SL,TP), null, null, null);
				}
													
			  }
					
					
			if(Bars[zzd2].High<Bars[zzi1].High &&  Bars[zzd1].Low<Bars[zzd2].Low && Bars[zzd1].Low<Bars[zzi1].Low) 
				{  Print("Внизу Максимум");
					vr.Time = Bars[zzd2].Time;
			  		vb.Time = Bars[zzd1].Time;
					vy.Time = Bars[zzi1].Time;
					if(Bars[zzd1].Low!=zzD) { zzD=Bars[zzd1].Low;
					if(Bars[zzd1].Close>Bars[zzd1].Open) cnD=(Bars[zzd1].Close-Bars[zzd1].Low)/2; else cnD=(Bars[zzd1].Open-Bars[zzd1].Low)/2;
					if(x<(Bars[zzd2].High-Bars[zzd1].Low))   Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1,  Bars[zzd1].Low+cnD+Instrument.Spread-kor, 0, Stops.InPips(SL,TP), null, null, null);
					}

				}			
					
					
			}	
        }
        
    }
}