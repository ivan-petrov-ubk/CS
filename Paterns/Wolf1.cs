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
    [TradeSystem("Wolf1")]
    public class Wolf1 : TradeSystem
    {
		[Parameter("Fisher H4=", DefaultValue = true)]
        public bool Pt { get; set; }	
		
		private ZigZag _wprInd;
		private double zz1=2,zz2=2,zz3=2,dt,A,B,C;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=3;
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {_wprInd.ReInit();

//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{  
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
				 
				if(zz3<zz2 && zz2>zz1)  
				{   // ВВЕРХУ
					zzd6=zzd5;zzd5=zzd4;zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu6=zzu5;zzu5=zzu4;zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					//dt = ((zzd3-zzd1)/(zzd5-zzd1)) - ((zzu3-zzu1)/(zzu5-zzu1));
					
					
					A=zzu1-zzu5; B=zzd5-zzd1; C=zzd1*zzu5-zzd5*zzu1;
					dt=Math.Abs(A*zzd3+B*zzu3+C)/Math.Sqrt(A*A+B*B);
					Print("{0} - {1} {2} {3}",dt,Bars[zzd5].Time,Bars[zzd3].Time,Bars[zzd1].Time);
					
							/*		if(Math.Abs(dt)<0.02) 
				{    
					var toolTrendLine = Tools.Create<TrendLineRay>();
         toolTrendLine.Point1 = new ChartPoint(Bars[zzd5].Time, Bars[zzd5].High);
         toolTrendLine.Point2 = new ChartPoint(Bars[zzd1].Time, Bars[zzd1].High);}  */
				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{   // ВНИЗУ 
					zzd6=zzd5;zzd5=zzd4;zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu6=zzu5;zzu5=zzu4;zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					
					//dt = ((zzd3-zzd1)/(zzd5-zzd1)) - ((zzu3-zzu1)/(zzu5-zzu1));
					A=zzu1-zzu5; B=zzd5-zzd1; C=zzd1*zzu5-zzd5*zzu1;
					dt=Math.Abs(A*zzd3+B*zzu3+C)/Math.Sqrt(A*A+B*B);
					Print("{0} - {1} {2} {3}",dt,Bars[zzd5].Time,Bars[zzd3].Time,Bars[zzd1].Time);
					/*				if(Math.Abs(dt)<0.02) 
				{ 
					var toolTrendLine = Tools.Create<TrendLineRay>();
         toolTrendLine.Point1 = new ChartPoint(Bars[zzd5].Time, Bars[zzd5].Low);
         toolTrendLine.Point2 = new ChartPoint(Bars[zzd1].Time, Bars[zzd1].Low);
				}  */
				}
				
				

				
				
			}
//===============================================================================================================================  
        }
      
    }
}