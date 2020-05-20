using System;
using IPro.Model.Client.MarketData;
using IPro.Model.Programming;

namespace IPro.Samples
{
    /// <summary>
    /// Extension methods for accessing particular value range of bar series
    /// </summary>
    public static class SubSeriesExtentions
    {
        /// <summary>
        /// Get series of open values
        /// </summary>
        public static ISeries<double> GetOpenSeries(this ISeries<Bar> barSeries)
        {
            return new SubDataSeries<double>(barSeries, bar => bar.Open);
        }

        /// <summary>
        /// Get series of close values
        /// </summary>
        public static ISeries<double> GetCloseSeries(this ISeries<Bar> barSeries)
        {
            return new SubDataSeries<double>(barSeries, bar => bar.Close);
        }

        /// <summary>
        /// Get series of high values
        /// </summary>
        public static ISeries<double> GetHighSeries(this ISeries<Bar> barSeries)
        {
            return new SubDataSeries<double>(barSeries, bar => bar.High);
        }

        /// <summary>
        /// Get series of low values
        /// </summary>
        public static ISeries<double> GetLowSeries(this ISeries<Bar> barSeries)
        {
            return new SubDataSeries<double>(barSeries, bar => bar.Low);
        }

        /// <summary>
        /// Get series of time values
        /// </summary>
        public static ISeries<DateTime> GetTimeSeries(this ISeries<Bar> barSeries)
        {
            return new SubDataSeries<DateTime>(barSeries, bar => bar.Time);
        }

        /// <summary>
        /// Get series of time values
        /// </summary>
        public static ISeries<int> GetTickCountSeries(this ISeries<Bar> barSeries)
        {
            return new SubDataSeries<int>(barSeries, bar => bar.TickCount);
        }

        /// <summary>
        /// Series of bar values
        /// </summary>
        private sealed class SubDataSeries<T> : ISeries<T>
        {
            private readonly ISeries<Bar> _barSeries;
            private readonly Func<Bar, T> _exctractFunc;

            /// <summary>
            /// ctor
            /// </summary>
            public SubDataSeries(ISeries<Bar> barSeries, Func<Bar, T> exctractFunc)
            {
                _barSeries = barSeries;
                _exctractFunc = exctractFunc;
            }

            /// <summary>
            /// Gets or sets value at specified index
            /// </summary>
            /// <returns>Value if exists, default{T} otherwise</returns>
            public T this[int index]
            {
                get
                {
                    var bar = _barSeries[index];
                    if (bar == null)
                        return default(T);

                    return _exctractFunc(bar);
                }
                set { }
            }

            /// <summary>
            /// Available data range
            /// </summary>
            public IndexRange Range
            {
                get { return _barSeries.Range; }
            }
        }
    }
}
