using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelMateWebV1.Helpers.Enums
{
    public enum RoomStatusEnum
    {
        Vacant = 1,
        Occupied = 2,
        Dirty = 3,
        Repair = 4,
        Unknown = 5
    }

    public enum PersonTypeEnum
    {
        Admin = 1,
        Staff = 2,
        Guest = 3,
        Accountant = 4,
        SalesAssistance = 5,
        StoreManager = 6,
        SeniorManager = 7,
        Child = 8

    }

    public enum RoomPaymentTypeEnum
    {
        InitialDeposit = 1,
        CashDeposit = 2,
        Restuarant = 3,
        Bar = 4,
        Laundry = 5,
        Miscellenous = 6,
        RoomTransfer = 7,
        ReservationDeposit = 8,
        Refund = 9,
        DebtorsBalance = 10,
        HalfDay = 11,
        GuestCredit = 12


    }

    public enum RoomPaymentStatusEnum
    {
        Debit = 1,
        Credit = 2
    }

    public enum PaymentMethodEnum
    {
        Cash = 1,
        Cheque = 2,
        CreditCard = 3,
        POSTBILL = 4,
        TRANSFERS = 5
    }

    public enum InvoiceStatusEnum
    {
        Pending = 1,
        Approved = 2,
        Paid = 3
    }
}