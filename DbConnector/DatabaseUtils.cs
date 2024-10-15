using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Xrmwise.Xrm.Framework
{
    public static class DatabaseUtils
    {
        public static SqlCommand GetProcCmd(this SqlConnection conn, string procName, int timeout, SqlParameter[] inputParams = null)
        {
            return GetSqlCmd(conn, procName, CommandType.StoredProcedure, timeout, inputParams);
        }

        public static SqlCommand GetTextCmd(this SqlConnection conn, string cmdText, int timeout, SqlParameter[] inputParams = null)
        {
            return GetSqlCmd(conn, cmdText, CommandType.Text, timeout, inputParams);
        }

        public static SqlDataReader ExecSqlCmd(SqlCommand cmd, SqlParameter[] inputParams = null)
        {
            if (inputParams != null && inputParams.Any())
            {
                cmd.Parameters.AddRange(inputParams);
                cmd.Prepare();
            }

            var dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                return dataReader;
            }

            dataReader.Close();
            return null;
        }
        public static T ExecScarlarCmd<T>(SqlCommand cmd, SqlParameter[] inputParams = null)
        {
            if (inputParams != null && inputParams.Any())
            {
                cmd.Parameters.AddRange(inputParams);
                cmd.Prepare();
            }

            var objValue = cmd.ExecuteScalar();
            return objValue == null ? default(T) : TypeConverter.ConvertTo<T>(objValue);
        }

        public static T GetSingleEntityByProc<T>(string connString, string procName, SqlParameter[] inputParams, int timeout, Func<SqlDataReader, T> toEntity)
        {
            return GetSingleEntity(connString, procName, inputParams, CommandType.StoredProcedure, timeout, toEntity);
        }

        public static List<T> GetMultipleEntitiesByProc<T>(string connString, string procName, SqlParameter[] inputParams, int timeout, Func<SqlDataReader, T> toEntity)
        {
            return GetMultipleEntities(connString, procName, inputParams, CommandType.StoredProcedure, timeout, toEntity);
        }

        public static T GetSingleEntityByCmd<T>(string connString, string cmdText, SqlParameter[] inputParams, int timeout, Func<SqlDataReader, T> toEntity)
        {
            return GetSingleEntity(connString, cmdText, inputParams, CommandType.Text, timeout, toEntity);
        }

        public static List<T> GetMultipleEntitiesByCmd<T>(string connString, string cmdText, SqlParameter[] inputParams, int timeout, Func<SqlDataReader, T> toEntity)
        {
            return GetMultipleEntities(connString, cmdText, inputParams, CommandType.Text, timeout, toEntity);
        }

        public static object ExecuteScalarText(string connString, string sqlText, SqlParameter[] inputParams, int timeout)
        {
            return ExecuteScalar(connString, sqlText, CommandType.Text, inputParams, timeout);
        }

        public static T ExecuteScalarText<T>(string connString, string sqlText, SqlParameter[] inputParams, int timeout)
        {
            var value = ExecuteScalar(connString, sqlText, CommandType.Text, inputParams, timeout);
            return TypeConverter.ConvertTo<T>(value);
        }

        public static T ExecuteScalarText<T>(SqlConnection sqlConnection, string sqlText, SqlParameter[] inputParams, int timeout, SqlTransaction transaction, bool useTransaction)
        {
            var value = ExecuteScalar(sqlConnection, sqlText, CommandType.Text, inputParams, timeout, transaction, useTransaction);
            return TypeConverter.ConvertTo<T>(value);
        }

        public static T ExecuteScalarProc<T>(string connString, string sqlText, SqlParameter[] inputParams, int timeout)
        {
            var value = ExecuteScalar(connString, sqlText, CommandType.StoredProcedure, inputParams, timeout);
            return TypeConverter.ConvertTo<T>(value);
        }

        public static T ExecuteScalarProc<T>(SqlConnection conn, string sqlText, SqlParameter[] inputParams, int timeout, SqlTransaction transaction)
        {
            var value = ExecuteScalar(conn, sqlText, CommandType.StoredProcedure, inputParams, timeout, transaction);
            return TypeConverter.ConvertTo<T>(value);
        }

        public static object ExecuteScalarText(SqlConnection conn, string sqlText, SqlParameter[] inputParams, int timeout, SqlTransaction transaction, bool useTransaction = true)
        {
            return ExecuteScalar(conn, sqlText, CommandType.Text, inputParams, timeout, transaction, useTransaction);
        }

        public static void ExecuteNonQueryText(string connString, string sqlText, SqlParameter[] inputParams, int timeout)
        {
            ExecuteNonQuery(connString, sqlText, CommandType.Text, inputParams, timeout);
        }
        public static void ExecuteNonQueryText(SqlConnection conn, string sqlText, SqlParameter[] inputParams, int timeout, SqlTransaction transaction, bool useTransaction = true)
        {
            ExecuteNonQuery(conn, sqlText, CommandType.Text, inputParams, timeout, transaction, useTransaction);
        }

        public static T GetFieldValue<T>(this SqlDataReader dataReader, string fieldName)
        {
            if (Enumerable.Range(0, dataReader.FieldCount).All(i => !string.Equals(dataReader.GetName(i), fieldName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return default(T);
            }

            return TypeConverter.ConvertTo<T>(dataReader[fieldName] == DBNull.Value ? null : dataReader[fieldName]);
        }

        private static SqlCommand GetSqlCmd(this SqlConnection conn, string cmdText, CommandType cmdType, int timeout, SqlParameter[] inputParams)
        {
            var cmd = new SqlCommand
            {
                CommandText = cmdText,
                CommandTimeout = timeout,
                CommandType = cmdType,
                Connection = conn
            };
            if (inputParams != null && inputParams.Any())
            {
                cmd.Parameters.AddRange(inputParams);
            }

            return cmd;
        }

        private static T GetSingleEntity<T>(string connString, string cmdText, SqlParameter[] inputParams, CommandType cmdType, int timeout, Func<SqlDataReader, T> toEntity)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                return GetSingleEntity(conn, cmdText, inputParams, cmdType, timeout, toEntity, null);
            }
        }

        public static List<T> GetMultipleEntities<T>(string connString, string cmdText, SqlParameter[] inputParams, CommandType cmdType, int timeout, Func<SqlDataReader, T> toEntity)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                return GetMultipleEntities(conn, cmdText, inputParams, cmdType, timeout, toEntity, null);
            }
        }

        public static object ExecuteScalar(string connString, string sqlText, CommandType cmdType, SqlParameter[] inputParams, int timeout)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                return ExecuteScalar(conn, sqlText, cmdType, inputParams, timeout, null);
            }
        }

        public static object ExecuteScalar(SqlConnection conn, string sqlText, CommandType cmdType, SqlParameter[] inputParams, int timeout, SqlTransaction transaction, bool useTransaction = true)
        {
            SqlTransaction tx = null;
            SqlCommand sqlCmd = null;
            var success = true;
            try
            {
                tx = useTransaction ? transaction ?? conn.BeginTransaction() : null;
                sqlCmd = GetSqlCmd(conn, sqlText, cmdType, timeout, inputParams);
                sqlCmd.Transaction = tx;

                if (inputParams != null && inputParams.Any())
                {
                    sqlCmd.Prepare();
                }

                var retval = sqlCmd.ExecuteScalar();
                return retval is DBNull ? null : retval;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                sqlCmd?.Dispose();
                if (transaction == null)
                {
                    if (success)
                    {
                        tx?.Commit();
                    }
                    else
                    {
                        tx?.Rollback();
                    }
                }
            }
        }
        public static void ExecuteNonQuery(string connString, string sqlText, CommandType cmdType, SqlParameter[] inputParams, int timeout)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                ExecuteNonQuery(conn, sqlText, cmdType, inputParams, timeout, null);
            }
        }

        public static List<T> GetMultipleEntities<T>(SqlConnection conn, string cmdText, SqlParameter[] inputParams, CommandType cmdType, int timeout, Func<SqlDataReader, T> toEntity, SqlTransaction transaction, bool useTransaction = true)
        {
            SqlTransaction tx = null;
            SqlCommand sqlCmd = null;
            SqlDataReader dataReader = null;
            var success = true;
            try
            {
                tx = useTransaction ? transaction ?? conn.BeginTransaction() : null;
                sqlCmd = conn.GetSqlCmd(cmdText, cmdType, timeout, inputParams);
                //sqlCmd.Parameters.Clear();
                sqlCmd.Transaction = tx;
                if (inputParams != null && inputParams.Any())
                {
                    sqlCmd.Prepare();
                }

                dataReader = sqlCmd.ExecuteReader();
                return dataReader.HasRows ? dataReader.ToEntities(toEntity) : new List<T>();
            }
            catch (Exception e)
            {
                success = false;
                throw;
            }
            finally
            {
                dataReader?.Close();
                sqlCmd?.Dispose();
                if (transaction == null)
                {
                    if (success)
                    {
                        tx?.Commit();
                    }
                    else
                    {
                        tx?.Rollback();
                    }
                }
            }
        }

        private static void ExecuteNonQuery(SqlConnection conn, string sqlText, CommandType cmdType, SqlParameter[] inputParams, int timeout, SqlTransaction transaction, bool useTransaction = true)
        {
            SqlTransaction tx = null;
            SqlCommand sqlCmd = null;
            var success = true;
            try
            {
                tx = useTransaction ? transaction ?? conn.BeginTransaction() : null;
                sqlCmd = GetSqlCmd(conn, sqlText, cmdType, timeout, inputParams);
                sqlCmd.Transaction = tx;
                if (inputParams != null && inputParams.Any())
                {
                    sqlCmd.Prepare();
                }
                sqlCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                sqlCmd?.Dispose();
                if (transaction == null)
                {
                    if (success)
                    {
                        tx?.Commit();
                    }
                    else
                    {
                        tx?.Rollback();
                    }
                }
            }
        }

        public static T GetSingleEntity<T>(SqlConnection conn, string cmdText, SqlParameter[] inputParams, CommandType cmdType, int timeout, Func<SqlDataReader, T> toEntity, SqlTransaction transaction, bool useTransaction = true)
        {
            SqlTransaction tx = null;
            SqlCommand sqlCmd = null;
            SqlDataReader dataReader = null;
            var success = true;
            try
            {
                tx = useTransaction ? transaction ?? conn.BeginTransaction() : null;
                sqlCmd = conn.GetSqlCmd(cmdText, cmdType, timeout, inputParams);
                sqlCmd.Transaction = tx;
                if (inputParams != null && inputParams.Any())
                {
                    //sqlCmd.Parameters.AddRange(inputParams);
                    sqlCmd.Prepare();
                }

                dataReader = sqlCmd.ExecuteReader();
                if (!dataReader.HasRows)
                {
                    return default(T);
                }

                dataReader.Read();
                return  toEntity(dataReader);
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                dataReader?.Close();
                sqlCmd?.Dispose();
                if (transaction == null)
                {
                    if (success)
                    {
                        tx?.Commit();
                    }
                    else
                    {
                        tx?.Rollback();
                    }
                }
            }
        }

        public static List<T> ToEntities<T>(this SqlDataReader dataReader, Func<SqlDataReader, T> toEntity)
        {
            var entityList = new List<T>();
            while (dataReader.Read())
            {
                var entity = toEntity(dataReader);
                entityList.Add(entity);
            }
            return entityList;
        }
    }
}
