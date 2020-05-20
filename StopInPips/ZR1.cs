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
    [TradeSystem("Z")]        //copy of "ZR"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("ExtDepth3:", DefaultValue = 3)]
        public int ED3 { get; set; }	
		[Parameter("ExtDepth50:", DefaultValue = 60)]
        public int ED50 { get; set; }
		[Parameter("SL:", DefaultValue = 200)]
        public int SL { get; set; }	
		[Parameter("TP:", DefaultValue = 200)]
        public int TP { get; set; }		
		
		private ZigZag _wprInd3;
		private ZigZag _wprInd50;
		public Fractals _frInd;
		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2,zzb,zzs;
		private double z50_1,z50_2,z50_3,stML;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		private VerticalLine vy,vb;
		private int ibuy,isell;
		private bool torg=true,torgU=true,torgD=true;
		private double sv=0,av1=0,av2=0,av3=0,av4=0,av5=0;
		
        protected override void Init()
        {
			
			_wprInd3= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd3.ExtDepth=ED3;
			_wprInd50= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd50.ExtDepth=ED50;
			 _frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_frInd.Range=30; 

			
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Yellow;
			vb = Tools.Create<VerticalLine>();
			vb.Color=Color.Blue;

			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd3.ReInit();
			 _wprInd50.ReInit();
			
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
			
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  { z50_3=0;z50_2=0;z50_1=0; torg=false;
		    zzu5=0;zzu4=0;zzu3=0;zzu2=0;zzu1=0; 
			zzd5=0;zzd4=0;zzd3=0;zzd2=0;zzd1=0;
			  if (posGuidBuy!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty; }
			if (posGuidSell!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  } 
		  if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Monday && Bars[Bars.Range.To-1].Time.Hour==01 )  torg=true;
		
//======================================================================================================================================
		  if( _wprInd50.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{ vb.Time=Bars[Bars.Range.To-1].Time;
				z50_3=z50_2; z50_2=z50_1;
			   z50_1 =_wprInd50.MainIndicatorSeries[Bars.Range.To-1];
				if (z50_3<z50_2 && z50_1<z50_2) { torgD=true; 
					var	vb1 = Tools.Create<VerticalLine>();
							vb1.Color=Color.Violet;
				vb1.Width=3;
				vb1.Time=Bars[Bars.Range.To-1].Time;}
				if (z50_3>z50_2 && z50_1>z50_2) { torgU=true;
				
				var	vb1 = Tools.Create<VerticalLine>();
							vb1.Color=Color.Purple;
				vb1.Width=3;
				vb1.Time=Bars[Bars.Range.To-1].Time;}
				
			}
//======================================================================================================================================
			if (posGuidBuy!=Guid.Empty && z50_1<z50_2) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && z50_1>z50_2) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}		

//======================================================================================================================================
			
//			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).Pips>100) 
//			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}	
//			{var res = Trade.UpdateMarketPosition(posGuidBuy,Bars[Bars.Range.To-1].Low, null, null); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
//			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).Pips>100) 
//			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}			
//			{var res = Trade.UpdateMarketPosition(posGuidSell,Bars[Bars.Range.To-1].High, null, null); if (res.IsSuccessful) posGuidSell = Guid.Empty;}			
//======================================================================================================================================
			if( _wprInd3.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    if(zzd5!=0 && zzd4!=0 && zzd3!=0 && zzd2!=0 && zzd1!=0) torg=true; else torg=false;
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd3.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
			    	vy.Time=Bars[Bars.Range.To-111111].Time;
					
//			if (posGuidBuy!=Guid.Empty && zzb>zzu1) 
//			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}

//======================================================================================================================================
//			if (posGuidBuy!=Guid.Empty) ibuy++; else ibuy=0;
//			if (posGuidSell!=Guid.Empty) isell++; else isell=0;
//			if(ibuy>5) {var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
//			if(isell>5) {var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
//	======================================================================================================================================				

					 // ВВЕРХУ
							
							if(torgD && torg && posGuidSell==Guid.Empty){ torgD=false;
								var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1,Stops.InPips(SL,TP), null, null);
								if (result2.IsSuccessful)  { posGuidSell=result2.Position.Id; zzs=zzu1; }
						       } 
						
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					vy.Time=Bars[Bars.Range.To-1].Time;

//======================================================================================================================================
//			if (posGuidBuy!=Guid.Empty) ibuy++; else ibuy=0;
//			if (posGuidSell!=Guid.Empty) isell++; else isell=0;
//			if(ibuy>5) {var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
//			if(isell>5) {var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
//======================================================================================================================================
					
					
//			if (posGuidSell!=Guid.Empty && zzs<zzu1) 
//			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}
			
				//  ВНИЗУ
						   
							if(torgU && torg && posGuidBuy==Guid.Empty){ torgU=false;
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1, Stops.InPips(SL,TP), null, null);
								if (result3.IsSuccessful)  { posGuidBuy=result3.Position.Id; zzb=zzu1;}
							} 
						
				}
	
			}
        }
//===============================================================================================================================        
    }
}