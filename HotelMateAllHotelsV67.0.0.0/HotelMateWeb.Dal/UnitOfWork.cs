using System.Data.Entity.Validation;
using HotelMateWeb.Dal.DataCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotelMateWeb.Dal
{
    public class UnitOfWork : IDisposable
    {
        private readonly HotelMateWebEntities _context = new HotelMateWebEntities();
        //private HotelMateWebEntities _context = HotelMateSingletonContext.Instance;

        private GenericRepository<Guest> _guestRepository;
        private GenericRepository<GuestRoom> _guestRoomRepository;
        private GenericRepository<Room> _roomRepository;
        private GenericRepository<RoomType> _roomTypeRepository;
        private GenericRepository<GuestRoomAccount> _guestRoomAccountRepository;
        private GenericRepository<BusinessAccount> _businessAccountRepository;
        private GenericRepository<GuestReservation> _guestReservationRepository;
        private GenericRepository<Transaction> _transactionRepository;
        private GenericRepository<Item> _itemRepository;
        private GenericRepository<RoomStatu> _roomStatusRepository;
        private GenericRepository<Person> _personRepository;
        private GenericRepository<PersonType> _personTypeRepository;
        private GenericRepository<Pictural> _picturalRepository;
        private GenericRepository<Expense> _expenseRepository;
        private GenericRepository<PaymentMethod> _paymentMethodRepository;
        private GenericRepository<EmployeeShift> _employeeShiftRepository;
        private GenericRepository<Supplier> _supplierRepository;
        private GenericRepository<PurchaseOrder> _purchaseOrderRepository;
        private GenericRepository<PurchaseOrderItem> _purchaseOrderItemRepository;
        private GenericRepository<Invoice> _invoiceRepository;
        private GenericRepository<InvoiceStatusType> _invoiceStatusTypeRepository;
        private GenericRepository<Store> _storeRepository;
        private GenericRepository<StoreItem> _storeItemRepository;
        private GenericRepository<StockItem> _stockItemRepository;
        private GenericRepository<DistributionPoint> _distributionPointRepository;
        private GenericRepository<DistributionPointItem> _distributionPointItemRepository;
        private GenericRepository<Batch> _batchRepository;
        private GenericRepository<POSItem> _pOSItemRepository;
        private GenericRepository<DamagedBatchItem> _damagedBatchItemRepository;
        private GenericRepository<TableItem> _tableItemRepository;
        private GenericRepository<SoldItemsAll> _soldItemRepository;
        private GenericRepository<Movie> _movieRepository;
        private GenericRepository<BusinessCorporateAccount> _businessCorporateAccountRepository;
        private GenericRepository<Taxi> _taxiRepository;
        private GenericRepository<Adventure> _adventureRepository;
        private GenericRepository<MovieCategory> _movieCategoryRepository;
        private GenericRepository<GuestPlaylist> _guestPlaylistRepository;
        private GenericRepository<Escort> _escortRepository;
        private GenericRepository<GuestLedger> _guestLedgerRepository;
        private GenericRepository<Payment> _paymentRepository;
        private GenericRepository<StockItemHotel> _stockItemHotelRepository;

        public GenericRepository<StockItemHotel> StockItemHotelRepository
        {
            get
            {
                if (this._stockItemHotelRepository == null)
                {
                    this._stockItemHotelRepository = new GenericRepository<StockItemHotel>(_context);
                }

                return _stockItemHotelRepository;
            }
        }


        public GenericRepository<Payment> PaymentRepository
        {
            get
            {
                if (this._paymentRepository == null)
                {
                    this._paymentRepository = new GenericRepository<Payment>(_context);
                }

                return _paymentRepository;
            }
        }

        public GenericRepository<GuestLedger> GuestLedgerRepository
        {
            get
            {
                if (this._guestLedgerRepository == null)
                {
                    this._guestLedgerRepository = new GenericRepository<GuestLedger>(_context);
                }
                return _guestLedgerRepository;
            }
        }


        public GenericRepository<Escort> EscortRepository
        {
            get
            {
                if (this._escortRepository == null)
                {
                    this._escortRepository = new GenericRepository<Escort>(_context);
                }
                return _escortRepository;
            }
        }


        public GenericRepository<GuestPlaylist> GuestPlaylistRepository
        {
            get
            {
                if (this._guestPlaylistRepository == null)
                {
                    this._guestPlaylistRepository = new GenericRepository<GuestPlaylist>(_context);
                }
                return _guestPlaylistRepository;
            }
        }



        public GenericRepository<MovieCategory> MovieCategoryRepository
        {
            get
            {
                if (this._movieCategoryRepository == null)
                {
                    this._movieCategoryRepository = new GenericRepository<MovieCategory>(_context);
                }
                return _movieCategoryRepository;
            }
        }
        


        public GenericRepository<Adventure> AdventureRepository
        {
            get
            {
                if (this._adventureRepository == null)
                {
                    this._adventureRepository = new GenericRepository<Adventure>(_context);
                }


                return _adventureRepository;
            }
        }


        public GenericRepository<Taxi> TaxiRepository
        {
            get
            {
                if (this._taxiRepository == null)
                {
                    this._taxiRepository = new GenericRepository<Taxi>(_context);
                }


                return _taxiRepository;
            }
        }


        public GenericRepository<BusinessCorporateAccount> BusinessCorporateAccountRepository
        {
            get
            {
                if (this._businessCorporateAccountRepository == null)
                {
                    this._businessCorporateAccountRepository = new GenericRepository<BusinessCorporateAccount>(_context);
                }


                return _businessCorporateAccountRepository;
            }
        }



        public GenericRepository<Movie> MovieRepository
        {
            get
            {
                if (this._movieRepository == null)
                {
                    this._movieRepository = new GenericRepository<Movie>(_context);
                }


                return _movieRepository;
            }
        }


        public GenericRepository<SoldItemsAll> SoldItemRepository
        {
            get
            {
                if (this._soldItemRepository == null)
                {
                    this._soldItemRepository = new GenericRepository<SoldItemsAll>(_context);
                }

                return _soldItemRepository;
            }
        }



        public GenericRepository<TableItem> TableItemRepository
        {
            get
            {
                if (this._tableItemRepository == null)
                {
                    this._tableItemRepository = new GenericRepository<TableItem>(_context);
                }

                return _tableItemRepository;
            }
        }



        public GenericRepository<DamagedBatchItem> DamagedBatchItemRepository
        {
            get
            {
                if (this._damagedBatchItemRepository == null)
                {
                    this._damagedBatchItemRepository = new GenericRepository<DamagedBatchItem>(_context);
                }

                return _damagedBatchItemRepository;
            }
        }

        public GenericRepository<POSItem> POSItemRepository
        {
            get
            {
                if (this._pOSItemRepository == null)
                {
                    this._pOSItemRepository = new GenericRepository<POSItem>(_context);
                }

                return _pOSItemRepository;
            }
        }


        public GenericRepository<Batch> BatchRepository
        {
            get
            {
                if (this._batchRepository == null)
                {
                    this._batchRepository = new GenericRepository<Batch>(_context);
                }

                return _batchRepository;
            }
        }



        public GenericRepository<DistributionPoint> DistributionPointRepository
        {
            get
            {
                if (this._distributionPointRepository == null)
                {
                    this._distributionPointRepository = new GenericRepository<DistributionPoint>(_context);
                }

                return _distributionPointRepository;
            }
        }

        public GenericRepository<DistributionPointItem> DistributionPointItemRepository
        {
            get
            {
                if (this._distributionPointItemRepository == null)
                {
                    this._distributionPointItemRepository = new GenericRepository<DistributionPointItem>(_context);
                }

                return _distributionPointItemRepository;
            }
        }




        //public IQueryable<GuestRoomAccount> GetAllForGuestByType(int? guestId, int? paymentTypeId)
        //{
        //    return from guestRoomAccounts in _context.GuestRoomAccounts
        //           orderby guestRoomAccounts.TransactionDate
        //           where guestRoomAccounts.GuestRoom.GuestId == guestId.Value && guestRoomAccounts.PaymentTypeId == paymentTypeId.Value
        //           select guestRoomAccounts;
        //}

        public GenericRepository<StockItem> StockItemRepository
        {
            get
            {
                if (this._stockItemRepository == null)
                {
                    this._stockItemRepository = new GenericRepository<StockItem>(_context);
                }

                return _stockItemRepository;
            }
        }

        public GenericRepository<PurchaseOrder> PurchaseOrderRepository
        {
            get
            {
                if (this._purchaseOrderRepository == null)
                {
                    this._purchaseOrderRepository = new GenericRepository<PurchaseOrder>(_context);
                }

                return _purchaseOrderRepository;
            }
        }

        public GenericRepository<PurchaseOrderItem> PurchaseOrderItemRepository
        {
            get
            {
                if (this._purchaseOrderItemRepository == null)
                {
                    this._purchaseOrderItemRepository = new GenericRepository<PurchaseOrderItem>(_context);
                }

                return _purchaseOrderItemRepository;
            }
        }

        public GenericRepository<Invoice> InvoiceRepository
        {
            get
            {
                if (this._invoiceRepository == null)
                {
                    this._invoiceRepository = new GenericRepository<Invoice>(_context);
                }

                return _invoiceRepository;
            }
        }

        public GenericRepository<InvoiceStatusType> InvoiceStatusTypeRepository
        {
            get
            {
                if (this._invoiceStatusTypeRepository == null)
                {
                    this._invoiceStatusTypeRepository = new GenericRepository<InvoiceStatusType>(_context);
                }

                return _invoiceStatusTypeRepository;
            }
        }

        public GenericRepository<Store> StoreRepository
        {
            get
            {
                if (this._storeRepository == null)
                {
                    this._storeRepository = new GenericRepository<Store>(_context);
                }

                return _storeRepository;
            }
        }

        public GenericRepository<StoreItem> StoreItemRepository
        {
            get
            {
                if (this._storeItemRepository == null)
                {
                    this._storeItemRepository = new GenericRepository<StoreItem>(_context);
                }

                return _storeItemRepository;
            }
        }
           

        public GenericRepository<PersonType> PersonTypeRepository
        {
            get
            {
                if (this._personTypeRepository == null)
                {
                    this._personTypeRepository = new GenericRepository<PersonType>(_context);
                }

                return _personTypeRepository;
            }
        }


        public GenericRepository<EmployeeShift> EmployeeShiftRepository
        {
            get
            {
                if (this._employeeShiftRepository == null)
                {
                    this._employeeShiftRepository = new GenericRepository<EmployeeShift>(_context);
                }

                return _employeeShiftRepository;
            }
        }

        public GenericRepository<Supplier> SupplierRepository
        {
            get
            {
                if (this._supplierRepository == null)
                {
                    this._supplierRepository = new GenericRepository<Supplier>(_context);
                }

                return _supplierRepository;
            }
        }


        public GenericRepository<Person> PersonRepository
        {
            get
            {
                if (this._personRepository == null)
                {
                    this._personRepository = new GenericRepository<Person>(_context);
                }

                return _personRepository;
            }
        }

        public GenericRepository<PaymentMethod> PaymentMethodRepository
        {
            get
            {
                if (this._paymentMethodRepository == null)
                {
                    this._paymentMethodRepository = new GenericRepository<PaymentMethod>(_context);
                }

                return _paymentMethodRepository;
            }
        }

        public GenericRepository<RoomStatu> RoomStatusRepository
        {
            get
            {
                if (this._roomStatusRepository == null)
                {
                    this._roomStatusRepository = new GenericRepository<RoomStatu>(_context);
                }

                return _roomStatusRepository;
            }
        }


        public GenericRepository<Item> ItemRepository
        {
            get
            {
                if (this._itemRepository == null)
                {
                    this._itemRepository = new GenericRepository<Item>(_context);
                }

                return _itemRepository;
            }
        }

        public GenericRepository<Expense> ExpenseRepository
        {
            get
            {
                if (this._expenseRepository == null)
                {
                    this._expenseRepository = new GenericRepository<Expense>(_context);
                }

                return _expenseRepository;
            }
        }

        public GenericRepository<Transaction> TransactionRepository
        {
            get
            {
                if (this._transactionRepository == null)
                {
                    this._transactionRepository = new GenericRepository<Transaction>(_context);
                }
                return _transactionRepository;
            }
        }


        public GenericRepository<GuestReservation> GuestReservationRepository
        {
            get
            {
                if (this._guestReservationRepository == null)
                {
                    this._guestReservationRepository = new GenericRepository<GuestReservation>(_context);
                }
                return _guestReservationRepository;
            }
        }



        public GenericRepository<BusinessAccount> BusinessAccountRepository
        {
            get
            {
                if (this._businessAccountRepository == null)
                {
                    this._businessAccountRepository = new GenericRepository<BusinessAccount>(_context);
                }
                return _businessAccountRepository;
            }
        }

        public GenericRepository<GuestRoomAccount> GuestRoomAccountRepository
        {
            get
            {
                if (this._guestRoomAccountRepository == null)
                {
                    this._guestRoomAccountRepository = new GenericRepository<GuestRoomAccount>(_context);
                }
                return _guestRoomAccountRepository;
            }
        }

        public GenericRepository<Guest> GuestRepository
        {
            get
            {
                if (this._guestRepository == null)
                {
                    this._guestRepository = new GenericRepository<Guest>(_context);
                }
                return _guestRepository;
            }
        }

        public GenericRepository<GuestRoom> GuestRoomRepository
        {
            get
            {
                if (this._guestRoomRepository == null)
                {
                    this._guestRoomRepository = new GenericRepository<GuestRoom>(_context);
                }
                return _guestRoomRepository;
            }
        }

        public GenericRepository<Room> RoomRepository
        {
            get
            {
                if (this._roomRepository == null)
                {
                    this._roomRepository = new GenericRepository<Room>(_context);
                }
                return _roomRepository;
            }
        }

        public GenericRepository<RoomType> RoomTypeRepository
        {
            get
            {
                if (this._roomTypeRepository == null)
                {
                    this._roomTypeRepository = new GenericRepository<RoomType>(_context);
                }
                return _roomTypeRepository;
            }
        }

        public GenericRepository<Pictural> PicturalRepository
        {
            get
            {
                if (this._picturalRepository == null)
                {
                    this._picturalRepository = new GenericRepository<Pictural>(_context);
                }
                return _picturalRepository;
            }
        }

        public void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                throw;
            }
            
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

