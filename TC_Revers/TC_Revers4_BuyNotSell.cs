
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
    [TradeSystem("TC_Revers4_BuyNotSell")] //copy of "TC_Revers4"
    public class TC_Revers4 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
        [Parameter("TP", DefaultValue = 0.02, MinValue = 0,  Postfix = "TP")]
        public double TP_ { get; set; }
        [Parameter("SL", DefaultValue = 0.3, MinValue = 0,  Postfix = "SL")]
        public double SL_ { get; set; }
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		public Alligator _allInd;
		public Fractals _frInd;
		public double SL_Buy,SL_Sell;
		public double SL=0.003;
		public double TP=0.0003;
		// 1. Фрактал выше/ниже зубов Alligator
		double fr_all_Up;     // Цена последней свечи с фракталом - полностью выше Аллигатора
		double fr_all_Down;   // Цена последней свечи с фракталом - полностью ниже Аллигатора
		bool fr_all_Down_L, fr_all_Up_L;
		public bool frUp=false;
		public bool frDown=true;
		// Fractal
		double frUpH = 0.0;   // Значение текущего верхнего Fractal
		double frUpL = 0.0;   // Значение Low - свечи с верхним фрактклом
		double frDownH = 0.0; // Значение High - свечи с нижним фракталом
		double frDownL = 0.0; // Значение текущего нижнего Fractal
		
		double aGuba5, aZub5, aChelust5;   // Челюсть Синяя
		public DateTime DTime;
		bool frFirstU=false;
		bool frFirstD=false;
		bool TPB=true;
		bool TPS=true;
		
		// Линии показывают АКТИВНЫЕ точки
		private VerticalLine vlR,vlB,vlG,vlY;
		
        protected override void Init()
        {   
            SL=SL_/100;
			TP=TP_/100;
			// Event occurs once at the start of the strategy
            Print("Starting TS on account:{2} {3} {0}, comment: {1}", this.Account.Number, CommentText,TP,SL);
						// Вставить индикатор Alligator
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			// Вставить индикатор Fractals
		    _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			// начальные значения для фракталов не касающиеся алигатора
			fr_all_Up=Bars[Bars.Range.To-1].High;
			fr_all_Down=Bars[Bars.Range.To-1].Low;
			
			vlR = Tools.Create<VerticalLine>(); vlR.Color=Color.Red;
			vlB = Tools.Create<VerticalLine>(); vlB.Color=Color.Blue;
			vlY = Tools.Create<VerticalLine>(); vlY.Color=Color.Yellow;
			vlG = Tools.Create<VerticalLine>(); vlG.Color=Color.DarkGreen;
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {   DTime = Bars[Bars.Range.To-1].Time;
			SL=SL_/100;	TP=TP_/100;
			//Print("TP_={0} SL_={1}",TP_,SL_);
            // Event occurs on every new bar
						// Значения Alligator около фрактала
			aGuba5 = _allInd.LipsSeries[Bars.Range.To-5];
            aZub5 = _allInd.TeethSeries[Bars.Range.To-5];
			aChelust5 =  _allInd.JawsSeries[Bars.Range.To-5];
			
			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			if(_frInd.TopSeries[Bars.Range.To-5]>0) { frUp=true; } else { frUp=false; }
			if(_frInd.BottomSeries[Bars.Range.To-5]>0) { frDown=true; } else { frDown=false; }

// ===  SET BARS FRACTAL ==============================================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			  if (frUp) {   // Print("Появился фрактал ВВЕРХ - {0}",Bars[Bars.Range.To-5].Time);
				          frUpH=Bars[Bars.Range.To-5].High; 
				          frUpL=Bars[Bars.Range.To-5].Low;   
			  			  if(frUpL>aGuba5 && frUpL>aChelust5 && frUpL>aZub5) //Низ Бар-Фрактала выше Alligator 
				   		  	{ fr_all_Up=frUpH; fr_all_Up_L=true; 
								//======= UPDATE SELL  ====================================================================
								if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending)
				   				{   var result = Trade.UpdatePendingPosition(posGuidSell, 0.1, fr_all_Up+TP, fr_all_Up+SL,null); 
				       				if (result.IsSuccessful) { posGuidSell = result.Position.Id;
 			           				Print("00 UpdatePending - !появился новый фр-рабочий - переносим цену Open={0} SL={1} TP={2} - {3}",fr_all_Up,SL_Sell,fr_all_Down-TP,DTime);}
				   				}
							       Print("01 Фрактал ВВЕРХ и выше Алигатора! - {0} ",Bars[Bars.Range.To-1].Time); } else 
							    {// Если же фр-касается аллигатора - отменяем пред отложеній ордер
									if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			       					{   var res = Trade.CancelPendingPosition(posGuidSell);
			     	   					if (res.IsSuccessful) { posGuidSell = Guid.Empty;
				       					Print("02 появился новый фр-рабочий ВВЕРХ и рабочий отменяем отложеный Sell CancelPending - {0}",DTime); }
				   					}
							   }
							// Если есть активный ордер - установить новый стоп	
 							if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
				   				{  var result2=Trade.UpdateMarketPosition(posGuidSell,frUpH,null, null); 
				   				   if (result2.IsSuccessful) {  posGuidSell = result2.Position.Id;
									    Print("03 SELL - !UpdateActive StopLoss когда Появился новый фрактал Up - SL={0} - {1}",frUpH,DTime);}  
			       				}
							   
			  			 }
			  
			  if (frDown)   {  // Print("Появился фрактал ВНИЗ - {0}",Bars[Bars.Range.To-5].Time);
				                frDownL=Bars[Bars.Range.To-5].Low; 
				                frDownH=Bars[Bars.Range.To-5].High;  
			  					if(frDownH<aGuba5 && frDownH<aChelust5 && frDownH<aZub5) 
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
										if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			       							{   var res = Trade.CancelPendingPosition(posGuidBuy);
			     	   							if (res.IsSuccessful) { posGuidBuy = Guid.Empty;
				       							Print("07 появился новый фр-рабочий ВНИЗ и касается АЛЛ отменяем отложеный Sell CancelPending - {0}",DTime); }
				   							}
									}
									 // Появился новый фрактал ВНИЗ и ОТКРЫТА позиция ВВЕРХУ Buy - переносим стоп - Работает!
				   					if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
				   						{ var result2=Trade.UpdateMarketPosition(posGuidBuy,frDownL,null, null); 
				     					  if (result2.IsSuccessful) {  posGuidBuy = result2.Position.Id;
											  Print("08 BuyStop - !UpdatePending StopLoss когда Появился новый фрактал Down - SL={0} {1}",frDownL-TP,DTime);}  
			                            }	
			  				}
			  
			  
			  
			  if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State!=PositionState.Closed) { frFirstU=true;
			  vlR.Time=Bars[Bars.Range.To-1000].Time;
			  }
			  if (frUp && frFirstU && posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) { 
				  frFirstU=false; vlR.Time=Bars[Bars.Range.To-1].Time;
			  }
			  
			  if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State!=PositionState.Closed) { frFirstD=true;
			  vlB.Time=Bars[Bars.Range.To-1000].Time;
			  }
			  if (frDown && frFirstD && posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) { 
				  frFirstD=false; vlB.Time=Bars[Bars.Range.To-1].Time;
			  }
			  
			  
//=== КОРЕКЦИЯ ===========================================================================================================							 
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 
			     {	var pos = Trade.GetPosition(posGuidBuy); if (pos.Profit<0) { TPB=false; TPS=true; }
					 posGuidBuy=Guid.Empty; Print("09 - Buy - Закрыто по StopLoss (Корекция) - {0} ",DTime);  }
				 
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 
				{   var pos = Trade.GetPosition(posGuidSell); if (pos.Profit<0) { TPB=true; TPS=false; }
					posGuidSell=Guid.Empty;  Print("10 -Sell - Закрыто по StopLoss (Корекция) - {0} ",DTime); }		
				
//====  BUYLIMIT  START  =====================================================================================================
			if(frDown && fr_all_Down_L && posGuidBuy==Guid.Empty) 
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
			if(frUp && fr_all_Up_L && posGuidSell==Guid.Empty)
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
				  private void Red() 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=Bars[Bars.Range.To-1].Time;
			   vl.Color=Color.Red;
			}
							  private void Blue() 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=Bars[Bars.Range.To-1].Time;
			   vl.Color=Color.Blue;
			}
    }
}