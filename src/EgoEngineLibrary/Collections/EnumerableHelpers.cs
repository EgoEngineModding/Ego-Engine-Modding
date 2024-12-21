// https://github.com/dotnet/runtime/blob/64f5b0a2643bf63b8b13cd2ed01b58d5629aa812/src/libraries/Common/src/System/Collections/Generic/EnumerableHelpers.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace EgoEngineLibrary.Collections;

internal class EnumerableHelpers
{
    /// <summary>Gets an enumerator singleton for an empty collection.</summary>
    internal static IEnumerator<T> GetEmptyEnumerator<T>() =>
        ((IEnumerable<T>)Array.Empty<T>()).GetEnumerator();
}
