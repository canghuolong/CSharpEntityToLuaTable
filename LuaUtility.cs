/*
 * @Description C#实体转换为lua table
 * @Author SunShubin
 * @Time 2018-03-12
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class LuaUtility
{
    public static string ToLuaTable(object obj)
    {
        return Serializer.Serialize(obj);
    }

    sealed class Serializer
    {
        StringBuilder builder;
        Serializer()
        {
            builder = new StringBuilder();
        }

        public static string Serialize(object obj)
        {
            var instance = new Serializer();
            instance.SerializeValueRoot(obj);
            return instance.builder.ToString();
        }


        void SerializeValueRoot(object value)
        {
            var type = value.GetType();
            var attributes = type.GetCustomAttributes(true);
            var luaName = GetAttributeName((MemberInfo)type);

            if (string.IsNullOrEmpty(luaName))
            {
                Debug.LogError("need to add attribute to your class");
                return;
            }
            builder.Append("return {" + "\n");

            SerializeValue(value, false);

            builder.Append("\n}");
        }

        void SerializeValue(object value, bool isArray, FieldInfo fieldInfo = null)
        {
            if(value == null){
                return;
            }

            var type = value.GetType();
            var luaName = string.Empty;
            if (fieldInfo != null)
            {
                luaName = GetAttributeName(fieldInfo);
            }
            else
            {
                luaName = GetAttributeName(type);
            }

            if (string.IsNullOrEmpty(luaName))
            {
                return;
            }

            IList asList;
            IDictionary asDict;
            string asStr;
            if (value == null)
            {
                builder.Append(luaName + "=");
                builder.Append("nil");
            }
            else if ((asStr = value as string) != null)
            {
                builder.Append(luaName + "=");
                builder.Append('"');
                builder.Append(asStr);
                builder.Append('"');
                builder.Append(',');
            }
            else if ((asList = value as IList) != null)
            {
                builder.Append("\n" + luaName + "={\n");
                foreach (var v in asList)
                {
                    SerializeValue(v, true);
                }
                builder.Append("},");
            }
            else if ((asDict = value as IDictionary) != null)
            {
                builder.Append(luaName + "={\n");
                foreach(var v in asDict){
                    var kv = (DictionaryEntry)v;
                    if(isNumber(kv.Key)){
                        builder.Append("[" + kv.Key +"]=");
                    }else{
                        builder.Append(kv.Key.ToString() + "=");
                    }
                    SerializeValue(kv.Value,true);
                }
                builder.Append("},");
            }
            else if (isNumber(value))
            {
                builder.Append(luaName + "=");
                builder.Append(value);
                builder.Append(',');
            }
            else if (value is float)
            {
                builder.Append(luaName + "=");
                builder.Append(((float)value).ToString("R"));
                builder.Append(',');
            }
            else if (value is Vector3)
            {
                var vector3 = (Vector3)value;
                builder.Append(luaName + "=");
                builder.Append("{" + Mathf.RoundToInt(vector3.x) + "," + Mathf.RoundToInt(vector3.y) + "," + Mathf.RoundToInt(vector3.z) + "," + "}");
                builder.Append(',');
            }
            else if (value is Boolean)
            {
                builder.Append((bool)value ? "true" : "false");
                builder.Append(',');
            }
            else
            {
                if (isArray)
                {
                    builder.Append("{");
                    var fieldInfos = type.GetFields();
                    foreach (var v in fieldInfos)
                    {
                        SerializeValue(v.GetValue(value), false, v);
                    }
                }
                else
                {
                    builder.Append(luaName);
                    builder.Append("={\n");
                    var fieldInfos = type.GetFields();
                    foreach (var v in fieldInfos)
                    {
                        SerializeValue(v.GetValue(value), false, v);
                    }
                }
                if (isArray)
                {
                    builder.Append("},");
                }
                else
                {
                    builder.Append("\n},");
                }

            }
        }

        bool isNumber(object value)
        {
            if (value is int
                 || value is uint
                 || value is long
                 || value is sbyte
                 || value is byte
                 || value is short
                 || value is ushort
               || value is ulong)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        string GetAttributeName(MemberInfo memberInfo)
        {
            string attributeName = string.Empty;
            var attributes = memberInfo.GetCustomAttributes(true);
            LuaAttribute luaAttribute = null;
            foreach (var v in attributes)
            {
                luaAttribute = v as LuaAttribute;
                if (luaAttribute != null)
                {
                    if (luaAttribute.useDefaultName)
                    {
                        attributeName = memberInfo.Name;
                    }
                    else
                    {
                        attributeName = luaAttribute.luaName;
                    }
                    break;
                }
            }
            return attributeName;
        }

    }
}

