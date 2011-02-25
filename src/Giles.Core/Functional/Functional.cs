using System;
using System.Collections.Generic;

namespace Giles.Core.Functional
{
    public static class Functional
    {
        public static void Each<T>(this IEnumerable<T> things, Action<T> action)
        {
            if (things == null)
                return;

            foreach (var thing in things)
                action(thing);
        }
    }
}