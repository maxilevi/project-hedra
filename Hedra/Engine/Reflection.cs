﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hedra.Engine
{
    public static class Reflection
    {
        public static Type[] GetTypesInNamespace(Assembly Assembly, string NameSpace)
        {
            return
                Assembly.GetTypes()
                    .Where(t => String.Equals(t.Namespace, NameSpace, StringComparison.Ordinal))
                    .ToArray();
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly Assembly, string Namespace = null)
        {
            IEnumerable<Type> types;
            try
            {
                types = Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }
            return Namespace != null ? types.Where(T => string.Equals(T.Namespace, Namespace, StringComparison.InvariantCulture)) : types;
        }
    }
}