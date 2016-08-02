﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using Mono.Cecil;
namespace ILRuntime.CLR.Utils
{
    static class Extensions
    {
        static List<IType> emptyParamList = new List<IType>();
        public static List<IType> GetParamList(this MethodReference def, ILRuntime.Runtime.Enviorment.AppDomain appdomain, IType contextType)
        {
            if (def.HasParameters)
            {
                List<IType> param = new List<IType>();
                var dt = appdomain.GetType(def.DeclaringType, contextType);
                foreach (var i in def.Parameters)
                {
                    IType t = null;
                    t = appdomain.GetType(i.ParameterType, dt);
                    if (t == null && def.IsGenericInstance)
                    {
                        GenericInstanceMethod gim = (GenericInstanceMethod)def;
                        string name = i.ParameterType.FullName;
                        for (int j = 0; j < gim.GenericArguments.Count; j++)
                        {
                            var gp = gim.ElementMethod.GenericParameters[j];
                            var ga = gim.GenericArguments[j];
                            if(name == gp.Name)
                            {
                                t = appdomain.GetType(ga, contextType);
                                break;
                            }
                            else if (name.Contains(gp.Name))
                            {
                                name = name.Replace(gp.Name, ga.FullName);
                            }
                        }
                        if (t == null)
                            t = appdomain.GetType(name);
                    }
                    if (t == null)
                        throw new KeyNotFoundException("Cannot find type:" + i.ParameterType);
                    param.Add(t);
                }
                return param;
            }
            else
                return emptyParamList;
        }

        public static object CheckPrimitiveTypes(this Type pt, object obj)
        {
            if (pt.IsPrimitive && pt != typeof(int))
            {
                if (pt == typeof(bool) && !(obj is bool))
                {
                    obj = (int)obj == 1;
                }
                else if (pt == typeof(byte) && !(obj is byte))
                    obj = (byte)(int)obj;
                else if (pt == typeof(short) && !(obj is short))
                    obj = (short)(int)obj;
                else if (pt == typeof(char) && !(obj is char))
                    obj = (char)(int)obj;
                else if (pt == typeof(ushort) && !(obj is ushort))
                    obj = (ushort)(int)obj;
                else if (pt == typeof(uint) && !(obj is uint))
                    obj = (uint)(int)obj;
                else if (pt == typeof(sbyte) && !(obj is sbyte))
                    obj = (sbyte)(int)obj;
                else if (pt == typeof(ulong) && !(obj is ulong))
                {
                    obj = (ulong)(long)obj;
                }
            }
            else if (pt.IsByRef)
            {
                return CheckPrimitiveTypes(pt.GetElementType(), obj);
            }
            else if (pt.IsEnum)
            {
                return Enum.ToObject(pt, obj);
            }
            return obj;
        }
    }
}
