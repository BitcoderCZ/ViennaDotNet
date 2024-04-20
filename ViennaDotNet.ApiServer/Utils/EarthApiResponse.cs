using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.ApiServer.Utils
{
    public class EarthApiResponse
    {
        public object result;
        public Dictionary<string, int>? updates = new Dictionary<string, int>();

        public EarthApiResponse(object _results)
        {
            result = _results;
        }

        public EarthApiResponse(object _results, Updates? _updates)
        {
            result = _results;
            if (_updates is null)
                updates = null;
            else
                updates.AddRange(_updates.map);
        }

        public class Updates
        {
            public Dictionary<string, int> map = new Dictionary<string, int>();
        }
    }

    public class EarthApiResponsePlus
    {
        public object result;
        public object? expiration;
        public object? continuationToken;
        public Dictionary<string, int>? updates = new Dictionary<string, int>();

        public EarthApiResponsePlus(object _results)
        {
            result = _results;
        }

        public EarthApiResponsePlus(object _results, Updates? _updates)
        {
            result = _results;
            if (_updates is null)
                updates = null;
            else
                updates.AddRange(_updates.map);
        }

        public class Updates
        {
            public Dictionary<string, int> map = new Dictionary<string, int>();
        }
    }
}
