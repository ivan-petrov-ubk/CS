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
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;		
		private ZigZag _wprInd3;
		public FisherTransformOscillator _ftoInd;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2,zzu7=2;
		private int zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7;
		private double zz1,zz2,zz3,zz4,zz5,zz6,zz7;
		private double zzf1,zzf2,zzf3,zzf4,zzf5,zzf6,zzf7;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,zzi7;
		private VerticalLine vy,vb;
		private bool torg=true;

	    private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		public string LogFileName = @"EUR_H1_Z3";
		
        protected override void Init()
        {    trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
			_wprInd3= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd3.ExtDepth=5;
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			vy = Tools.Create<VerticalLine>();
			vy.Color=Color.Red;	
			vb = Tools.Create<VerticalLine>();
			vb.Color=Color.Blue;	
        }        
//===============================================================================================================================
        protected override void NewBar()
        {	_wprInd3.ReInit();
//=== КОРЕКЦИЯ ===========================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) posGuidSell=Guid.Empty;  

			if ( Bars[Bars.Range.To-1].Time.Hour>6 &&  Bars[Bars.Range.To-1].Time.Hour<20 )  torg=true; else torg=false;
//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  }
			if( _ftoInd.FisherSeries[Bars.Range.To-2]>0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]<0) 
			{
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
			{var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
			{var res2 = Trade.CancelPendingPosition(posGuidBuy); if (res2.IsSuccessful) posGuidBuy = Guid.Empty;}
			}
		    if( _ftoInd.FisherSeries[Bars.Range.To-2]<0  &&  _ftoInd.FisherSeries[Bars.Range.To-1]>0) 
		    {
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
			{var res3 = Trade.CloseMarketPosition(posGuidSell); if (res3.IsSuccessful) posGuidSell = Guid.Empty;}	
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
			{var res4 = Trade.CancelPendingPosition(posGuidSell); if (res4.IsSuccessful) posGuidSell = Guid.Empty;}	
				
			}           			
		  
		  
			if( _wprInd3.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{    
				 zz3 =zz2; zz2 =zz1;   zz1 =_wprInd3.MainIndicatorSeries[Bars.Range.To-1];
				  zzi3=zzi2;	 zzi2=zzi1;  zzi1= Bars.Range.To-1;
				  
				
//====== ВВЕРХУ ПИК =====================================================================================================================
				if(zz3<zz2 && zz2>zz1)  
				{ 	// ВВЕРХУ
				zzd7=zzd6;zzd6=zzd5;	zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
				zzu7=zzu6;zzu6=zzu5;	zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
				zzf7=zzf6;zzf6=zzf5;	zzf5=zzf4; zzf4=zzf3; zzf3=zzf2; zzf2=zzf1; zzf1=_ftoInd.FisherSeries[zzi2];
					if( 
						zzu4>zzu6 && 
						zzu3>zzu5 && 
						zzu3>zzu1 && 
						zzu4>zzu2 &&
						zzf3>0 && zzf2<0 && zzf1<0
					   ) // ВВЕРХУ
						{	
							if(posGuidSell==Guid.Empty){  	
								var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid, -1, Stops.InPips(200,null), null, null);
								if (result3.IsSuccessful)  posGuidSell=result3.Position.Id;  } 
							
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Blue;
							toolPolyLine.Width=4;	

							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd6].Time, Bars[zzd6].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].High));
							//toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time, zz1));
							XXPrint("1;'{0}';{1};{2};{3};{4};{5};{6}",
							Bars[Bars.Range.To-1].Time,
							Bars[zzd1].High*100000,
							Bars[zzd2].Low*100000,
							Bars[zzd3].High*100000,
							Bars[zzd4].Low*100000,
							Bars[zzd5].High*100000,
							Bars[zzd6].Low*100000
							);
						}
				}				
//==== ВНИЗУ ПИК ======================================================================================================================				
				if(zz3>zz2 && zz2<zz1)  
				{ 	// ВНИЗУ
				zzd7=zzd6;zzd6=zzd5;	zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zzi2; 
				zzu7=zzu6;zzu6=zzu5;	zzu5=zzu4; zzu4=zzu3; zzu3=zzu2; zzu2=zzu1; zzu1=zz2;
				zzf7=zzf6;zzf6=zzf5;	zzf5=zzf4; zzf4=zzf3; zzf3=zzf2; zzf2=zzf1; zzf1=_ftoInd.FisherSeries[zzi2];
					
					if( zzu4<zzu6 && 
						zzu3<zzu5 && 
						zzu3<zzu1 && 
						zzu4<zzu2 &&
						zzf3<0 && zzf2>0 && zzf1>0
					  )	//  ВНИЗУ
						{	
							if(posGuidBuy==Guid.Empty){  	
								var result1 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,null), null, null);
								if (result1.IsSuccessful)  posGuidBuy=result1.Position.Id;  } 
							
							
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Red;
							toolPolyLine.Width=4;		

							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd6].Time, Bars[zzd6].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd5].Time, Bars[zzd5].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd4].Time, Bars[zzd4].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd3].Time, Bars[zzd3].Low));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd2].Time, Bars[zzd2].High));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzd1].Time, Bars[zzd1].Low));	
							//toolPolyLine.AddPoint(new ChartPoint(Bars[Bars.Range.To-1].Time, zz1));
							XXPrint("2;'{0}';{1};{2};{3};{4};{5};{6}",
							Bars[Bars.Range.To-1].Time,
							Bars[zzd1].High*100000,
							Bars[zzd2].Low*100000,
							Bars[zzd3].High*100000,
							Bars[zzd4].Low*100000,
							Bars[zzd5].High*100000,
							Bars[zzd6].Low*100000
							);
						}
				}
//=============================================================================================================================== 	
			}
        }
	protected void XXPrint(string xxformat, params object[] parameters)
		{var logString=string.Format(xxformat,parameters)+Environment.NewLine;
			File.AppendAllText(trueLogPath, logString);}
    }
}