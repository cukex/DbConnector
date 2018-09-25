using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbConnector.Core.Extensions
{
    internal static class DbDataReaderExtensions
    {
        public class ColumnMap
        {
            public string PropName { get; set; }

            public bool IsPropNullable { get; set; }

            public string ColName { get; set; }

            public int? ColIndex { get; set; }
        }

        /// <summary>
        /// Creates a map list using the properties of the T type and the ColumnSchema.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="odr">The extended NpgsqlDataReader</param>
        /// <returns>A list with the ColumnMap objects.</returns>
        public static List<ColumnMap> GetColumnMaps<T>(this DbDataReader odr)
        {
            List<ColumnMap> map = new List<ColumnMap>();
            var cSchema = odr.GetColumnSchema();

            if (cSchema != null && cSchema.Any())
            {
                var props = typeof(T).GetProperties();

                foreach (var p in props)
                {
                    if (p.CanWrite && !p.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute)))
                    {
                        ColumnAttribute cAttr = p.GetCustomAttribute<ColumnAttribute>();

                        var dbCol = cSchema.FirstOrDefault(c => c.ColumnName.ToLower() == p.GetColumnAttributeName().ToLower());

                        if (dbCol != null)
                        {
                            map.Add(new ColumnMap
                            {
                                PropName = p.Name,
                                IsPropNullable = p.IsNullable(),
                                ColIndex = dbCol.ColumnOrdinal,
                                ColName = dbCol.ColumnName
                            });
                        }
                        else
                        {
#if DEBUG
                            Debug.WriteLine("Column name " + p.GetColumnAttributeName() + " not found in column schema for property " + p.PropertyType + " of object " + typeof(T));
#endif
                        }
                    }
                }
            }

            return map;
        }

        private static T GetObject<T>(this DbDataReader odr, List<ColumnMap> columnMaps = null)
        {
            columnMaps = columnMaps ?? odr.GetColumnMaps<T>();

            T obj = Activator.CreateInstance<T>();

            Type objType = obj.GetType();

            foreach (var map in columnMaps)
            {
                var pInfo = objType.GetProperty(map.PropName);

                if (pInfo != null)
                {
                    object value = map.ColIndex.HasValue ? odr[map.ColIndex.Value] : odr[map.ColName];

                    if (value != DBNull.Value)
                    {
                        if ((Nullable.GetUnderlyingType(pInfo.PropertyType) ?? pInfo.PropertyType) != value.GetType())
                        {
                            if (!(pInfo.PropertyType.IsEnum && value.GetType().IsNumeric()))
                            {
                                throw new Exception("Failed to map column " + map.ColName + " of type " + value.GetType() + " to property " + pInfo.Name + " of type " + pInfo.PropertyType);
                            }
                        }

                        pInfo.SetValue(obj, value);
                    }
                }
            }

            return obj;
        }

        public static T GetValue<T>(this DbDataReader odr)
        {
            T obj = default(T);
            Type objType = obj.GetType();

            object value = odr[0];

            if (value != DBNull.Value)
            {
                if ((Nullable.GetUnderlyingType(objType) ?? objType) != value.GetType())
                {
                    if (!(objType.IsEnum && value.GetType().IsNumeric()))
                    {
                        throw new Exception("Failed to map column " + odr.GetName(0) + " of type " + value.GetType() + " to object of type " + objType);
                    }
                }

                obj = (T)(value);
            }

            return obj;
        }

        public static T ToObject<T>(this DbDataReader odr, List<ColumnMap> columnMaps = null)
            where T : new()
        {
            return odr.GetObject<T>(columnMaps);
        }

        public static List<T> ToList<T>(this DbDataReader odr)
        {
            List<T> projectedData = new List<T>();

            if (odr.HasRows)
            {
                if (typeof(T) == typeof(Dictionary<string, object>))
                {
                    projectedData = (List<T>)Convert.ChangeType(odr.ToListDictionary(), typeof(List<Dictionary<string, object>>));
                }
                else if (typeof(T).IsClass)
                {
                    var columnMaps = odr.GetColumnMaps<T>();

                    while (odr.Read())
                    {
                        projectedData.Add(odr.GetObject<T>(columnMaps));
                    }
                }
                else
                {
                    while (odr.Read())
                    {
                        projectedData.Add(odr.GetValue<T>());
                    }
                }
            }

            return projectedData;
        }

        public static T ToSingle<T>(this DbDataReader odr)
        {
            T projectedData = default(T);

            if (odr.HasRows)
            {
                if (typeof(T) == typeof(DataTable))
                {
                    projectedData = (T)Convert.ChangeType(odr.ToDataTable(), typeof(T));
                }
                else if (typeof(T) == typeof(Dictionary<string, object>))
                {
                    projectedData = (T)Convert.ChangeType(odr.ToListDictionary().First(), typeof(T));
                }
                else if (typeof(T).IsClass)
                {
                    var columnMaps = odr.GetColumnMaps<T>();

                    odr.Read();

                    projectedData = odr.GetObject<T>(columnMaps);
                }
                else
                {
                    odr.Read();

                    projectedData = odr.GetValue<T>();
                }
            }

            return projectedData;
        }

        public static List<Dictionary<string, object>> ToListDictionary(
            this DbDataReader odr,
            List<string> columnNamesToInclude = null,
            List<string> columnNamesToExclude = null)
        {
            var projectedData = new List<Dictionary<string, object>>();

            if (odr.HasRows)
            {
                List<string> colNamesToUse = new List<string>();

                if (columnNamesToInclude != null && columnNamesToInclude.Any())
                {
                    colNamesToUse.AddRange(columnNamesToInclude);

                    if (columnNamesToExclude != null)
                    {
                        columnNamesToExclude.ForEach(n => colNamesToUse.Remove(n));
                    }
                }
                else if (columnNamesToExclude != null && columnNamesToExclude.Any())
                {
                    var colSchema = odr.GetColumnSchema();

                    foreach (var c in colSchema)
                    {
                        if (!columnNamesToExclude.Any(n => n == c.ColumnName))
                        {
                            colNamesToUse.Add(c.ColumnName);
                        }
                    }
                }
                else
                {
                    colNamesToUse.AddRange(odr.GetColumnSchema().Select(c => c.ColumnName));
                }


                if (colNamesToUse.Any())
                {
                    while (odr.Read())
                    {
                        var row = new Dictionary<string, object>();

                        foreach (var colname in colNamesToUse)
                        {
                            if (!row.ContainsKey(colname))
                            {
                                row.Add(colname, odr[colname]);
                            }
                        }

                        projectedData.Add(row);
                    }
                }
            }

            return projectedData;
        }

        public static DataTable ToDataTable(this DbDataReader odr)
        {
            var projectedData = new DataTable();

            if (odr.HasRows)
            {
                projectedData.Load(odr);
            }

            return projectedData;
        }
    }
}
