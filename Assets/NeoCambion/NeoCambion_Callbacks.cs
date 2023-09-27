namespace NeoCambion
{
    using System;
    using System.Collections.Generic;

    public delegate void Callback();
    public delegate void Callback<T>(params T[] args);
    public delegate Treturn Callback<Targs, Treturn>(params Targs[] args);

    public class NamedCallback
    {
        public string name;
        private Callback callback;
        public NamedCallback(string name, Callback callback)
        {
            this.name = name;
            this.callback = callback;
        }

        public void Set(Callback callback) { this.callback = callback; }
        public void Invoke()
        {
            Ext_Callback.InvokeIfValid(callback);
        }
    }

    public static class Ext_Callback
    {
        public static void InvokeIfValid(Callback callback)
        {
            if (callback != null)
                callback.Invoke();
        }

        public static void Invoke(this Callback[] callbacks, int index = -1)
        {
            if (index > -1 && index < callbacks.Length)
            {
                InvokeIfValid(callbacks[index]);
            }
            else
            {
                foreach (Callback callback in callbacks)
                {
                    InvokeIfValid(callback);
                }
            }
        }

        public static void Invoke(this List<Callback> callbacks, int index = -1)
        {
            if (index > -1 && index < callbacks.Count)
            {
                InvokeIfValid(callbacks[index]);
            }
            else
            {
                foreach (Callback callback in callbacks)
                {
                    InvokeIfValid(callback);
                }
            }
        }

        public static void Invoke(this NamedCallback[] callbacks, int index = -1)
        {
            if (index > -1 && index < callbacks.Length)
            {
                callbacks[index].Invoke();
            }
            else
            {
                foreach (NamedCallback callback in callbacks)
                {
                    callback.Invoke();
                }
            }
        }

        public static void Invoke(this List<NamedCallback> callbacks, int index = -1)
        {
            if (index > -1 && index < callbacks.Count)
            {
                callbacks[index].Invoke();
            }
            else
            {
                foreach (NamedCallback callback in callbacks)
                {
                    callback.Invoke();
                }
            }
        }

        public static void Invoke(this NamedCallback[] callbacks, string callbackName)
        {
            if (callbackName != null)
            {
                foreach (NamedCallback callback in callbacks)
                {
                    if (callback.name == callbackName)
                        callback.Invoke();
                }
            }
            else
            {
                foreach (NamedCallback callback in callbacks)
                {
                    callback.Invoke();
                }
            }
        }

        public static void Invoke(this List<NamedCallback> callbacks, string callbackName)
        {
            if (callbackName != null)
            {
                foreach (NamedCallback callback in callbacks)
                {
                    if (callback.name == callbackName)
                        callback.Invoke();
                }
            }
            else
            {
                foreach (NamedCallback callback in callbacks)
                {
                    callback.Invoke();
                }
            }
        }

        public static int IndexOf(this NamedCallback[] callbacks, string callbackName)
        {
            for (int i = 0; i < callbacks.Length; i++)
            {
                if (callbackName == callbacks[i].name)
                    return i;
            }
            return -1;
        }

        public static int IndexOf(this List<NamedCallback> callbacks, string callbackName)
        {
            for (int i = 0; i < callbacks.Count; i++)
            {
                if (callbackName == callbacks[i].name)
                    return i;
            }
            return -1;
        }

        public static bool Contains(this NamedCallback[] callbacks, string callbackName)
        {
            foreach (NamedCallback callback in callbacks)
            {
                if (callbackName == callback.name)
                    return true;
            }
            return false;
        }

        public static bool Contains(this List<NamedCallback> callbacks, string callbackName)
        {
            foreach (NamedCallback callback in callbacks)
            {
                if (callbackName == callback.name)
                    return true;
            }
            return false;
        }
    }

    namespace Unity
    {
        using UnityEngine.Events;

        public static class UnityCallbackUtility
        {
            public static UnityAction UAction(this Callback callback) => new UnityAction(callback);
        }
    }
}