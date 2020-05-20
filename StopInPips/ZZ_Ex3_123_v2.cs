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
    [TradeSystem("ZZ_Ex3_123_v2")]    //copy of "ZZ_Ex3_123"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 15)]
        public int ED { get; set; }	
		[Parameter("SL:", DefaultValue = 200)]
        public int SL { get; set; }	
		[Parameter("TP:", DefaultValue = 200)]
        public int TP { get; set; }		
		
		private ZigZag _wprInd;
		private StochasticOscillator _stoInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		private VerticalLine vy;
		private bool torg=true;
		private double sv=0,av1=0,av2=0,av3=0,av4=0,av5=0;
		
        protected override void Init()
        {
			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Yellow;

			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  {  zzu5=0;zzu4=0;zzu3=0;zzu2=0;zzu1=0; zzd5=0;zzd4=0;zzd3=0;zzd2=0;zzd1=0;
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  }
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    if(zzd5!=0 && zzd4!=0 && zzd3!=0 && zzd2!=0 && zzd1!=0) torg=true; else torg=false;
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
			    	Print("ВВЕРХ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);	
					vy.Time=Bars[Bars.Range.To-111111].Time;
					sv=0;for (int i=zzd1; i>zzd2; i--) { sv=sv+Bars[i].TickCount; }
					av5=av4;av4=av3;av3=av2;av2=av1;av1=Math.Round(sv/(zzd1-zzd2),0);
					
							// Если есть активный ордер - установить новый стоп	
	if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active && Trade.GetPosition(posGuidSell).Pips>-200) 
		{  
			var result2=Trade.UpdateMarketPosition(posGuidSell,Bars[zzd1].High,null, null);
				if (result2.IsSuccessful) posGuidSell = result2.Position.Id; }   

		
		
	var toolText1 = Tools.Create<Text>();
        toolText1.Point=new ChartPoint(Bars[zzd1-1].Time, Bars[zzd1].High+0.001);
		toolText1.FontSize=10;					
        toolText1.Caption=string.Format("{0}",av1);	
					
					 if(zzu1>zzu2 && 
						zzu3>zzu2 && 
						zzu3>zzu1 &&
						zzu3>zzu4
						
						
						) // ВВЕРХУ
						{  
		var toolPolyLine = Tools.Create<PolyLine>();
			toolPolyLine.Color=Color.Red;
			toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].High));

							if(av1>av3) {
							if(torg && posGuidSell==Guid.Empty && posGuidBuy==Guid.Empty){
								var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(SL,TP), null, null);
								if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;
						       } } else {
							if(torg && posGuidBuy==Guid.Empty  && posGuidSell==Guid.Empty){
								var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(SL,TP), null, null);
								if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id;
							} }

						}
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					Print("НИЗ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);	
					vy.Time=Bars[Bars.Range.To-1].Time;
					
												sv=0;
							for (int i=zzd1; i>zzd2; i--) { sv=sv+Bars[i].TickCount; } 
							av5=av4;av4=av3;av3=av2;av2=av1;av1=Math.Round(sv/(zzd1-zzd2),0);
							
	var toolText2 = Tools.Create<Text>();
        toolText2.Point=new ChartPoint(Bars[zzd1-1].Time, Bars[zzd1-1].Low);
		toolText2.FontSize=10;					
         toolText2.Caption=string.Format("{0}",av1);

// Если есть активный ордер - установить новый стоп	
	if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
		{  var result2=Trade.UpdateMarketPosition(posGuidBuy,Bars[zzd1].Low,null, null); 
				if (result2.IsSuccessful) posGuidBuy = result2.Position.Id; }  
							
							
					if( zzu1>zzu3 && 
						zzu2>zzu1 && 
						zzu2>zzu3 && 
						zzu4>zzu3 &&
						zzu2>zzu4
						
					  )//  ВНИЗУ
						{
						 
		var toolPolyLine = Tools.Create<PolyLine>();
			toolPolyLine.Color=Color.Blue;
			toolPolyLine.Width=4;				
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].Low));
														
									if(av1>av3) {
							if(torg && posGuidBuy==Guid.Empty && posGuidSell==Guid.Empty){
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(SL,TP), null, null);
								if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;
							} } else {
							if(torg && posGuidSell==Guid.Empty && posGuidBuy==Guid.Empty){
								var result4 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(SL,TP), null, null);
								if (result4.IsSuccessful)  posGuidSell=result4.Position.Id;
						       } }
						}
				}
	
			}
        }
//===============================================================================================================================        
    }
}