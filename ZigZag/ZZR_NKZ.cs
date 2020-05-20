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
    [TradeSystem("Z")]         //copy of "ZZR_V4"
    public class ZZ_Ex1 : TradeSystem
    {
		
		private ZigZag _wprInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2,korU,korD;
		private double zztb2,zzts2,zzb,zzs;
		private double z50_1,z50_2,z50_3,stML,sprd;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5;
		private int zzi1,zzi2,zzi3,zzi4,V=0;
		private VerticalLine vy,vb;
		private int ibuy,isell;
		private bool torg=true,torg2=false, torgU=true,torgD=true;
		private double sv=0,av1=0,av2=0,av3=0,av4=0,av5=0;
		private Text toolText;
		public FisherTransformOscillator _ftoInd;	
		private bool FsU,FsD,nu,nd;	
		private double fr,fr5,fr4,fr3,fr2,fr1;
		
        protected override void Init()
        {
			
			zzd5=0; zzd4=0; zzd3=0; zzd2=0; zzd1=0;
			
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=5;
			_wprInd.ExtDeviation=0;
			_wprInd.ExtBackStep=3;
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);			
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Yellow;
			vb = Tools.Create<VerticalLine>();
			vb.Color=Color.Blue;

			korU=0.00020;
			korD=0.00020;
			
		toolText = Tools.Create<Text>();
		toolText.Color=Color.Blue;	 
		toolText.Style = TextStyle.Italic;			 
		toolText.FontSize=12;	 
			
        }        
//===============================================================================================================================
        protected override void NewBar()
        {
 			_wprInd.ReInit();
			sprd=Instrument.Spread;
			 var spr=Math.Round(Instrument.Spread*100000,0)+16;
			fr=_ftoInd.FisherSeries[Bars.Range.To-1];
			
		//toolText.Point=new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].High);
        //toolText.Caption=string.Format("Spred={0}",spr);
		
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  
			
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    if(zzd3!=0 && zzd2!=0 && zzd1!=0) torg2=true; else torg2=false;
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;

//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
					fr5=fr4;   fr4=fr3;   fr3=fr2;   fr2=fr1;   fr1=fr;
					
			    	vy.Time=Bars[Bars.Range.To-1].Time; 
					 // ВВЕРХУ - но тренд ВНИЗ
					Print("ВВЕРХУ {0} - {1}<{2}={3} [{4}]",Bars[Bars.Range.To-1].Time,zzu1,zzu3,zzu3<zzu1,torg2);
							if(zzu3>zzu1 && zzu3>zzu5 && zzu4>zzu2 && fr1<-0.3 && fr2<0.3 && fr3>0.3 && fr4>0.3 && fr5>0.3 && torg2) 
							{
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.SpringGreen;
							toolPolyLine.Width=4;
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].High));									
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].Low));	
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].High));
							//toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Close));
					if (posGuidSell==Guid.Empty) {		
							 var result207 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,
					 		                 Stops.InPips(100,100),null,null); 
						     if (result207.IsSuccessful)  posGuidSell=result207.Position.Id;
						}
						    } 
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2;
					zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2; 
					fr5=fr4;   fr4=fr3;   fr3=fr2;   fr2=fr1;   fr1=fr;
					vb.Time=Bars[Bars.Range.To-1].Time;
					Print("ВНИЗУ {0} - {1}<{2} = {3} [{4}]",Bars[Bars.Range.To-1].Time,zzu1,zzu3,zzu3>zzu1,torg2);
							if(zzu3<zzu1 && zzu3<zzu5 && zzu2>zzu4 && fr1>0.3 && fr2>0.3 && fr3<-0.3 && fr4<-0.3 && fr5<-0.3 && torg2 ) 
							{	
								
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;		
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].Low));	
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].High));								
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].Low));
							//toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time, Bars[Bars.Range.To-1].Close));
					if (posGuidBuy==Guid.Empty) { 		
						     var result107 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								             Stops.InPips(100,100),null,null);
						     if (result107.IsSuccessful)  posGuidBuy=result107.Position.Id;
						} 
									
							} 
				}
	
			}
        }
//===============================================================================================================================        
    }
}