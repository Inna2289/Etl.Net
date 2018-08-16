﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Core.StreamNodes
{
    public abstract class StreamNodeBase<TStream, TIn> : StreamNodeBase where TStream : IStream<TIn>
    {
        public TStream Input { get; private set; }
        public StreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath)
            : base(input.ExecutionContext, name, parentNodeNamePath)
        {
            this.Input = input;
        }
    }
    public abstract class StreamNodeBase<TStream, TIn, TArgs> : StreamNodeBase<TStream, TIn> where TStream : IStream<TIn>
    {
        public TArgs Arguments { get; private set; }
        public StreamNodeBase(TStream input, string name, IEnumerable<string> parentNodeNamePath, TArgs arguments)
            : base(input, name, parentNodeNamePath)
        {
            this.Arguments = arguments;
        }
    }
    public abstract class StreamNodeBase : INodeContext
    {
        private IExecutionContext _executionContext;
        public StreamNodeBase(IExecutionContext executionContext, string name, IEnumerable<string> parentNodeNamePath)
        {
            this._executionContext = executionContext;
            this.TypeName = this.GetType().Name;
            this.NodeNamePath = (parentNodeNamePath ?? new string[] { }).Concat(new[] { name }).ToArray();
            this.Tracer = new Tracer(executionContext, this);
        }

        public IEnumerable<string> NodeNamePath { get; private set; }
        public virtual string TypeName { get; private set; }

        protected ITracer Tracer { get; private set; }

        protected IStream<T> CreateStream<T>(string streamName, IPushObservable<T> observable)
        {
            return new Stream<T>(this.Tracer, this._executionContext, streamName, observable);
        }

        protected IDeferedPushObservable<TOut> CreateObservable<TIn, TOut>(TIn input, Action<TIn, Action<TOut>> populateObserver)
        {
            return new DeferedPushObservable<TOut>(i => populateObserver(input, i), true);
        }

        protected ISortedStream<T> CreateSortedStream<T>(string streamName, IPushObservable<T> observable, IEnumerable<ISortCriteria<T>> sortCriterias)
        {
            return new SortedStream<T>(this.Tracer, this._executionContext, streamName, observable, sortCriterias);
        }
        protected ISortedStream<T> CreateStream<T>(string streamName, IPushObservable<T> observable, ISortedStream<T> streamIn)
        {
            return new SortedStream<T>(this.Tracer, this._executionContext, streamName, observable, streamIn.SortCriterias);
        }
        protected IKeyedStream<T> CreateKeyedStream<T>(string streamName, IPushObservable<T> observable, IEnumerable<ISortCriteria<T>> sortCriterias)
        {
            return new KeyedStream<T>(this.Tracer, this._executionContext, streamName, observable, sortCriterias);
        }
        protected IKeyedStream<T> CreateStream<T>(string streamName, IPushObservable<T> observable, IKeyedStream<T> streamIn)
        {
            return new KeyedStream<T>(this.Tracer, this._executionContext, streamName, observable, streamIn.SortCriterias);
        }

        protected Func<TIn, ErrorManagementItem<TIn, TOut>> ErrorManagementWrapFunction<TIn, TOut>(Func<TIn, TOut> call)
        {
            return (TIn input) =>
            {
                try
                {
                    return new ErrorManagementItem<TIn, TOut>(input, call(input));
                }
                catch (Exception ex)
                {
                    return new ErrorManagementItem<TIn, TOut>(input, ex);
                }
            };
        }
        protected Func<TIn1, TIn2, ErrorManagementItem<TIn1, TIn2, TOut>> ErrorManagementWrapFunction<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> call)
        {
            return (TIn1 input1, TIn2 input2) =>
            {
                try
                {
                    return new ErrorManagementItem<TIn1, TIn2, TOut>(input1, input2, call(input1, input2));
                }
                catch (Exception ex)
                {
                    return new ErrorManagementItem<TIn1, TIn2, TOut>(input1, input2, ex);
                }
            };
        }
    }
}
