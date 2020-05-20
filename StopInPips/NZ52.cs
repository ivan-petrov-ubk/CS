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
    [TradeSystem("NKZ-123_V3")]           //copy of "NKZ-123_V2"
    public class ZZ_Ex1 : TradeSystem
    {
		
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }	
		[Parameter("TP :", DefaultValue = 100)]
		public int TP1 { get;set; }	
		[Parameter("StopLoss:", DefaultValue = 100)]
        public int SL1 { get; set; }	
		[Parameter("Отступ Stop :", DefaultValue = 10)]
		public int dl { get;set; }	
		[Parameter("Fractal", DefaultValue = 11)]
		public int frac { get;set; }	
	
		
		private int NKZ,i,mgS,mgB;
		private bool first,t1,t2,per;
		public int iFT=0,k=0,ks=0;
		private double nkz2,nkz4,nkz2v,nkz4v,kf,zmax,zmin;

		// private PolyLine toolPolyLine;
		private Rectangle toolRectangle;
		private Rectangle toolRectangle1;	
		private Rectangle toolRectangle2;
		
		private DateTime dt0; 
	
		public DateTime DTime; // Время
		private int ci = 0;
		private bool FsU,FsD,nu,nd;		
		private double dlt,frUp,frDown,frUp0,frDown0;
		public Fractals _frInd;		
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
		public double frU1,frU2,frU3,frU4,frU5;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3,frD4,frD5;    // Значение Low - свечи с верхним фрактклом
		public double fsU1,fsU2,fsU3,fsU4,fsU5;    // Значение Low - свечи с верхним фрактклом
		public double fsD1,fsD2,fsD3,fsD4,fsD5;    // Значение Low - свечи с верхним фрактклом
		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PCU,PCD,UBU,UBD,Ot;		
		public int PRDi,PRUi,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmU3,tmD1,tmD2,tmD3;
		public bool frU,frD;
		private VerticalLine vy,vb;		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();		
		private string trueLogPath = "";	

		public ISeries<Bar> _barM15;
		private int _lastIndexM15 = -1;
		public Period periodM15;	
		public FisherTransformOscillator _ftoInd,_ftoIndM15;		
		public double Fs,Fs1;
		public DateTime tmM15;		
		
        protected override void Init()
        {	
			dlt=dl*Instrument.Point; // Отступ от стопа
			InitLogFile();  // Запись логов

			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);	
			_frInd.Range=frac-2;
			
			periodM15 = new Period(PeriodType.Minute, 15);
			_barM15 = GetCustomSeries(Instrument.Id,periodM15);
			_ftoIndM15   = GetIndicator<FisherTransformOscillator>(Instrument.Id, periodM15); 		

			Fs=0;		Fs1=0;	
        }   
		
        protected override void NewQuote()
        {
			if (_lastIndexM15 < _barM15.Range.To-1) {     		    	
					Fs1=Fs; Fs =_ftoIndM15.FisherSeries[_barM15.Range.To-1];
					tmM15 = _barM15[_barM15.Range.To-1].Time;
					_lastIndexM15 = _barM15.Range.To-1;  
				} 
		}		
		
//===========================================================================
        protected override void NewBar()
        {   
			DTime = Bars[ci].Time;
			ci = Bars.Range.To - 1;
			
			if(Fs!=0 && Fs1!=0 &&  Fs>0) {tu=true; td=false;}
			if(Fs!=0 && Fs1!=0 &&  Fs<0) {tu=false; td=true;}
			
//====  Fractal ====================================================================================
			
			  if (_frInd.TopSeries[Bars.Range.To-frac]>0) 		{	
				  if(frU) { frU=false;		
				  frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-frac];
				  tmU3=tmU2; tmU2=tmU1; tmU1=Bars[Bars.Range.To-frac].Time;  
				  } else {
					  if(_frInd.TopSeries[Bars.Range.To-frac]>frU1) {
						  frU1=_frInd.TopSeries[Bars.Range.To-frac];
					  	  tmU1=Bars[Bars.Range.To-frac].Time; }
				  }
				  }
			  if (_frInd.BottomSeries[Bars.Range.To-frac]>0)   { 
				  if(!frU) { frU=true;
				  frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-frac];
				     tmD3=tmD2; tmD2=tmD1; tmD1=Bars[Bars.Range.To-frac].Time;
				  } else 
				  {
					 if (_frInd.BottomSeries[Bars.Range.To-frac]<frD1) {
						 frD1=_frInd.BottomSeries[Bars.Range.To-frac];
					     tmD1=Bars[Bars.Range.To-frac].Time;
					 } 
				  }
				  } 			
// Если пересечение зоны было (nu) и торгуем вверх (tu) ===========================================================		
			if(tu) 
			{
// Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВНИЗУ 
		   if (_frInd.TopSeries[Bars.Range.To-frac]>0)    
		   { 
           		if( frU3>frU2 && frU1>frU2 ) {
					var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[ci].Time; vl1.Color=Color.Aqua;vl1.Width=2;
					SLU=frD2-Instrument.Spread-dlt; SLU=Math.Round(SLU, Instrument.PriceScale);
					UBU=Math.Round((Bars[ci].Close-SLU)*Math.Pow(10,Instrument.PriceScale),0);
					PRUi=TP1;
					UBUi=(int)UBU;
					Print("{8} PATERN BUY -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRUi,UBUi,2,tmU1,tmU2,tmU3,3,Instrument.Name);
					//XXPrint("PATERN BUY -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRUi,UBUi,PCU,tmU1,tmU2,tmU3,Ot);
					if(UBU<SL1) {
				    //XXPrint("BUY -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRU,UBU,PCU,tmU1,tmU2,tmU3,Ot);
                    Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,Stops.InPips(UBUi,PRUi), null, null);
					frD=false;}
				}  
		   }
		}
// Если пересечение зоны было (nd) и торгуем вниз (td) ===========================================================				
			if(td) 
			{
// Определение патерна   ПИК ПОСЛЕДНЕГО ФРАКТАЛА ПАТЕРНА ВВЕРХУ 
 			if (_frInd.BottomSeries[Bars.Range.To-frac]>0)	   
			{ 
				if( frD2>frD3 && frD2>frD1  ) 
				{ var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[ci].Time; vl1.Color=Color.Aqua;vl1.Width=2;
					
					SLD=frU2+Instrument.Spread+dlt; SLD=Math.Round(SLD, Instrument.PriceScale);

					UBD=Math.Round((SLD-Bars[ci].Close)*Math.Pow(10,Instrument.PriceScale),0);
					
					PRDi=TP1;
					UBDi=(int)UBD;

					Print("{8}  PATERN SELL -- {0} -- TP(п)={1} SL(p)={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6}  Ot={7}",Bars[ci].Time,PRDi,UBDi,2,tmD1,tmD2,tmD3,3,Instrument.Name);
					//XXPrint("PATERN SELL -- {0} -- TP(п)={1} SL(p)={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6}  Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot);				
					if(UBD<SL1) {
					//XXPrint("SELL -- {0} -- TP={1} SL={2} TP/SL={3} Fract 1-{4} 2-{5} 3-{6} Ot={7}",Bars[ci].Time,PRDi,UBDi,PCD,tmD1,tmD2,tmD3,Ot);
					Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi), null, null);
					frU=false; }
				} 
			}
			}
			
			}
			        
//===============================================================================================================================   

		protected void InitLogFile()
		{trueLogPath=PathToLogFile+"\\"+Instrument.Name.ToString()+".log";}
		
		protected void XXPrint(string xxformat, params object[] parameters)
		{
				var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);
		}
    }
}