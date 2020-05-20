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
    [TradeSystem("Fisher")]
    public class Fisher : TradeSystem
    {
        // Simple parameter example
        [Parameter("Some comment", DefaultValue = "Hello, world!")]
        public string CommentText { get; set; }
		private enum SignalType
		{
			SignalLineDownTop,
			SignalLineTopDown,
			ZeroLineDownTop,
			ZeroLineTopDown,
			None
		}
		
		public Period _period = new Period(PeriodType.Hour, 1);
        public FisherTransformOscillator _ftoInd;
		public FisherTransformOscillator _ftoIndH1;
		public VerticalLine toolVerticalLine ;
		public Guid sectionGuid = new Guid("CBCBEF39-114E-4FB5-B518-D480B4A21DA0");
		public Guid toolVLine = new Guid() ;
		public int Is1 = 1;
		
			
        protected override void Init()
        {
            // Event occurs once at the start of the strategy
            ///Print("Starting TS on account: {0}, comment: {1}", this.Account.Number, CommentText);
			
			_ftoInd   = GetIndicator<FisherTransformOscillator>(Instrument.Id, Timeframe);
                // Print("Timeframe = {0}",Timeframe.Count); 
		     	//Print("Timeframe2 = {0}",_period.Count); 
			//_ftoIndH1 = GetIndicator<FisherTransformOscillator>(Instrument.Id, _period);
			
			
			//Свойство PriceMode возвращает или задает тип цены, по которой рассчитывается значение серии индикатора.
			//_ftoInd.PriceMode = PriceMode.Close;
			
			//Свойство Method возвращает или задает метод вычисления значения скользящей средней для индикатора.
			//_ftoInd.Ma1Methods = MaMethods.Sma;
			
			
        }        

        protected override void NewQuote()
        {
            // Событие происходит на каждой новой котировки
			//Print("NQ   Is1 = {0} - {1]", Is1,_ftoInd.FisherSeries[Bars.Range.To-1]);
        }
        
		//private bool firstBar = true;
        //private Guid posGuid;

        protected override void NewBar()
        {
			//Print("Is1 = {0} - {1]", Is1,_ftoInd.FisherSeries[Bars.Range.To-1]);
			//Print("NewBar!!");
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]>0 && Is1<0 ) {
			// if ( _ftoInd.Ma1Series[Bars.Range.To-1]>0 && Is1<0 ) {
			// if ( _ftoInd.Ma2Series[Bars.Range.To-1]>0 && Is1<0 ) {
			//if ( _ftoInd.Ma2Series[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1] && Is1<0 ) { // Пересечение красной и синей МА
			//if ( _ftoInd.FisherSeries[Bars.Range.To-1]>_ftoInd.Ma1Series[Bars.Range.To-1] && Is1<0 ) {
				Is1 = 1;
		    var vline = Tools.Create<VerticalLine>();
            vline.Color=Color.Red;
		    vline.Time=Bars[Bars.Range.To-1].Time;	
			}
			
			if ( _ftoInd.FisherSeries[Bars.Range.To-1]<0 && Is1>0 ) { //Fisher  пересекает 0
			// if ( _ftoInd.Ma1Series[Bars.Range.To-1]<0 && Is1>0 ) {    // МА1 пересекает 0
		    // if ( _ftoInd.Ma2Series[Bars.Range.To-1]<0 && Is1>0 ) {    // МА1 пересекает 0
			// if ( _ftoInd.Ma2Series[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1] && Is1<0 ) {  // МА1 пересекает МА2
		    //if ( _ftoInd.FisherSeries[Bars.Range.To-1]<_ftoInd.Ma1Series[Bars.Range.To-1] && Is1>0 ) {	// Fisher и МА1 	
				Is1 = -1;
		    var vline = Tools.Create<VerticalLine>();
            vline.Color=Color.Blue;
		    vline.Time=Bars[Bars.Range.To-1].Time;	
			}
			
			
            // Событие происходит на каждом новом баре
			
			//Свойство UpSeries возвращает или задает значения верхней серии индикатора.
			//Print("Fisher transform oscillator up series value: {0}", _ftoInd.UpSeries[Bars.Range.To-1]);
			//Print("Fisher значения ВЕРХНЕЙ серии индикатора: {0}", _ftoInd.UpSeries[Bars.Range.To-1]);
			//     Fisher transform oscillator up series value: 0,134204199634398
			
			//Свойство DownSeries возвращает или задает нижнюю серию значений индикатора
			//Print("Fisher значения НИЖНЕЙ серии индикатора: {0}", _ftoInd.DownSeries[Bars.Range.To-1]);
			
			//Свойство FisherSeries возвращает или задает значение Fisher серии индикатора.
			//Print("Fisher значения ВСЕХ серии индикатора {0}", _ftoInd.FisherSeries[Bars.Range.To-1]);
			//Print("Основной {0}", _ftoInd.FisherSeries[Bars.Range.To-1]);
			//Print("Значение Н1 {0}", _ftoIndH1.FisherSeries[Bars.Range.To-1]);
			//	   Fisher transform oscillator fisher series value: 0,134204199634398
			
			//Свойство Ma1Period возвращает или задает значение периода линии Ma1 индикатора.
			//Print("FisherTransformOscillator ma1 period value: {0}", _ftoInd.Ma1Period);
			//     FisherTransformOscillator ma1 period value: 9
						
			//Свойство Ma1Series возвращает или задает серию значений периода линии Ma1 индикатора.
			//Print("Fisher transform oscillator Ma1 series value: {0}", _ftoInd.Ma1Series[Bars.Range.To-1]);
			
			//Свойство Ma1Period возвращает или задает значение периода линии Ma1 индикатора.
			//Print("FisherTransformOscillator ma2 period value: {0}", _ftoInd.Ma2Period);
			//     FisherTransformOscillator ma2 period value: 9
						
			//Свойство Ma1Series возвращает или задает серию значений периода линии Ma2 индикатора.
			//Print("Fisher transform oscillator Ma2 series value: {0}", _ftoInd.Ma2Series[Bars.Range.To-1]);

			
			//Свойство Period возвращает или задает значение периода индикатора.
			//Print("FisherTransformOscillator period value: {0}", _ftoInd.Period);
			//     FisherTransformOscillator period value: 18
			
  // Открыть рыночную позицию типа Buy , а затем закрыть ее с помощью указанного идентификатора.
  //if ((_ftoInd.UpSeries[Bars.Range.To-1]>0) && (posGuid==Guid.Empty))
  //  {
  //      var result = Trade.Buy(Instrument .Id, 0.1);
  //      if (result.IsSuccessful) posGuid = result.Position.Id;
		    
 
           //firstBar = false;
//    }

//	if ((_ftoInd.DownSeries[Bars.Range.To-1]<0) && (posGuid!=Guid.Empty))
 //    {
   //     if (posGuid==Guid.Empty) return;
     //   var res = Trade.CloseMarketPosition(posGuid);
       // if (res.IsSuccessful) posGuid = Guid.Empty;
    //}

			
		//Trade.Buy(Instrument.Id, 0.2); 	
		//Trade.Sell(Instrument.Id, 0.2);	
		//	Trade.CloseMarketPosition(_positionGuid);
			
	
          //var series = GetCustomSeries(Instrument.Id, Period.H1);
          //var result = Series.Sma(series, series.Range.To, 15, PriceMode.Close);
		  //var lowestPrice = result. ;
          //Print("Series = {0}",lowestPrice);
			
			
        }
        
	
		
        protected override void PositionChanged(IPosition position, ModificationType type)
        {
            // Событие происходит при каждом изменении позиций
            if (type==ModificationType.Closed)
            {
                Print("Position {0} was closed at price {1}", position.Number, position.ClosePrice);
            }
        }
    }
}