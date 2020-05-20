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
    [TradeSystem("TC_Revers4_v2")] //copy of "TC_Revers4"
    public class TC_Revers4 : TradeSystem
    {		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public Alligator _allInd;
		public Fractals _frInd;
		public double SL_Buy,SL_Sell;
		public double SL=0.002;
		public double TP=0.00005;
		//  Значение текущего бара
		double  BarH,BarL,BarC,BarO;
		// 1. Фрактал выше/ниже зубов Alligator
		double fr_all_Up;     // Цена последней свечи с фракталом - полностью выше Аллигатора
		double fr_all_Down;   // Цена последней свечи с фракталом - полностью ниже Аллигатора
		bool fr_all_Down_L, fr_all_Up_L;
		public bool frUp=false;
		public bool frDown=true;
		// Fractal
		int fRan=3;
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpH_Old = 0.0;   // Значение текущего верхнего Fractal
		double frUpL = 0.0;   // Значение Low - свечи с верхним фрактклом
		double frDownH = 0.0; // Значение High - свечи с нижним фракталом
		double frDownL = 0.0; // Значение текущего нижнего Fractal
		double frDownL_Old = 0.0; // Значение текущего нижнего Fractal
		public DateTime frDTime; 
		double aGuba, aZub, aChelust;   // Челюсть Синяя		
		double aGubaFr, aZubFr, aChelustFr;   // Челюсть Синяя
		double frBarH,frBarL,frBarC,frBarO;
		public DateTime DTime;
		bool frFirstU=false;
		bool frFirstD=false;
		// Линии показывают АКТИВНЫЕ точки
		private VerticalLine vlR,vlB,vlG,vlY;
		
        protected override void Init()
        {   // Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_frInd.Range=fRan;
		}        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {  
//=============================================================================================================						
			// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;
			// Значения Alligator около Цены
			aGuba =     _allInd.LipsSeries[Bars.Range.To-1];
            aZub =      _allInd.TeethSeries[Bars.Range.To-1];
			aChelust =  _allInd.JawsSeries[Bars.Range.To-1];
//=============================================================================================================			
			// Значения Бара у фрактала
			frBarH = Bars[Bars.Range.To-fRan].High;
			frBarL = Bars[Bars.Range.To-fRan].Low;
			frBarC = Bars[Bars.Range.To-fRan].Close;
			frDTime = Bars[Bars.Range.To-fRan].Time;
			// Значения Alligator около фрактала
			aGubaFr =     _allInd.LipsSeries[Bars.Range.To-fRan];
            aZubFr =      _allInd.TeethSeries[Bars.Range.To-fRan];
			aChelustFr =  _allInd.JawsSeries[Bars.Range.To-fRan];
//=============================================================================================================						
			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			if(_frInd.TopSeries[Bars.Range.To-5]>0) { frUp=true; } else { frUp=false; }
			if(_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=true; } else { frDown=false; }

// ===  SET BARS FRACTAL ==============================================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			  if (frUp) {   Print("Появился фрактал ВВЕРХ - {0}",Bars[Bars.Range.To-5].Time);
				          frUpH=frBarH; 
				          frUpL=frBarL;   
				  //Низ Бар-Фрактала выше Alligator 
 //==========================================================================================================================================
			  			  if(frUpL>aGubaFr && frUpL>aChelustFr && frUpL>aZubFr) 
				   		  	{ fr_all_Up=frUpH; fr_all_Up_L=true; 				
				 // Переносим цену-отк к текущему фракталу + ТР  стоплос переносим на SL от цены-отк 
								if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending)
				   				{   var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, fr_all_Up+TP, fr_all_Up+SL,null); 
				       				if (result.IsSuccessful) { posGuidSell = result.Position.Id;
 			           				Print("00 UpdatePending - !появился новый фр-рабочий - переносим цену Open={0} SL={1} TP={2} - {3}",fr_all_Up,SL_Sell,fr_all_Down-TP,DTime);}
				   				}
							       Print("01 Фрактал ВВЕРХ и выше Алигатора! - {0} ",Bars[Bars.Range.To-1].Time); 
							} else 
							   {
				 // Если же фр-касается аллигатора - отменяем пред отложеный ордер
								//	if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			       				//	{   var res = Trade.CancelPendingPosition(posGuidSell);
			     	   			//		if (res.IsSuccessful) { posGuidSell = Guid.Empty;
				       			//		Print("02 появился новый фр-рабочий ВВЕРХ и рабочий отменяем отложеный Sell CancelPending - {0}",DTime); }
				   				//	}
							   }
 //==========================================================================================================================================							   
							// Если есть активный ордер - установить новый стоп	
 							if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				   				{   if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			       							{   var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   							if (res.IsSuccessful) posGuidBuy = Guid.Empty; }
											if (frUpH_Old>frUpH) {
									var result2=Trade.UpdateMarketPosition(posGuidSell,frUpH+TP,null, null); 
				   				   if (result2.IsSuccessful) {  posGuidSell = result2.Position.Id;
									    Print("03 SELL - !UpdateActive StopLoss когда Появился новый фрактал Up - SL={0} - {1}",frUpH,DTime);}  }
			       				}
 //==========================================================================================================================================
							   frUpH_Old=frUpH;
			  			 }
			  
			  if (frDown)   {  // Print("Появился фрактал ВНИЗ - {0}",Bars[Bars.Range.To-5].Time);
				                frDownL=frBarL; 
				                frDownH=frBarH;  
				  				if(frDownH<aGubaFr && frDownH<aChelustFr && frDownH<aZubFr) 
				   					{ fr_all_Down=frDownL; fr_all_Down_L=true;
									//======= UPDATE BUY  =========================================================================	
									   if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
										{ var result = Trade.UpdatePendingPosition(posGuidBuy, 0.1, fr_all_Down-TP,fr_all_Down-SL, null); 
											if (result.IsSuccessful) { posGuidBuy = result.Position.Id;  
				   							Print("04 Sell UpdatePending - появился новый фр-рабочий - переносим цену и StopLoss - Open={0} - SL={1} - TP={2} ",fr_all_Down,SL_Buy,fr_all_Up+TP,DTime);
										    } else { Print("05 Sell UpdatePending - ERROR - - Open={0} - SL={1} - TP={2}  ",fr_all_Down,SL_Buy,fr_all_Up+TP,DTime); }
										}	
					 				   Print("06 Фрактал НИЖЕ Алигатора! - {0}",Bars[Bars.Range.To-1].Time); } else 
									{// Если же фр-касается аллигатора - отменяем пред отложеній ордер
										//if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			       						//	{   var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   					//		if (res.IsSuccessful) { posGuidBuy = Guid.Empty;
				       					//		Print("07 появился новый фр-рабочий ВНИЗ и касается АЛЛ отменяем отложеный Sell CancelPending - {0}",DTime); }
				   						//	}
									}
									 // Появился новый фрактал ВНИЗ и ОТКРЫТА позиция ВВЕРХУ Buy - переносим стоп - Работает!
				   					if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active ) 
				   						{  
										  if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			       							{   var res = Trade.CancelPendingPosition(posGuidSell);
			     	   							if (res.IsSuccessful) posGuidSell = Guid.Empty; }	
											
										  if(frDownL_Old!=frDownL) {
										  var result2=Trade.UpdateMarketPosition(posGuidBuy,frDownL-TP,null, null); 
				     					  if (result2.IsSuccessful) {  posGuidBuy = result2.Position.Id;
											  Print("08 BuyStop - !UpdatePending StopLoss когда Появился новый фрактал Down - SL={0} {1}",frDownL-TP,DTime);}  
			                            } }
										frDownL_Old=frDownL;
			  				}
	  
