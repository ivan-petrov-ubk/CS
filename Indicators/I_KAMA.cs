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
    [Indicator("KAMA", Location = IndicatorLocation.Chart)]
    public class KAMA : Indicator
    {
		
		[Parameter("SP :", DefaultValue = 2.0, MinValue = 2.0)]
		public double SP { get; set; }
		
		[Parameter("FP :", DefaultValue = 30.0, MinValue = 3.0)]
		public double FP { get; set; }
		
		[Parameter("NP :", DefaultValue = 10, MinValue = 3)]
		public int NP { get; set; }

		[Parameter("PriceMode :", DefaultValue = PriceMode.Weighted)]
		public PriceMode PM { get; set;}
		
		[Parameter("D :", DefaultValue = 25, MinValue =2)]
		public int DD { get; set;}

		
		[Series("KAMAR", Color = Color.Navy, Style = SeriesStyle.Line, Width = 1)]
        public IIndicatorSeries KAMAR { get; set; } 

        [Series("KAMAUp", Color = Color.DarkGreen, Style = SeriesStyle.Chars, CharCode = CharCode.Circle, Width = 2)]
        public IIndicatorSeries KAMAUp { get; set; } 

		[Series("KAMADn", Color = Color.LightSeaGreen, Style = SeriesStyle.Chars, CharCode = CharCode.Circle, Width = 2)]
        public IIndicatorSeries KAMADn { get; set; } 
		
		
		
        protected override void Calculate(int FromIndex,int ToIndex)
        {
            // calculate

			if((Bars.Range.To - NP) < 0) return;
			if(KAMAR[FromIndex - 1].Equals(double.NaN)) KAMAR[FromIndex-1] = GetPrice(Bars[FromIndex - 1],PM);
			
			for(int ii = FromIndex; ii <= ToIndex; ii++)
			{
				
				double Volatility = 0.0;
				double Change = 0.0;
				
				Change = Math.Abs(GetPrice(Bars[ii],PM) - GetPrice(Bars[ii - NP],PM)); 
			
				for(int i = ii - NP; i <= ii; i++)
				{
					Volatility += Math.Abs(GetPrice(Bars[i],PM) - GetPrice(Bars[i - 1],PM));
				}

				double ER = Change/Volatility;
			
				double SC = 2.0 /(SP + 1);
				double FC = 2.0 /(FP + 1);
			
				double SSC = ER * (FC - SC) + SC;
				
				SSC *= SSC;
			
				KAMAR[ii] = KAMAR[ii - 1] + SSC * (GetPrice(Bars[ii],PM) - KAMAR[ii - 1]);
				
				KAMAUp[ii] = double.NaN;
				KAMADn[ii] = double.NaN;
				
				if(KAMAR[ii] >= (Bars[ii].Close + ((double)DD) * Instrument.Point)) 
				{
					KAMAUp[ii] = KAMAR[ii];
				}
				if(KAMAR[ii] <= (Bars[ii].Close - ((double)DD) * Instrument.Point)) 
				{
					KAMADn[ii] = KAMAR[ii];
				}
				
			}
		}
    }
}