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
// http://system-fx.ru/torgovaya-strategiya-zz-ili-zigzag-udachi/
// Торговать будем по следующему алгоритму:
// 1. Смотрим на график и если индикатор «ZigZag» в данный момент идет вниз, ждем, когда сформируется линия идущая вверх. 
//	  Как только это произошло, устанавливаем отложенный ордер на продажу (Sell Stop) немного ниже локального минимума (нижний угол «ZigZag’a»).
// 	  Если же индикатор шел вверх, переломился и пошел вниз, мы будем ставить отложенный ордер на покупку (Buy Stop). 
//    Размещаем его немного выше локального максимума (верхний угол «ZigZag’a»).
// 2. Если рынок пошел в сторону одного из наших отложенных ордеров, но цена так и не дошла до них и развернулась, 
//     с соответсвующим разворотом «ZigZag’a», то необходимо передвинуть этот ордер поближе.
// 3. Как только позиция будет открыта, устанавливаем стоп-лосс. Его размещаем на расстоянии от точки входа на рынок, указанном ниже.
//    Как только будет образован противоположный экстремум «ZigZag’a», стоп-лосс необходимо переместить точно на его место, не учитывая спрэд.
// 4. Тейк-профит ставим на расстоянии также указанном ниже. А можно его и не ставить, тогда позиции закрываются по стоп-лоссам.
//    Оптимальные значения отложенных ордеров следующие:
// — Таймфрейм M5. Ордер Sell Stop: тейк-профит: 250-400 пипсов, стоп-лосс: 5-10 пипсов, отступ: 20-25 пипсов. Ордер Buy Stop: тейк-профит: 200-250 пипсов, стоп-лосс: 5-10 пипсов, отступ: 5-10 пипсов.
// — Таймфрейм M15. Ордер Sell Stop: тейк-профит: 250-300 пипсов, стоп-лосс: 5-10 пипсов, отступ: 12-18 пипсов. Ордер Buy Stop: тейк-профит: 80-150 пипсов, стоп-лосс: 5-10 пипсов, отступ: 5-10 пипсов.
// — Таймфрейм H1. Ордер Sell Stop: тейк-профит: 150-180 пипсов, стоп-лосс: 10-15 пипсов, отступ: 5-7 пипсов. Ордер Buy Stop: тейк-профит: 250-400 пипсов, стоп-лосс: 10-15 пипсов, отступ: 10-15 пипсов.77


namespace IPro.TradeSystems
{
    [TradeSystem("Z")]       //copy of "Z"
    public class ZZ_Ex1 : TradeSystem
    {
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2,korU,korD;
		private double zztb2,zzts2,zzb,zzs;
		private double z50_1,z50_2,z50_3,stML;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		private VerticalLine vy,vb;
		private int ibuy,isell;
		private bool torg=true,torgU=true,torgD=true;
		private double sv=0,av1=0,av2=0,av3=0,av4=0,av5=0;
		
        protected override void Init()
        {
			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=15;
			_wprInd.ExtDeviation=0;
			_wprInd.ExtBackStep=3;
			
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Yellow;
			vb = Tools.Create<VerticalLine>();
			vb.Color=Color.Blue;

				korU=0.00020;
			korD=0.00020;
			
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
		  { z50_3=0;z50_2=0;z50_1=0; torg=false;
		    zzu5=0;zzu4=0;zzu3=0;zzu2=0;zzu1=0; 
			zzd5=0;zzd4=0;zzd3=0;zzd2=0;zzd1=0;
			  if (posGuidBuy!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty; }
			if (posGuidSell!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  } 
		  if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Monday && Bars[Bars.Range.To-1].Time.Hour==01 )  torg=true;
		
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    if(zzd3!=0 && zzd2!=0 && zzd1!=0) torg=true; else torg=false;
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;

//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
			    	vy.Time=Bars[Bars.Range.To-1].Time;
					 // ВВЕРХУ
							
							if(zzu3>zzu1 && torg ) 
							{
								if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending)
									{ var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   				  if (res.IsSuccessful) posGuidBuy = Guid.Empty; }
								
								if(posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending)
				   				   { var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, Bars[zzd2].Low-korD-Instrument.Spread, Bars[zzd2].Low-korD-Instrument.Spread+0.00025,Bars[zzd2].Low-korD-Instrument.Spread-0.0025); 
				       				 if (result.IsSuccessful) posGuidSell = result.Position.Id; }	
									
								if(posGuidSell==Guid.Empty)
								   {var result3 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellStop, 0.1,  Bars[zzd2].Low-korD-Instrument.Spread, 0, Stops.InPips(15,250), null, null, null);								
									if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id; }
						    } 
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					vb.Time=Bars[Bars.Range.To-1].Time;
							if(zzu3<zzu1 && torg) 
							{
								if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending)
									{ var res = Trade.CancelPendingPosition(posGuidSell);
			     	   				  if (res.IsSuccessful) posGuidSell = Guid.Empty; }
									
								if(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending)
				   				   { var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, Bars[zzd2].High+korD+Instrument.Spread, Bars[zzd2].High+korD+Instrument.Spread-0.00025,Bars[zzd2].High+korD+0.0025); 
				       				 if (result.IsSuccessful) posGuidBuy = result.Position.Id; }	

									
								if(posGuidBuy==Guid.Empty)
								    { var result2 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyStop, 0.1,  Bars[zzd2].High+korU+Instrument.Spread, 0, Stops.InPips(15,250), null, null, null);
									  if (result2.IsSuccessful)  posGuidSell=result2.Position.Id; }
							} 
				}
	
			}
        }
//===============================================================================================================================        
    }
}