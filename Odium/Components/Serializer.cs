using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;

namespace Odium.Components
{
    public static class Serializer
    {
        public static byte[] Il2ToByteArray(this Il2CppSystem.Object obj)
        {
            if (obj == null) return null;
            try
            {
                var bf = new Il2CppSystem.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var ms = new Il2CppSystem.IO.MemoryStream();
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
            catch (Exception e)
            {
                OdiumConsole.LogException(e);
                return null;
            }
        }

        public static byte[] ManagedToByteArray(this object obj)
        {
            if (obj == null) return null;
            try
            {
                var bf = new BinaryFormatter();
                var ms = new MemoryStream();
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
            catch (Exception e)
            {
                OdiumConsole.LogException(e);
                return null;
            }
        }

        public static T FromByteArray<T>(this byte[] data)
        {
            if (data == null) return default;
            try
            {
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream(data))
                {
                    var obj = bf.Deserialize(ms);
                    return (T)obj;
                }
            }
            catch (Exception e)
            {
                OdiumConsole.LogException(e);
                return default;
            }
        }

        public static T IL2CPPFromByteArray<T>(this byte[] data)
        {
            if (data == null) return default;
            try
            {
                var bf = new Il2CppSystem.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var ms = new Il2CppSystem.IO.MemoryStream(data);
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
            catch (Exception e)
            {
                OdiumConsole.LogException(e);
                return default;
            }
        }

        public static T FromIL2CPPToManaged<T>(this Il2CppSystem.Object obj)
        {
            return FromByteArray<T>(Il2ToByteArray(obj));
        }

        public static T FromManagedToIL2CPP<T>(this object obj)
        {
            return IL2CPPFromByteArray<T>(ManagedToByteArray(obj));
        }

        public static object[] FromIL2CPPArrayToManagedArray(this Il2CppSystem.Object[] obj)
        {
            var Parameters = new object[obj.Length];
            for (var i = 0; i < obj.Length; i++)
                if (obj[i].GetIl2CppType().Attributes == Il2CppSystem.Reflection.TypeAttributes.Serializable)
                    Parameters[i] = FromIL2CPPToManaged<object>(obj[i]);
                else
                    Parameters[i] = obj[i];
            return Parameters;
        }

        public static Il2CppSystem.Object[] FromManagedArrayToIL2CPPArray(this object[] obj)
        {
            Il2CppSystem.Object[] Parameters = new Il2CppSystem.Object[obj.Length];
            for (var i = 0; i < obj.Length; i++)
            {
                if (obj[i].GetType().Attributes == TypeAttributes.Serializable)
                    Parameters[i] = FromManagedToIL2CPP<Il2CppSystem.Object>(obj[i]);
                else
                    Parameters[i] = (Il2CppSystem.Object)obj[i];
            }
            return Parameters;
        }
    }
}
