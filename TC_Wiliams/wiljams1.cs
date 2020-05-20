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
    [TradeSystem("Alligator1")]
    public class Alligator1 : TradeSystem
    {
        // Simple parameter example
		public Alligator _allInd;
		public AwesomeOscillator _awoInd;
		public AcceleratorOscillator _aoInd;
		public Fractals _frInd;
		// Alligator
		double alGuba;  //  Губы
		double alZub;    // Зубы
		double alChelust;   // Челюсть
				// Alligator для Fractal
		double alGuba5;  //  Губы
		double alZub5;    // Зубы
		double alChelust5;   // Челюсть
		// Fractal
		double frUp = 0.0;
		double frDown = 0.0;
		public DateTime frUp_Time;
		public DateTime frDown_Time;
		public DateTime fr_all_Up_Time;
		public DateTime fr_all_Down_Time;		
		
		// AO
		double aoUp, aoUp1, aoUp2 ;
		double aoDown, aoDown1, aoDown2;
		//AC
		double acUp;
		double acDown;
	//  Сигналы! ==================================================
		// 1. Фрактал выше/ниже зубов алигатора
		double fr_all_Up;
		double fr_all_Down;
			
	    double Sr = 1;
        
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
            //_aoInd =  GetIndicator<AcceleratorOscillator>(Instrument.Id, Timeframe);
		    
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
        
			// Print("Alligator челюсть синяя: {0}", _allInd.JawsSeries[Bars.Range.To-1]);
			//Print("Alligator губы зеленая: {0}", _allInd.LipsSeries[Bars.Range.To-1]);
		    //Print("Alligator зубы красная: {0}", _allInd.TeethSeries[Bars.Range.To-1]);
			alGuba = _allInd.LipsSeries[Bars.Range.To-1];
            alZub = _allInd.TeethSeries[Bars.Range.To-1];
			alChelust =  _allInd.JawsSeries[Bars.Range.To-1];
			
			alGuba5 = _allInd.LipsSeries[Bars.Range.To-8];
            alZub5 = _allInd.TeethSeries[Bars.Range.To-10];
			alChelust5 =  _allInd.JawsSeries[Bars.Range.To-13];
			
			// Данные индиктора Fractal
			  if (_frInd.TopSeries[Bars.Range.To-5]>0) {frUp =Bars[Bars.Range.To-5].Close; frUp_Time = Bars[Bars.Range.To-5].Time;}
			  if (_frInd.BottomSeries[Bars.Range.To-5]>0) {frDown = Bars[Bars.Range.To-5].Close; frDown_Time = Bars[Bars.Range.To-5].Time;}
			// Print("Fractals Вверх: {0} ", frUp);
			// Print("Fractals Вниз: {0} ", frDown);
			  
			
			// Данные индиктора Awesome oscillator (AO) Зеленая-UP красная-Down (-1 до +1)
			// Print("Awesome oscillator down series value: {0} -- {1}", _awoInd.SeriesDown[Bars.Range.To-2],Bars[Bars.Range.To-2].Time);
			// Print("Awesome oscillator up series value: {0} -- {1}", _awoInd.SeriesUp[Bars.Range.To-2],Bars[Bars.Range.To-2].Time);
            // aoUp =    _awoInd.SeriesUp[Bars.Range.To-2]; 
			aoDown =  _awoInd.SeriesDown[Bars.Range.To-2];
			
			 
			// Данные индиктора Accelerator Oscillator (AC) Зеленая-UP красная-Down (-1 до +1)
			//Print("Accelerator oscillator down series value: {0}", _aoInd.SeriesDown[Bars.Range.To - 1]);
			//Print("Accelerator oscillator up series value: {0}", _aoInd.SeriesUp[Bars.Range.To - 1]);
			 // acUp   = _aoInd.SeriesUp[Bars.Range.To - 1];
			 // acDown = _aoInd.SeriesDown[Bars.Range.To - 1];
			  
			  // Сигралы ! ===============================================================================
			  // 1. Фрактал выше/ниже зубов алигатора : 
			  if (frUp>alZub5 && fr_all_Up_Time!=frUp_Time)   { fr_all_Up=frUp; fr_all_Up_Time=frUp_Time;}
			    //   var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=fr_all_Up_Time;
				//   Print("fr_all_Up={0}, fr_all_Up_Time={1} frUp>alZub5={2}>{3}",fr_all_Up,fr_all_Up_Time,frUp,alZub5); 
			    //} else fr_all_Up=-1;
			  
			  if (frDown<alZub5 && frDown>0 && fr_all_Down_Time!=frDown_Time) { fr_all_Down=frDown;  fr_all_Down_Time=frDown_Time;}
				    // var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Blue; vline.Time=fr_all_Down_Time;
				    // var hline = Tools.Create<HorizontalLine>(); hline.Price = frDown;
				    // Print("fr_all_Down={0}, fr_all_Down_Time={1} frDown<alZub5={2}<{3}",fr_all_Down,fr_all_Down_Time,frDown,alZub5);  
				   //} else fr_all_Down=-1;
			   
			  // 2. Сиграл на покупку АО - БЛЮДЦЕ!
			  aoUp  =    _awoInd.SeriesUp[Bars.Range.To-2];
			  aoDown  =    _awoInd.SeriesDown[Bars.Range.To-2];
			  aoDown1 = _awoInd.SeriesDown[Bars.Range.To-3]; 
			  aoUp1 = _awoInd.SeriesUp[Bars.Range.To-3]; 
			  aoDown2 = _awoInd.SeriesDown[Bars.Range.To-4];
			  aoUp2 = _awoInd.SeriesUp[Bars.Range.To-4];
			  // Print("aoUp2>aoDown1 {0}>{1} | aoDown>aoDown1 {2}>{3} | aoDown>0 {4}>0 | aoDown1>0 {5]>0 | aoUp2>0 {6}>0",aoUp2,aoDown1,aoDown,aoDown1,aoDown,aoDown1,aoUp2);
			    // Print("{0} {1} {2}",aoUp,aoDown1,aoDown2); 
			  if (aoUp>0 && aoDown1>0 && aoDown2>0) {  
				  var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Red; vline.Time=Bars[Bars.Range.To-4].Time;
				  Print("{0} ---  {1} | {2} | {3}",Bars[Bars.Range.To-4].Time,aoUp,aoDown1,aoDown2); 
			                }
			   if (aoDown<0 && aoUp1>0 && aoUp2>0) {  
				  var vline = Tools.Create<VerticalLine>(); vline.Color=Color.Blue; vline.Time=Bars[Bars.Range.To-4].Time;
				  Print("{0} ---  {1} | {2} | {3}",Bars[Bars.Range.To-4].Time,aoDown,aoUp1,aoUp2); 
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
		
    }
}