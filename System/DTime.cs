using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;

namespace IPro.TradeSystems
{
    [TradeSystem("DTime")]
    public class DTime : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		public DateTime t1;
        public DateTime t2;
		bool Time_Start;
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
		      t1 = Bars[Bars.Range.To-5].Time;
			//Print("Date = {0}",t1.Date);             // Date = 07.11.2016 0:00:00
			//Print("Day = {0}",t1.Day);               // Day = 4
			//Print("DayOfWeek = {0}",t1.DayOfWeek);   // DayOfWeek = Friday
			//Print("DayOfYear = {0}",t1.DayOfYear);   // DayOfYear = 309
			//Print("Hour = {0}",t1.Hour);             // Hour = 22
			//Print("Kind = {0}",t1.Kind);             // Minute = 0
			//Print("Millisecond = {0}",t1.Millisecond);   // Millisecond = 0
			//Print("Minute = {0}",t1.Minute);        //  Minute = 0
			//Print("Month = {0}",t1.Month);          //  Month = 11
			//Print("Second = {0}",t1.Second);        //  Second = 0
			//Print("Ticks = {0}",t1.Ticks);          //  Ticks = 636138936000000000
			//Print("TimeOfDay = {0}",t1.TimeOfDay);  //  TimeOfDay = 22:00:00
			//Print("Year = {0}",t1.Year);            //  Year = 2016
			//Print("===============================================================");
			Time_Start = (t1.TimeOfDay.ToString()=="22:00:00");
			Print("{0} = {1}",Time_Start,t1.TimeOfDay);
			
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