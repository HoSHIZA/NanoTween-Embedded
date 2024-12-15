//
//   A tweening system for asset embedding.
//
//   https://github.com/HoSHIZA/NanoTween
//   
//   Copyright (c) 2024 HoSHIZA
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in all
//   copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//   SOFTWARE.

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NanoTweenRootNamespace.Easing;
using NanoTweenRootNamespace.Extensions;
using NanoTweenRootNamespace.Pooling.Buffer;
using UnityEngine;

namespace NanoTweenRootNamespace
{
    internal sealed class NanoTweenBuilderBuffer<T> : AbstractPooledBuffer<NanoTweenBuilderBuffer<T>>
    {
        public NanoTweenData<T> Data;
        public MonoBehaviour Owner;
        
        public bool BindOnSchedule;
        public bool ScheduleOnBind;
        
        public bool Preserve;
        
        protected override void Reset()
        {
            Data = NanoTweenData<T>.Default;
            
            ScheduleOnBind = true;
            BindOnSchedule = true;
            
            Preserve = false;
        }
    }
    
    [PublicAPI]
    internal struct NanoTweenBuilder<T> : IDisposable
    {
        internal readonly ushort Revision;
        internal NanoTweenBuilderBuffer<T> Buffer;
        
        internal NanoTweenBuilder(NanoTweenBuilderBuffer<T> buffer)
        {
            Revision = buffer.Revision;
            Buffer = buffer;
        }
        
        #region Creation
        
        [MethodImpl(256)]
        public static NanoTweenBuilder<T> Create(MonoBehaviour owner, T from, T to, float duration, Func<T, T, float, T> lerp)
        {
            var buffer = NanoTweenBuilderBuffer<T>.GetPooled();
            
            buffer.Owner = owner;
            
            buffer.Data.From = from;
            buffer.Data.To = to;
            buffer.Data.Core.Duration = duration;
            
            buffer.Data.LerpFunction = lerp;
            
            return new NanoTweenBuilder<T>(buffer);
        }
        
        [MethodImpl(256)]
        public static NanoTweenBuilder<T> Create(T from, T to, float duration, Func<T, T, float, T> lerp)
        {
            var buffer = NanoTweenBuilderBuffer<T>.GetPooled();
            
            buffer.Data.From = from;
            buffer.Data.To = to;
            buffer.Data.Core.Duration = duration;
            
            buffer.Data.LerpFunction = lerp;
            
            return new NanoTweenBuilder<T>(buffer);
        }
        
        #endregion
        
        #region Building - From, To
        
        /// <summary>
        /// Sets the value of <c>From</c>.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> From(T value)
        {
            ValidateBuffer();
            
            Buffer.Data.FromGetter = null;
            Buffer.Data.From = value;
            
            return this;
        }
        
        /// <summary>
        /// Sets the <c>From</c> value using <c>getter</c> at the time the tween starts after the delay.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> From(Func<T> getter)
        {
            ValidateBuffer();

            Buffer.Data.FromGetter = getter;
            Buffer.Data.From = default;
            
            return this;
        }
        
        /// <summary>
        /// Sets the value of <c>To</c>.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> To(T value)
        {
            ValidateBuffer();

            Buffer.Data.ToGetter = null;
            Buffer.Data.To = value;
            
            return this;
        }
        
        /// <summary>
        /// Sets the <c>To</c> value using <c>getter</c> at the time the tween starts after the delay.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> To(Func<T> getter)
        {
            ValidateBuffer();
            
            Buffer.Data.ToGetter = getter;
            Buffer.Data.To = default;
            
            return this;
        }
        
        #endregion
        
        #region Building - With{...}
        
        /// <summary>
        /// Sets <c>Ease</c> using Enum.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithEase(Ease ease)
        {
            ValidateBuffer();

            Buffer.Data.Core.Ease = ease;
            
            return this;
        }
        
        /// <summary>
        /// Sets <c>Ease</c> with Enum, using type and power.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithEase(EaseType type, EasePower power)
        {
            ValidateBuffer();
            
            Buffer.Data.Core.Ease = EaseUtility.GetFunction(type, power);
            
            return this;
        }
        
        /// <summary>
        /// Sets <c>Ease</c> with <see cref="EaseData"/>.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithEase(EaseData ease)
        {
            ValidateBuffer();
            
            Buffer.Data.Core.Ease = ease;
            
            return this;
        }
        
        /// <summary>
        /// Sets <c>Ease</c> with AnimationCurve.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithEase(AnimationCurve curve)
        {
            ValidateBuffer();

            Buffer.Data.Core.Ease = curve;
            
            return this;
        }
        
        /// <summary>
        /// Sets <c>Ease</c> using the easing function.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithEase(Func<float, float> easeFunction)
        {
            ValidateBuffer();
            
            Buffer.Data.Core.Ease = easeFunction;
            
            return this;
        }
        
        /// <summary>
        /// Sets the delay duration and delay type.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithDelay(float delay, NDelayType delayType = NDelayType.FirstLoop, 
            NDelayMode delayMode = NDelayMode.AffectOnDuration)
        {
            ValidateBuffer();
            
            Buffer.Data.Core.Delay = delay;
            Buffer.Data.Core.DelayType = delayType;
            Buffer.Data.Core.DelayMode = delayMode;
            
            return this;
        }
        
