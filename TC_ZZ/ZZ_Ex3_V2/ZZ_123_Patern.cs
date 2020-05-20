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
		
		private ZigZag _wprInd3;
		private ZigZag _wprInd16;
		private ZigZag _wprInd50;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5;
		private double zz1,zz2,zz3,zz4;
		private int zzi1,zzi2,zzi3,zzi4;
		private VerticalLine vy,vb;
		private bool torg=true;
		
		
        protected override void Init()
        {
			_wprInd3= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd3.ExtDepth=3;
			//_wprInd16= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			//_wprInd16.ExtDepth=16;
			_wprInd50= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd50.ExtDepth=50;
			

			
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Red;	
			vb = Tools.Create<VerticalLine>();
			vb.Color=Color.Blue;	
        }        
//===============================================================================================================================
        protected override void NewBar()
        {	_wprInd3.ReInit();
			//_wprInd16.ReInit();
			_wprInd50.ReInit();
//======================================================================================================================================
Print("{0} -- {1} - {2} - {3}",Bars[Bars.Range.To-1].Time,_wprInd3.MainIndicatorSeries[Bars.Range.To-1],_wprInd3.MainIndicatorSeries[Bars.Range.To-2],_wprInd3.MainIndicatorSeries[Bars.Range.To-3]);
			
			
			if( _wprInd50.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{ 	var	vb1 = Tools.Create<VerticalLine>();
							vb1.Color=Color.Blue;	
				vb1.Time=Bars[Bars.Range.To-1].Time;}
			
			if( _wprInd3.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd3.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ 	// ВВЕРХУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
							var	vy1 = Tools.Create<VerticalLine>();
							vy1.Color=Color.Red;		
							vy1.Time=Bars[Bars.Range.To-1].Time;
							vy1.Label="Label1";
					
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
							var	vy1 = Tools.Create<VerticalLine>();
							vy1.Color=Color.Red;		
							vy1.Time=Bars[Bars.Range.To-1].Time;
							vy1.Label="Label1";
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