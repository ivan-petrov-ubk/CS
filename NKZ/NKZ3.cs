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
    [TradeSystem("NKZ3")]
    public class NKZ3 : TradeSystem
    {
        // Simple parameter example
        [Parameter("Версия ", DefaultValue = "18")]
        public string CommentText { get; set; }
		[Parameter("Время 9:01 :")]
        public DateTime dt1 { get; set; }
		[Parameter("Buy:", DefaultValue = false)]
        public bool tu { get; set; }	
		[Parameter("Sell:", DefaultValue = false)]
        public bool td { get; set; }
		[Parameter("SOUND:", DefaultValue = false)]
        public bool su { get; set; }
		[Parameter("ExtDepth:", DefaultValue = 7)]
        public int dept { get; set; }
		
		private int NKZ,k=0;	
		double MAX = double.MinValue;		
		double MIN = double.MaxValue;
		private Rectangle toolRectangle,toolRectangle1;
		private DateTime MinTime,MaxTime,RTime;
		public int iFT=0;		
		private double nkz2,nkz4,nkz2v,nkz4v,kf=0.090909,zmax,zmin;
		private double zzd1,zzd2,zzd3,zzd4,zzd5,zzd6,zzd7;
		private int    zzi1,zzi2,zzi3,zzi4,zzi5,zzi6,zzi7;	
		
		private ZigZag _wprInd;
		private double zz1,zz2,zz3,zz4,zz5;
		private int zi1,zi2,zi3,zi4,zi5;
		private VerticalLine vl;	
		private bool first;
		
        protected override void Init()
        {
			_wprInd= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd.ExtDepth=dept;
			
			if (Instrument.Name == "EURUSD") { NKZ=506;  }
			if (Instrument.Name == "GBPUSD") { NKZ=792;  }
			if (Instrument.Name == "AUDUSD") { NKZ=343;  }
			if (Instrument.Name == "NZDUSD") { NKZ=357;  }
			if (Instrument.Name == "USDJPY") { NKZ=539;  }
			if (Instrument.Name == "USDCAD") { NKZ=573;  }
			if (Instrument.Name == "USDCHF") { NKZ=603;  }
			if (Instrument.Name == "AUDJPY") { NKZ=550;  }
			if (Instrument.Name == "AUDNZD") { NKZ=412;  }
			if (Instrument.Name == "CHFJPY") { NKZ=1430; }
			if (Instrument.Name == "EURAUD") { NKZ=682;  }
			if (Instrument.Name == "AUDCAD") { NKZ=357;  }
			if (Instrument.Name == "EURCAD") { NKZ=762;  }
			if (Instrument.Name == "EURCHF") { NKZ=627;  }
			if (Instrument.Name == "EURGBP") { NKZ=484;  }
			if (Instrument.Name == "EURJPY") { NKZ=781;  }
			if (Instrument.Name == "GBPCHF") { NKZ=924;  }
			if (Instrument.Name == "GBPJPY") { NKZ=1045; }
			
			toolRectangle = Tools.Create<Rectangle>(); 
			toolRectangle.BorderColor=Color.Aqua; 
			toolRectangle.Color=Color.DarkSeaGreen;
			
			toolRectangle1 = Tools.Create<Rectangle>(); 
			toolRectangle1.BorderColor=Color.Aqua; 
			toolRectangle1.Color=Color.DarkSeaGreen;
			k=0; first=true;
        }        

        
        protected override void NewBar()
        {   _wprInd.ReInit();

		
					
			if(first) 
			{   first=false;
				Print("First1");
			    dt1=dt1.AddHours(-3);
				iFT = TimeToIndex(dt1, Timeframe);
				MAX = Bars[iFT].High;
			    MIN = Bars[iFT].Low;
			    MaxTime=dt1;
				MinTime=dt1;
				vl.Time=dt1; 
				//Print("First - RTime={0} Max={1} Min={2} iFT={3}",RTime,MAX,MIN,iFT);
				
			} else {
			
				//Print("NOT First !!!!!  - RTime={0} Max={1} Min={2} iFT={3}",RTime,MAX,MIN,iFT);
			if(Bars[Bars.Range.To-1].High > MAX) { MAX = Bars[Bars.Range.To-1].High; MaxTime = Bars[Bars.Range.To-1].Time; }
			if(Bars[Bars.Range.To-1].Low < MIN) { MIN = Bars[Bars.Range.To-1].Low; MinTime = Bars[Bars.Range.To-1].Time; }
			}
		
			if(tu) { zmax = MAX; 
				//Print("tu=true  - RTime={0} Max={1} Min={2} iFT={3}",RTime,MAX,MIN,iFT);
					 RTime=MaxTime;
				     nkz4 = zmax-((NKZ-5-(NKZ*kf))*Instrument.Point); 
				     nkz2 = zmax-(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
			         nkz4v= zmax-((NKZ-5)*Instrument.Point); 
				     nkz2v= zmax-((NKZ*2)*Instrument.Point);
					 nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale); 
				
                   }
			
			if(td) { zmin = MIN;
				//Print("td=true  - RTime={0} Max={1} Min={2} iFT={3}",RTime,MAX,MIN,iFT);
				    RTime=MinTime;
				    nkz4 = zmin+((NKZ-5-(NKZ*kf))*Instrument.Point);  
				    nkz2 = zmin+(((NKZ*2)-(NKZ*2*kf))*Instrument.Point);
					 nkz4v= zmin+((NKZ-5)*Instrument.Point);  
				    nkz2v= zmin+(((NKZ*2))*Instrument.Point);
					nkz4=Math.Round(nkz4,Instrument.PriceScale);  
					nkz2=Math.Round(nkz2,Instrument.PriceScale);  
					nkz4v=Math.Round(nkz4v,Instrument.PriceScale);  
					nkz2v=Math.Round(nkz2v,Instrument.PriceScale);  
  
			       }
			
			//Print("Rtime={0} nkz4={1} nkz4v={2} tu={3} td={4} k={5} zmax={6} zmin={7}",RTime,nkz4,nkz4v,tu,td,k,zmax,zmin);
			
        	toolRectangle.Point1=new ChartPoint(RTime, nkz4);
          	toolRectangle.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(4).AddHours(12), nkz4v);
			
			toolRectangle1.Point1=new ChartPoint(RTime, nkz2);
          	toolRectangle1.Point2=new ChartPoint(Bars[Bars.Range.To-1].Time.AddHours(4), nkz2v);
			//vl.Time=RTime; 
			
		
//======================================================================================================================================
			if( _wprInd.MainIndicatorSeries[Bars.Range.To-1]>0) 
			{   Print("{0} zz1={1} zz2={2} zz3={3}",Bars[Bars.Range.To-1].Time,zz1,zz2,zz3);
				 zz3 = zz2;	 zz2 = zz1;   zz1 = _wprInd.MainIndicatorSeries[Bars.Range.To-1];
				 zi3 = zi2;	 zi2 = zi1;   zi1 = Bars.Range.To-1;
				 
				if(zz3<zz2 && zz2>zz1)  
				{ // ВВЕРХУ
					
					zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; 
					zzi7=zzi6; zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zi2; 
	//var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[zzi1].Time; vl1.Color=Color.Red;
					//if( zzd4>zzd6 && zzd4>zzd2 && zzd3>zzd1  && (zzd3-zzd2)*0.5+zzd2<zzd1  ) // ВВЕРХУ  && (zzd3-zzd2)*0.5+zzd2<zzd1 && nkz4<zzd5 && td 
					if( zzd3<zzd5 && zzd3<zzd1  && zzd4<zzd6 && nkz4>zzd4 && tu )	
					{
 							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.Aqua;
							toolPolyLine.Width=4;	
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi6].Time, zzd6));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, zzd5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, zzd4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, zzd3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, zzd2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, zzd1));	
							var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[zzi4].Time; vl1.Color=Color.Red;
						}
				}				
				
				if(zz3>zz2 && zz2<zz1)  
				{ // ВНИЗУ
					zzd7=zzd6; zzd6=zzd5; zzd5=zzd4; zzd4=zzd3; zzd3=zzd2; zzd2=zzd1; zzd1=zz2; 
					zzi7=zzi6; zzi6=zzi5; zzi5=zzi4; zzi4=zzi3; zzi3=zzi2; zzi2=zzi1; zzi1=zi2; 
//var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[zzi1].Time; vl1.Color=Color.Blue;
					//if( zzd4<zzd6 && zzd4<zzd2 && zzd3<zzd1   && (zzd2-zzd3)*0.5+zzd3>zzd1 ) //  ВНИЗУ  && (zzd2-zzd3)*0.5+zzd3>zzd1 && nkz4>zzd5 && tu
					if( zzd3>zzd5 && zzd3>zzd1 && zzd4>zzd6 && nkz4<zzd4 && td )
					{
							var toolPolyLine = Tools.Create<PolyLine>();
							toolPolyLine.Color=Color.White;
							toolPolyLine.Width=4;		
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi6].Time, zzd6));
		 					toolPolyLine.AddPoint(new ChartPoint(Bars[zzi5].Time, zzd5));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi4].Time, zzd4));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi3].Time, zzd3));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi2].Time, zzd2));
							toolPolyLine.AddPoint(new ChartPoint(Bars[zzi1].Time, zzd1)); 	
							var vl1 = Tools.Create<VerticalLine>(); vl1.Time=Bars[zzi4].Time; vl1.Color=Color.Blue;
						}
				}

			
			}
			
		}	
        			
			
              
    }
}