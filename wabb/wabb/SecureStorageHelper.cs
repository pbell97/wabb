using Newtonsoft.Json;
using Xamarin.Essentials;

namespace wabb
{
    // This class is intended to wrap serialization into the SecureStorage call
    //  may need additional testing to ensure that it works for all types
    class SecureStorageHelper
    {
        public T GetItem<T>(string name)
        {
            // Retrieve JsonConvert'ed object
            var jsonValue = SecureStorage.GetAsync(name).Result;
            if (jsonValue == null) return default(T);
            return JsonConvert.DeserializeObject<T>(jsonValue);
        }

        public bool StoreItem<T>(string name, T value)
        {
            try
            {
                // Try to store JsonConvert'ed object
                var jsonValue = JsonConvert.SerializeObject(value);
                SecureStorage.SetAsync(name, jsonValue);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool RemoveItem(string name)
        {
            return SecureStorage.Remove(name);
        }

        public void RemoveAllItems()
        {
            SecureStorage.RemoveAll();
        }
    }
}