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
    [Indicator("xFour", Location = IndicatorLocation.Chart)]
    public class xFour : Indicator
    {
		[Parameter("PeriodATR", DefaultValue = 10)]
		public int PeriodAtr { get; set; }
		
		[Parameter("kATR", DefaultValue = 2.0)]
		public double kATR { get; set; }
		
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

		ISeries<double> VTS16;  //double VTS8[MAXBARSCOUNT];
		ISeries<double> VTS17;  //double VTS9[MAXBARSCOUNT];
		ISeries<double> VTS18; //double VTS10[MAXBARSCOUNT];
		ISeries<double> VTS19; //double VTS11[MAXBARSCOUNT];
		ISeries<double> VTS20; //double VTS12[MAXBARSCOUNT];
		ISeries<double> VTS21; //double VTS13[MAXBARSCOUNT];
		ISeries<double> VTS22; //double VTS14[MAXBARSCOUNT];
		ISeries<double> VTS23; //double VTS15[MAXBARSCOUNT];
		
		ISeries<double> VTS24;  //double VTS8[MAXBARSCOUNT];
		ISeries<double> VTS25;  //double VTS9[MAXBARSCOUNT];
		ISeries<double> VTS26; //double VTS10[MAXBARSCOUNT];
		ISeries<double> VTS27; //double VTS11[MAXBARSCOUNT];
		ISeries<double> VTS28; //double VTS12[MAXBARSCOUNT];
		ISeries<double> VTS29; //double VTS13[MAXBARSCOUNT];
		
		
		[Series("VTS_TS", Color = Color.DeepSkyBlue,  Style = SeriesStyle.Chars, CharCode = CharCode.Lozenge, Width = 1)]
		public IIndicatorSeries VTS_TS {get;set;} //double VTS_TS[MAXBARSCOUNT];
	
		[Series("VTS_TS1", Color = Color.ForestGreen, Style = SeriesStyle.Chars, CharCode = CharCode.Lozenge, Width = 1)]
		public IIndicatorSeries VTS_TS1 {get;set;} //double VTS_TS1[MAXBARSCOUNT];

		private Period _period;
		private AverageTrueRange ATR;
		
		protected override void Init()
		{
			
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
			
			VTS16 = CreateSeries<double>();
			VTS17 = CreateSeries<double>();
			VTS18 = CreateSeries<double>();
			VTS19 = CreateSeries<double>();
			VTS20 = CreateSeries<double>();
			VTS21 = CreateSeries<double>();
			VTS22 = CreateSeries<double>();
			VTS23 = CreateSeries<double>();
			
			VTS24 = CreateSeries<double>();
			VTS25 = CreateSeries<double>();
			VTS26 = CreateSeries<double>();
			VTS27 = CreateSeries<double>();
			VTS28 = CreateSeries<double>();
			VTS29 = CreateSeries<double>();

			_period = Timeframe;
			ATR = GetInvisibleIndicator<AverageTrueRange>(
														 Instrument.Id,
														 _period,
															PeriodAtr
														 );

		}
		
		protected override void Calculate(int FromIndex,int ToIndex)
        {
            // calculate
			
			for(int i = FromIndex; i <= ToIndex; i++)
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
       			VTS24[i] = Math.Max(VTS22[i], VTS22[i-1]);//VTS6[i+1];
       			VTS25[i] = Math.Min(VTS23[i], VTS23[i-1]);//VTS7[i+1];
       			VTS26[i] = Math.Max(VTS24[i], VTS24[i-1]);//VTS6[i+1];
       			VTS27[i] = Math.Min(VTS25[i], VTS25[i-1]);//VTS7[i+1];
       			VTS28[i] = Math.Max(VTS26[i], VTS26[i-1]);//VTS6[i+1];
       			VTS29[i] = Math.Min(VTS27[i], VTS27[i-1]);//VTS7[i+1];
				
				VTS_TS[i]  = VTS_TS[i - 1];
       			VTS_TS1[i] = VTS_TS1[i - 1];
       			if (Bars[i].Close > VTS29[i]) 
				{
           			VTS_TS[i] = VTS28[i];
           			VTS_TS1[i] = double.NaN;//VTS_TS[i+1];
           		}
       			if (Bars[i].Close < VTS28[i]) 
				{
           			VTS_TS1[i]= VTS29[i];
           			VTS_TS[i] = double.NaN;//VTS_TS1[i+1];
           		}
       		}
        }
    }
}