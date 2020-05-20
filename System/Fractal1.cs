// Торговая стратегия - Три фрактала образуют патерн 1-2-3
//  Откат должен в зону 20%-80% от волны 1-2


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
    [TradeSystem("Fractal1")]
    public class Fractal1 : TradeSystem
    {
        // Simple parameter example
		
		public Fractals _frInd,_frInd3;
		
		// Fractal если true - то вверх или вниз
		bool frUp1,frUp2,frUp3;
		
		double frUpH=0.0;
		double frDownH=0.0;
		bool pBuy=false;
		bool pSell=false;
		double frH1,frH2,frH3;   // Значение текущего верхнего Fractal
		double frL1,frL2,frL3;    // Значение Low - свечи с верхним фрактклом
        double frC1,frC2,frC3;
		
		double frH31;   // Значение текущего верхнего Fractal
		double frL31;    // Значение Low - свечи с верхним фрактклом
		double frUpH3=0.0;
		double frDownH3=0.0;
		
		double BarH,BarL,BarC;
		double frH=0, frL=0;
		HorizontalLine Hl;
		VerticalLine  Bl;
		VerticalLine  Rl;
		
				private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		//private Guid Rl=Guid.Empty;
		//private Guid Bl=Guid.Empty;
		
		
		public double frSU=0,frSD=0;
		public double frSU3=0,frSD3=0;
		int i=0;
		
		
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }

        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			_frInd = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			_frInd.Range = 9;
			//_frInd3 = GetIndicator<Fractals>(Instrument.Id, Timeframe);
			//_frInd3.Range = 3;
			
        }        

        protected override void NewQuote()
        {
            // Event occurs on every new quote
        }
        
        protected override void NewBar()
        {
            // Event occurs on every new bar
			// Значения текущего Бара
			BarH = Bars[Bars.Range.To-1].High;
			BarL = Bars[Bars.Range.To-1].Low;
			BarC = Bars[Bars.Range.To-1].Close;
		

			//  frUp frDown - Истина если появился НОВЫЙ фрактал Вверх/Вниз
			//Print("Fractal!! - Top={0} Bottom={1}",_frInd.TopSeries[Bars.Range.To-5],_frInd.BottomSeries[Bars.Range.To-5]);
			frSU=_frInd.TopSeries[Bars.Range.To-11];
			frSD=_frInd.BottomSeries[Bars.Range.To-11];

			//frSU3=_frInd3.TopSeries[Bars.Range.To-5];
			//frSD3=_frInd3.BottomSeries[Bars.Range.To-5];

           
		// =================================================================================================================================				 
     		// Срабатывает - когда появился новый фрактал - frUp frDown=true!
			// Запоминаем значения Свечи бара-фрактала(frUpH) и время (frUp_Time)
			
			if (frSU>0)    { 
				frUp3=frUp2;frUp2=frUp1;frUp1=true;
				frH3=frH2;frH2=frH1; frH1=Bars[Bars.Range.To-11].High; 
				frL3=frL2;frL2=frL1; frL1=Bars[Bars.Range.To-11].Low;
				frC3=frC2;frC2=frC1; frC1=Bars[Bars.Range.To-11].Close;
				
			}
			
			 if (frSD>0)    { 
				frUp3=frUp2;frUp2=frUp1;frUp1=false;
				frH3=frH2;frH2=frH1; frH1=Bars[Bars.Range.To-11].High; 
				frL3=frL2;frL2=frL1; frL1=Bars[Bars.Range.To-11].Low;
				frC3=frC2;frC2=frC1; frC1=Bars[Bars.Range.To-11].Close;
			    
			 }
			  
			 
			 
			// Print("frUp1={6} - frUp2={7} - frUp3={8} ---frH1={0},frH2={1},frH3={2},frL1={3},frL2={4},frL3={5}",frH1,frH2,frH3,frL1,frL2,frL3,frUp1,frUp2,frUp3);
			 // Print("{0} -- {1}",(!frUp1 &&  frUp2 && !frUp3),(frUp1  &&  !frUp2 && frUp3 ));
			 //if(!frUp1 &&  frUp2 && !frUp3)  { Print("BUY OK! -- {0}-{1}-{2}-{3}-{4} -- {5}",frL3,frH2,frL1,(frL3+((frH2-frL3)*0.5)>frL1),(frL3+((frH2-frL3)*0.2)<frL1),Bars[Bars.Range.To-1].Time); }  
				  //HLine(frL1); Print("BUY -- frH1={0},frH2={1},frH3={2},frL1={3},frL2={4},frL3={5}",frH1,frH2,frH3,frL1,frL2,frL3);}
			 //if(frUp1  &&  !frUp2 && frUp3 )  { Print("SELL OK! -- {0}-{1}-{2}-{3}-{4} -- {5}",frH3,frL2,frH1,(frL2+((frH3-frL2)*0.5)<frH1),(frL2+((frH3-frL2)*0.8)>frH1),Bars[Bars.Range.To-1].Time);  } 
				  //HLine(frH1); Print("SELL -- frH1={0},frH2={1},frH3={2},frL1={3},frL2={4},frL3={5}",frH1,frH2,frH3,frL1,frL2,frL3);}
			  
			  // if(!frUp3 &&  frUp2 && !frUp1) { Red();  Print("BUY -- frH1={0},frH2={1},frH3={2},frL1={3},frL2={4},frL3={5}",frH1,frH2,frH3,frL1,frL2,frL3);}
			 // if(frUp3 &&  !frUp2 && frUp1)  { Blue(); Print("SELL -- frH1={0},frH2={1},frH3={2},frL1={3},frL2={4},frL3={5}",frH1,frH2,frH3,frL1,frL2,frL3);}
			  
			 // %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% 
			 if(!frUp3 &&  frUp2  && !frUp1 && (frL3+((frH2-frL3)*0.5)>frL1) && (frL3+((frH2-frL3)*0.2)<frL1) && posGuidBuy==Guid.Empty) { 
				  pBuy=true; Red(Bars[Bars.Range.To-1].Time);Print("BUY OPEN OK!!");
			
				  //var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1, Math.Round(Instrument.Ask, Instrument.PriceScale), -1, Stops.InPrice(frL1,null),null,null);
				var result=Trade.Buy(Instrument.Id, 0.1);		 
				 if (result.IsSuccessful)  posGuidBuy=result.Position.Id;  
			  }
			  
			  if( frUp3 &&  !frUp2 &&  frUp1 && (frL2+((frH3-frL2)*0.5)<frH1) && (frL2+((frH3-frL2)*0.8)>frH1) && posGuidSell==Guid.Empty) { 
				     pSell=true; frL=frL2; Blue(Bars[Bars.Range.To-1].Time); Print("SELL OPEN OK!!"); 
			  
				  //var result = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1, Math.Round(Instrument.Ask, Instrument.PriceScale), -1, Stops.InPrice(frH1,null),null,null);
				var result=Trade.Sell(Instrument.Id, 0.1);			 
				  if (result.IsSuccessful)  posGuidSell=result.Position.Id;  
			  }
			  
			  
			  
			  
			  if(posGuidBuy!=Guid.Empty && Bars[Bars.Range.To-1].Close>(frL1+(frH2-frL3)))  { 
			     var res = Trade.CloseMarketPosition(posGuidBuy);
				 if (res.IsSuccessful) posGuidBuy = Guid.Empty;  }
			  
			  if(posGuidSell!=Guid.Empty  && Bars[Bars.Range.To-1].Close<(frH1-(frH3-frL2))) { 
			     var res = Trade.CloseMarketPosition(posGuidSell);
                 if (res.IsSuccessful) posGuidSell = Guid.Empty;   }
			  
			  			
			  // Корекция
			if (posGuidBuy!=Guid.Empty && Trade.GetPosition(posGuidBuy).State==PositionState.Closed) posGuidBuy=Guid.Empty;
			if (posGuidSell!=Guid.Empty && Trade.GetPosition(posGuidSell).State==PositionState.Closed)  posGuidSell=Guid.Empty; 
						
						
			//			/===============================================================================================================================
			// Появился новый фрактал ВВЕРХ и открыта позиция Sell - переосим стоп 
					 if(frSU>0 && posGuidBuy!=Guid.Empty ) { 
			
			
				    var res3=Trade.UpdateMarketPosition(posGuidSell, frL1, null, null); 
				 if (res3.IsSuccessful) { Print("15 SellStop - UpdatePending StopLoss когда Появился новый фрактал Up ");}
				 else  { var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) posGuidSell = Guid.Empty; }
					 }
								 
					 // Появился новый фрактал ВНИЗ и ОТКРЫТА позиция Buy - переносим стоп - Работает!
			if(frSD>0 && posGuidSell!=Guid.Empty)  {
			
				 var result2=Trade.UpdateMarketPosition(posGuidBuy, frH1,null, null); 
				if (result2.IsSuccessful) { Print("14 BuyStop - !UpdatePending StopLoss когда Появился новый фрактал Down - {0}");}  
			  else { var res0 = Trade.CloseMarketPosition(posGuidBuy); if (res0.IsSuccessful) posGuidBuy = Guid.Empty;}
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
		      			  private void Red(DateTime Dt) 
            {
		   	   //Tools.Remove<VerticalLine>(Rl);
				

			   var rel = Tools.Create<VerticalLine>();
               rel.Time=Dt;
			   rel.Color=Color.Red;
				//Rl=rel.Id;
			}
			      private void Blue(DateTime Dt) 
            {
		       //Tools.Remove(Bl);
				//Print("OK!");
			   Bl = Tools.Create<VerticalLine>();
               Bl.Time=Dt;
			   Bl.Color=Color.Blue;
			}
					private void Green(DateTime Dt) 
			{
			var vl = Tools.Create<VerticalLine>();
                vl.Time=Dt;
			    vl.Color=Color.LightSeaGreen;
			}	
				     private void Yellow(DateTime Dt) 
            {
		   var vl = Tools.Create<VerticalLine>();
               vl.Time=Dt;
			   vl.Color=Color.Yellow;
			}	
			
					private void HLine(double PriceH) 
			{
			    Tools.Remove(Hl);
				Hl = Tools.Create<HorizontalLine>();
                Hl.Price = PriceH;
			    Hl.Text=PriceH.ToString();
			}
    }
}