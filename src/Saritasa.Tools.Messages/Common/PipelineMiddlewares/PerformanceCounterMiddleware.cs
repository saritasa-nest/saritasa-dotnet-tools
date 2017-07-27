// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NET452
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Common.PipelineMiddlewares
{
    /// <summary>
    /// Represents performance counter that count total messages passed.
    /// </summary>
    public class PerformanceCounterMiddleware : IMessagePipelineMiddleware, IDisposable
    {
        /// <inheritdoc />
        public string Id { get; set; };

        /// <summary>
        /// Total processed messages counter.
        /// </summary>
        public const string TotalMessagesProcessed = "Total Messages Processed";

        /// <summary>
        /// Messages per second counter.
        /// </summary>
        public const string RateMessagesProcessed = "Messages per Second Processed";

        /// <summary>
        /// Average message processing duration counter.
        /// </summary>
        public const string AverageMessagesDuration = "Average Message Processing Duration";

        /// <summary>
        /// Base counter for average message execution duration counter.
        /// </summary>
        public const string AverageMessagesDurationBase = "Base Average Message Processing Duration";

        private bool initialized;

        private readonly string category;

        private PerformanceCounter performanceCounterTotal;
        private PerformanceCounter performanceCounterRate;
        private PerformanceCounter performanceCounterAvg;
        private PerformanceCounter performanceCounterAvgBase;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="dict">Parameters.</param>
        public PerformanceCounterMiddleware(IDictionary<string, string> dict)
        {
            if (dict.ContainsKey("id"))
            {
                Id = dict["id"];
            }
            else
            {
                Id = this.GetType().Name;
            }
            if (dict.ContainsKey("category"))
            {
                this.category = dict["category"];
            }
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="category">Performance counter category.</param>
        public PerformanceCounterMiddleware(string category = "Saritasa Tools Messages")
        {
            this.category = category;
            Id = this.GetType().Name;
        }

        void Initialize()
        {
            if (!PerformanceCounterCategory.Exists(category))
            {
                var counterDataCollection = new CounterCreationDataCollection();

                var counterTotal = new CounterCreationData(TotalMessagesProcessed, "Total processed messages by application",
                    PerformanceCounterType.NumberOfItems64);
                counterDataCollection.Add(counterTotal);
                var counterRate = new CounterCreationData(RateMessagesProcessed, "Average messages per second",
                    PerformanceCounterType.RateOfCountsPerSecond64);
                counterDataCollection.Add(counterRate);
                var counterAvg = new CounterCreationData(AverageMessagesDuration, "Average message processing time in ms",
                    PerformanceCounterType.AverageCount64);
                counterDataCollection.Add(counterAvg);
                var counterAvgBase = new CounterCreationData(AverageMessagesDurationBase,
                    "Base average message processing time in ms",
                    PerformanceCounterType.AverageBase);
                counterDataCollection.Add(counterAvgBase);

                PerformanceCounterCategory.Create(category, "Saritasa Tools Messages", PerformanceCounterCategoryType.SingleInstance,
                    counterDataCollection);
            }

            performanceCounterTotal = new PerformanceCounter(category, TotalMessagesProcessed, false);
            performanceCounterRate = new PerformanceCounter(category, RateMessagesProcessed, false);
            performanceCounterAvg = new PerformanceCounter(category, AverageMessagesDuration, false);
            performanceCounterAvgBase = new PerformanceCounter(category, AverageMessagesDurationBase, false);
            initialized = true;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            if (!initialized)
            {
                Initialize();
            }

            performanceCounterTotal.Increment();
            performanceCounterRate.Increment();
            if (messageContext.Items.TryGetValue(MessageContextConstants.ExecutionDurationKey, out object durationObj))
            {
                performanceCounterAvg.IncrementBy((long)durationObj);
            }
            performanceCounterAvgBase.Increment();
        }

        private bool disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object.
        /// </summary>
        /// <param name="disposing">Dispone managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (performanceCounterTotal != null)
                    {
                        performanceCounterTotal.Dispose();
                        performanceCounterTotal = null;
                    }
                    if (performanceCounterRate != null)
                    {
                        performanceCounterRate.Dispose();
                        performanceCounterRate = null;
                    }
                    if (performanceCounterAvg != null)
                    {
                        performanceCounterAvg.Dispose();
                        performanceCounterAvg = null;
                    }
                    if (performanceCounterAvgBase != null)
                    {
                        performanceCounterAvgBase.Dispose();
                        performanceCounterAvgBase = null;
                    }
                }
                disposed = true;
            }
        }
    }
}
#endif
