// Credit to Bruno Zell @ stack overflow for giving an example on how to use extensions for quickly grabbing private members via reflection.
// https://stackoverflow.com/questions/95910/find-a-private-field-with-reflection

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.GraphicsBuffer;

namespace InitialNobles
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets the value of a private field via reflection. T must be the type of the field being retrieved.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetPrivateField<T>(this object target, string fieldName)
        {
            var field = FindField(target, fieldName);

            if (field == null)
            { throw new MemberNotFoundException("The field '" + fieldName + "' could not be found on '" + target.GetType().Name + "'"); }

            return (T)field?.GetValue(target);
        }

        /// <summary>
        /// Sets the value of a private field via reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public static void SetPrivateField<T>(this object target, string fieldName, T value)
        {
            // This could be done without being generic, but by doing it generic we avoid boxing/unboxing.
            var field = FindField(target, fieldName);            

            if (field == null)
            { throw new MemberNotFoundException("The field '" + fieldName + "' could not be found on '" + target.GetType().Name + "'"); }

            field.SetValue(target, value);
        }

        private static FieldInfo FindField(object obj, string fieldName)
        {
            var objType = obj.GetType();
            var field = objType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null && objType != typeof(object))
            {
                var type = objType.BaseType;

                while (type != typeof(object))
                {
                    field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (field == null)
                    { type = type.BaseType; }
                    else
                    { break; }
                }
            }

            return field;
        }

        public static void SetPrivateProperty<T>(this object target, string fieldName, T value)
        {
            var property = FindProperty(target, fieldName);

            if (property == null)
            { throw new MemberNotFoundException("Could not find property '" + fieldName + "' on class '" + target.GetType().Name + "' or its base types."); }

            //Needed for public properties with private setters
            if (!property.CanWrite)
            { property = property.DeclaringType.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance); }


            property.SetValue(target, value);
        }

        private static PropertyInfo FindProperty(object obj, string fieldName)
        {
            var objType = obj.GetType();
            var property = objType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (property == null)
            {
                var type = objType.BaseType;

                while (type != typeof(object))
                {
                    property = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (property == null)
                    { type = type.BaseType; }
                    else
                    { break; }
                }
            }

            return property;
        }
    }
}
