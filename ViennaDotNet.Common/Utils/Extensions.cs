using Newtonsoft.Json;

namespace ViennaDotNet.Common.Utils
{
    public static class Extensions
    {
        public static async Task<T?> AsJson<T>(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
                return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
        }
    }
}
