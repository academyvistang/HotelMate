#region Assembly POSData.dll, v1.0.0.0
// C:\Projects\HotelMateAllHotelsV25.0.0.0\POSService\POSService\bin\Debug\POSData.dll
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace HotelMate.DataWrapper.Core
{
    public interface IDbWrapper
    {
        DataSet GetDataSet(SqlCommand sqlCommand);
        DataSet GetDataSet(SqlCommand sqlCommand, List<SqlParameter> parameters);
        DataSet GetDataSet(SqlCommand sqlCommand, object paramObject);
        int InsertData(SqlCommand sqlCommand, List<SqlParameter> parameters);
        int InsertData(SqlCommand sqlCommand, object paramObject);
    }
}
