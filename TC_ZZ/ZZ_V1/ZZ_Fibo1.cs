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
    [TradeSystem("ZZ_Fibo1")]  //copy of "ZZ_Ex3"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }		
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2,cn;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2,zzu7=2,zzu8=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7,zzd8;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,zzi7,zzi8,V=0,i=0;
		public VerticalLine vy,vw,vg;
		
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			vy=Tools.Create<VerticalLine>(); vy.Color=Color.Yellow;
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
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    vy.Time=Bars[Bars.Range.To-1].Time;
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
				 
				if(zz3<zz2 && zz2>zz1)  
				{ // ПИК ВВЕРХУ 
					i++;
					zzd8=zzd7; zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu8=zzu7; zzu7=zzu6; zzu6=zzu5; zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
			    	Print("ВВЕРХ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);		 
					if(i>8 && zzu7<zzu5 && zzu5<zzu3 && zzu1<zzu3) // ВВЕРХУ
						{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzd1].Time;vr.Width=3;
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}	
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}		
			
						if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}
						 if(posGuidBuy==Guid.Empty){
							 cn=Math.Round(Bars[zzd3].Low-((Bars[zzd3].High-Bars[zzd2].Low)*1.618),5);
							    var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, cn, 0, Stops.InPips(200,100), null, null, null);
						        if (result1.IsSuccessful)  { posGuidSell=result1.Position.Id; zzts2=zzu2; } 
						       } 
						 }
				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{ // ЯМА  ВНИЗУ
					i++;
					zzd8=zzd7; zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu8=zzu7;zzu7=zzu6;zzu6=zzu5;zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					Print("НИЗ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);			
					if(i>8 &&  zzu7>zzu5 && zzu5>zzu3 && zzu1>zzu3) //  ВНИЗУ
						{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzd1].Time; vr.Width=3;
							
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}	
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}		
			
							if(posGuidSell==Guid.Empty){ 
							 cn=Math.Round(zzu3+((zzu2-zzu3)*1.618),5);
						 var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1, cn , 0, Stops.InPips(200,100), null, null, null);	
						     if (result.IsSuccessful) { posGuidBuy=result.Position.Id; zztb2=zzu2;}
							} 
						}
				}
				
			/*	if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending && Bars[Bars.Range.To-1].Low<zztb2) 
				{    zztb2=-1000;
					var res = Trade.CancelPendingPosition(posGuidSell);
			     	   			if (res.IsSuccessful)  posGuidSell = Guid.Empty; 
				}
				if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending && Bars[Bars.Range.To-1].High>zzts2) 
				{  zzts2=1000;
					var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   			if (res.IsSuccessful)  posGuidBuy = Guid.Empty; 
				}*/
				

				
			}
        }
//===============================================================================================================================        
    }
}