using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Indicators.Standard;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using System.Collections.Generic;

namespace IPro.Indicators
{
    [Indicator("ATS1", Location = IndicatorLocation.Chart)]
    public class ATS1 : Indicator
    {
//        [Series("Result")]
//        public IIndicatorSeries Result { get; set; } 
//extern int    PeriodAtr = 10;
//extern double kATR      = 2.0;
//extern int    GrupNum   = 3;

		[Parameter("PeriodATR", DefaultValue = 10)]
		public int PeriodAtr { get; set; }
		
		[Parameter("kATR", DefaultValue = 2.0)]
		public double kATR { get; set; }
		
		[Parameter("GrupNum", DefaultValue = 3)]
		public int GrupNum { get; set; }
		
		ISeries<double> VTS;   //double VTS[MAXBARSCOUNT];
		ISeries<double> VTS1;  //double VTS1[MAXBARSCOUNT];
		ISeries<double> VTS2;  //double VTS2[MAXBARSCOUNT];
		ISeries<double> VTS3;  //double VTS3[MAXBARSCOUNT];
		ISeries<double> VTS4;  //double VTS4[MAXBARSCOUNT];
		ISeries<double> VTS5;  //double VTS5[MAXBARSCOUNT];
		ISeries<double> VTS6;  //double VTS6[MAXBARSCOUNT];
		ISeries<double> VTS7;  //double VTS7[MAXBARSCOUNT];

		ISeries<double> VTS8;  //double VTS8[MAXBARSCOUNT];
		ISeries<double> VTS9;  //double VTS9[MAXBARSCOUNT];
		ISeries<double> VTS10; //double VTS10[MAXBARSCOUNT];
		ISeries<double> VTS11; //double VTS11[MAXBARSCOUNT];
		ISeries<double> VTS12; //double VTS12[MAXBARSCOUNT];
		ISeries<double> VTS13; //double VTS13[MAXBARSCOUNT];
		ISeries<double> VTS14; //double VTS14[MAXBARSCOUNT];
		ISeries<double> VTS15; //double VTS15[MAXBARSCOUNT];

		[Series("VTS16", Color = Color.Aqua)]
		public IIndicatorSeries VTS16 {get;set;} //double VTS16[MAXBARSCOUNT];
		
		[Series("VTS17", Color = Color.Magenta)]
		public IIndicatorSeries VTS17 {get;set;} //double VTS17[MAXBARSCOUNT];
	
		[Series("VTS18", Color = Color.Aqua)]
		public IIndicatorSeries VTS18 {get;set;} //double VTS18[MAXBARSCOUNT];
	
		[Series("VTS19", Color = Color.Magenta)]
		public IIndicatorSeries VTS19 {get;set;} //double VTS19[MAXBARSCOUNT];
	
		[Series("VTS20", Color = Color.Aqua)]
		public IIndicatorSeries VTS20 {get;set;} //double VTS20[MAXBARSCOUNT];
	
		[Series("VTS21", Color = Color.Magenta)]
		public IIndicatorSeries VTS21 {get;set;} //double VTS21[MAXBARSCOUNT];
	
		[Series("VTS22", Color = Color.Aqua)]
		public IIndicatorSeries VTS22 {get;set;} //double VTS22[MAXBARSCOUNT];
	
		[Series("VTS23", Color = Color.Magenta)]
		public IIndicatorSeries VTS23 {get;set;} //double VTS23[MAXBARSCOUNT];
	
		private Period _period;
		private AverageTrueRange ATR;
		
