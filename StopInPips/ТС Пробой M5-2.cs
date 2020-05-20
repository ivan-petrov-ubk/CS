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
    [TradeSystem("ТС Пробой M5")] //copy of "ТС Пробой Н1"
    public class ТС_Пробой_Н1 : TradeSystem
    {   public double H1,L1,O1,C1,H2,L2,O2,C2,O3,C3;
		public HorizontalLine Line1,Line2,Line3;
		public DateTime DTime; // Время
		private ZigZag _wprInd3;
		private bool PrD=true,PrU=true,isF,isFU=true,isFD=false;
		public FisherTransformOscillator _ftoInd;
		private Guid posGuidBuy=Guid.Empty;
		private Guid posGuidSell=Guid.Empty;
		
        protected override void Init()
        {   _ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe); 
			Line1 = Tools.Create<HorizontalLine>();
			Line2 = Tools.Create<HorizontalLine>();
			_wprInd3= GetIndicator<ZigZag>(Instrument.Id, Timeframe);
			_wprInd3.ExtDepth=3;
			isF=true;
        }        

        protected override void NewBar()
        {   _wprInd3.ReInit();
		  if(_ftoInd.Ma2Series[Bars.Range.To-1]>0) isF=true; else isF=false;
		  if(_ftoInd.Ma1Series[Bars.Range.To-1]<_ftoInd.Ma2Series[Bars.Range.To-1] && _ftoInd.Ma1Series[Bars.Range.To-2]>_ftoInd.Ma2Series[Bars.Range.To-2]) isFU=true; else isFU=false;
		  if(_ftoInd.Ma1Series[Bars.Range.To-1]>_ftoInd.Ma2Series[Bars.Range.To-1] && _ftoInd.Ma1Series[Bars.Range.To-2]<_ftoInd.Ma2Series[Bars.Range.To-2]) isFD=true; else isFD=false;
		  if (C1>L1 && !PrD) PrD=true; // Пересечение PrD=ВНИЗ или PrU=ВВЕРХ
		  if (C1<H1 && !PrU) PrU=true;	 
           
		    O1 = Bars[Bars.Range.To-1].Open;
			C1 = Bars[Bars.Range.To-1].Close;
			DTime = Bars[Bars.Range.To-1].Time;

		  if ( DTime.Hour==00 && DTime.Minute==00 ) 
          { 	PrD=true; PrU=true;
         		var highestIndex = Series.Highest(Bars.Range.To, 288, PriceMode.High);
     			var highestPrice = Bars[highestIndex].High;
			     	H1 = highestPrice;
		    	var lowestIndex  = Series.Lowest(Bars.Range.To, 288, PriceMode.Low);
			    var lowestPrice = Bars[lowestIndex].Low;
			     	L1 = lowestPrice;
			  		Line1.Price = H1;
			  		Line1.Text = string.Format("{0}",Math.Round((H1-L1)*100000,0));
					Line2.Price = L1;
		  }	    
		  
		  if (C1>H1 && DTime.Minute==00 && PrU) 
		  {			PrU=false;
					var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Red;
			    var result3 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Buy, 0.1,Instrument.Bid, -1, Stops.InPips(200,600), null, null);
				if (result3.IsSuccessful)  posGuidBuy=result3.Position.Id;
		  } 
		  
		  if (C1<L1 && DTime.Minute==00 && PrD) 
		  {			PrD=false;
			  		var toolVerticalLine=Tools.Create<VerticalLine>();
     				toolVerticalLine.Time = Bars[Bars.Range.To-1].Time;
				    toolVerticalLine.Color=Color.Blue;
			  	var result2 = Trade.OpenMarketPosition(Instrument.Id, ExecutionRule.Sell, 0.1,Instrument.Ask, -1,Stops.InPips(200,600), null, null);
				if (result2.IsSuccessful)  posGuidSell=result2.Position.Id;
		  } 
		
		if(isFU && posGuidBuy!=Guid.Empty) {var res1 = Trade.CloseMarketPosition(posGuidBuy); if (res1.IsSuccessful) posGuidBuy = Guid.Empty;}
		if(isFD && posGuidSell!=Guid.Empty) {var res2 = Trade.CloseMarketPosition(posGuidSell); if (res2.IsSuccessful) posGuidSell = Guid.Empty;}           
		  
        }
    }
}