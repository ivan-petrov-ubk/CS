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
    [TradeSystem("Fisher_ZZM15M5")]    //copy of "Fisher_H4H1_M15"
    public class ZZ_Ex1 : TradeSystem
    {
		/*[Parameter("Fisher H4=", DefaultValue = true)]
        public bool F4U { get; set; }
		[Parameter("Fisher M15_D=", DefaultValue = true)]
        public bool F15D { get; set; }
		[Parameter("Fisher M15_U=", DefaultValue = true)]
        public bool F15U { get; set; }		*/
		
		public DateTime tmM15,tmU,tmD;
		public bool F15U,F15D;
		public ISeries<Bar> _barM15;
		public Period periodM15;		
		private int _lastIndexM15 = -1;	
		public FisherTransformOscillator _ftoIndM15;
		//public FisherTransformOscillator _ftoIndM5 ;
				private ZigZag _wprInd;
		//		public Fractals _frInd;
	
		public double Fs,Fs1,Fh,Fh1,F4,F41;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		private double zz1=2,zz2=2,zz3=2;
		private double zztb2,zzts2;
		private double zzu1=2,zzu2=2,zzu3=2,zzu4=2,zzu5=2,zzu6=2;
		private double zzd1,zzd2,zzd3,zzd4,zzd5,zzd6;
		private int tp1,sl1;
		private int zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,V=0,ku=0,kd=0,FU,FD;
		
		private int ci;
		private int zi1,zi2,zi3,zi4,zi5;		

				private bool PrD=true,PrU=true,isF,isFU=true,isFD=false,isFMU=true,isFMD=false,isFM2U=true,isFM2D=false;
		
        protected override void Init()
        {
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15);
			// _ftoIndM5    = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=7;
        }        
		

        protected override void NewQuote()
        {
				if (_lastIndexM15 < _barM15.Range.To-1) {     		    	
					Fs =_ftoIndM15.FisherSeries[_barM15.Range.To];
					Fs1=_ftoIndM15.FisherSeries[_barM15.Range.To-1];
					tmM15=_barM15[_barM15.Range.To-1].Time;
					if(_lastIndexM15>0 && Fs>0 && Fs1<0)  { F15U=true;  tmU=tmM15; } 
					if(_lastIndexM15>0 && Fs<0 && Fs1>0)  { F15D=true;  tmD=tmM15; }
					_lastIndexM15 = _barM15.Range.To-1;  
				}

        }
//=====================================================================================================================================
        protected override void NewBar()
        {   _wprInd.ReInit();
 			ci = Bars.Range.To - 1;

//=== КОРЕКЦИЯ ========================================================================================================================							 
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) 	posGuidBuy=Guid.Empty;   
		    if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed) 	posGuidSell=Guid.Empty;  
			
//=== Закрытие всех ордеров если пятница 16:00 (19:00 Kiev) ===========================================================================
          if ( Bars[Bars.Range.To-1].Time.DayOfWeek==DayOfWeek.Friday && Bars[Bars.Range.To-1].Time.Hour==16 ) 
		  {  
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Active) 
					{ var res = Trade.CloseMarketPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Active) 
					{ var res = Trade.CloseMarketPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
			
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Pending) 
					{ var res = Trade.CancelPendingPosition(posGuidBuy); if (res.IsSuccessful) posGuidBuy = Guid.Empty;}
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Pending) 
					{ var res = Trade.CancelPendingPosition(posGuidSell); if (res.IsSuccessful) posGuidSell = Guid.Empty;}	
		  }
		
//===============================================================================================================================		  
			if( _wprInd.MainIndicatorSeries[ci]>0) 
			{    // Значения 3 значений - для определения направления
				 zz3 =zz2;	 zz2 =zz1;   zz1 =_wprInd.MainIndicatorSeries[ci];
				 zi2=zi1;  zi1=ci;
				//Print("{0} - {1} {2}",Bars[ci].Time,zz1<zz2,zz3<zz2);
//======================= ПИК 
				if(zz2<zz3 && zz2<zz1) // ВНИЗУ
				{   zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; 
					zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zi2;
					//Print("U {0} - {1} - {2} - {3} - {4}",Bars[ci].Time,tmM15,F15U,Bars[zzi1].Time,Bars[zzi2].Time);
					// if(F15U>Bars[zzi2].Time && F15U<Bars[zzi1].Time) { 
					
					if(F15U) { F15U=false;
						tp1=(int)(Math.Round(Math.Abs(zzd3-zzd2),5)*100000)-(int)(Math.Round(Math.Abs(zzd1-Bars[ci].Close),5)*100000);
						sl1=(int)(Math.Round(Math.Abs(zzd1-Bars[ci].Close),5)*100000);
						if(tp1>100) {
						Print("U {0} - {1} - {2}",Bars[ci].Time,tp1,tmM15);	
					var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Red; vr.Time=Bars[zzi2].Time;
					
									var vr1=Tools.Create<VerticalLine>(); vr1.Color=Color.DarkOrange; vr1.Time=tmU; vr1.Width=4;
							
				if(posGuidBuy==Guid.Empty){  	
				var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Ask,-1,Stops.InPips(sl1+50,tp1-20),null);
				if (result.IsSuccessful) posGuidBuy=result.Position.Id; 
						} 
						
							
					}}
				}
				if(zz2>zz3 && zz2>zz1) // ВВЕРХУ
				{   zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; 
					zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zi2; 
					
					// if(F15D>Bars[zzi2].Time && F15D<Bars[zzi1].Time) { 
					if(F15D) { F15D=false;
					tp1= (int)(Math.Round(Math.Abs(zzd3-zzd2),5)*100000)-(int)(Math.Round(Math.Abs(zzd1-Bars[ci].Close),5)*100000);
						sl1= (int)(Math.Round(Math.Abs(zzd1-Bars[ci].Close),5)*100000);
						
						if(tp1>100) {
					Print("D {0} - {1} - {2}",Bars[ci].Time,tp1,tmM15);	
					var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[zzi2].Time;
						var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.DarkOrange; vr2.Time=tmD; vr2.Width=4;
							
			/*	if(posGuidSell==Guid.Empty){
		   		var result=Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Bid,-1,Stops.InPips(sl1+50,tp1-20),null);
				if (result.IsSuccessful) posGuidSell=result.Position.Id; 
						} */
						}
					}
				}
			}			
//===============================================================================================================================        
    	}
}
}