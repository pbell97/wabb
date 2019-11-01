using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Essentials;
using Newtonsoft.Json;

namespace wabb
{
    class SecureStorageHelper
    {
        public T GetItem<T>(string name)
        {
            var jsonValue = SecureStorage.GetAsync(name).Result;
            return JsonConvert.DeserializeObject<T>(jsonValue);
        }

        public bool StoreItem<T>(string name, T value)
        {
            try
            {
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