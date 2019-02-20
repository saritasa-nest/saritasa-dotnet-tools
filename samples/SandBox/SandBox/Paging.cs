using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Saritasa.Tools.Common.Pagination;

namespace SandBox
{
    /// <summary>
    /// Pagination samples.
    /// </summary>
    public static class Paging
    {
        [Serializable]
        private class ProductWrapper
        {
            public string Name { get; }

            public ProductWrapper(Product product)
            {
                this.Name = product.Name;
            }
        }

        public static void Try1()
        {
            var repository = new AnotherProductsRepository();
            var products = repository.GetAll();

            int offset = 0, limit = 10;
            var subset = OffsetLimitListFactory.FromSource(products, offset, limit);

            var all = PagedListFactory.FromSource(products, 2, 10);
            var all2 = all.Convert(p => new ProductWrapper(p));
            var dto = all2.ToMetadataObject();

            var bytes = new byte[0];
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, dto);
                ms.Flush();
                bytes = ms.ToArray();
            }
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                var dto2 = bf.Deserialize(ms) as MetadataDto<ProductWrapper, PagedListMetadata>;
            }

            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented, serializerSettings);
        }
    }
}
