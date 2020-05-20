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
    [TradeSystem("Patern_123-V1")]       //copy of "Patern_123"
    public class ZZ_Ex1 : TradeSystem
    {
		
		private ZigZag _wprInd;
		private double zz1=2,zz2=2,zz3=2;
		private int    zzt1,zzt2,zzt3,zzt4,zzt5,zzt6,zzt7,zzt8;
		private double ot,zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7,zzd8;
		private int    zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,zzi7,zzi8;
		private VerticalLine vy,vb;
		
        protected override void Init()
        {_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=5; }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzt3=zzt2;	 zzt2=zzt1;  zzt1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzi8=zzi7; zzi7=zzi6; zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zzt2; 
					zzd8=zzd7; zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2;
				Print("ВВЕРХУ  {0}",Bars[Bars.Range.To].Time);
					ot = zzd2+((zzd3-zzd2)*0.5);
					if( zzd6>zzd8 && zzd4>zzd6 &&  zzd4>zzd2 && zzd3>zzd1 && ot<zzd1) // ВВЕРХУ
						 {   Print("BLUE ВВЕРХУ {0} - 1-{1} 2-{2} 3-{3} 4-{4} 5-{5} 6-{6} 7-{7}",Bars[Bars.Range.To].Time,zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7);
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi6].Time, zzd6));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, zzd5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, zzd4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, zzd3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, zzd2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, zzd1));							
						}
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzi8=zzi7; zzi7=zzi6; zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zzt2; 
					zzd8=zzd7; zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2;
				    Print("ВНИЗУ {0}",Bars[Bars.Range.To].Time);
					ot = zzd2-((zzd2-zzd3)*0.5);
					if( zzd6<zzd8 &&   zzd4<zzd6 &&  zzd4<zzd2 && zzd3<zzd1 && ot>zzd1) // ВНИЗУ
					{
						 Print("RED ВНИЗУ {0} - 1-{1} 2-{2} 3-{3} 4-{4} 5-{5} 6-{6} 7-{7}",Bars[Bars.Range.To].Time,zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7);
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=4;		
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi6].Time, zzd6));
		 					toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, zzd5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, zzd4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, zzd3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, zzd2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, zzd1)); 	
						}
				}
	
			}
        }
//===============================================================================================================================        
    }
}