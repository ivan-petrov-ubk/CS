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
    [TradeSystem("Time_logic")]
    public class Time_logic : TradeSystem
    {
       		public DateTime t1;
        public DateTime t2;
		bool Time_Start;
        protected override void NewBar()
        {
			t1 = Bars[Bars.Range.To-1].Time;
			t2 = Bars[Bars.Range.To-2].Time;
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
			Print("{0}>{1} = {2}",t1,t2,t1<t2);
			
        }
        
 
    }
}