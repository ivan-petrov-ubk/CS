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
    [TradeSystem("Patrn123_Fractal")]
    public class Patrn123_Fractal : TradeSystem
    {
        // Simple parameter example
        [Parameter("Proect", DefaultValue = "04_08_2018")]
        public string CommentText { get; set; }
 		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
				
		
		public Fractals _frInd;
		private FisherTransformOscillator _ftoInd;		
		public double frU1,frU2,frU3,frU4,frU5;   // Значение текущего верхнего Fractal
		public double frD1,frD2,frD3,frD4,frD5;    // Значение Low - свечи с верхним фрактклом
		public double fsU1,fsU2,fsU3,fsU4,fsU5;    // Значение Low - свечи с верхним фрактклом
		public double fsD1,fsD2,fsD3,fsD4,fsD5;    // Значение Low - свечи с верхним фрактклом
		public double TPD,TPU,SLU,SLD;
		public double PRD,PRU,PCU,PCD,UBU,UBD;		
		public int PRDi,PRUi,UBDi,UBUi;
		public DateTime tmU1,tmU2,tmD1,tmD2;
		public bool frU,frD;

        
		protected override void Init()
        {
				_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
				_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
        }        

         
        protected override void NewBar()
        {
			// =========================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			//var vr=Tools.Create<VerticalLine>(); vr.Color=Color.Blue; vr.Time=Bars[Bars.Range.To-5].Time;
			//ПИК ВВЕРХУ
			if (_frInd.TopSeries[Bars.Range.To-5]>0)	   { 
				frU5=frU4; frU4=frU3; frU3=frU2; frU2=frU1; frU1=_frInd.TopSeries[Bars.Range.To-5];
				fsU5=fsU4; fsU4=fsU3; fsU3=fsU2; fsU2=fsU1; fsU1=_ftoInd.FisherSeries[Bars.Range.To-5];
           		if( frD2>frD3 && frD2>frD1 && fsU2>0 && fsD1<0 && fsU1<0 ) 
				{ 
					TPD=frU1-((frU2-frD1)*1.618); TPD=Math.Round(TPD, Instrument.PriceScale);
					SLD=frU2+Instrument.Spread; SLD=Math.Round(SLD, Instrument.PriceScale);
					PRD=Math.Round((Bars[Bars.Range.To-1].Close-TPD)*Instrument.LotSize,0);
					PRDi=(int)PRD;
					UBDi=(int)UBD;
					UBD=Math.Round((SLD-Bars[Bars.Range.To-1].Close)*Instrument.LotSize,0);
					PCD=Math.Round(PRD/UBD, 2);
					Print("SELL -- {0} -- PRD={1} UBD={2} PCD={3}",Bars[Bars.Range.To-1].Time,PRD,UBD,PCD);
					if(UBD>20 && UBD<200 && PCD>1) {
					Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,	Stops.InPips(UBDi,PRDi), null, null);    						
									
				    frU=false; var vr1=Tools.Create<VerticalLine>(); vr1.Color=Color.Blue; vr1.Time=Bars[Bars.Range.To-1].Time;vr1.Width=4;}
					else { var vr1=Tools.Create<VerticalLine>(); vr1.Color=Color.Blue; vr1.Time=Bars[Bars.Range.To-1].Time;}} 
			}
		   // ПИК ВНИЗУ
		   if (_frInd.BottomSeries[Bars.Range.To-5]>0)    
		   { 
			    frD5=frD4; frD4=frD3; frD3=frD2; frD2=frD1; frD1=_frInd.BottomSeries[Bars.Range.To-5];  
				fsD5=fsD4; fsD4=fsD3; fsD3=fsD2; fsD2=fsD1; fsD1=_ftoInd.FisherSeries[Bars.Range.To-5];
           		if( frU3>frU2 && frU1>frU2 && fsD2<0 && fsU1>0 && fsD1>0 ) {

					TPU=frD1+((frU1-frD2)*1.618); TPU=Math.Round(TPU, Instrument.PriceScale);
					SLU=frD2-Instrument.Spread; SLU=Math.Round(SLU, Instrument.PriceScale);
					PRU=Math.Round((TPU-Bars[Bars.Range.To-1].Close)*Instrument.LotSize,0);
					UBU=Math.Round((Bars[Bars.Range.To-1].Close-SLU)*Instrument.LotSize,0);
					PCU=Math.Round(PRU/UBU, 2);
					Print("BUY -- {0} -- PRU={1} UBU={2} PCU={3}",Bars[Bars.Range.To-1].Time,PRU,UBU,PCU);
					PRUi=(int)PRU;
					UBUi=(int)UBU;
					if(UBU>20 && UBU<200 && PCU>1) {
                    Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy,0.1,Instrument.Bid, -1,
								Stops.InPips(UBUi,PRUi), null, null);
														
			    	frD=false; var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.Red; vr2.Time=Bars[Bars.Range.To-1].Time;vr2.Width=4;}
				else 	{frD=false; var vr2=Tools.Create<VerticalLine>(); vr2.Color=Color.Red; vr2.Time=Bars[Bars.Range.To-1].Time; }
				}  
		   }

		   
		}
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
    }
}