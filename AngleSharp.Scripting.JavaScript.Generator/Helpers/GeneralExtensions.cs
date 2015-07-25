﻿namespace AngleSharp.Scripting.JavaScript.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    static class GeneralExtensions
    {
        public static IEnumerable<BindingClass> GetBindings(this IDictionary<String, List<Type>> mappings)
        {
            foreach (var mapping in mappings)
            {
                var name = mapping.Key;
                var types = mapping.Value;
                var type = types.Where(m => !m.IsEnum).FirstOrDefault() ?? types.First();
                
                if (type.GetDomNoInterfaceObjectAttribute() != null)
                    continue;

                var binding = new BindingClass(name, type.Name, type.Namespace, type.ResolveBase());
                yield return binding.GetClassBindings(types, name);
            }
        }
        
        static BindingClass GetClassBindings(this BindingClass binding, List<Type> types, String name)
        {
            foreach (var type in types)
            {
                binding.AttachProperties(type.GetAll(m => m.GetProperties()));
                binding.AttachEvents(type.GetAll(m => m.GetEvents()));
                binding.AttachMethods(type.GetAll(m => m.GetMethods()));
                binding.AttachFields(type.GetAll(m => m.GetFields()));
                binding.AttachConstructors(type.GetConstructors());
            }

            return binding;
        }

        static String ResolveBase(this Type type)
        {
            var name = GetDomNameOrNull(type.BaseType);

            if (name == null)
            {
                var interfaces = type.GetInterfaces();

                for (int i = 0; i < interfaces.Length; i++)
                {
                    name = GetDomNameOrNull(interfaces[i]);

                    if (name != null)
                        return name;
                }

                return "Object";
            }

            return name;
        }

        static String GetDomNameOrNull(Type type)
        {
            return type != null && type.GetDomNoInterfaceObjectAttribute() == null && type.GetDomNameAttributes().Any() ? type.GetDomNameAttributes().First().OfficialName : null;
        }
    }
}
