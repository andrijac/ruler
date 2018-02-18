using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ruler.Test
{
	public class Helper
	{
		/// <summary>
		/// Taken from https://stackoverflow.com/a/2444090/
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetPublicPropertiesFromInterface(Type type)
		{
			if (type.IsInterface)
			{
				var propertyInfos = new List<PropertyInfo>();

				var considered = new List<Type>();
				var queue = new Queue<Type>();

				considered.Add(type);
				queue.Enqueue(type);

				while (queue.Count > 0)
				{
					var subType = queue.Dequeue();

					foreach (var subInterface in subType.GetInterfaces())
					{
						if (considered.Contains(subInterface))
						{
							continue;
						}

						considered.Add(subInterface);
						queue.Enqueue(subInterface);
					}

					var typeProperties = subType.GetProperties(
						BindingFlags.FlattenHierarchy
						| BindingFlags.Public
						| BindingFlags.Instance);

					var newPropertyInfos = typeProperties
						.Where(x => !propertyInfos.Contains(x));

					propertyInfos.InsertRange(0, newPropertyInfos);
				}

				return propertyInfos.ToArray();
			}

			return type.GetProperties(BindingFlags.FlattenHierarchy
				| BindingFlags.Public | BindingFlags.Instance);
		}
	}
}