		protected override void Init()
		{
			  // IndicatorBuffers(8);
   				//SetIndexBuffer(0, VTS16);
   				//SetIndexBuffer(1, VTS17);
   				//SetIndexBuffer(2, VTS18);
   				//SetIndexBuffer(3, VTS19);
   				//SetIndexBuffer(4, VTS20);
   				//SetIndexBuffer(5, VTS21);
   				//SetIndexBuffer(6, VTS22);
   				//SetIndexBuffer(7, VTS23);

			VTS   = CreateSeries<double>();
			VTS1  = CreateSeries<double>();
			VTS2  = CreateSeries<double>();
			VTS3  = CreateSeries<double>();
			VTS4  = CreateSeries<double>();
			VTS5  = CreateSeries<double>();
			VTS6  = CreateSeries<double>();
			VTS7  = CreateSeries<double>();

			VTS8  = CreateSeries<double>();
			VTS9  = CreateSeries<double>();
			VTS10 = CreateSeries<double>();
			VTS11 = CreateSeries<double>();
			VTS12 = CreateSeries<double>();
			VTS13 = CreateSeries<double>();
			VTS14 = CreateSeries<double>();
			VTS15 = CreateSeries<double>();
			
			_period = Timeframe;
			ATR = GetInvisibleIndicator<AverageTrueRange>(
														 Instrument.Id,
														 _period,
															PeriodAtr
														 );
			
		}
		
		
        protected override void Calculate(int FromIndex, int ToIndex)
        {
            // calculate
		    //int    counted_bars=IndicatorCounted();
//---- 
   
			//int    StartBars = Bars - counted_bars+1;
   			//if (StartBars<Sb) StartBars = Sb;
   			for(int i = FromIndex; i<=ToIndex; i++)
			{
       			VTS[i] = (Bars[i].Close - kATR * ATR.SeriesAtr[i]); //iATR(NULL,0,PeriodAtr,i));
       			VTS1[i]= (Bars[i].Close + kATR * ATR.SeriesAtr[i]); //iATR(NULL,0,PeriodAtr,i));
       		}
   			for(int i = FromIndex; i <= ToIndex; i++)
			{
       			VTS2[i]  = Math.Max(VTS[i],   VTS[i-1]  );
       			VTS3[i]  = Math.Min(VTS1[i],  VTS1[i-1] );
       			VTS4[i]  = Math.Max(VTS2[i],  VTS2[i-1] );//VTS2[i+1];
       			VTS5[i]  = Math.Min(VTS3[i],  VTS3[i-1] );//VTS3[i+1];
       			VTS6[i]  = Math.Max(VTS4[i],  VTS4[i-1] );//VTS4[i+1];
       			VTS7[i]  = Math.Min(VTS5[i],  VTS5[i-1] );//VTS5[i+1];
       			VTS8[i]  = Math.Max(VTS6[i],  VTS6[i-1] );//VTS6[i+1];
       			VTS9[i]  = Math.Min(VTS7[i],  VTS7[i-1] );//VTS7[i+1];
       			VTS10[i] = Math.Max(VTS8[i],  VTS8[i-1] );//VTS8[i+1];
       			VTS11[i] = Math.Min(VTS9[i],  VTS9[i-1] );//VTS9[i+1];
       			VTS12[i] = Math.Max(VTS10[i], VTS10[i-1]);//VTS10[i+1];
       			VTS13[i] = Math.Min(VTS11[i], VTS11[i-1]);//VTS11[i+1]; 
       			VTS14[i] = Math.Max(VTS12[i], VTS12[i-1]);//VTS10[i+1];
       			VTS15[i] = Math.Min(VTS13[i], VTS13[i-1]);//VTS11[i+1]; 
       			VTS16[i] = Math.Max(VTS14[i], VTS14[i-1]);//VTS6[i+1];
       			VTS17[i] = Math.Min(VTS15[i], VTS15[i-1]);//VTS7[i+1];
       			VTS18[i] = Math.Max(VTS16[i], VTS16[i-1]);//VTS6[i+1];
       			VTS19[i] = Math.Min(VTS17[i], VTS17[i-1]);//VTS7[i+1];
       			VTS20[i] = Math.Max(VTS18[i], VTS18[i-1]);//VTS6[i+1];
       			VTS21[i] = Math.Min(VTS19[i], VTS19[i-1]);//VTS7[i+1];
       			VTS22[i] = Math.Max(VTS20[i], VTS20[i-1]);//VTS6[i+1];
       			VTS23[i] = Math.Min(VTS21[i], VTS21[i-1]);//VTS7[i+1];
       		}

        }
    }
}