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
    [TradeSystem("Z")]        //copy of "ZZ_V4"
    public class ZZ_Ex1 : TradeSystem
    {
		[Parameter("Period:", DefaultValue = 5)]
        public int PR { get; set; }	
		[Parameter("ExDeph:", DefaultValue = 5)]
        public int ED { get; set; }	
		[Parameter("Fibo:", DefaultValue = 0.81)]
        public double FB { get; set; }	
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2,korU,korD;
		private double zztb2,zzts2,zzb,zzs;
		private double z50_1,z50_2,z50_3,stML;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu,zzd;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzD,zzU;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		private VerticalLine vy,vb;
		private HorizontalLine hy,hb;
		private int ibuy,isell;
		private bool torg=true,torg2=false, torgU=true,torgD=true;
		private double sv=0,av1=0,av2=0,av3=0,av4=0,av5=0;
		
        protected override void Init()
        {
			
			zzd5=0; zzd4=0; zzd3=0; zzd2=0; zzd1=0;
			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=ED;
			_wprInd.ExtDeviation=0;
			_wprInd.ExtBackStep=3;
			
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.ForestGreen;
			vb = Tools.Create<VerticalLine>();
			vb.Color=Color.Blue;
			hy = Tools.Create<HorizontalLine>();
			hb = Tools.Create<HorizontalLine>();
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
		
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
			
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  { torg=false;
		    zzu5=0;zzu4=0;zzu3=0;zzu2=0;zzu1=0; 
			zzd5=0;zzd4=0;zzd3=0;zzd2=0;zzd1=0;
			  if (posGuidBuy!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty; }
			if (posGuidSell!=Guid.Empty) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  } 
		  if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Monday && Bars[Bars.Range.To-1].Time.Hour==01 )  torg=true;
		
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;

//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{   // ВВЕРХУ  
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					zzU=zzi2;zzu=zz2; torgU=true;
					vy.Time=Bars[zzU].Time;
					hy.Price=zzu;
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{   // ВНИЗУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					zzD=zzi2;zzd=zz2; torgD=true;
					vb.Time=Bars[zzD].Time;
					hb.Price=zzd;
				}
	
			}
//==========================================================================================================================
			
//			if(Bars[Bars.Range.To-1].Close>zzu && Bars[Bars.Range.To-2].Close>zzu) {
//			if(Bars[Bars.Range.To-1].Close>zzu && zzu2<zzu4 &&  zzu1>zzu3 && (zzu2-zzu3)*FB>(zzu2-zzu1) && (zzu2-zzu3)*0.236<(zzu2-zzu1)) {	// ВЕРХ
		    if(Bars[Bars.Range.To-1].Close>zzu-0.00020) {
			if(torg && torgU && posGuidBuy==Guid.Empty){ torgU=false;
				var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,600), null, null);
				if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;
				//var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(200,600), null, null);
				//if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;
							}}
//			if(Bars[Bars.Range.To-1].Close<zzd && Bars[Bars.Range.To-2].Close<zzd) {
//			if(Bars[Bars.Range.To-1].Close<zzd && zzu4<zzu2 && zzu3>zzu1 && (zzu3-zzu2)*FB>(zzu1-zzu2)  && (zzu3-zzu2)*0.236<(zzu1-zzu2)) { //  НИЗ	
			if(Bars[Bars.Range.To-1].Close<zzd+0.00020) {	
			if(torgD && torg && posGuidSell==Guid.Empty){ torgD=false;
				var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(200,600), null, null);
				if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;
				//var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,600), null, null);
				//if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;

					}}
			if(posGuidBuy!=Guid.Empty) ibuy++;
			if(posGuidSell!=Guid.Empty) isell++;
			if(ibuy>PR && posGuidBuy!=Guid.Empty) {var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) {posGuidBuy = Guid.Empty; ibuy=0;}}
			if(isell>PR && posGuidSell!=Guid.Empty) {var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) {posGuidSell = Guid.Empty; isell=0;}}
			
//===============================================================================================================================        			
        }

    }
}