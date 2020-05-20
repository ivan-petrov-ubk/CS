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
    [TradeSystem("AO_Div3")]
    public class AO_Div3 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		
				public AwesomeOscillator _awoInd;
		double aoUp1,aoDown1,aoUp2,aoDown2,aoUp3,aoDown3,aoUp4,aoDown4,aoUp5,aoDown5;

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
						_awoInd = GetIndicator<AwesomeOscillator>(Instrument.Id, Timeframe);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			aoUp1    = _awoInd.SeriesUp[Bars.Range.To-1];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown1  = _awoInd.SeriesDown[Bars.Range.To-1]; // Червоні лінії
			aoUp2    = _awoInd.SeriesUp[Bars.Range.To-2];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown2  = _awoInd.SeriesDown[Bars.Range.To-2];
			aoUp3    = _awoInd.SeriesUp[Bars.Range.To-3];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown3  = _awoInd.SeriesDown[Bars.Range.To-3];
			aoUp4    = _awoInd.SeriesUp[Bars.Range.To-4];  // Зелені лінії - Вверху>0  Внизу<0
			aoDown4  = _awoInd.SeriesDown[Bars.Range.To-4];
			
			// Пересечение 0 Снизу ВВЕРХ - зеленые столбики	 
				 if (aoUp2<=0 && aoUp1>=0)     {var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Red;   }
			
 			// Пересечение 0 Сверху вниз красные столбики	
			     if (aoDown2>=0 && aoDown1<=0)  { var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Blue;   } 
			
			// Вверху ГОРБ Красний на Зеленый
			 	 if(aoUp4>0.0002 && aoUp3>0.0002 && aoDown2>0.0002 && aoDown1>0.0002) 
     				{ 	}
					
			// // Вверху ВПАДИНА Зеленый на Красний
			 	 if(aoDown4>0.0002 && aoDown3>0.0002 && aoUp2>0.0002 && aoUp1>0.0002) 
     				{	}

			// ВНИЗУ ГОРБ Красний на Зеленый
			 	if(aoDown4<-0.0002 && aoDown3<-0.0002 && aoUp2<-0.0002 && aoUp1<-0.0002 ) 
     				{   	}
					
			// // ВНИЗУ ВПАДИНА Зеленый на Красний
			 	if(aoUp4<-0.0002 && aoUp3<-0.0002 && aoDown2<-0.0002 && aoDown1<-0.0002) 
     				{	}

			/*	var toolVerticalLine=Tools.Create<VerticalLine>();
     			toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				toolVerticalLine.Color=Color.Red;  */
					
			
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