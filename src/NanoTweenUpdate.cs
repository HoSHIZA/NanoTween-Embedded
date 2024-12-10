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
using System.Collections;
using System.Runtime.CompilerServices;
using NanoTweenRootNamespace.Extensions;
using UnityEngine;

namespace NanoTweenRootNamespace
{
    internal static class NanoTweenUpdate
    {
        private class DataWrapper<T> 
        {
            public NanoTweenData<T> Data;
        }
        
        public static Coroutine StartTweenCoroutine<T>(MonoBehaviour owner, in NanoTweenData<T> data)
        {
            var wrapper = new DataWrapper<T>
            {
                Data = data,
            };
            
            return owner.StartCoroutine(UpdateEnumerator(wrapper));
        }
        
        private static IEnumerator UpdateEnumerator<T>(DataWrapper<T> wrapper)
        {
            while (true)
            {
                if (wrapper.Data.Core.State is NTweenState.Idle)
                {
                    yield return null;
                }
                
                if (wrapper.Data.Core.State is NTweenState.Completed or NTweenState.Canceled)
                {
                    yield break;
                }
                
                if (wrapper.Data.Core.State is NTweenState.Scheduled)
                {
                    InitializeTween(wrapper);
                }
                
                UpdateTween(wrapper);
                
                yield return null;
            }
        }
        
        #region Update
        
        [MethodImpl(256)]
        private static void UpdateTween<T>(DataWrapper<T> wrapper)
        {
            ref var data = ref wrapper.Data;
            
            var delta = data.Core.TimeKind switch
            {
                TimeKind.Time => Time.deltaTime,
                _ => Time.unscaledDeltaTime,
            };
            
            data.Core.Time += delta * data.Core.PlaybackSpeed;
            
            if (data.Core.State is NTweenState.Delayed)
            {
                if (data.Core.Time >= data.Core.Delay)
                {
                    RunTween(wrapper);
                }

                return;
            }
            
            var totalDuration = data.Core.LoopCount > 0
                ? data.Core.AffectLoopsOnDuration
                    ? data.Core.Duration * data.Core.LoopCount 
                    : data.Core.Duration
                : double.PositiveInfinity;
            
            var loopDuration = data.Core.AffectLoopsOnDuration
                ? data.Core.Duration
                : data.Core.Duration / data.Core.LoopCount;
            
            if (data.Core.Time >= totalDuration)
            {
                CompleteTween(wrapper, totalDuration, loopDuration);
                return;
            }
            
            UpdateTweenValue(wrapper, loopDuration);
        }

        [MethodImpl(256)]
        private static void InitializeTween<T>(DataWrapper<T> wrapper)
        {
            ref var data = ref wrapper.Data;
            
            data.Core.State = data.Core.Delay > 0
                ? NTweenState.Delayed
                : NTweenState.Running;

            data.Callback.OnStartAction?.Invoke();

            if (data.Core.State is NTweenState.Running)
            {
                RunTween(wrapper);
            }
        }

        [MethodImpl(256)]
        private static void RunTween<T>(DataWrapper<T> wrapper)
        {
            ref var data = ref wrapper.Data;
            
            if (data.FromGetter != null)
            {
                data.From = data.FromGetter.Invoke();
            }

            if (data.ToGetter != null)
            {
                data.To = data.ToGetter.Invoke();
            }
            
            data.Core.State = NTweenState.Running;
            data.Callback.OnStartDelayedAction?.Invoke();
            
            if (data.Core.DelayMode is NDelayMode.AffectOnDuration)
            {
                data.Core.Time -= data.Core.Delay;
            }
        }

        [MethodImpl(256)]
        private static void UpdateTweenValue<T>(DataWrapper<T> wrapper, in float duration)
        {
            ref var data = ref wrapper.Data;
            
            var currentLoop = Mathf.FloorToInt((float)(data.Core.Time / duration));
            var timeInCurrentLoop = data.Core.Time % duration;

            var t = (float)(timeInCurrentLoop / duration);

            if (data.Core.LoopType is NLoopType.Yoyo && currentLoop % 2 == 1)
            {
                t = 1 - t;
            }
            
            data.Callback.InvokeUpdate(ref data, t);
        }

        [MethodImpl(256)]
        private static void CompleteTween<T>(DataWrapper<T> wrapper, double totalDuration, in float duration)
        {
            ref var data = ref wrapper.Data;
            
            data.Core.Time = totalDuration;

            UpdateTweenValue(wrapper, duration);

            data.Core.State = NTweenState.Completed;
            data.Callback.OnCompleteAction?.Invoke();
        }

        #endregion
    }
}
