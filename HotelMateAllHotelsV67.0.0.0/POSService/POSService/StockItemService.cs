using HotelMate.DataWrapper;
using Microsoft.Practices.EnterpriseLibrary.Data;
using POSData;
using POSService.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSService
{
    public static class StockItemService
    {
        // Methods

        public static IEnumerable<StockItem> GetSpecificItem(int id)
        {
            SqlParameter parameter = new SqlParameter("@Id", SqlDbType.Int)
            {
                Value = id
            };

            DatabaseProviderFactory factory = new DatabaseProviderFactory();
            return factory.CreateDefault().ExecuteList<StockItem>("GetSpecificItem", id);
        }
        public static IEnumerable<Category> GetCategories(int p)
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory();
            return factory.CreateDefault().ExecuteList<Category>("GetCategories", new object[0]);
        }

        public static IEnumerable<Guest> GetCurrentGuests(int hotelId)
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory();
            return factory.CreateDefault().ExecuteList<Guest>("GetCurrentGuests", new object[0]);
        }

        public static IEnumerable<StockItem> GetStockItems(int hotelId)
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory();
            return factory.CreateDefault().ExecuteList<StockItem>("GetStockItems", new object[0]);
        }



        public static void UpdateSalesHouseKeeping(List<StockItem> lst, int transactionId, int guestId, int personId, int hotelId, int guestRoomId, string connectionString,
       int paymentMethodId, string paymentMethodNote, DateTime timeOfSale, int distributionPointId, int paymentTypeId = 1)
        {
            timeOfSale = DateTime.Now;
            DbWrapper wrapper = new DbWrapper(connectionString);
            decimal num = 0M;
            foreach (StockItem item in lst)
            {
                decimal num2 = item.UnitPrice * item.Quantity;
                num += num2;
                List<SqlParameter> list = new List<SqlParameter>();
                SqlParameter parameter = new SqlParameter("@ItemId", SqlDbType.Int)
                {
                    Value = item.Id
                };
                list.Add(parameter);

                SqlParameter parameter2 = new SqlParameter("@Qty", SqlDbType.Int)
                {
                    Value = item.Quantity
                };
                list.Add(parameter2);

                SqlParameter parameter3 = new SqlParameter("@TotalPrice", SqlDbType.Decimal)
                {
                    Value = num2
                };
                list.Add(parameter3);

                SqlParameter parameter4 = new SqlParameter("@TransactionId", SqlDbType.Int)
                {
                    Value = transactionId
                };
                list.Add(parameter4);

                SqlParameter parameter5 = new SqlParameter("@GuestId", SqlDbType.Int)
                {
                    Value = guestId
                };

                list.Add(parameter5);

                SqlParameter parameter6 = new SqlParameter("@GuestRoomId", SqlDbType.Int)
                {
                    Value = guestRoomId
                };
                list.Add(parameter6);

                SqlParameter parameter7 = new SqlParameter("@PersonId", SqlDbType.Int)
                {
                    Value = personId
                };
                list.Add(parameter7);

                SqlParameter parameter8 = new SqlParameter("@DateSold", SqlDbType.DateTime)
                {
                    Value = DateTime.Now
                };
                list.Add(parameter8);

                SqlParameter parameter9 = new SqlParameter("@IsActive", SqlDbType.Bit)
                {
                    Value = true
                };
                list.Add(parameter9);

                SqlParameter parameterPtId = new SqlParameter("@PaymentTypeId", SqlDbType.Int)
                {
                    Value = paymentTypeId
                };

                list.Add(parameterPtId);

                SqlParameter parameterPtMId = new SqlParameter("@PaymentMethodId", SqlDbType.Int)
                {
                    Value = paymentMethodId
                };

                list.Add(parameterPtMId);

                SqlParameter parameterPtMNote = new SqlParameter("@PaymentMethodNote", SqlDbType.VarChar)
                {
                    Value = paymentMethodNote
                };

                list.Add(parameterPtMNote);

                SqlParameter parameterTos = new SqlParameter("@TimeOfSale", SqlDbType.DateTime)
                {
                    Value = timeOfSale
                };

                list.Add(parameterTos);

                SqlParameter DistributionPoint = new SqlParameter("@DistributionPointId", SqlDbType.Int)
                {
                    Value = distributionPointId
                };

                list.Add(DistributionPoint);

                SqlCommand command = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "InsertHouseKeepingData"
                };

                wrapper.InsertData(command, list);
            }
        }




        public static void UpdateSales(List<StockItem> lst, int transactionId, int guestId, int personId, int hotelId, int guestRoomId, string connectionString,
            int paymentMethodId, string paymentMethodNote, DateTime timeOfSale, int distributionPointId, int paymentTypeId = 1)
        {
            timeOfSale = DateTime.Now;
            DbWrapper wrapper = new DbWrapper(connectionString);
            decimal num = 0M;
            foreach (StockItem item in lst)
            {
                decimal num2 = item.UnitPrice * item.Quantity;
                num += num2;
                List<SqlParameter> list = new List<SqlParameter>();
                SqlParameter parameter = new SqlParameter("@ItemId", SqlDbType.Int)
                {
                    Value = item.Id
                };
                list.Add(parameter);

                SqlParameter parameter2 = new SqlParameter("@Qty", SqlDbType.Int)
                {
                    Value = item.Quantity
                };
                list.Add(parameter2);

                SqlParameter parameter3 = new SqlParameter("@TotalPrice", SqlDbType.Decimal)
                {
                    Value = num2
                };
                list.Add(parameter3);

                SqlParameter parameter4 = new SqlParameter("@TransactionId", SqlDbType.Int)
                {
                    Value = transactionId
                };
                list.Add(parameter4);

                SqlParameter parameter5 = new SqlParameter("@GuestId", SqlDbType.Int)
                {
                    Value = guestId
                };
                list.Add(parameter5);


                SqlParameter parameter6 = new SqlParameter("@GuestRoomId", SqlDbType.Int)
                {
                    Value = guestRoomId
                };
                list.Add(parameter6);

                SqlParameter parameter7 = new SqlParameter("@PersonId", SqlDbType.Int)
                {
                    Value = personId
                };
                list.Add(parameter7);

                SqlParameter parameter8 = new SqlParameter("@DateSold", SqlDbType.DateTime)
                {
                    Value = timeOfSale
                };
                list.Add(parameter8);

                SqlParameter parameter9 = new SqlParameter("@IsActive", SqlDbType.Bit)
                {
                    Value = true
                };
                list.Add(parameter9);

                SqlParameter parameterPtId = new SqlParameter("@PaymentTypeId", SqlDbType.Int)
                {
                    Value = paymentTypeId
                };

                list.Add(parameterPtId);

                SqlParameter parameterPtMId = new SqlParameter("@PaymentMethodId", SqlDbType.Int)
                {
                    Value = paymentMethodId
                };

                list.Add(parameterPtMId);

                SqlParameter parameterPtMNote = new SqlParameter("@PaymentMethodNote", SqlDbType.VarChar)
                {
                    Value = paymentMethodNote
                };

                list.Add(parameterPtMNote);

                SqlParameter parameterTos = new SqlParameter("@TimeOfSale", SqlDbType.DateTime)
                {
                    Value = timeOfSale
                };

                list.Add(parameterTos);

                SqlParameter DistributionPoint = new SqlParameter("@DistributionPointId", SqlDbType.Int)
                {
                    Value = distributionPointId
                };

                list.Add(DistributionPoint);

                SqlCommand command = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "InsertSalesData"
                };

                wrapper.InsertData(command, list);
            }


            if (guestRoomId == 0)
                return;

            List<SqlParameter> parameters = new List<SqlParameter>();

            SqlParameter parameter10 = new SqlParameter("@GuestRoomId", SqlDbType.Int)
            {
                Value = guestRoomId
            };
            parameters.Add(parameter10);

            SqlParameter parameter11 = new SqlParameter("@Amount", SqlDbType.Decimal)
            {
                Value = num
            };
            parameters.Add(parameter11);

            SqlParameter parameter12 = new SqlParameter("@TransactionId", SqlDbType.Int)
            {
                Value = transactionId
            };
            parameters.Add(parameter12);

            SqlParameter parameter13 = new SqlParameter("@PaymentTypeId", SqlDbType.Int)
            {
                Value = paymentTypeId
            };
            parameters.Add(parameter13);

            SqlParameter parameter14 = new SqlParameter("@TransactionDate", SqlDbType.DateTime)
            {
                Value = DateTime.Now
            };
            parameters.Add(parameter14);

            SqlParameter parameterPtMId15 = new SqlParameter("@PaymentMethodId", SqlDbType.Int)
            {
                Value = paymentMethodId
            };

            parameters.Add(parameterPtMId15);


            SqlParameter parameterPtMNote16 = new SqlParameter("@PaymentMethodNote", SqlDbType.VarChar)
            {
                Value = paymentMethodNote
            };

            parameters.Add(parameterPtMNote16);


            SqlCommand sqlCommand = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "InsertGuestRoomSales"
            };

            wrapper.InsertData(sqlCommand, parameters);
        }

        public static void CloseTill(int cashierId, string connectionString)
        {
            DbWrapper wrapper = new DbWrapper(connectionString);
            string sqlText = "UPDATE SOLDITEMSAll SET TILLOPEN = 0 WHERE PERSONID = " + cashierId.ToString();
            SqlCommand cmd = new SqlCommand(sqlText);
            List<SqlParameter> parameters = new List<SqlParameter>();
            wrapper.InsertData(cmd, parameters);
        }

        public static DataSet GetSoldItems(int cashierId, string connectionString)
        {
            DbWrapper wrapper = new DbWrapper(connectionString);
            string sqlText = "SELECT SI.STOCKITEMNAME, SIA.Qty, SIA.TOTALPRICE FROM  SOLDITEMSAll SIA INNER JOIN STOCKITEM SI ON SI.ID = SIA.ITEMID  WHERE SIA.TILLOPEN = 1 AND SIA.PERSONID = " + cashierId.ToString();
            SqlCommand cmd = new SqlCommand(sqlText);
            List<SqlParameter> parameters = new List<SqlParameter>();
            return wrapper.GetDataSet(cmd, parameters);
        }

        public static DataSet GetSoldItems(string connectionString)
        {
            DbWrapper wrapper = new DbWrapper(connectionString);
            string sqlText = @"SELECT SI.STOCKITEMNAME, SIA.Qty, SIA.TOTALPRICE,SIA.DATESOLD,P.DISPLAYNAME,RPT.NAME,SIA.PAYMENTMETHODNOTE,PT.NAME,SIA.TIMEOFSALE,SIA.GUESTID
                        FROM  SOLDITEMSAll SIA INNER JOIN STOCKITEM 
                        SI ON SI.ID = SIA.ITEMID INNER JOIN [dbo].[Person] P ON P.PersonID = SIA.PersonId INNER JOIN ROOMPAYMENTTYPE RPT ON RPT.ID = SIA.PAYMENTTYPEID
                        INNER JOIN PAYMENTMETHOD PT ON PT.Id = SIA.PaymentMethodId";
            SqlCommand cmd = new SqlCommand(sqlText);
            List<SqlParameter> parameters = new List<SqlParameter>();
            return wrapper.GetDataSet(cmd, parameters);
        }



        public static void DeleteTableItems(int tableId, string connectionString, int personId)
        {
            DbWrapper wrapper = new DbWrapper(connectionString);
            string sqlText = "DELETE FROM TABLEITEM WHERE TABLEID = " + tableId.ToString() + " AND CASHIER =  " + personId.ToString();
            SqlCommand cmd = new SqlCommand(sqlText);
            List<SqlParameter> parameters = new List<SqlParameter>();
            wrapper.InsertData(cmd, parameters);
        }

        public static void TransferTill(int currentCashier, int newCashier, string connectionString)
        {
            DbWrapper wrapper = new DbWrapper(connectionString);
            string sqlText = "UPDATE TABLEITEM SET CASHIER  = " + newCashier.ToString() + " WHERE CASHIER =  " + currentCashier.ToString();
            SqlCommand cmd = new SqlCommand(sqlText);
            List<SqlParameter> parameters = new List<SqlParameter>();
            wrapper.InsertData(cmd, parameters);
        }

        private static void InsertPaymentData(int paymentTypeId, DateTime paymentDate, int paymentMethodId, decimal total, decimal subTotal, string tax, decimal taxAmount,
          string discount, decimal discountAmount, string serviceCharge, decimal serviceChargeAmount, string resident, decimal residentAmount,
          int cashierId, int guestId, string notes, string recieptNumber, decimal paid, decimal outstanding, int type, bool isActive,
          int distributionPointId, string connectionString)
        {
            DbWrapper wrapper = new DbWrapper(connectionString);

            List<SqlParameter> list = new List<SqlParameter>();

            SqlParameter parameter1 = new SqlParameter("@PaymentDate", SqlDbType.DateTime)
            {
                Value = paymentDate
            };

            list.Add(parameter1);

            SqlParameter parameter2 = new SqlParameter("@PaymentMethodId", SqlDbType.Int)
            {
                Value = paymentMethodId
            };

            list.Add(parameter2);

            SqlParameter parameter3 = new SqlParameter("@Total", SqlDbType.Decimal)
            {
                Value = total
            };

            list.Add(parameter3);

            SqlParameter parameter4 = new SqlParameter("@SubTotal", SqlDbType.Decimal)
            {
                Value = subTotal
            };

            list.Add(parameter4);

            SqlParameter parameter5 = new SqlParameter("@Tax", SqlDbType.VarChar)
            {
                Value = tax
            };

            list.Add(parameter5);

            SqlParameter parameter6 = new SqlParameter("@TaxAmount", SqlDbType.Decimal)
            {
                Value = taxAmount
            };

            list.Add(parameter6);

            SqlParameter parameter7 = new SqlParameter("@Discount", SqlDbType.VarChar)
            {
                Value = discount
            };

            list.Add(parameter7);

            SqlParameter parameter8 = new SqlParameter("@DiscountAmount", SqlDbType.Decimal)
            {
                Value = discountAmount
            };

            list.Add(parameter8);

            SqlParameter parameter9 = new SqlParameter("@ServiceCharge", SqlDbType.VarChar)
            {
                Value = serviceCharge
            };

            list.Add(parameter9);

            SqlParameter parameter10 = new SqlParameter("@ServiceChargeAmount", SqlDbType.Decimal)
            {
                Value = serviceChargeAmount
            };

            list.Add(parameter10);

            SqlParameter parameter11 = new SqlParameter("@Resident", SqlDbType.VarChar)
            {
                Value = resident
            };

            list.Add(parameter11);

            SqlParameter parameter12 = new SqlParameter("@ResidentAmount", SqlDbType.Decimal)
            {
                Value = residentAmount
            };

            list.Add(parameter12);

            SqlParameter parameter13 = new SqlParameter("@CashierId", SqlDbType.Int)
            {
                Value = cashierId
            };

            list.Add(parameter13);

            SqlParameter parameter14 = new SqlParameter("@GuestId", SqlDbType.Int)
            {
                Value = guestId
            };

            list.Add(parameter14);

            SqlParameter parameter15 = new SqlParameter("@Notes", SqlDbType.VarChar)
            {
                Value = notes
            };

            list.Add(parameter15);

            SqlParameter parameter16 = new SqlParameter("@ReceiptNumber", SqlDbType.VarChar)
            {
                Value = recieptNumber
            };

            list.Add(parameter16);

            SqlParameter parameter17 = new SqlParameter("@Paid", SqlDbType.Decimal)
            {
                Value = paid
            };

            list.Add(parameter17);

            SqlParameter parameter18 = new SqlParameter("@Outstanding", SqlDbType.Decimal)
            {
                Value = outstanding
            };

            list.Add(parameter18);


            SqlParameter parameter19 = new SqlParameter("@Type", SqlDbType.Int)
            {
                Value = type
            };

            list.Add(parameter19);

            SqlParameter parameter20 = new SqlParameter("@IsActive", SqlDbType.Bit)
            {
                Value = isActive
            };

            list.Add(parameter20);

            SqlParameter parameter21 = new SqlParameter("@DistributionPointId", SqlDbType.Int)
            {
                Value = distributionPointId
            };

            list.Add(parameter21);


            SqlParameter parameter22 = new SqlParameter("@PaymentTypeId", SqlDbType.Int)
            {
                Value = paymentTypeId
            };

            list.Add(parameter22);



            SqlCommand command = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "InsertPaymentData"
            };

            wrapper.InsertData(command, list);
        }


        private static void InsertPaymentDataCredit(int paymentTypeId, DateTime paymentDate, int paymentMethodId, decimal total, decimal subTotal, string tax, decimal taxAmount,
            string discount, decimal discountAmount, string serviceCharge, decimal serviceChargeAmount, string resident, decimal residentAmount,
            int cashierId, int businessAccountId, string notes, string recieptNumber, decimal paid, decimal outstanding, int type, bool isActive,
            int distributionPointId, string connectionString)
        {
            DbWrapper wrapper = new DbWrapper(connectionString);

            List<SqlParameter> list = new List<SqlParameter>();

            SqlParameter parameter1 = new SqlParameter("@PaymentDate", SqlDbType.DateTime)
            {
                Value = paymentDate
            };

            list.Add(parameter1);

            SqlParameter parameter2 = new SqlParameter("@PaymentMethodId", SqlDbType.Int)
            {
                Value = paymentMethodId
            };

            list.Add(parameter2);

            SqlParameter parameter3 = new SqlParameter("@Total", SqlDbType.Decimal)
            {
                Value = total
            };

            list.Add(parameter3);

            SqlParameter parameter4 = new SqlParameter("@SubTotal", SqlDbType.Decimal)
            {
                Value = subTotal
            };

            list.Add(parameter4);

            SqlParameter parameter5 = new SqlParameter("@Tax", SqlDbType.VarChar)
            {
                Value = tax
            };

            list.Add(parameter5);

            SqlParameter parameter6 = new SqlParameter("@TaxAmount", SqlDbType.Decimal)
            {
                Value = taxAmount
            };

            list.Add(parameter6);

            SqlParameter parameter7 = new SqlParameter("@Discount", SqlDbType.VarChar)
            {
                Value = discount
            };

            list.Add(parameter7);

            SqlParameter parameter8 = new SqlParameter("@DiscountAmount", SqlDbType.Decimal)
            {
                Value = discountAmount
            };

            list.Add(parameter8);

            SqlParameter parameter9 = new SqlParameter("@ServiceCharge", SqlDbType.VarChar)
            {
                Value = serviceCharge
            };

            list.Add(parameter9);

            SqlParameter parameter10 = new SqlParameter("@ServiceChargeAmount", SqlDbType.Decimal)
            {
                Value = serviceChargeAmount
            };

            list.Add(parameter10);

            SqlParameter parameter11 = new SqlParameter("@Resident", SqlDbType.VarChar)
            {
                Value = resident
            };

            list.Add(parameter11);

            SqlParameter parameter12 = new SqlParameter("@ResidentAmount", SqlDbType.Decimal)
            {
                Value = residentAmount
            };

            list.Add(parameter12);

            SqlParameter parameter13 = new SqlParameter("@CashierId", SqlDbType.Int)
            {
                Value = cashierId
            };

            list.Add(parameter13);

            SqlParameter parameter14 = new SqlParameter("@BusinessAccountId", SqlDbType.Int)
            {
                Value = businessAccountId
            };

            list.Add(parameter14);

            SqlParameter parameter15 = new SqlParameter("@Notes", SqlDbType.VarChar)
            {
                Value = notes
            };

            list.Add(parameter15);

            SqlParameter parameter16 = new SqlParameter("@ReceiptNumber", SqlDbType.VarChar)
            {
                Value = recieptNumber
            };

            list.Add(parameter16);

            SqlParameter parameter17 = new SqlParameter("@Paid", SqlDbType.Decimal)
            {
                Value = paid
            };

            list.Add(parameter17);

            SqlParameter parameter18 = new SqlParameter("@Outstanding", SqlDbType.Decimal)
            {
                Value = outstanding
            };

            list.Add(parameter18);


            SqlParameter parameter19 = new SqlParameter("@Type", SqlDbType.Int)
            {
                Value = type
            };

            list.Add(parameter19);

            SqlParameter parameter20 = new SqlParameter("@IsActive", SqlDbType.Bit)
            {
                Value = isActive
            };

            list.Add(parameter20);

            SqlParameter parameter21 = new SqlParameter("@DistributionPointId", SqlDbType.Int)
            {
                Value = distributionPointId
            };

            list.Add(parameter21);

            SqlParameter parameter22 = new SqlParameter("@PaymentTypeId", SqlDbType.Int)
            {
                Value = paymentTypeId
            };

            list.Add(parameter22);

            SqlCommand command = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "InsertPaymentDataCredit"
            };

            wrapper.InsertData(command, list);
        }


        public static void UpdateSales(int transactionId, int guestId, int? businessAccountId, int personId, int hotelId, int? guestRoomId, string connectionString,
        int? paymentMethodId, string paymentMethodNote, DateTime timeOfSale, int? distributionPointId, bool isHotel, string recieptNumber,
        decimal total, decimal subTotal, string tax, decimal taxAmount, string discount, decimal discountAmount, string resident, decimal residentAmount,
        string serviceCharge, decimal serviceChargeAmount, decimal paid, decimal outstanding, int cashierId = 0)
        {
            int paymentTypeId = transactionId;

            timeOfSale = DateTime.Now;

            DbWrapper wrapper = new DbWrapper(connectionString);

            decimal num = 0M;


            if (businessAccountId.HasValue && businessAccountId > 0)
            {
                InsertPaymentDataCredit(paymentTypeId,timeOfSale
                         , paymentMethodId.Value
                         , total
                         , subTotal
                         , tax
                         , taxAmount
                         , discount
                         , discountAmount
                         , serviceCharge
                         , serviceChargeAmount
                         , resident
                         , residentAmount
                         , cashierId
                         , businessAccountId.Value
                         , ""
                         , recieptNumber
                         , paid
                         , outstanding
                         , 2
                         , true
                         , distributionPointId.Value, connectionString);
            }
            else
            {
                 InsertPaymentData(paymentTypeId,timeOfSale
                 , paymentMethodId.Value
                 , total
                 , subTotal
                 , tax
                 , taxAmount
                 , discount
                 , discountAmount
                 , serviceCharge
                 , serviceChargeAmount
                 , resident
                 , residentAmount
                 , cashierId
                 , guestId
                 , ""
                 , recieptNumber
                 , paid
                 , outstanding
                 , 2
                 , true
                 , distributionPointId.Value, connectionString);
        }


            if (discountAmount > 0)
            {
                if (cashierId == 0)
                    cashierId = personId;

                SqlCommand sqlCommandSD = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "InsertSalesDiscount"
                };
                List<SqlParameter> parametersSD = new List<SqlParameter>();

                SqlParameter parameterDD = new SqlParameter("@DiscountDate", SqlDbType.DateTime)
                {
                    Value = timeOfSale
                };
                parametersSD.Add(parameterDD);

                SqlParameter parameterRR = new SqlParameter("@ReceiptNumber", SqlDbType.VarChar)
                {
                    Value = recieptNumber
                };
                parametersSD.Add(parameterRR);

                SqlParameter parameterAmount = new SqlParameter("@Amount", SqlDbType.Decimal)
                {
                    Value = discountAmount
                };

                parametersSD.Add(parameterAmount);

                SqlParameter parameterDiscountPerson = new SqlParameter("@PersonId", SqlDbType.Int)
                {
                    Value = personId
                };

                parametersSD.Add(parameterDiscountPerson);

                SqlParameter parameterDiscountActualCashierId = new SqlParameter("@ActualCashierId", SqlDbType.Int)
                {
                    Value = cashierId
                };

                parametersSD.Add(parameterDiscountActualCashierId);

                wrapper.InsertData(sqlCommandSD, parametersSD);

            }



            if (guestRoomId == 0)
                return;

            List<SqlParameter> parameters = new List<SqlParameter>();

            SqlParameter parameter10 = new SqlParameter("@GuestRoomId", SqlDbType.Int)
            {
                Value = guestRoomId
            };
            parameters.Add(parameter10);

            SqlParameter parameter11 = new SqlParameter("@Amount", SqlDbType.Decimal)
            {
                Value = num
            };
            parameters.Add(parameter11);

            SqlParameter parameter12 = new SqlParameter("@TransactionId", SqlDbType.Int)
            {
                Value = transactionId
            };
            parameters.Add(parameter12);

            SqlParameter parameter13 = new SqlParameter("@PaymentTypeId", SqlDbType.Int)
            {
                Value = paymentTypeId
            };
            parameters.Add(parameter13);

            SqlParameter parameter14 = new SqlParameter("@TransactionDate", SqlDbType.DateTime)
            {
                Value = DateTime.Now
            };
            parameters.Add(parameter14);

            SqlParameter parameterPtMId15 = new SqlParameter("@PaymentMethodId", SqlDbType.Int)
            {
                Value = paymentMethodId
            };

            parameters.Add(parameterPtMId15);


            SqlParameter parameterPtMNote16 = new SqlParameter("@PaymentMethodNote", SqlDbType.VarChar)
            {
                Value = paymentMethodNote
            };

            parameters.Add(parameterPtMNote16);


            SqlCommand sqlCommand = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "InsertGuestRoomSales"
            };

            wrapper.InsertData(sqlCommand, parameters);
        }
    }
}
