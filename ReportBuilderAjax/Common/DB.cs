using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ReportBuilderAjax.Web.Common
{
    public class DBHelper : IDisposable  
    {
        private const string RETURN_VALUE_NAME_PATTERN = "ReturnValue";

        private string  _connectionString = null;
        private SqlConnection _connection = null;
        private SqlTransaction _transaction = null;
        private bool _isCloseConnection = true;

        public DBHelper() : this(null)
        {}

        public DBHelper(string connectionString)
        {
            _connectionString = string.IsNullOrEmpty(connectionString) ? 
                                                                           ConfigurationManager.ConnectionStrings[0].ConnectionString : 
                                                                                                                                          connectionString;
        }

        public DBHelper(bool closeConnection) 
        {
            _isCloseConnection = closeConnection;   
        }

        #region IDisposable Members

        public void Dispose()
        {
            CloseConnectionNow();
        }

        #endregion

        #region String Helper Methods

        public string DBCommandTextToString(string commandText, params object[] args) 
        {
            if (args == null || args.Length == 0) return string.Format("DB Query - {0}", commandText);
            
            StringBuilder messageBuilder = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                messageBuilder.AppendFormat("{0},", args[i]);
            }
            return string.Format("DB Query - {0} ({1})", commandText, messageBuilder.ToString(0, messageBuilder.Length - 1));
        }
        public string DBCommandTextToString(string commandText, SqlParameterCollection args)
        {
            if (args == null || args.Count == 0) return string.Format("DB Query - {0}", commandText);

            StringBuilder messageBuilder = new StringBuilder();
            for (int i = 0; i < args.Count; i++)
            {
                messageBuilder.AppendFormat("{0},", args[i]);
            }
            return string.Format("DB Query - {0} ({1})", commandText, messageBuilder.ToString(0, messageBuilder.Length - 1));
        }

        #endregion

        #region Connection

        public void OpenConnection() 
        {
            lock (new object())
            {
                try
                {
                    if (_connection == null)
                    {
                        _connection = new SqlConnection(_connectionString);
                    }

                    if (_connection.State == ConnectionState.Closed)
                    {
                        _connection.Open();
                    }

                    /*if (_connection.State != ConnectionState.Connecting)
                    {
                        _connection.Open();
                    }*/

                }
                catch (Exception e)
                {
                    _connection = null;
                    throw new Exception(e.Message);
                }
            }
        }
        
        protected void CloseConnection()
        {
            if (_isCloseConnection) CloseConnectionNow();
        }

        public void CloseConnectionNow() 
        {
            if ( _connection != null && _connection.State != ConnectionState.Closed && _transaction == null) 
            {
                try 
                {
                    _connection.Close();
                    _connection = null;
                }
                catch(Exception e) 
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public SqlTransaction GetCurrentTransaction()
        {
            return _transaction;
        }

        public void SetTransaction(SqlTransaction transaction)
        {
            _transaction = transaction;
        }

        #endregion

        #region Transaction

        public void CreateTransaction()
        {   if(_connection == null)
            {
                OpenConnection();
            }
            if (_connection != null)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
            }
            //ClearTransaction();
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
            }
            //ClearTransaction();
        }

        private void ClearTransaction()
        {
            _transaction = null;
            CloseConnectionNow();
        }

        #endregion

        #region Command

        public bool TryMakeCommand(out SqlCommand command, string commandText, CommandType commandType, params object[] args)
        {
            try
            {
                command = GetCommand(commandText, commandType, args);
            }
            catch(Exception ex)
            {
                command = null;
                throw new Exception(string.Format("{0}\r\n{1}", ex.Message, DBCommandTextToString(commandText, args)));
            }


            return (command != null);
        }

        public bool IsExistsCommandParameter(string commandText, CommandType commandType, string parameterName)
        {
            bool isExists = false;

            SqlCommand command;
            try
            {
                OpenConnection();
                if (_connection == null) return false;

                command = new SqlCommand(commandText, _connection);
                if (_transaction != null)
                {
                    command.Transaction = _transaction;
                }
                command.CommandType = commandType;

                SqlCommandBuilder.DeriveParameters(command);

                foreach (SqlParameter parameter in command.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.Input || parameter.Direction == ParameterDirection.InputOutput)
                    {
                        if (parameter.ParameterName.ToLower() == parameterName.ToLower())
                        {
                            isExists = true;
                            break;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                CloseConnectionNow();
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(commandText, parameterName)));
            }

            return isExists;
        }

        public SqlCommand GetCommand(string commandText, CommandType commandType, params object[] args) 
        {
            SqlCommand command;
            try 
            {
                OpenConnection();
                if (_connection == null) return null;
                
                command = new SqlCommand(commandText, _connection);
                if (_transaction != null)
                {
                    command.Transaction = _transaction;
                }
                command.CommandType = commandType;

                if (args != null && (args.Length % 2) == 0 ) 
                {
                    for ( int i=0; i<args.Length; i+=2 ) 
                    {
                        command.Parameters.AddWithValue(args[i].ToString(), args[i + 1]);
                    }
                }				
            }
            catch(Exception e) 
            {
                CloseConnectionNow();

                command = null;
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(commandText, args)));
            }

            return command;
        }

        #endregion

        #region Execute(Reader/NonQuery/ReturnValue) / Get Scalar

        #region ExecuteNonQuery

        public int ExecuteNonQuery(string commandText, CommandType commandType, params object[] args) 
        {
            SqlCommand command;
            if (!TryMakeCommand(out command, commandText, commandType, args)) return 0;
            
            int result;

            try 
            {
                result = command.ExecuteNonQuery();
            }
            catch(Exception e) 
            {
                CloseConnectionNow();
                result = 0;
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(commandText, args)));
            }

            CloseConnection();
            return result;
        }

        #endregion

        #region ExecuteReader

        public SqlDataReader ExecuteReader(string commandText, CommandType commandType, params object[] args) 
        {
            SqlCommand command;
            if (!TryMakeCommand(out command, commandText, commandType, args)) return null;

            SqlDataReader sqlDataReader;
            try 
            {
                sqlDataReader = command.ExecuteReader();
            }
            catch(Exception e) 
            {
                CloseConnectionNow();
                sqlDataReader = null;
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(commandText, args)));
            }

            CloseConnection();
            return sqlDataReader;
        }

        #endregion
		
        #region ExecuteReturnValue

        protected void AddReturnParameterToCommand(SqlCommand command)
        {
            if(command == null) return;

            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.Direction = ParameterDirection.ReturnValue;
            sqlParameter.ParameterName = RETURN_VALUE_NAME_PATTERN;

            command.Parameters.Add(sqlParameter);
        }

        public T ExecuteReturnValue<T>(string commandText, CommandType commandType, params object[] args) where T:IConvertible
        {
            SqlCommand command;
            if (!TryMakeCommand(out command, commandText, commandType, args)) return default(T);
            

            AddReturnParameterToCommand(command);
            object returnValue = null;
            try
            {
                command.ExecuteNonQuery();
                returnValue = command.Parameters[RETURN_VALUE_NAME_PATTERN].Value;
            }
            catch (Exception e)
            {
                CloseConnectionNow();
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(commandText, args)));

            }
            finally
            {
                CloseConnection();
                if (returnValue == null) returnValue = default(T);   
            }

            return (T)returnValue;
        }

        public int ExecuteReturnInt(string commandText, CommandType commandType, params object[] args)
        {
            try 
            {
                return ExecuteReturnValue<int>(commandText, commandType, args);
            }
            catch(Exception e) 
            {
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(commandText, args)));
            }
        }

        #endregion

        #region GetScalar

        public T GetScalar<T>(string commandText, CommandType commandType, params object[] args) where T : IConvertible
        {
            SqlCommand command;
            if (!TryMakeCommand(out command, commandText, commandType, args)) return default(T);

            object scalar = null;
            try 
            {
                scalar = command.ExecuteScalar();
            }
            catch(Exception e) 
            {
                CloseConnectionNow();
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(commandText, args)));
            }
            finally
            {
                CloseConnection();

                if (scalar == null) scalar = default(T);
            }

            return (T)Convert.ChangeType(scalar, typeof(T));
        }

        #endregion

        #endregion

        #region GetDataSet

        public DataSet GetDataSet(string commandText, CommandType commandType, params object[] args) 
        {
            SqlCommand command;
            if (!TryMakeCommand(out command, commandText, commandType, args)) return null;

            DataSet dataSet = GetDataSet(command, args);

            CloseConnection();
            return dataSet;
        }

        public DataSet GetDataSet(SqlCommand command, params object[] args)
        {
            if (command == null) return null;

            DataSet dataSet = new DataSet();
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            
            try
            {
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                CloseConnectionNow();
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(command.CommandText, args)));
            }

            CloseConnection();
            return dataSet;
        }

        #endregion

        #region GetDataTable

        public DataTable GetDataTable(DataSet dataSet) 
        {
            return GetDataTable(dataSet, 0);
        }

        public DataTable GetDataTable(DataSet dataSet, int tableIndex)
        {
            DataTable dataTable = null;
            if ((dataSet != null) && (dataSet.Tables.Count - 1 >= tableIndex))
            {
                dataTable = dataSet.Tables[tableIndex];
            }
            return dataTable;
        }
        
        public DataTable GetDataTable(string commandText, CommandType commandType, params object[] args) 
        {
            SqlCommand command;
            if (!TryMakeCommand(out command, commandText, commandType, args)) return null;

            DataTable dataTable = GetDataTable(command, args);
			
            CloseConnection();
            return dataTable;
        }

        public DataTable GetDataTable(SqlCommand command, params object[] args)
        {
            if (command == null) return null;

            DataTable dataTable = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            try
            {
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataTable);
            }
            catch (Exception e)
            {
                CloseConnectionNow();
                throw new Exception(string.Format("{0}\r\n{1}", e.Message, DBCommandTextToString(command.CommandText, args)));
            }
            CloseConnection();
            return dataTable;
        }
    		
        #endregion

        #region GetDataRow
        
        public DataRow GetDataRow(string commandText, CommandType commandType, params object[] args) 
        {
            DataTable dataTable = GetDataTable(commandText, commandType, args);	
            if (dataTable == null || dataTable.Rows == null || dataTable.Rows.Count == 0) return null;
			
            return dataTable.Rows[0];
        }

        public DataRow GetDataRow(SqlCommand command)
        {
            DataTable dataTable = GetDataTable(command);
            if (dataTable == null || dataTable.Rows == null || dataTable.Rows.Count == 0) return null;

            return dataTable.Rows[0];
        }

        #endregion
    }
}