using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Internal
{
    public static class TypesLoader
    {
        public static Type LoadType(string fullName, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var t = assembly.GetType(fullName, false, true);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }
    }
}
