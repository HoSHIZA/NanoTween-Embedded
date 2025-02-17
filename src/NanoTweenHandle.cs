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

using JetBrains.Annotations;
using UnityEngine;

namespace NanoTweenRootNamespace
{
    [PublicAPI]
    internal struct NanoTweenHandle
    {
        public readonly int Id;
        public readonly MonoBehaviour Owner;
        public readonly Coroutine Routine;
        
        public bool RunsAsCoroutine => Routine != null && Owner != null;
        public bool RunsAsUpdater => !RunsAsCoroutine && Id >= 0;
        
        public NanoTweenHandle(int id)
        {
            Id = id;
            Owner = null;
            Routine = null;
        }
        
        public NanoTweenHandle(MonoBehaviour owner, Coroutine routine)
        {
            Id = -1;
            Owner = owner;
            Routine = routine;
        }

        public bool IsValid()
        {
            return RunsAsUpdater || RunsAsCoroutine;
        }

        public static readonly NanoTweenHandle Invalid = new(-1);
    }
}