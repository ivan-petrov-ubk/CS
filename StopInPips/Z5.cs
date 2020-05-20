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
    [TradeSystem("Z5")]     //copy of "ZZ_5Volna"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 12)]
        public int ED { get; set; }		
		
		private ZigZag _wprInd;
		public AwesomeOscillator _awoInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,V=0;
		
		
        protected override void Init()
        {
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
			
			//if(posGuidBuy!=Guid.Empty && posGuidBuy)
			
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{  
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
				 
				if(zz3<zz2 && zz2>zz1)  
				{ // 1 - ВВЕРХУ
					zzd6=zzd5;zzd5=zzd4;zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu6=zzu5;zzu5=zzu4;zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
			    	Print("ВВЕРХ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);		 

//			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
//			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}						
					
// Патерн 1-2-3  zzu1-пик вверху
// 				
if(zzu3>zzu1 && zzu5>zzu3 && zzu4>zzu2) 					
					{
							//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzd3].Time; vr.Width=4;

		var toolPolyLine = Tools.Create<PolyLine>();
			toolPolyLine.Color=Color.Blue;
			toolPolyLine.Width=4;				
        					//toolPolyLine.AddPoint(new ChartPoint(Bars[zzd6].Time, Bars[zzd6].Low));
   							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To].Time, Bars[Bars.Range.To].Low));
						
var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,150), null, null);									   
//var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPrice(Bars[zzd1].High,null), null, null);		
						 if (result2.IsSuccessful) posGuidBuy=result2.Position.Id; //Stops.InPips(200,100) Stops.InPrice(Bars[zzd1].High,Bars[Bars.Range.To-1].Low-0.001)
						
						}
				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd6=zzd5;zzd5=zzd4;zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu6=zzu5;zzu5=zzu4;zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					Print("НИЗ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);			

//			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
//			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}					
					
//if(zzu5>zzu3 && zzu1>zzu3 && zzu6>zzu4)
// Патерн 1-2-3  zzu1-пик ВНИЗУ
// 				
if(zzu2>zzu4 && zzu1>zzu3 && zzu3>zzu5) 						
					{
						//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzd3].Time; vr.Width=4;
			

var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(200,150), null, null);					
//var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPrice(Bars[zzd1].Low,null), null, null);		
						if (result1.IsSuccessful) posGuidSell=result1.Position.Id; //Stops.InPips(200,100)  Stops.InPrice(Bars[zzd1].Low,Bars[Bars.Range.To-1].High+0.001),
						 
								var toolPolyLine1 = Tools.Create<PolyLine>();
			toolPolyLine1.Color=Color.Black;
			toolPolyLine1.Width=4;				
        					//toolPolyLine.AddPoint(new ChartPoint(Bars[zzd6].Time, Bars[zzd6].Low));
   							toolPolyLine1.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].Low));
							toolPolyLine1.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].High));
							toolPolyLine1.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].Low));
							toolPolyLine1.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].High));
							toolPolyLine1.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].Low));
							toolPolyLine1.AddPoint(new ChartPoint(Bars[Bars.Range.To].Time, Bars[Bars.Range.To].High));
						}
				}
			}
        }
//===============================================================================================================================        
    }
}