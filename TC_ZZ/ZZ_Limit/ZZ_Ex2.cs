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
    [TradeSystem("ZZ_Ex2")] //copy of "ZZ_Ex1"
    public class ZZ_Ex1 : TradeSystem
    {
		private ZigZag _wprInd;
		private double zz1=2,zz2=2,zz3=2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2;
		private int zzd1,zzd2,zzd3,zzd4;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red;vr.Width=5; vr.Time=Bars[Bars.Range.To-1].Time; 
        }        

        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			_wprInd.ReInit();

			//Print("{0} - {1}",Bars[Bars.Range.To-1].Time, _wprInd.MainIndicatorSeries[Bars.Range.To-1]);
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{  //var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[Bars.Range.To-1].Time; 
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
				 
				if(zz3<zz2 && zz2>zz1)  { // ВВЕРХУ
					zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
                     V=1;
			    Print("ВВЕРХ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);		 
				if(zzu1>zzu2 && zzu3>zzu2 && zzu1>zzu3 && zzu2>zzu4 && zzu3>zzu4) // ВВЕРХУ
					{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzd1].Time;
					var result1 = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.SellLimit, 0.1,  zzu1+Instrument.Spread-0.0002, 0, Stops.InPips(200,200), Bars[zzd1].Time.AddHours(15), null, null);}
				}				
				
				if(zz3>zz2 && zz2<zz1)  { // ВНИЗУ
					zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					V=2;
				Print("НИЗ {0} 1={1} 2={2} 3={3} 4={4}",Bars[zzd1].Time,zzu1,zzu2,zzu3,zzu4);			
				if(zzu4>zzu2 && zzu3>zzu1 && zzu2>zzu3 && zzu4>zzu3) //  ВНИЗУ
					{var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzd1].Time;
					var result = Trade.OpenPendingPosition(Instrument.Id, ExecutionRule.BuyLimit, 0.1, zzu1-Instrument.Spread+0.0002, 0, Stops.InPips(200,200), Bars[zzd1].Time.AddHours(15), null, null);	}

				}
			}
        }
        
    }
}