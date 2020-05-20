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
    [TradeSystem("ZZ_123_Patern")]
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth:", DefaultValue = 5)]
        public int ED { get; set; }	
		
		private ZigZag _wprInd;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5;
		private double zz1,zz2,zz3,zz4;
		private int zzi1,zzi2,zzi3,zzi4;
		private VerticalLine vy;
		private bool torg=true;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Yellow;	
        }        
//===============================================================================================================================
        protected override void NewBar()
        {	_wprInd.ReInit();
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ 	// ВВЕРХУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					vy.Time=Bars[Bars.Range.To-1].Time;
Print("ВЕРХ - {0} {1} {2} {3} - {4}",Bars[Bars.Range.To-1].Time,zzu2>zzu4,zzu3>zzu1,Math.Round((zzu3-zzu2)/(zzu3-zzu4)*100,0),(zzu3-zzu4)>(zzu3-zzu2)*2);
					 if(zzu2>zzu4 && 
						zzu3>zzu1 && 
						(zzu3-zzu4)>(zzu3-zzu2)*2
					   ) // ВВЕРХУ
						{	var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].High));
						}
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ 	// ВНИЗУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					vy.Time=Bars[Bars.Range.To-1].Time;
Print("ВНИЗ - {0} {1} {2} {3} - {4}",Bars[Bars.Range.To-1].Time,zzu4>zzu2,zzu1>zzu3,Math.Round((zzu2-zzu3)/(zzu4-zzu3)*100,0),(zzu4-zzu3)>(zzu2-zzu3)*2);

					if( zzu4>zzu2 && 
						zzu1>zzu3 &&
						(zzu4-zzu3)>(zzu2-zzu3)*2
					  )	//  ВНИЗУ
						{	var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;				
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].Low));		
						}
				}
//=============================================================================================================================== 	
			}
        }
    }
}