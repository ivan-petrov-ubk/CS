using System;
using IPro.Model.MarketData;
using IPro.Model.Trade;
using IPro.Model.Client.Trade;
using IPro.Model.Programming;
using IPro.Model.Programming.Indicators;
using IPro.Model.Programming.Chart;
using IPro.Model.Programming.TradeSystems;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming.Indicators.Standard;
using System.Collections.Generic;

namespace IPro.TradeSystems
{
    [TradeSystem("Wiliams_Svechi")]
    public class Wiliams_Svechi : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		
     	[Parameter("LR_Use_moving_average_1", DefaultValue = true)]
        public bool UseFirstMA { get; set; }
        [Parameter("LR_Period", DefaultValue = 8, MinValue = 1)]
        public int PeriodMA1 { get; set; }
        [Parameter("LR_Shift", DefaultValue = 0, MinValue = -1000, MaxValue = 1000)]
		public int ShiftMA1 { get; set; }
        [Parameter("LR_Method", DefaultValue = MaMethods.Ema)]
		public MaMethods MethodMA1 { get; set; }
        [Parameter("LR_Apply_to", DefaultValue = PriceMode.Close)]
        public PriceMode ApplyMA1To { get; set; }

        [Parameter("LR_Use_moving_average_2", DefaultValue = true)]
		public bool UseSecondMA { get; set; }
        [Parameter("LR_Period", DefaultValue = 21, MinValue = 1)]
        public int PeriodMA2 { get; set; }
        [Parameter("LR_Shift", DefaultValue = 0, MinValue = -1000, MaxValue = 1000)]
		public int ShiftMA2 { get; set; }
        [Parameter("LR_Method", DefaultValue = MaMethods.Ema)]
		public MaMethods MethodMA2 { get; set; }
        [Parameter("LR_Apply_to", DefaultValue = PriceMode.Close)]
        public PriceMode ApplyMA2To { get; set; }
		
        		// Линии показывают АКТИВНЫЕ точки
		private Guid _positionGuidB=Guid.Empty;
		private Guid _positionGuidS=Guid.Empty;
		private VerticalLine vlR,vlB,vlG,vlY;
		private HorizontalLine hline;
		private HorizontalLine lline;
		double BarH1,BarL1,BarC1,BarO1; 
		double BarH2,BarL2,BarC2,BarO2; 
		double BarH3,BarL3,BarC3,BarO3; 
		double Z1,Z3,Body,MaxB;
		public DateTime DTime1,DTime2,DTime3;
		public int k;
		private MovingAverage _ma1;
		private MovingAverage _ma2;
		private Period _period = new Period(PeriodType.Minute, 1);
		public double _ma1Value;
		public double _ma2Value;
		public bool Bar3B,Bar3S; 
		
		
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			vlR = Tools.Create<VerticalLine>();
			vlR.Color=Color.Red;
			vlB = Tools.Create<VerticalLine>();
			vlB.Color=Color.Blue;
			 _period=Timeframe;
				_ma1 = GetIndicator<MovingAverage>(Instrument.Id, _period, PeriodMA1, ShiftMA1, MethodMA1, ApplyMA1To);
            	_ma2 = GetIndicator<MovingAverage>(Instrument.Id, _period, PeriodMA2, ShiftMA2, MethodMA2, ApplyMA2To);
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
				// Значения текущего Бара
			BarH1 = Bars[Bars.Range.To-1].High;
			BarL1 = Bars[Bars.Range.To-1].Low;
			BarC1 = Bars[Bars.Range.To-1].Close;
			BarO1 = Bars[Bars.Range.To-1].Open;
			DTime1 = Bars[Bars.Range.To-1].Time;
			
			BarH2 = Bars[Bars.Range.To-2].High;
			BarL2 = Bars[Bars.Range.To-2].Low;
			BarC2 = Bars[Bars.Range.To-2].Close;
			BarO2 = Bars[Bars.Range.To-2].Open;
			DTime2 = Bars[Bars.Range.To-2].Time;
			
			BarH3 = Bars[Bars.Range.To-3].High;
			BarL3 = Bars[Bars.Range.To-3].Low;
			BarC3 = Bars[Bars.Range.To-3].Close;
			BarO3 = Bars[Bars.Range.To-3].Open;
			DTime3 = Bars[Bars.Range.To-3].Time;
			
			_ma1Value = _ma1.SeriesMa[Bars.Range.To-1];
			_ma2Value = _ma2.SeriesMa[Bars.Range.To-1];
			// Корекция
			if (_positionGuidB!=Guid.Empty && Trade.GetPosition(_positionGuidB).State==PositionState.Closed) _positionGuidB=Guid.Empty; 
			if (_positionGuidS!=Guid.Empty && Trade.GetPosition(_positionGuidS).State==PositionState.Closed) _positionGuidS=Guid.Empty; 
			// Определяем максимальный размер тела 30 свечок
			MaxB=0;
			for (int i=1; i<30; i++) {  Body=Bars[Bars.Range.To-i].High-Bars[Bars.Range.To-i].Low; 
				  if (Body>MaxB) MaxB=Body; }
			
