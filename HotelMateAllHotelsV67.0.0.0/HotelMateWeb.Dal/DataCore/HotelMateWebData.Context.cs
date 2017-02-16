﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HotelMateWeb.Dal.DataCore
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class HotelMateWebEntities : DbContext
    {
        public HotelMateWebEntities()
            : base("name=HotelMateWebEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<GuestReservation> GuestReservations { get; set; }
        public virtual DbSet<GuestRoom> GuestRooms { get; set; }
        public virtual DbSet<GuestRoomAccount> GuestRoomAccounts { get; set; }
        public virtual DbSet<Hotel> Hotels { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<PersonType> PersonTypes { get; set; }
        public virtual DbSet<Pictural> Picturals { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<RoomPaymentTypeStatu> RoomPaymentTypeStatus { get; set; }
        public virtual DbSet<RoomStatu> RoomStatus { get; set; }
        public virtual DbSet<RoomTransferHistory> RoomTransferHistories { get; set; }
        public virtual DbSet<RoomType> RoomTypes { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<EmployeeShift> EmployeeShifts { get; set; }
        public virtual DbSet<InvoiceStatusType> InvoiceStatusTypes { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<DistributionPointItem> DistributionPointItems { get; set; }
        public virtual DbSet<POSItem> POSItems { get; set; }
        public virtual DbSet<TableItem> TableItems { get; set; }
        public virtual DbSet<MovieCategory> MovieCategories { get; set; }
        public virtual DbSet<Taxi> Taxis { get; set; }
        public virtual DbSet<Adventure> Adventures { get; set; }
        public virtual DbSet<Movie> Movies { get; set; }
        public virtual DbSet<GuestPlaylist> GuestPlaylists { get; set; }
        public virtual DbSet<Escort> Escorts { get; set; }
        public virtual DbSet<GuestOrder> GuestOrders { get; set; }
        public virtual DbSet<GuestOrderItem> GuestOrderItems { get; set; }
        public virtual DbSet<Guest> Guests { get; set; }
        public virtual DbSet<SoldItemsAll> SoldItemsAlls { get; set; }
        public virtual DbSet<GuestLedger> GuestLedgers { get; set; }
        public virtual DbSet<GuestBillItem> GuestBillItems { get; set; }
        public virtual DbSet<GuestPaidItem> GuestPaidItems { get; set; }
        public virtual DbSet<GuestRequestItem> GuestRequestItems { get; set; }
        public virtual DbSet<EscortPicture> EscortPictures { get; set; }
        public virtual DbSet<GuestChat> GuestChats { get; set; }
        public virtual DbSet<GuestChatMessage> GuestChatMessages { get; set; }
        public virtual DbSet<GuestFeedBack> GuestFeedBacks { get; set; }
        public virtual DbSet<GuestCredential> GuestCredentials { get; set; }
        public virtual DbSet<PurchaseOrderStoreType> PurchaseOrderStoreTypes { get; set; }
        public virtual DbSet<FoodMatrix> FoodMatrices { get; set; }
        public virtual DbSet<PaymentOrder> PaymentOrders { get; set; }
        public virtual DbSet<SalesDiscount> SalesDiscounts { get; set; }
        public virtual DbSet<SchoolPicture> SchoolPictures { get; set; }
        public virtual DbSet<GuestMessage> GuestMessages { get; set; }
        public virtual DbSet<BarTable> BarTables { get; set; }
        public virtual DbSet<BusinessAccount> BusinessAccounts { get; set; }
        public virtual DbSet<BusinessCorporateAccount> BusinessCorporateAccounts { get; set; }
        public virtual DbSet<PrinterTable> PrinterTables { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<RoomPaymentType> RoomPaymentTypes { get; set; }
        public virtual DbSet<Batch> Batches { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public virtual DbSet<StorePoint> StorePoints { get; set; }
        public virtual DbSet<StorePointItem> StorePointItems { get; set; }
        public virtual DbSet<DamagedBatchItem> DamagedBatchItems { get; set; }
        public virtual DbSet<DistributionPoint> DistributionPoints { get; set; }
        public virtual DbSet<UsedItemsByHotel> UsedItemsByHotels { get; set; }
        public virtual DbSet<ExpensesType> ExpensesTypes { get; set; }
        public virtual DbSet<StockItem> StockItems { get; set; }
        public virtual DbSet<StockItemHotel> StockItemHotels { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<StoreItem> StoreItems { get; set; }
        public virtual DbSet<Person> People { get; set; }
    }
}
