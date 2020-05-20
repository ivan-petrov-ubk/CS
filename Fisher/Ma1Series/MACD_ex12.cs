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

namespace IPro.TradeSystems
{
    [TradeSystem("MACD trading strategy")]
  [ParametersGroup(__ui_cGroup_Parameters, "LR_MACD_indicator_parameters", Order = -2)]
  [ParametersGroup(__ui_cGroup_Signals, "LR_Generated_signals", Order = -1)]
  [ParametersGroup(__ui_cGroup_Props, "LR_Order_properties", Order = 0)]	
    public class StrategyMACD: TradeSystem
    {   
		#region Parameters
	// ================================================================
        [Parameter("LR_Period", DefaultValue = 18, MinValue = 1)]
        public int Period { get; set; }
        [Parameter("LR_Ma1Period", DefaultValue = 9, MinValue = 1)]
        public int Ma1Period { get; set; }
        [Parameter("LR_Ma1Method", DefaultValue = MaMethods.Sma)]
        public MaMethods Ma1Method { get; set; }
        [Parameter("LR_Ma2Period", DefaultValue = 21, MinValue = 1)]
        public int Ma2Period { get; set; }
        [Parameter("LR_Ma2Method", DefaultValue = MaMethods.Sma)]
        public MaMethods Ma2Method { get; set; }
         [Series("LR_Up", Color = Color.LightSeaGreen, Style = SeriesStyle.Histogram)]
        public IIndicatorSeries UpSeries { get; set; }
        [Series("LR_Down", Color = Color.DarkGoldenrod, Style = SeriesStyle.Histogram)]
        public IIndicatorSeries DownSeries { get; set; }
        [Series("LR_Fisher", Color = Color.Gray)]
        public IIndicatorSeries FisherSeries { get; set; }
        [Series("LR_Ma1", Color = Color.Blue)]
        public IIndicatorSeries Ma1Series { get; set; }
        [Series("LR_Ma2", Color = Color.Red)]
        public IIndicatorSeries Ma2Series { get; set; }
		private ISeries<double> _values;       
	// ================================================================

