using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AuthProject.WorkflowTest
{
    public delegate ParametersToObject InputTypeParametersToObject(params Type[] methodGenericTypes);

    public delegate object ParametersToObject(params object[] parameter);

    public delegate object GetPropertyValue(object entity);

    public static class ReflectionExtensions
    {
        public static MethodInfo GetMethodInfoByName(this Type type, string methodName)
        {
            return type.GetMethod(methodName);
        }

        public static Type[] GetParametersByMethodName(this Type type, string methodName)
        {
            return GetMethodInfoByName(type, methodName).GetParameters().Select(x => x.ParameterType).ToArray();
        }

        public static Type[] GetGenericTypesReturnValue(this MethodInfo methodInfo) =>
            methodInfo.ReturnParameter.ParameterType.GetGenericArguments();


        // attention!!! FP
        public static InputTypeParametersToObject Invoke(this Type typeEntity, object entity, string methodName) =>
            methodGenericTypes =>
                parameters =>
                    typeEntity.GetMethod(methodName).MakeGenericMethod(methodGenericTypes).Invoke(entity, parameters);

        //

        public static readonly Func<string,GetPropertyValue> GetPropertyValue = propertyName =>
            entity => entity.GetType().GetProperty(propertyName).GetValue(entity);
    }
}