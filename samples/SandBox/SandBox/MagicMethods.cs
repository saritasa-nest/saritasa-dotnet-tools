using System;
using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools.Common.Extensions;

namespace SandBox
{
    /// <summary>
    /// C# shortcuts tests.
    /// </summary>
    public static class MagicMethods
    {
        public class ClassOfCollections
        {
            public List<int> ListOfInts { get; } = new List<int>();
        }

        public static void AddToCollections()
        {
            var obj1 = new ClassOfCollections
            {
                ListOfInts = { 1, 2, 3 }
            };

            var obj2 = new ClassOfCollections
            {
                ListOfInts =
                {
                    { new List<int>() { 1, 2, 3 }.Where(x => x > 0) },
                    { new int[] { 4, 5, 6 } }
                }
            };
        }
    }
}