//=== КОРЕКЦИЯ ===========================================================================================================							 
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {	posGuidBuy=Guid.Empty; Print("09 - Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);  }
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				{ posGuidSell=Guid.Empty;  Print("10 -Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }		
				
//====  BUYLIMIT  START  =====================================================================================================
			if(frDown && fr_all_Down_L && posGuidBuy==Guid.Empty && !( posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active ) ) 
			{
				        // Открываем BuyLimit
				var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, fr_all_Down-TP, 0, Stops.InPips(300,null), null, null, null);
						 if (result.IsSuccessful) { posGuidBuy=result.Position.Id;  
						         Print("11 BuyLimit! OPEN - O={0} - SL={1} - TP={2} {3}",fr_all_Down,SL_Buy,fr_all_Up+TP,DTime);
													} else {
							     Print("12 BuyLimit! ERROR - O={0} - SL={1} - TP={2} {3}",fr_all_Down,SL_Buy,fr_all_Up+TP,DTime);  	 
					           }  
			}
//==   SELLIMIT START   ================================================================================						  
			if(frUp && fr_all_Up_L && posGuidSell==Guid.Empty && !(posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active ) )
			{ 
			    var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1, fr_all_Up+TP, 0, Stops.InPips(300,null), null, null, null);
				//Stops.InPips(200,null)
				if (result.IsSuccessful) { posGuidSell = result.Position.Id;
					   Print("13 SellLimit OPEN - O={0} - SL={1} - TP={2} {3}",fr_all_Up,SL_Sell,fr_all_Down-TP,DTime);
				}  else  {   
					  Print("14 SellLimit ERROR - O={0} - SL={1} - TP={2} {3}",fr_all_Up,SL_Sell,fr_all_Down-TP,DTime);     
					     }
			 }		
			

        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
    }
}