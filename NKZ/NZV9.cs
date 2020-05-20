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
    [TradeSystem("NZV9")]
    public class NZV9 : TradeSystem
    {
  		private ZigZag _wprInd;
		private double zz1,zz2,zz3;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3;		
		public DateTime zzi1,zzi2,zzi3;
		private double zzU1,zzU2,zzU3;
		private double zzD1,zzD2,zzD3;
		//public VerticalLine vr;
		//public HorizontalLine vh;
		public int k=0;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			//_wprInd.ExtDepth=6;
			//vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red;
			//vh = Tools.Create<HorizontalLine>(); 
        }        


        
        protected override void NewBar()
        {
			if(k==0)  InitFr(300); else {
			
			//_wprInd.ReInit();
			Print("{0} - {1}",Bars[Bars.Range.To-1].Time,_wprInd.MainIndicatorSeries[Bars.Range.To-1]);
			
//========== Если появился пик зигзага  ================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    // Значения 3 значений - для определения направления
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars[Bars.Range.To-1].Time;
				Print("{0} - {1}",Bars[Bars.Range.To-1].Time,_wprInd.MainIndicatorSeries[Bars.Range.To-1]);
				
			//======================  ПИК - ВЕРШИНА ВВЕРХУ
				if(zz2>zz3 && zz2>zz1)  
				{ // Берем 4 вершины - zzd-индекс zzu-значение 
					Print("{0} - {1}",zzi2,zz2);
					var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=zzi2;
					zzU3=zzU2; zzU2=zzU1; zzU1=zz2; 
					tmU3=tmU2; tmU2=tmU1; tmU1=zzi2;
				}					
			//======================= ПИК - ВЕШИНА ВНИЗУ
				if(zz2<zz3 && zz2<zz1)  
				{   // Берем 4 вершины - zzd-индекс zzu-значение
					Print("{0} - {1}",zzi2,zz2);
					var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=zzi2;
					zzD3=zzD2; zzD2=zzD1; zzD1=zz2; 
					tmD3=tmD2; tmD2=tmD1; tmD1=zzi2;
				}

				//Print("{0} - {1}",zzi1,zz1);	
				//_wprInd.ReInit();
			}	
			}
//==========================================================================================================
			k++;
        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
		
				protected void InitFr(int kl)
		{
				var i=0; 

				Print("Start InitFr");
			
			while ( i<kl ) 
			{ 
				if(_wprInd.MainIndicatorSeries[Bars.Range.To-i]>0) {
				Print("{1} - ZZ={0}",_wprInd.MainIndicatorSeries[Bars.Range.To-i],i);
					
				var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-i].Time;
				}
			  	  i++;
        	}			
			
		} 
//===============  END History ===============================================================================

		
    }
}