    //  [Parameter("LR_Fast_EMA", DefaultValue = 12, MinValue = 1, GroupId = __ui_cGroup_Parameters)]
    //    public int FastEMA { get; set; }
    //    [Parameter("LR_Slow_EMA", DefaultValue = 26, MinValue = 1, GroupId = __ui_cGroup_Parameters)]
    //    public int SlowEMA { get; set; }
    //    [Parameter("LR_MACD_SMA", DefaultValue = 9, MinValue = 1, GroupId = __ui_cGroup_Parameters)]
	//	public int MacdSMA { get; set; }
    //    [Parameter("LR_Apply_to", DefaultValue = PriceMode.Close, GroupId = __ui_cGroup_Parameters)]
    //    public PriceMode PositionPriceMode { get; set; }

        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Buy, Header = "LR_Indicator_and_MACD_signal_down_to_up_intersection", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType SignalBottomToTopOrder { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Sell, Header = "LR_indicator_and_MACD_up_to_down_intersection", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType SignalTopToBottomOrder { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Buy, Header = "LR_Intersection_zero_line_with_indicator_line_down_to_up", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType ZeroLineBottomToTopOrder { get; set; }
        [Parameter("LR_Position_type", DefaultValue = AdvisorOrderType.Sell, Header = "LR_Intersection_zero_line_with_indicator_line_up_to_down", GroupId = __ui_cGroup_Signals)]
		public AdvisorOrderType ZeroLineTopToBottomOrder { get; set; }

        [Parameter("LR_Lot", DefaultValue = 0.01, IsLot = true, GroupId = __ui_cGroup_Props)]
        public decimal LotVolume { get; set; }
        [Parameter("LR_Take_Profit", DefaultValue = 0, GroupId = __ui_cGroup_Props, Postfix = "LR_Points", MaxValue=99999)]
        public int TakeProfit { get; set; }
        [Parameter("LR_Stop_Loss", DefaultValue = 0, GroupId = __ui_cGroup_Props, Postfix = "LR_Points", MaxValue=99999)]
        public int StopLoss { get; set; }
        [Parameter("LR_Position_closing_on_inverse_signal", DefaultValue = true, GroupId = __ui_cGroup_Props)]
        public bool IsDoCloseOnReverseSignal { get; set; }
		
		#endregion		
		
		#region UI
		
		private const string __ui_cGroup_Parameters = "g-indicator-params-1";
		private const string __ui_cGroup_Signals="g-order-signals";
		private const string __ui_cGroup_Props = "g-order-props";
		
		#endregion
		
		private enum SignalType
		{
			SignalLineDownTop,
			SignalLineTopDown,
			ZeroLineDownTop,
			ZeroLineTopDown,
			None
		}
		//private MovingAverageConvergenceDivergence _macd;
		private FisherTransformOscillator _macd;
		private Period _period = new Period(PeriodType.Minute, 1);
		private Period  periodM5 = new Period(PeriodType.Minute, 5);
        private Period  periodM15 = new Period(PeriodType.Minute, 15);
		private Period  periodH1 = new Period(PeriodType.Hour, 1);
		private ISeries<Bar> _barSeries;
		private int _macdIndex;
		private double _macdMainValue;
		private double _macdSignalValue;
		private bool _firstQuote=true;
		private bool _isSignalLineHigher=false;
		private bool _isSignalWasPositive=false;
		private bool _isAnyErrors=false;
		private Guid _positionGuid=Guid.Empty;
		private SignalType _positionOpendSignalType=SignalType.None;
		
		
		protected override void Init()
        {
            Print("LR_Trade_strategy_initalization");
			//Print("Period = {0}", Period);
            //Print("Ma1Period = ",Ma1Period);
            //Print("Ma2Period = ",Ma1Period);			
			
			
			if ((LotVolume>Account.MaxLot)||(LotVolume<Account.MinLot))
			{
                Print("LR_Invalid_lot_value");
                Print("LR_Lot_minimum_value " + (double)Account.MinLot);
                Print("LR_Lot_maximum_value " + (double)Account.MaxLot);
				_isAnyErrors=true;
				return;
			}
			
			var _instrumentId=this.Instrument.Id;
			_period=Timeframe; // Период берется текущий - Н1
			//Print("_period = {0} " ,_period);
            //_macd = GetIndicator<FisherTransformOscillator>(Instrument.Id, _period, FastEMA, SlowEMA, MacdSMA, PositionPriceMode);
			// Створити індикатор
			//_macd = GetIndicator<FisherTransformOscillator>(Instrument.Id, _period, Ma1Period, Ma2Method, Ma2Method);
			_macd = GetIndicator<FisherTransformOscillator>(Instrument.Id, _period);
			_barSeries = GetCustomSeries(Instrument.Id, _period);	// Номер текущего бара
			// Print("_barSeries = {0} " ,_barSeries.Range.To);
        }
		
        protected override void NewQuote() // изменилась котировка
        {
			// Ниже описанный фрагмент преобразовівает индекс из Time Frame M1 в М5.
			// var newIndex = TransformIndex(oldIndex, periodM1, periodM5);
			_macdIndex = TransformIndex(_barSeries.Range.To, _period, _period);	
			//  Print("_macdIndex = {0} " , _macdIndex);
			_macdMainValue   = _macd.FisherSeries[_macdIndex];
			_macdSignalValue = _macd.Ma1Series[_macdIndex];
			
			Analyze();
			

        }
		
		protected override void NewBar()
        {
			Analyze();
		}

		private void Analyze()
		{
		        
			Print("_macdSignalValue - _macdMainValue = {0} - {1}" ,_macdSignalValue,_macdMainValue); 
			
			if (_isAnyErrors) return; //Анализ Лота - если (LotVolume>Account.MaxLot)||(LotVolume<Account.MinLot)
			
			if (_firstQuote) // если первый раз проход
			{    // Если еще нет коитовок! 
				if ((_macdMainValue==0) || (_macdSignalValue==0) || (_macdMainValue==double.NaN) || (_macdSignalValue==double.NaN))
					{
						return;
					}
				
				_firstQuote=false;
					// !!!  ГЛАВНЫЕ СИГНАЛЫ  ===============================================
				if (_macdSignalValue>_macdMainValue) _isSignalLineHigher=true;  
     			if (_macdSignalValue>0) _isSignalWasPositive=true;
				    //  ==================================================================================	
			}
			
			if (!_isSignalLineHigher) // Если сигнал пересечения ВНИЗ
			{
				if (_macdSignalValue>_macdMainValue)
				{
					_isSignalLineHigher=true;
					CrossSignalLineEvent();
				}
			}
			else 
				{
				if (_macdSignalValue<=_macdMainValue) 
				{
					_isSignalLineHigher=false;
					CrossSignalLineEvent();
				}
			}
			
			if (!_isSignalWasPositive && (_macdSignalValue>=0.0)) 
				{
				_isSignalWasPositive=true;	
				CrossZeroLineEvent();
				}
			
			if (_isSignalWasPositive && (_macdSignalValue<0.0)) 
				{
				_isSignalWasPositive=false;	
				CrossZeroLineEvent();
				}	
		}
		
		private void CrossSignalLineEvent()
		{
			if (_isSignalLineHigher)
			{
                Print("LR_Indicator_and_MACD_signal_down_to_up_intersection");
				
				if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
				{
					CheckForReversSignal(SignalType.SignalLineDownTop); //  Закрытие ордера по реверс - сигналу
					return; 
				}
				// если не включено реверс - по стопу и ТР
				if (SignalBottomToTopOrder.Equals(AdvisorOrderType.Buy)) Buy(SignalType.SignalLineDownTop); 		
				if (SignalBottomToTopOrder.Equals(AdvisorOrderType.Sell)) Sell(SignalType.SignalLineDownTop);	
			}
			else
			{
                Print("LR_indicator_and_MACD_up_to_down_intersection");
				
				if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
				{
					CheckForReversSignal(SignalType.SignalLineTopDown);
					return; 
				}
				
				if (SignalTopToBottomOrder.Equals(AdvisorOrderType.Buy)) Buy(SignalType.SignalLineTopDown);			
				if (SignalTopToBottomOrder.Equals(AdvisorOrderType.Sell)) Sell(SignalType.SignalLineTopDown);	
			}
		}
		
		private void CrossZeroLineEvent()
		{
			if (_isSignalWasPositive) 
			{
                Print("LR_Intersection_zero_line_with_indicator_line_up_to_down");
				
				if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
				{
					CheckForReversSignal(SignalType.ZeroLineDownTop);
					return; 
				}
			
				if (ZeroLineBottomToTopOrder.Equals(AdvisorOrderType.Buy)) Buy(SignalType.ZeroLineDownTop);			
				if (ZeroLineBottomToTopOrder.Equals(AdvisorOrderType.Sell)) Sell(SignalType.ZeroLineDownTop);	
			}
			else 
			{
                Print("LR_indicator_and_MACD_up_to_down_intersection");
				
				if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal)
				{
					CheckForReversSignal(SignalType.ZeroLineTopDown);
					return; 
				}
				if (ZeroLineTopToBottomOrder.Equals(AdvisorOrderType.Buy)) Buy(SignalType.ZeroLineTopDown);			
				if (ZeroLineTopToBottomOrder.Equals(AdvisorOrderType.Sell)) Sell(SignalType.ZeroLineTopDown);	
			}
		}
	
		private void Buy(SignalType signal)
		{
			if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal) return; 
			
			var result=Trade.Buy(Instrument.Id, LotVolume, Stops.InPips(StopLoss, TakeProfit)); 		
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				_positionOpendSignalType=signal;
			}
    	}
		
