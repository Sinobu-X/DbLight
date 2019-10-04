using System;
using System.Collections.Concurrent;
using System.Reflection;
using DbLight.DataAnnotations;

namespace DbLight.Common
{
    public class DbModelHelper
    {
        private static ConcurrentDictionary<Type, DbModelInfo> _modelCaches = new ConcurrentDictionary<Type, DbModelInfo>();
        
        public static bool IsTuple(Type tuple){
            if (!tuple.IsGenericType){
                return false;
            }

            var openType = tuple.GetGenericTypeDefinition();
            return openType == typeof(ValueTuple<>)
                   || openType == typeof(ValueTuple<,>)
                   || openType == typeof(ValueTuple<,,>)
                   || openType == typeof(ValueTuple<,,,>)
                   || openType == typeof(ValueTuple<,,,,>)
                   || openType == typeof(ValueTuple<,,,,,>)
                   || openType == typeof(ValueTuple<,,,,,,>)
                   || (openType == typeof(ValueTuple<,,,,,,,>) && IsTuple(tuple.GetGenericArguments()[7]));
        }
        
        public static DbModelInfo GetModelInfo(Type type){
            if (_modelCaches.TryGetValue(type, out var modelInfo)){
                return modelInfo;
            }
            modelInfo = GetModelInfo(type, 0);
            _modelCaches[type] = modelInfo;
            return modelInfo;
        }

        private static DbModelInfo GetModelInfo(Type type, int level){
            var m = new DbModelInfo{
                Type = type
            };

            if (m.Type.IsPrimitive ||
                m.Type == typeof(string) ||
                m.Type == typeof(decimal) ||
                m.Type == typeof(DateTime) ||
                m.Type == typeof(DateTime?) ||
                m.Type == typeof(byte[])){
                m.Kind = DbModelKind.Value;
            }
            else if (IsTuple(m.Type)){
                m.Kind = DbModelKind.Tuple;
            }
            else if (m.Type.IsClass && m.Type.GetProperties().Length > 0){
                m.Kind = DbModelKind.Object;
            }
            else{
                m.Kind = DbModelKind.Other;
            }

            if (m.Kind == DbModelKind.Object){
                m.TableName = m.Type.Name;
                m.DatabaseName = "";

                var tableAttribute = m.Type.GetCustomAttribute<TableAttribute>();
                if (tableAttribute != null){
                    if (!string.IsNullOrEmpty(tableAttribute.Name)){
                        m.TableName = tableAttribute.Name;
                    }
                    if (!string.IsNullOrEmpty(tableAttribute.Database)){
                        m.DatabaseName = tableAttribute.Database;
                    }
                }
            }

            if (level <= 2){
                if (m.Kind == DbModelKind.Tuple){
                    foreach (var fieldInfo in m.Type.GetFields()){
                        var member = new DbMemberInfo{
                            MemberType = DbMemberType.Field,
                            ColumnName = fieldInfo.Name,
                            MemberName = fieldInfo.Name,
                            FieldInfo = fieldInfo,
                            Model = GetModelInfo(fieldInfo.FieldType, level + 1)
                        };

                        m.Members.Add(member);
                    }
                }
                else if (m.Kind == DbModelKind.Object){
                    foreach (var propertyInfo in m.Type.GetProperties()){
                        var member = new DbMemberInfo(){
                            MemberType = DbMemberType.Property,
                            ColumnName = propertyInfo.Name,
                            MemberName = propertyInfo.Name,
                            PropertyInfo = propertyInfo
                        };

                        var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                        if (columnAttribute != null){
                            if (!string.IsNullOrEmpty(columnAttribute.Name)){
                                member.ColumnName = columnAttribute.Name;
                            }
                            member.Identity = columnAttribute.Identity;
                        }
                        
                        var notMappedAttribute = propertyInfo.GetCustomAttribute<NotMappedAttribute>();
                        if (notMappedAttribute != null){
                            member.NotMapped = true;
                        }

                        member.Model = GetModelInfo(propertyInfo.PropertyType, level + 1);

                        m.Members.Add(member);
                    }
                }
            }

            return m;
        }
    }
}