﻿using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPush.Operators
{
    public class TakeUntilSubject<TIn, TFrom> : PushSubject<TIn>
    {
        private object _lockObject = new object();
        private IDisposable _disp1;
        private IDisposable _disp2;
        public TakeUntilSubject(IPushObservable<TIn> observable, IPushObservable<TFrom> fromObservable)
        {
            _disp1 = observable.Subscribe(PushValue, Complete, PushException);
            _disp2 = fromObservable.Subscribe(HandleOnPushTrigger);
        }

        private void HandleOnPushTrigger(TFrom obj)
        {
            lock (_lockObject)
            {
                Complete();
            }
        }
        public override void Dispose()
        {
            _disp1.Dispose();
            _disp2.Dispose();
            base.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TIn> TakeUntil<TIn, TFrom>(this IPushObservable<TIn> observable, IPushObservable<TFrom> fromObservable)
        {
            return new TakeUntilSubject<TIn, TFrom>(observable, fromObservable);
        }
    }
}
