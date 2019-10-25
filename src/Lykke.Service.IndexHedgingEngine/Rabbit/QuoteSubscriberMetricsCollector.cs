// Copyright (c) Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Lykke.Service.IndexHedgingEngine.Rabbit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.ExchangeAdapter.Contracts;
    using Common.Log;
    using global::Common;
    using global::Common.Log;

    // TODO: REMOVE. This class is needed only to find problem with slow message consumption via logs.
    // When problem would be fixed, it should be removed.
    public class QuoteSubscriberMetricsCollector
    {
        private static readonly TimeSpan MetricInterval = TimeSpan.FromMinutes(1);
        private readonly ConcurrentDictionary<string, Metric> _metricsForInterval =
            new ConcurrentDictionary<string, Metric>();

        private TimerTrigger _timer;
        private ILog _log;

        public QuoteSubscriberMetricsCollector(ILogFactory logFactory)
        {
            _timer = new TimerTrigger("QuoteSubscriberMetricsCollectorTimer", MetricInterval, logFactory,
                TimerHandler);
            _log = logFactory.CreateLog("QuoteSubscriberMetricsCollector");
            _timer.Start();
        }

        public void Log(long time, TickPrice tickPrice)
        {
            _metricsForInterval.AddOrUpdate(tickPrice.Source,
                s => new Metric
                {
                    Exchange = tickPrice.Source,
                    Count = 0,
                    SumTime = 0,
                    MaxTime = 0
                },
                (s, metric) =>
                {
                    metric.Count++;
                    metric.SumTime += time;
                    if (metric.MaxTime < time)
                    {
                        metric.MaxTime = time;
                    }

                    return metric;
                });
        }

        private Task TimerHandler(ITimerTrigger timer, TimerTriggeredHandlerArgs args,
            CancellationToken cancellationtoken)
        {
            List<Metric> data = _metricsForInterval.Values.ToList();
                _metricsForInterval.Clear();

            foreach (var metric in data)
            {
                _log.Info("Statistic by exchange", new
                {
                    IHEMetric = metric
                });
            }

            return Task.CompletedTask;
        }

        public class Metric
        {
            public string Exchange { get; set; }

            public bool IsEccxt => this.Exchange.Contains("(e)");

            public long Count { get; set; }

            public long MaxTime { get; set; }

            public decimal AvgTime => (decimal)this.SumTime / this.Count;

            public long SumTime { get; set; }
        }
    }
}