        /// <summary>
        /// Sets the count and type of loops.
        /// </summary>
        /// <param name="loops">Loop count.</param>
        /// <param name="loopType">Loop Type.</param>
        /// <param name="affectOnDuration">If <c>true</c>, the <c>duration</c> is incremented by the number of <c>loops</c>, otherwise the <c>duration</c> is the <c>total duration</c>.</param>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithLoops(int loops, NLoopType loopType = NLoopType.Restart,
            bool affectOnDuration = true)
        {
            ValidateBuffer();
            
            Buffer.Data.Core.LoopCount = Math.Max(0, loops);
            Buffer.Data.Core.LoopType = loopType;
            Buffer.Data.Core.AffectLoopsOnDuration = affectOnDuration;
            
            return this;
        }

        /// <summary>
        /// Sets the TimeKind.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithTimeKind(TimeKind timeKind)
        {
            ValidateBuffer();
            
            Buffer.Data.Core.TimeKind = timeKind;
            
            return this;
        }

        /// <summary>
        /// Sets the playback speed.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> WithPlaybackSpeed(float speed)
        {
            ValidateBuffer();
            
            Buffer.Data.Core.PlaybackSpeed = Mathf.Max(speed, 0f);
            
            return this;
        }

        /// <summary>
        /// Specifies whether to set the <c>From</c> value after binding.
        /// </summary>
        [MethodImpl(256)]
        public NanoTweenBuilder<T> WithBindOnSchedule(bool enable)
        {
            ValidateBuffer();
            
            Buffer.BindOnSchedule = enable;
            
            return this;
        }
        
        /// <summary>
        /// Specifies whether to start the tween immediately after binding.
        /// </summary>
        [MethodImpl(256)]
        public NanoTweenBuilder<T> WithScheduleOnBind(bool enable)
        {
            ValidateBuffer();

            Buffer.ScheduleOnBind = enable;
            
            return this;
        }
        
        #endregion
        
        #region Building - On{...}
        
        /// <summary>
        /// Subscribes to <c>OnUpdateAction</c>. Called every time the tween is updated. Does not consider the delay.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> OnUpdate([NotNull] Action<T> callback)
        {
            ValidateBuffer();
            
            Buffer.Data.Callback.SubscribeToOnUpdateAction(callback);
            
            return this;
        }
        
        /// <summary>
        /// Subscribes to <c>OnStartAction</c>. Called when the tween prepared to start.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> OnStart([NotNull] Action callback)
        {
            ValidateBuffer();
            
            Buffer.Data.Callback.OnStartAction += callback;
            
            return this;
        }
        
        /// <summary>
        /// Subscribes to <c>OnStartDelayedAction</c>. Called when the tween starts after delay.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> OnStartDelayed([NotNull] Action callback)
        {
            ValidateBuffer();
            
            Buffer.Data.Callback.OnStartDelayedAction += callback;
            
            return this;
        }
        
        /// <summary>
        /// Subscribes to <c>OnCancelAction</c>. Called when the tween is canceled.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> OnCancel([NotNull] Action callback)
        {
            ValidateBuffer();
            
            Buffer.Data.Callback.OnCancelAction += callback;
            
            return this;
        }

        /// <summary>
        /// Subscribes to <c>OnCompleteAction</c>. Called when the tween completes.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> OnComplete([NotNull] Action callback)
        {
            ValidateBuffer();
            
            Buffer.Data.Callback.OnCompleteAction += callback;
            
            return this;
        }
        
        /// <summary>
        /// Subscribes to <c>OnPauseAction</c>. Called when the tween is paused.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> OnPause([NotNull] Action callback)
        {
            ValidateBuffer();
            
            Buffer.Data.Callback.OnPauseAction += callback;
            
            return this;
        }
        
        /// <summary>
        /// Subscribes to <c>OnPauseAction</c>. Called when the tween is resumed.
        /// </summary>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> OnResume([NotNull] Action callback)
        {
            ValidateBuffer();
            
            Buffer.Data.Callback.OnResumeAction += callback;
            
            return this;
        }
        
        #endregion
        
        #region Building - Other
        
        /// <summary>
        /// Does not call <c>Dispose()</c> on binding if <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <c>Dispose()</c> must be called manually to return the buffer to the pool.
        /// </remarks>
        [MethodImpl(256)]
        public readonly NanoTweenBuilder<T> Preserve(bool preserve = true)
        {
            ValidateBuffer();
            
            Buffer.Preserve = preserve;
            
            return this;
        }
        
        #endregion
        
        #region Bind
        
        /// <summary>
        /// Run tween with target binding.
        /// </summary>
        public NanoTweenHandle Run<TTarget>(TTarget target, Action<T, TTarget> action)
            where TTarget : class
        {
            ValidateBuffer();
            
            if (Buffer.ScheduleOnBind)
            {
                Buffer.Data.Core.State = NTweenState.Scheduled;
            }
            
            SetCallbackData(target, action);
            
            return Schedule();
        }
        
        /// <summary>
        /// Run tween without binding.
        /// </summary>
        public NanoTweenHandle RunWithoutBinding()
        {
            ValidateBuffer();
            
            if (Buffer.ScheduleOnBind)
            {
                Buffer.Data.Core.State = NTweenState.Scheduled;
            }
            
            return Schedule();
        }
        
        /// <summary>
        /// Run tween with data binding.
        /// </summary>
        public NanoTweenHandle Bind(Action<T> action)
        {
            ValidateBuffer();
            
            if (Buffer.ScheduleOnBind)
            {
                Buffer.Data.Core.State = NTweenState.Scheduled;
            }
            
            SetCallbackData(action);
            
            return Schedule();
        }
        
        /// <summary>
        /// Run tween with data binding. Does not allocate garbage when binding.
        /// </summary>
        public NanoTweenHandle BindWithState<TState>(TState state, Action<T, TState> action) 
            where TState : class
        {
            ValidateBuffer();
            
            if (Buffer.ScheduleOnBind)
            {
                Buffer.Data.Core.State = NTweenState.Scheduled;
            }
            
            SetCallbackData(state, action);
            
            return Schedule();
        }
        
        /// <summary>
        /// Run tween with data binding. Does not allocate garbage when binding.
        /// </summary>
        public NanoTweenHandle BindWithState<TState1, TState2>(TState1 state1, TState2 state2, Action<T, TState1, TState2> action) 
            where TState1 : class
            where TState2 : class
        {
            ValidateBuffer();
            
            if (Buffer.ScheduleOnBind)
            {
                Buffer.Data.Core.State = NTweenState.Scheduled;
            }

            SetCallbackData(state1, state2, action);
            
            return Schedule();
        }
        
        /// <summary>
        /// Run tween with data binding. Does not allocate garbage when binding.
        /// </summary>
        public NanoTweenHandle BindWithState<TState1, TState2, TState3>(TState1 state1, TState2 state2, TState3 state3, 
            Action<T, TState1, TState2, TState3> action) 
            where TState1 : class
            where TState2 : class
            where TState3 : class
        {
            ValidateBuffer();
            
            if (Buffer.ScheduleOnBind)
            {
                Buffer.Data.Core.State = NTweenState.Scheduled;
            }
            
            SetCallbackData(state1, state2, state3, action);
            
            return Schedule();
        }
        
        [MethodImpl(256)]
        internal readonly void SetCallbackData(Action<T> action)
        {
            Buffer.Data.Callback.ValueUpdateAction = action;
        }
        
        [MethodImpl(256)]
        internal readonly void SetCallbackData<TState>(TState state, Action<T, TState> action)
            where TState : class
        {
            Buffer.Data.Callback.CallbackStates = 1;
            Buffer.Data.Callback.State1 = state;
            Buffer.Data.Callback.ValueUpdateAction = action;
        }
        
        [MethodImpl(256)]
        internal readonly void SetCallbackData<TState1, TState2>(TState1 state1, TState2 state2, Action<T, TState1, TState2> action)
            where TState1 : class
            where TState2 : class
        {
            Buffer.Data.Callback.CallbackStates = 2;
            Buffer.Data.Callback.State1 = state1;
            Buffer.Data.Callback.State2 = state2;
            Buffer.Data.Callback.ValueUpdateAction = action;
        }
        
        [MethodImpl(256)]
        internal readonly void SetCallbackData<TState1, TState2, TState3>(TState1 state1, TState2 state2, TState3 state3, 
            Action<T, TState1, TState2, TState3> action)
            where TState1 : class
            where TState2 : class
            where TState3 : class
        {
            Buffer.Data.Callback.CallbackStates = 3;
            Buffer.Data.Callback.State1 = state1;
            Buffer.Data.Callback.State2 = state2;
            Buffer.Data.Callback.State3 = state3;
            Buffer.Data.Callback.ValueUpdateAction = action;
        }
        
        #endregion
        
        private NanoTweenHandle Schedule()
        {
            if (Buffer.BindOnSchedule && Buffer.Data.Callback.ValueUpdateAction != null)
            {
                Buffer.Data.Callback.InvokeStart(ref Buffer.Data);
            }
            
            var handle = !Buffer.Owner
                ? NanoTweenUpdate.Run(Buffer.Data) 
                : NanoTweenUpdate.RunAsCoroutine(Buffer.Owner, Buffer.Data);
            
            if (!Buffer.Preserve)
            {
                Dispose();
            }

            return handle;
        }
        
        public void Dispose()
        {
            if (Buffer == null) return;
            
            NanoTweenBuilderBuffer<T>.Release(Buffer);
            
            Buffer = null;
        }
        
        [MethodImpl(256)]
        private readonly void ValidateBuffer()
        {
            if (Buffer == null || Buffer.Revision != Revision)
            {
                throw new InvalidOperationException($"[NanoTween] NanoTweenBuilder<{typeof(T).Name}> is not initialized before execution, or binding has already been done. Use Preserve() to reuse the builder.");
            }
        }
    }
}