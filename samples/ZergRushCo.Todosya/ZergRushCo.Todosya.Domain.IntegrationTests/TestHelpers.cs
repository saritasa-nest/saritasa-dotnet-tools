using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ZergRushCo.Todosya.Domain.IntegrationTests
{
    public static class TestHelpers
    {
        public static Stream GetManifestStream(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(
                $@"ZergRushCo.Todosya.Domain.IntegrationTests.{name}");
        }
    }
}
