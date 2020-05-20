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
    [TradeSystem("Fisher2")]
    public class Fisher2 : TradeSystem
    {
        // Simple parameter example
        public FisherTransformOscillator _ftoInd;
		public VerticalLine toolVerticalLine ;
        public int IsF0 = 1;  // Пересечение Fisher 0  -  1-ВВЕРХ(Red) -1-НИЗ(Blue)
		public int IsFMA1 = 1; //
		public int Tred1 = 1;
		public int Tred2 = 1;
		public int CountFMARed = -1;
		public int CountFMABlue = -1;
		public double OldF = -1;
		public double sF=0,sF1=0;
		bool isSell, isBuy;
		private Guid _positionGuid=Guid.Empty;
		
		protected override void Init()
        {
            // Event occurs once at the start of the strategy
           _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			
			sF = _ftoInd.FisherSeries[Bars.Range.To-1];
			sF1 = _ftoInd.FisherSeries[Bars.Range.To-2];
			// Стратегия №1 - Вход при пересечении фишером 0  а выход при пересечении МА1 и фишера
			//if ( sF>0  && IsF0<0 && isSell) ClosePosition();  
			if ( sF1<0  && sF>0 ) { isBuy=true; Red(); }  // Fisher  пересекает 0 Red
			//if ( sF<0  && IsF0>0 && isBuy) ClosePosition();  
			if ( sF1>0  && sF<0 ) { isSell=true; Blue();} // Fisher  пересекает 0 Blue
			//Print("{0} - {1} - {2}",sF,Bars[Bars.Range.To-1],Bars[Bars.Range.To-1].Time);
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1] && IsFMA1<0 ) {IsFMA1 = 1; ClosePosition();}  // Red
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1] && IsFMA1>0 ) {IsFMA1 = -1; ClosePosition();}	// Fisher и МА1 Blue
			// ==============================================================================

			
			// Стратегия №2 - Вход при пересечении фишером 0  а выход при пересечении МА1 и фишера
			// Добавил условие что МА1 должна лежать ниже направления входа
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0 && _ftoInd.Ma1Series[Bars.Range.To-1]<0  && IsF0<0 ) { IsF0 = 1; Buy();}  // Fisher  пересекает 0 Red
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0 && _ftoInd.Ma1Series[Bars.Range.To-1]>0  && IsF0>0 ) { IsF0 = -1; Sell();} // Fisher  пересекает 0 Blue
			
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1] && IsFMA1<0 ) {IsFMA1 = 1; ClosePosition();}  // Red
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1] && IsFMA1>0 ) {IsFMA1 = -1; ClosePosition();}	// Fisher и МА1 Blue
			// ==============================================================================
             
			// Стратегия №3  - Вход на 3 волне по фишеру
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0  && IsF0<0 ) { IsF0 = 1; CountFMARed=0;CountFMABlue=0;}  // Fisher Вверх пересекает 0 Red
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0  && IsF0>0 ) { IsF0 = -1; CountFMABlue=0;CountFMARed=0;} // Fisher Вниз пересекает 0 Blue
			
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1] && IsFMA1<0  ) {IsFMA1 = 1; CountFMARed=CountFMARed+1; }  // Red
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1] && IsFMA1>0  ) {IsFMA1 = -1; CountFMABlue=CountFMABlue+1; }	// Fisher и МА1 Blue
            
			//if ( IsF0>0 && CountFMABlue==1 && IsFMA1>0 && Tred1>0) { Red(); Tred1=-1; Buy();};
			//if ( IsF0>0 && CountFMABlue==2 && IsFMA1<0 && Tred1<0) { Blue(); Tred1=1; ClosePosition();}; 
			
			//if ( IsF0<0 && CountFMARed==1 && IsFMA1<0 && Tred2>0) { Red(); Tred2=-1; Sell();};
			//if ( IsF0<0 && CountFMARed==2 && IsFMA1>0 && Tred2<0) { Blue(); Tred2=1; ClosePosition();}; 
			// ================================================================================================================
			
			// Стратегия №4 - Вход при пересечении фишером 0 а выход после 5 баров 
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0  && IsF0<0 ) { IsF0 = 1;  Buy(); CountFMARed=Bars.Range.To-1; }  // Fisher  пересекает 0 Red
			//if ((CountFMARed+3)==Bars.Range.To-1) ClosePosition(); 
			//if (CountFMARed>0 && CountFMARed<6) { CountFMARed=CountFMARed+1; } else { ClosePosition(); CountFMARed=-1; }
			
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0  && IsF0>0 ) { IsF0 = -1; Sell(); CountFMABlue=Bars.Range.To-1; } // Fisher  пересекает 0 Blue
			//if ((CountFMABlue+3)==Bars.Range.To-1) ClosePosition(); 
			//if (CountFMABlue>0 && CountFMABlue<6) { CountFMABlue=CountFMABlue+1; } else { ClosePosition(); CountFMABlue=-1; }
			// ===================================================================================================================
			
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0  && IsF0<0 ) { IsF0 = 1;  Sell(); }   // Fisher  пересекает 0 Red
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0  && IsF0>0 ) { IsF0 = -1; Buy(); } // Fisher  пересекает 0 Blue
			
			//OldF = _ftoInd.FisherSeries[Bars.Range.To-1] - _ftoInd.FisherSeries[Bars.Range.To-2] ;
			//if ((IsF0>0 && OldF<0) || (IsF0<0 && OldF>0)) ClosePosition();
			
			// Print("Разница - {0} - {1} - {2} ", OldF,Bars.Range.To-1,Bars.Range.To-2);
        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
           
        }
		
		private void Red()
		{
		    var vline = Tools.Create<VerticalLine>();
            vline.Color=Color.Red;
		    vline.Time=Bars[Bars.Range.To-1].Time;	
		}
		
		private void Blue()
		{
		    var vline = Tools.Create<VerticalLine>();
            vline.Color=Color.Blue;
		    vline.Time=Bars[Bars.Range.To-1].Time;	
		}
		
		private void Buy()
		{
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Buy(Instrument.Id, 0.1); 		
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
		
		private void Sell()
		{
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Sell(Instrument.Id, 0.1); 
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
		
		
		private void ClosePosition()
		{
			var result =Trade.CloseMarketPosition(_positionGuid);
			if (result.IsSuccessful) 
			{
                Print("LR_Position_closed_by_inverse_signal", result.Position.Number, result.Position.ClosePrice);	
				_positionGuid=Guid.Empty;
				
			}
		}
   
	
		}
	
}