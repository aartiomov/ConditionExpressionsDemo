﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ConditionExpressionsDemo.Core
{
    /// <summary>
    /// Helpers
    /// </summary>
    public static class ReflectionUtility
    {
        public static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
        {
            string retVal = null;
            if (propertyExpression != null)
            {
                var lambda = (LambdaExpression)propertyExpression;
                MemberExpression memberExpression;
                if (lambda.Body is UnaryExpression)
                {
                    var unaryExpression = (UnaryExpression)lambda.Body;
                    memberExpression = (MemberExpression)unaryExpression.Operand;
                }
                else
                {
                    memberExpression = (MemberExpression)lambda.Body;
                }
                retVal = memberExpression.Member.Name;

            }
            return retVal;
        }
        
        public static bool IsDerivativeOf(this Type type, Type typeToCompare)
        {
            if (type == null)
            {
                throw new NullReferenceException();
            }

            var retVal = type.BaseType != null;
            if (retVal)
            {
                retVal = type.BaseType == typeToCompare;
            }
            if (!retVal && type.BaseType != null)
            {
                retVal = type.BaseType.IsDerivativeOf(typeToCompare);
            }
            return retVal;
        }

        public static T[] GetFlatObjectsListWithInterface<T>(this object obj)
        {
            var retVal = new List<T>();

            var objectType = obj.GetType();

            if (objectType.GetInterface(typeof(T).Name) != null)
            {
                retVal.Add((T)obj);
            }

            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var objects = properties.Where(x => x.PropertyType.GetInterface(typeof(T).Name) != null)
                                    .Select(x => (T)x.GetValue(obj)).ToList();

            retVal.AddRange(objects.Where(x => x != null).SelectMany(x => x.GetFlatObjectsListWithInterface<T>()));

            var collections = properties.Select(x => x.GetValue(obj, null))
                                        .Where(x => x is IEnumerable && !(x is String))
                                        .Cast<IEnumerable>();

            foreach (var collection in collections)
            {
                foreach (var collectionObject in collection)
                {
                    if (collectionObject is T)
                    {
                        retVal.AddRange(collectionObject.GetFlatObjectsListWithInterface<T>());
                    }
                }
            }

            return retVal.ToArray();


        }

    }
}