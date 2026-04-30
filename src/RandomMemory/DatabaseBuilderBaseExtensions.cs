using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomMemory
{
    public static class DatabaseBuilderExtensions
    {
        public static void AppendDynamic(this DatabaseBuilderBase builder, Type dataType, IList<object> tableData)
        {
            var appendMethod = builder.GetType()
                .GetMethods()
                .Where(x => x.Name == "Append")
                .FirstOrDefault(x => x.GetParameters()[0].ParameterType.GetGenericArguments()[0] == dataType);

            if (appendMethod == null)
            {
                throw new InvalidOperationException("Append(IEnumerable<DataType>) can not found. DataType:" + dataType);
            }

            var dynamicArray = Array.CreateInstance(dataType, tableData.Count);
            for (int i = 0; i < tableData.Count; i++)
            {
                dynamicArray.SetValue(Convert.ChangeType(tableData[i], dataType), i);
            }

            appendMethod.Invoke(builder, [dynamicArray]);
        }
    }
}
