using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ABC.Managers
{
    public class Utilities
    {
        public static object CleanProperties<T>(object baseobject)
        {
            Type type = baseobject.GetType();

            object newBase = (T)Activator.CreateInstance(typeof(T));
            Type newType = newBase.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.GetValue(baseobject, null) == null)
                {
                    foreach (PropertyInfo newprop in newType.GetProperties())
                    {
                        if (newprop.Name == property.Name)
                        {
                            property.SetValue(baseobject, newprop.GetValue(newBase));
                        }
                    }
                }
            }

            return baseobject;
        }
    }
}