			//if(BarC>BarO &&	BarC>BarH-((BarH-BarL)/3) &&  BarO<BarL+((BarH-BarL)/3)) vlR.Time=Bars[Bars.Range.To-1].Time;
			//if(BarO>BarC &&	BarO>BarH-((BarH-BarL)/3) &&  BarC<BarL+((BarH-BarL)/3)) vlB.Time=Bars[Bars.Range.To-1].Time;
			//Z1 = BarL+((BarH-BarL)/3);
			//Z3 = BarH-((BarH-BarL)/3);
			
			
			//if(BarC>BarO &&	(BarC-BarO)>MaxB/2) { vlR.Time=Bars[Bars.Range.To-1].Time; Print("S1 - {0} - {1}> {2}",BarH,BarL,MaxB/2); }
			//if(BarC<BarO &&	(BarO-BarC)>MaxB/2) { vlB.Time=Bars[Bars.Range.To-1].Time; Print("S2 - {0} - {1}> {2}",BarH,BarL,MaxB/2); }
			
			// определяем патерн свечей "Поглощение" && BarC2<BarO && BarO2>BarC    && BarC2>BarO && BarO2<BarC
			// тренд ВНИЗ!
// ===================================================================================================================================
			// BarC3>BarO3 - свеча бычья  (BarO3-BarC3)>MaxB/2 - Величиной больше половины максимальной с 30 пред
			// BarO3<(BarL3+((BarH3-BarL3)/3)) -  Открытие свечи в области 1 
			// BarC3>(BarH3-((BarH3-BarL3)/3)) -  Закрытие в области 3
			if( BarC3>BarO3 && (BarC3-BarO3)>MaxB/3 && BarO3<(BarL3+((BarH3-BarL3)/3)) && BarC3>(BarH3-((BarH3-BarL3)/3)) ) 
			{// vlR.Time=DTime3;  
			    Bar3B=true; }   else Bar3B=false;
			if( BarO3>BarC3 && (BarO3-BarC3)>MaxB/3 && BarO3>(BarH3-((BarH3-BarL3)/3)) && BarC3<(BarL3+((BarH3-BarL3)/3)) ) 
			{    // vlB.Time=DTime3;
			     Bar3S=true; } else Bar3S=false;
			
// ===================================================================================================================================			
			// ВНИЗ
			if( BarO2>BarC2 && (BarH2-BarL2)>MaxB/3 && (BarO2-BarC2)*3<(BarH2-BarL2) && BarO2<(BarL2+(BarH2-BarL2)/2) && _ma2Value>_ma1Value)
					{ vlR.Time=DTime2;
				if(_positionGuid!=Guid.Empty) { ClosePosition(); k=0; }
					 Sell(); 
					}
			if( BarC2>BarO2 && (BarH2-BarL2)>MaxB/3 && (BarC2-BarO2)*3<(BarH2-BarL2) && BarC2>(BarL2+(BarH2-BarL2)/2) && _ma1Value>_ma2Value)
			   { vlB.Time=DTime2;	
			 if(_positionGuid!=Guid.Empty) { ClosePosition(); k=0; }
					 Buy(); 
				}
			
			//if(BarC3>BarO3 && BarC2<BarO2 && (BarC3-BarO3)>MaxB/2 && (BarO2-BarC2)>MaxB/2 && BarC<(BarL2+(BarH2-BarL2)/2) && BarC2<BarO3) 
			  //   { vlR.Time=DTime;  
					 //if(_positionGuid!=Guid.Empty) { ClosePosition(); k=0; }
					 //Sell(); 
				// }
				 
			//if(BarC3<BarO3 && BarC2>BarO2 && (BarO3-BarC3)>MaxB/2 && (BarC2-BarO2)>MaxB/2  && BarC>(BarL2+(BarH2-BarL2)/2) && BarC2>BarO3)  
			  //   { vlB.Time=DTime; 
					// if(_positionGuid!=Guid.Empty) { ClosePosition(); k=0; }
					// Buy(); 
				// }
			
			//if(BarC>BarO &&	(BarH-BarL)>MaxB/2 && BarC>(BarH-0.0005)  &&  BarO<Z3) { vlR.Time=Bars[Bars.Range.To-1].Time; Print("S1 - {0}",MaxB); }
			//if(BarC>BarO && (BarH-BarL)>MaxB/2 && BarO<(BarH+0.0005)  &&  BarO>Z1) { vlR.Time=Bars[Bars.Range.To-1].Time; Print("S2 - {0}",MaxB); }
			//if(BarC>BarO &&	 BarO<Z1  &&  BarC>Z3) { vlR.Time=Bars[Bars.Range.To-1].Time; Print("S3"); }
			
			if(k>4 && _positionGuid!=Guid.Empty)  { ClosePosition(); k=0; }
			if(_positionGuid!=Guid.Empty) k++;
				 
			//Print("k={0} - {1}",k,DTime);
			
				 
        }
        
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Event occurs on every change of the positions
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
				private void Buy()
		{
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Buy(Instrument.Id, 0.1); 		
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
		
		private void Sell()
		{
			if (_positionGuid!=Guid.Empty) return; 
			
			var result=Trade.Sell(Instrument.Id, 0.1); 
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				
			}
    	}
			private void ClosePosition()
		{
			var result =Trade.CloseMarketPosition(_positionGuid);
			if (result.IsSuccessful) 
			{
                Print("LR_Position_closed_by_inverse_signal", result.Position.Number, result.Position.ClosePrice);	
				_positionGuid = Guid.Empty;			
			}
		}
    }
}