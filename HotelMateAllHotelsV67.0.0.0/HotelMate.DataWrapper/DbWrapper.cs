using HotelMate.DataWrapper.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelMate.DataWrapper
{
    public class DbWrapperTest : IDbWrapper
    {
        private readonly string _connectionString;
        private readonly SqlConnectionStringBuilder _sqlConnectiontringBuilder;


        public DbWrapperTest(string connectionString)
        {
            _connectionString = connectionString;
        }
           

        public DbWrapperTest(string sqlServerInstanceName, string dataBaseName)
        {
            _sqlConnectiontringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = sqlServerInstanceName,
                InitialCatalog = dataBaseName,
                IntegratedSecurity = true
            };

            // enable integratedsecurity if there is no user name or password
            _connectionString = _sqlConnectiontringBuilder.ConnectionString;
        }

        public DbWrapperTest(string sqlServerInstanceName, string dataBaseName, string dbUserName, string dbPassword)
        {
            _sqlConnectiontringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = sqlServerInstanceName,
                InitialCatalog = dataBaseName,
                UserID = dbUserName,
                Password = dbPassword
            };

            _connectionString = _sqlConnectiontringBuilder.ConnectionString;
        }


        public DataSet GetDataSet(SqlCommand sqlCommand)
        {
            DataSet ds;

            try
            {
                SqlConnection connection;

                using (connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    sqlCommand.Connection = connection;

                    sqlCommand.CommandTimeout = 5;

                    var da = new SqlDataAdapter(sqlCommand);

                    ds = new DataSet();

                    da.Fill(ds);

                }
            }
            catch (Exception exception)
            {
                ds = null;
                //_logWriter.Write("Dal.DbWrapper.GetDataSet", "General", 1, 1, TraceEventType.Error, "Dal.DbWrapper.GetDataSet", _loggerFormatter.CreateLoggerDictionary(exception));
            }

            return ds;
        }


        private static SqlParameter[] GetParametersFromObject(object paramObject)
        {
            return paramObject.GetType().GetProperties().Select(propInfo => new SqlParameter(propInfo.Name, propInfo.GetValue(paramObject, null))).ToArray();
        }

        public DataSet GetDataSet(SqlCommand sqlCommand, List<SqlParameter> parameters)
        {
            DataSet ds;

            try
            {
                SqlConnection connection;

                using (connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    sqlCommand.Connection = connection;

                    if (null != parameters && parameters.Count > 0)
                        sqlCommand.Parameters.AddRange(parameters.ToArray());

                    sqlCommand.CommandTimeout = 5;

                    var da = new SqlDataAdapter(sqlCommand);
                    ds = new DataSet();

                    da.Fill(ds);
                }
            }
            catch (Exception exception)
            {
                ds = null;
                //_logWriter.Write("Dal.DbWrapper.GetDataSet", "General", 1, 1, TraceEventType.Error, "Dal.DbWrapper.GetDataSet", _loggerFormatter.CreateLoggerDictionary(exception));
            }

            return ds;
        }



        public string GetData(SqlCommand sqlCommand, object paramObject)
        {
            string data;

            try
            {
                SqlConnection connection;
                using (connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    sqlCommand.Connection = connection;

                    sqlCommand.Parameters.AddRange(GetParametersFromObject(paramObject));

                    sqlCommand.CommandTimeout = 5;

                    data = sqlCommand.ExecuteScalar().ToString();
                }
            }
            catch (Exception exception)
            {
                data = string.Empty;
                //_logWriter.Write("Dal.DbWrapper.GetData", "General", 1, 1, TraceEventType.Error, "Dal.DbWrapper.GetData", _loggerFormatter.CreateLoggerDictionary(exception));
            }

            return data;
        }

        public DataSet GetDataSet(SqlCommand sqlCommand, object paramObject)
        {
            DataSet ds;

            try
            {
                SqlConnection connection;

                using (connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    sqlCommand.Connection = connection;

                    sqlCommand.Parameters.AddRange(GetParametersFromObject(paramObject));

                    sqlCommand.CommandTimeout = 5;

                    var da = new SqlDataAdapter(sqlCommand);
                    ds = new DataSet();

                    da.Fill(ds);
                }
            }
            catch (Exception exception)
            {
                ds = null;
                //_logWriter.Write("Dal.DbWrapper.GetDataSet", "General", 1, 1, TraceEventType.Error, "Dal.DbWrapper.GetDataSet", _loggerFormatter.CreateLoggerDictionary(exception));
            }

            return ds;
        }

        public int InsertData(SqlCommand sqlCommand, object paramObject)
        {
            int numberOfUpdatedRecords;

            try
            {
                SqlConnection connection;
                using (connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    sqlCommand.Connection = connection;

                    sqlCommand.Parameters.AddRange(GetParametersFromObject(paramObject));

                    sqlCommand.CommandTimeout = 5;

                    numberOfUpdatedRecords = sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                numberOfUpdatedRecords = 0;
                //_logWriter.Write("Dal.DbWrapper.InsertData - object", "General", 1, 1, TraceEventType.Error, "Dal.DbWrapper.InsertData - object", _loggerFormatter.CreateLoggerDictionary(exception));
            }

            return numberOfUpdatedRecords;
        }


        public int InsertData(SqlCommand sqlCommand, List<SqlParameter> parameters)
        {
            int numberOfUpdatedRecords;

            try
            {
                SqlConnection connection;
                using (connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    sqlCommand.Connection = connection;

                    if (null != parameters && parameters.Count > 0)
                        sqlCommand.Parameters.AddRange(parameters.ToArray());

                    sqlCommand.CommandTimeout = 5;

                    numberOfUpdatedRecords = sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                numberOfUpdatedRecords = 0;
                //_logWriter.Write("Dal.DbWrapper.InsertData - list", "General", 1, 1, TraceEventType.Error, "Dal.DbWrapper.InsertData - list", _loggerFormatter.CreateLoggerDictionary(exception));
            }

            return numberOfUpdatedRecords;
        }
    }
}
