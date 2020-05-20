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
    [TradeSystem("ТС Пробой Н1")]
    public class ТС_Пробой_Н1 : TradeSystem
    {   public double H1,L1,O1,C1,H2,L2,O2,C2,O3,C3,U1,D1;
		public HorizontalLine Line1,Line2,Line3;
		public VerticalLine V1,V2;
		public DateTime DTime,TmU,TmD; // Время
		public int Dt,BarU,BarD,LgUH,LgUL,LgDH,LgDL,Pr;
		public bool LogU=false,LogD=false;
		public double MaxUH,MaxUL,MaxDL,MaxDH;
		
		private static string PathToLogFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
		private string trueLogPath = "";
		public string LogFileName = @"L_1";
			
        protected override void Init()
        {	trueLogPath=PathToLogFile+"\\"+LogFileName+".LOG";
			Line1 = Tools.Create<HorizontalLine>();
			Line2 = Tools.Create<HorizontalLine>();
			V1 = Tools.Create<VerticalLine>();
			V1.Color=Color.Red; V1.Width=3;
			V2 = Tools.Create<VerticalLine>();
			V2.Color=Color.Blue; V2.Width=3;
			XXPrint("НИЗ ПАРА ДАТА ЧАС ДЕЛЬТА TP SL BarTP BarSL");
        }        

        protected override void NewBar()
        {	
			O1 = Bars[Bars.Range.To-1].Open;
			C1 = Bars[Bars.Range.To-1].Close;
			U1 =  Bars[Bars.Range.To-1].High;
			D1 =  Bars[Bars.Range.To-1].Low;
			DTime = Bars[Bars.Range.To-1].Time;
//=============================================================================================================================================		  			
// Максимальне та мінімальне знячення за день - на H1 - періоді..
			if ( DTime.Hour==23 && DTime.Minute==00 ) 
          { 	
         		var highestIndex = Series.Highest(Bars.Range.To, 24, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
			     	H1 = highestPrice;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 24, PriceMode.Low);
			    var lowestPrice = Bars[lowestIndex].Low;
			     	L1 = lowestPrice;
			        // Різниця в пунктах між MAX-MIN
			  		Dt=(int)Math.Round((H1-L1)*100000,0);
			  		// Малюємо горизонтальну лінію на рівнях
			  		Line1.Price = H1;
			  		Line1.Text = string.Format("{0}",Math.Round((H1-L1)*100000,0));
					Line2.Price = L1;
		  }
//== СТАТИСТИКА ================================================================================================================================		  
		  // Якщо ціна пересікла рівень знизу вверх - 
		  if(!LogU && U1>H1) {LogU=true; 
			     MaxUH=Bars[Bars.Range.To-1].High;
				 MaxUL=Bars[Bars.Range.To-1].Low;
			     TmU=Bars[Bars.Range.To-1].Time;
		         BarU=Bars.Range.To-1; 
		   		 V1.Time=TmU; 
		  }
		  // Якщо ціна пересікла рівень зверху вниз
		  if(!LogD && D1<L1) {LogD=true; 
				MaxDH=Bars[Bars.Range.To-1].High;
				MaxDL=Bars[Bars.Range.To-1].Low;
				TmD=Bars[Bars.Range.To-1].Time;
				BarD=Bars.Range.To-1;
		        V2.Time=TmD; 
		  }
		  // Визначення MAX MIN
		  if (LogU && Bars[Bars.Range.To-1].High>MaxUH) { MaxUH=Bars[Bars.Range.To-1].High; LgUH=Bars.Range.To-1-BarU;}
		  if (LogU && Bars[Bars.Range.To-1].Low<MaxUL)  { MaxUL=Bars[Bars.Range.To-1].Low;  LgUL=Bars.Range.To-1-BarU;}
		  if (LogD && Bars[Bars.Range.To-1].High>MaxDH) { MaxDH=Bars[Bars.Range.To-1].High; LgDH=Bars.Range.To-1-BarD;}
		  if (LogD && Bars[Bars.Range.To-1].Low<MaxDL)  { MaxDL=Bars[Bars.Range.To-1].Low;  LgDL=Bars.Range.To-1-BarD;}
		  
		  if ( DTime.Hour==00 && DTime.Minute==00 ) 
		  {
			  if (LogU) { LogU=false;
				  		  var TP=(int)Math.Round((MaxUH-H1)*100000,0);
						  var SL=(int)Math.Round((MaxUL-H1)*100000,0);	
				  if(SL>-200) Pr=TP; else Pr=-200;
				          XXPrint("ВЕРХ {0} {1} {2} {3} {4} {5} {6} {7} ",this.Instrument.Name,TmU,Dt,TP,SL,LgUH,LgUL,Pr);
				  			MaxUH=0;MaxUL=0;}
			  if (LogD) { LogD=false; 
				  		  var TP=(int)Math.Round((L1-MaxDL)*100000,0);
						  var SL=(int)Math.Round((L1-MaxDH)*100000,0);	
					if(SL>-200) Pr=TP; else Pr=-200;
				  		  XXPrint("НИЗ {0} {1} {2} {3} {4} {5} {6} {7}",this.Instrument.Name,TmD,Dt,TP,SL,LgDL,LgDH,Pr);
				  }
		  } 
        }

		protected void XXPrint(string xxformat, params object[] parameters)
		{var logString=string.Format(xxformat,parameters)+Environment.NewLine;
				File.AppendAllText(trueLogPath, logString);}
//==================================================================================================================================
    }
}