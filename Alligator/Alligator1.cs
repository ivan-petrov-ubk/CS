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
		public Fractals _frInd;
		double lGuba;  //  Губы
		double lZub;    // Зубы
		double lChelust;   // Челюсть
	    double Sr = 1;
        
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
			_allInd = GetIndicator<Alligator>(Instrument.Id, Timeframe);
			 _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
           
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			         // Print("Бар № {0} - {1} ==================================================================",Bars.Range.To, Bars[Bars.Range.To-1].Close);
			          // Print("Alligator челюсть синяя: {0}", _allInd.JawsSeries[Bars.Range.To-1]);
			           //Print("Alligator губы зеленая: {0}", _allInd.LipsSeries[Bars.Range.To-1]);
		            	//Print("Alligator зубы красная: {0}", _allInd.LipsSeries[Bars.Range.To-1]);
			Print("Низ={0}  - Верх={1} ",_frInd.BottomSeries[Bars.Range.To-5], _frInd.TopSeries[Bars.Range.To-5]);
			if ( _frInd.TopSeries[Bars.Range.To-5]>0 ) Print("YES!! - {0}",_frInd.TopSeries[Bars.Range.To-5]); else   Print("NO!! - {0}",_frInd.TopSeries[Bars.Range.To-5]);         
			//Print("Fractals range value: {0}", _frInd. );
			           
			          
		//				   if (_frInd.BottomSeries[Bars.Range.To-5] >0){
		//					   Print("Fractals bottom series value: {0} ", Sr );
		//				   var vline = Tools.Create<VerticalLine>();
       ///     vline.Color=Color.Blue;/
			/// vline.Time=Bars[Bars.Range.To-5].Time;	
			//			   }
			//			   if (_frInd.TopSeries[Bars.Range.To-5] >0){
		//					   Print("Fractals bottom series value: {0} ", Sr );
		//				   var vline = Tools.Create<VerticalLine>();
       //     vline.Color=Color.Red;
		//    vline.Time=Bars[Bars.Range.To-5].Time;	
		//				   }
			//Sr = _frInd.TopSeries[];
			           //Print("Fractals: {0} ", _frInd.BottomSeries[Bars.Range.To-5] );
		                //Print("-");
			//lGuba = _allInd.LipsSeries[Bars.Range.To-1];
            //lZub = _allInd.TeethSeries[Bars.Range.To-1];
			//lChelust =  _allInd.JawsSeries[Bars.Range.To-1];
			
			//Print("{0} - {1} - {2}",lChelust,lZub,lGuba);
			//if (lZub>lChelust && lGuba>lZub && Bars[Bars.Range.To-1].Close>lChelust) Red();
			//if (lZub<lChelust && lGuba<lZub && Bars[Bars.Range.To-1].Close<lChelust) Blue();
			
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