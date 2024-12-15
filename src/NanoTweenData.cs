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
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using NanoTweenRootNamespace.Easing;
using Unity.Collections.LowLevel.Unsafe;

namespace NanoTweenRootNamespace
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NanoTweenData<T>
    {
        public NTweenState State;
        
        public NanoTweenDataCore Core;
        public NanoTweenCallbackData Callback;
        
        public T From;
        public T To;
        
        public Func<T> FromGetter;
        public Func<T> ToGetter;
        
        public Func<T, T, float, T> LerpFunction;

        [MethodImpl(256)]
        public T Lerp(float t)
        {
            return LerpFunction.Invoke(From, To, Core.Ease.Evaluate(t));
        }
        
        public static readonly NanoTweenData<T> Default = new()
        {
            Core = NanoTweenDataCore.Default,
            Callback = NanoTweenCallbackData.Default,
        };
    }
    
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NanoTweenDataCore
    {
        public NTweenState State;

        public EaseData Ease;
        
        public double Time;
        public float Duration;
        public float PlaybackSpeed;
        
        public TimeKind TimeKind;
        
        public NDelayType DelayType;
        public NDelayMode DelayMode;
        public float Delay;
        
        public NLoopType LoopType;
        public bool AffectLoopsOnDuration;
        public int LoopCount;

        private double? _totalDuration;
        private double? _loopDuration;
        
        public double TotalDuration => _totalDuration ??= LoopCount > 0
            ? AffectLoopsOnDuration
                ? Duration * LoopCount 
                : Duration
            : double.PositiveInfinity;
        
        public double LoopDuration => _loopDuration ??= AffectLoopsOnDuration 
            ? Duration 
            : Duration / LoopCount;

        public void ResetCalculatedDuration()
        {
            _totalDuration = null;
            _loopDuration = null;
        }
        
        public static readonly NanoTweenDataCore Default = new()
        {
            Ease = EaseData.Default,
            PlaybackSpeed = 1,
            AffectLoopsOnDuration = false,
            LoopCount = 1,
        };
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NanoTweenCallbackData
    {
        public byte CallbackStates;
        public object State1;
        public object State2;
        public object State3;
        
        public object ValueUpdateAction;
        
        public object OnUpdateAction;
        public Action OnStartAction;
        public Action OnStartDelayedAction;
        public Action OnCancelAction;
        public Action OnCompleteAction;
        public Action OnPauseAction;
        public Action OnResumeAction;
        
        [MethodImpl(256)]
        public void InvokeValueUpdate<T>(in T value)
        {
            switch (CallbackStates)
            {
                case 0: UnsafeUtility.As<object, Action<T>>(ref ValueUpdateAction)?.Invoke(value); break;
                case 1: UnsafeUtility.As<object, Action<T, object>>(ref ValueUpdateAction)?.Invoke(value, State1); break;
                case 2: UnsafeUtility.As<object, Action<T, object, object>>(ref ValueUpdateAction)?.Invoke(value, State1, State2); break;
                case 3: UnsafeUtility.As<object, Action<T, object, object, object>>(ref ValueUpdateAction)?.Invoke(value, State1, State2, State3); break;
            }
            
            UnsafeUtility.As<object, Action<T>>(ref OnUpdateAction)?.Invoke(value);
        }
        
        [MethodImpl(256)]
        public void SubscribeToOnUpdateAction<T>(Action<T> callback)
        {
            UnsafeUtility.As<object, Action<T>>(ref OnUpdateAction) += callback;
        }
        
        public static readonly NanoTweenCallbackData Default = new()
        {
        };
    }

    [PublicAPI]
    public enum NTweenState : byte
    {
        Idle,
        Scheduled,
        Delayed,
        Running,
        Completed,
        Canceled,
    }
    
    [PublicAPI]
    public enum NLoopType : byte
    {
        Restart,
        Yoyo,
    }
    
    [PublicAPI]
    public enum NDelayType : byte
    {
        FirstLoop,
        EveryLoop,
    }
    
    [PublicAPI]
    public enum NDelayMode : byte
    {
        AffectOnDuration,
        SkipValuesDuringDelayed,
    }
}