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
    [TradeSystem("Patern_123-V2")]       //copy of "Patern_123"
    public class ZZ_Ex1 : TradeSystem
    {
		
		private ZigZag _wprInd;
		private double zz1=2,zz2=2,zz3=2;
		private int    zzt1,zzt2,zzt3,zzt4,zzt5,zzt6;
		private double zzd1,zzd2,zzd3,zzd4,zzd5,zzd6;
		private int    zzi1,zzi2,zzi3,zzi4,zzi5,zzi6;
		private VerticalLine vy,vb;
		
        protected override void Init()
        {
			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=5;
		
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Red;
			vb = Tools.Create<VerticalLine>();
			vb.Color=Color.Blue;

			
        }        
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
					zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zzt2; 
					zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2;
			    	vy.Time=Bars[zzt2].Time;
					Print("ВВЕРХУ - {0} {1} {2} {3} {4} {5}",
					     Bars[Bars.Range.To-1].Time,zzd4<zzd2,zzd4<zzd6,zzd5<zzd1,zzd3<zzd5,zzd3<zzd1);
					
					// if(zzd4<zzd2 && zzd4<zzd6 && zzd5<zzd1 && zzd3<zzd5 &&	zzd3<zzd1 ) // ВВЕРХУ
					if(zzd5<zzd1 && zzd3<zzd5 && zzd3<zzd1 ) // ВВЕРХУ
						 {  
		var toolPolyLine = Tools.Create<PolyLine>();
			toolPolyLine.Color=Color.Red;
			toolPolyLine.Width=4;	
				/*			toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, Bars[zzi5].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, Bars[zzi4].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, Bars[zzi3].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, Bars[zzi2].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, Bars[zzi1].High)); */
							
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
					zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zzt2; 
					zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2;
					vb.Time=Bars[zzt2].Time;
					//Print("ВНИЗУ  - {0} {1} {2} {3} {4} {5}",
				
					    // Bars[Bars.Range.To-1].Time,zzd3>zzd5,zzd3>zzd1,zzd4>zzd6,zzd4>zzd2,zzd5>zzd1);
						//Самый строгий вариант ....	
					    //if( zzd3>zzd5 && zzd3>zzd1 && zzd4>zzd6 &&	zzd4>zzd2 && zzd5>zzd1 )//  ВНИЗУ
						//Менее строгий вариант ....	
					    if( zzd3>zzd5 && zzd3>zzd1 &&  zzd5>zzd1 )//  ВНИЗУ

					{
						 
		var toolPolyLine = Tools.Create<PolyLine>();
			toolPolyLine.Color=Color.Blue;
			toolPolyLine.Width=4;		
							
							/* toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, Bars[zzi5].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, Bars[zzi4].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, Bars[zzi3].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, Bars[zzi2].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, Bars[zzi1].Low)); */
							
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