		private void Sell(SignalType signal)
		{
			if ((_positionGuid!=Guid.Empty) && IsDoCloseOnReverseSignal) return; 
			
			var result=Trade.Sell(Instrument.Id, LotVolume, Stops.InPips(StopLoss, TakeProfit)); 
			if (result.IsSuccessful) 
			{
				_positionGuid=result.Position.Id;
				_positionOpendSignalType=signal;
			}
    	}
		
		protected override void PositionChanged(IPosition position, ModificationType type)
		{
			if ((type.Equals(ModificationType.Closed))&&(position.Id==_positionGuid)) 
			{
				_positionGuid=Guid.Empty; 
				_positionOpendSignalType=SignalType.None;
			}
		}
		
		private void CheckForReversSignal(SignalType currentSignal)
		{
			switch (currentSignal) 
			{
				case SignalType.SignalLineDownTop:  
					if (_positionOpendSignalType==SignalType.SignalLineTopDown) ClosePosition();
				break;	
				
				case SignalType.SignalLineTopDown:
					if (_positionOpendSignalType==SignalType.SignalLineDownTop) ClosePosition();
				break;
				
				case SignalType.ZeroLineTopDown:
					if (_positionOpendSignalType==SignalType.ZeroLineDownTop) ClosePosition();
				break;	
				
				case SignalType.ZeroLineDownTop:
					if (_positionOpendSignalType==SignalType.ZeroLineTopDown) ClosePosition();
				break;	
			}
		}
		
		private void ClosePosition()
		{
			var result =Trade.CloseMarketPosition(_positionGuid);
			if (result.IsSuccessful) 
			{
                Print("LR_Position_closed_by_inverse_signal", result.Position.Number, result.Position.ClosePrice);	
				_positionGuid=Guid.Empty;
				_positionOpendSignalType=SignalType.None;
			}
		}
	}
}