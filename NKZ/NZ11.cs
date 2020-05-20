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
    [TradeSystem("NZ11")]             //copy of "NZ1"
    public class ZZ_Ex1 : TradeSystem
    {

		public Fractals _frInd;		

		
        protected override void Init()
        {	

			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=3;
			

        }        
//===========================================================================
        protected override void NewBar()
        {   
		
		
/*				Print("Bott - {0}  -1={1} -2={2} -3={3} -4={4} -5={5} ",
				  Bars[Bars.Range.To-1].Time,
				  _frInd.BottomSeries[Bars.Range.To-1],
				  _frInd.BottomSeries[Bars.Range.To-2],
				  _frInd.BottomSeries[Bars.Range.To-3],
				  _frInd.BottomSeries[Bars.Range.To-4],
				  _frInd.BottomSeries[Bars.Range.To-5]);

			  	Print("Top - {0}  -1={1} -2={2} -3={3} -4={4} -5={5} ",
				  Bars[Bars.Range.To-1].Time,
				  _frInd.TopSeries[Bars.Range.To-1],
				  _frInd.TopSeries[Bars.Range.To-2],
				  _frInd.TopSeries[Bars.Range.To-3],
				  _frInd.TopSeries[Bars.Range.To-4],
				  _frInd.TopSeries[Bars.Range.To-5]);
*/
			Print("Bott - {0}  Bootom={1} Top={2} ",Bars[Bars.Range.To-1].Time,_frInd.BottomSeries[Bars.Range.To-4],_frInd.TopSeries[Bars.Range.To-4]);
			
			
        }
//===============================================================================================================================   

    }
}
