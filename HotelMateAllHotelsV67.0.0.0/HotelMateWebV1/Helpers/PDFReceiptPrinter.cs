using HotelMateWeb.Dal.DataCore;
using HotelMateWebV1.Helpers.Enums;
using Invoicer.Models;
using Invoicer.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using HotelMateWebV1.Models;

namespace HotelMateWebV1.Helpers
{
    public static class PDFReceiptPrinter
    {
        public static string PrintInvoice(string path, string currency, string reference, string imagePath, string companyReg, string[] companyDetails, string[] clientDetails, List<ItemRow> itemsRows, List<TotalRow> totalRows, List<DetailRow> detailRows, string footerWebsite)
        {
            if (string.IsNullOrEmpty(reference))
            {
                reference = DateTime.Now.ToString();
            }

            string filename = new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, currency)
                 .TextColor("#CC0000")
                 .BackColor("#FFD6CC")
                 .Image(imagePath, 125, 27)
                 .Reference(reference)
                 .Company(Address.Make("FROM", companyDetails, companyReg, ""))
                 .Client(Address.Make("BILLING TO", clientDetails))
                 .Items(itemsRows)
                 .Totals(totalRows)
                 .Details(detailRows)
                 .Footer(footerWebsite)
                 .Save(path,0);

            return filename;
        }

        public static string PrintInvoicePayment(string path, GuestRoomAccount gra)
        {
            string[] splitShopDetails = null;

            var guest = gra.GuestRoom.Guest;

            try
            {
                var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                splitShopDetails = shopDetails.Split('@').ToArray();

                if (splitShopDetails.Count() < 7)
                {
                    splitShopDetails = null;
                }

            }
            catch (Exception)
            {
            }

            if (splitShopDetails == null)
            {
                splitShopDetails = new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }

            string currency = "NGN", reference = gra.Id.ToString() + "_" + DateTime.Now.ToShortTimeString().Replace(":", ""), imagePath = path, companyReg = splitShopDetails[6];
            string[] companyDetails = splitShopDetails, clientDetails = GetClientInformation(guest);
            List<ItemRow> itemsRows = new List<ItemRow>();
            List<TotalRow> totalRows = new List<TotalRow>();
            List<DetailRow> detailRows = new List<DetailRow>();
            string footerWebsite = splitShopDetails[5];

            var initialDeposit = gra;

            var gr = guest.GuestRooms.FirstOrDefault();
            var noOfNights = gr.CheckoutDate.Subtract(gr.CheckinDate).Days;
            var roomDetails = "Room " + gr.RoomNumber + "-" + gr.Room.RoomType1.Name;
            var amountPaid = decimal.Zero;

            if (initialDeposit != null)
            {
                amountPaid = initialDeposit.Amount;
            }

            if (string.IsNullOrEmpty(reference))
            {
                reference = DateTime.Now.ToString();
            }

            string filename = new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, currency)
                 .TextColor("#CC0000")
                 .BackColor("#FFD6CC")
                 .Image(imagePath, 125, 27)
                 .Reference(reference)
                 .Company(Address.Make("FROM", companyDetails, companyReg, ""))
                 .Client(Address.Make("BILLING TO", clientDetails))
                 .Items(new List<ItemRow>
                 {
                    ItemRow.Make(initialDeposit.RoomPaymentType.Description, initialDeposit.PaymentMethod.Description , (decimal)1, 0, amountPaid, amountPaid)
                 })
                   .Totals(new List<TotalRow> {
                    TotalRow.Make("Sub Total", amountPaid),
                    TotalRow.Make("TAX @ 0%", decimal.Zero),
                    TotalRow.Make("Total", amountPaid, true),
               })
                  .Details(new List<DetailRow>
                  {
                    DetailRow.Make("PAYMENT INFORMATION", "A copy of this receipt will also be emailed to you.", "", "If you have any questions concerning this receipt, contact our front office or a duty manager.", "", "Thank you for your business.")
                  })
                 .Footer(footerWebsite)
                 .Save(path, 0);

            return reference;
        }

        public static string PrintInvoiceChecking(string path, GuestRoomAccount gra)
        {
            string[] splitShopDetails = null;

            var guest = gra.GuestRoom.Guest;

            try
            {
                var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                splitShopDetails = shopDetails.Split('@').ToArray();

                if (splitShopDetails.Count() < 7)
                {
                    splitShopDetails = null;
                }

            }
            catch (Exception)
            {
            }

            if (splitShopDetails == null)
            {
                splitShopDetails = new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }

            string currency = "NGN", reference = gra.Id.ToString() + "_" + DateTime.Now.ToShortTimeString().Replace(":", ""), imagePath = path, companyReg = splitShopDetails[6];
            string[] companyDetails = splitShopDetails, clientDetails = GetClientInformation(guest);
            List<ItemRow> itemsRows = new List<ItemRow>();
            List<TotalRow> totalRows = new List<TotalRow>();
            List<DetailRow> detailRows = new List<DetailRow>();
            string footerWebsite = splitShopDetails[5];

            var initialDeposit = gra;

            var gr = guest.GuestRooms.FirstOrDefault();
            var noOfNights = gr.CheckoutDate.Subtract(gr.CheckinDate).Days;
            var roomDetails = "Room " + gr.RoomNumber + "-" + gr.Room.RoomType1.Name;
            var amountPaid = decimal.Zero;

            if (initialDeposit != null)
            {
                amountPaid = initialDeposit.Amount;
            }

            if (string.IsNullOrEmpty(reference))
            {
                reference = DateTime.Now.ToString();
            }

            string filename = new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, currency)
                 .TextColor("#CC0000")
                 .BackColor("#FFD6CC")
                 .Image(imagePath, 125, 27)
                 .Reference(reference)
                 .Company(Address.Make("FROM", companyDetails, companyReg, ""))
                 .Client(Address.Make("BILLING TO", clientDetails))
                 .Items(new List<ItemRow>
                 {
                    ItemRow.Make(initialDeposit.RoomPaymentType.Description, initialDeposit.PaymentMethod.Description , (decimal)1, 0, amountPaid, amountPaid)
                 })
                   .Totals(new List<TotalRow> {
                    TotalRow.Make("Sub Total", amountPaid),
                    TotalRow.Make("TAX @ 0%", decimal.Zero),
                    TotalRow.Make("Total", amountPaid, true),
               })
                  .Details(new List<DetailRow>
                  {
                    DetailRow.Make("PAYMENT INFORMATION", "A copy of this receipt will also be emailed to you.", "", "If you have any questions concerning this receipt, contact our front office or a duty manager.", "", "Thank you for your business.")
                  })
                 .Footer(footerWebsite)
                 .Save(path,0);

            return reference;
        }

        public static string PrintInvoiceCheckout(string path, Guest guest)
        {
            string[] splitShopDetails = null;

            try
            {
                var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                splitShopDetails = shopDetails.Split('@').ToArray();

                if (splitShopDetails.Count() < 7)
                {
                    splitShopDetails = null;
                }

            }
            catch (Exception)
            {
            }

            if (splitShopDetails == null)
            {
                splitShopDetails = new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }

            string currency = "NGN", reference = guest.Id.ToString() + "_" + DateTime.Now.ToShortTimeString().Replace(":", ""), imagePath = path, companyReg = splitShopDetails[6];
            string[] companyDetails = splitShopDetails, clientDetails = GetClientInformation(guest);
            List<ItemRow> itemsRows = new List<ItemRow>();
            List<TotalRow> totalRows = new List<TotalRow>();
            List<DetailRow> detailRows = new List<DetailRow>();
            string footerWebsite = splitShopDetails[5];

            var initialDeposit = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).FirstOrDefault(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit);

            var gr = guest.GuestRooms.FirstOrDefault();
            var noOfNights = gr.CheckoutDate.Subtract(gr.CheckinDate).Days;
            var roomDetails = "Room " + gr.RoomNumber + "-" + gr.Room.RoomType1.Name;
            var amountPaidCredit = decimal.Zero;
            var amountPaidDebit = decimal.Zero;
            var balance = decimal.Zero;

            amountPaidCredit = guest.GetGuestTotalPaid();

            var guestItems = guest.GetGuestItems().Select( x => new ItemRow { Amount = x.Amount, Description = x.RoomPaymentType.Description, Name = x.RoomPaymentType.Description, Price = x.Amount, Discount = decimal.Zero.ToString(), Total = x.Amount, VAT = decimal.Zero }).ToList();

            amountPaidDebit = guestItems.Sum(x => x.Amount);

            balance = amountPaidCredit - amountPaidDebit;

            if (string.IsNullOrEmpty(reference))
            {
                reference = DateTime.Now.ToString();
            }

            string filename = new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, currency)
                 .TextColor("#CC0000")
                 .BackColor("#FFD6CC")
                 .Image(imagePath, 125, 27)
                 .Reference(reference)
                 .Company(Address.Make("FROM", companyDetails, companyReg, ""))
                 .Client(Address.Make("BILLING TO", clientDetails))
                 .Items(guestItems)
                 .Totals(new List<TotalRow> {
                    TotalRow.Make("Payments Total", amountPaidCredit),
                    TotalRow.Make("Sub Total", amountPaidDebit),
                    TotalRow.Make("TAX @ 0%", decimal.Zero),
                    TotalRow.Make("Balance", balance, true),
                  })
                  .Details(new List<DetailRow>
                  {
                    DetailRow.Make("CHECK-OUT INFORMATION", "A copy of this receipt will also be emailed to you.", "", "If you have any questions concerning this receipt, contact our front office or a duty manager.", "", "Thank you for your business.")
                  })
                 .Footer(footerWebsite)
                 .Save(path,0);

            return reference;
        }

        public static string PrintInvoiceCheckout(string path, GuestRoomAccountViewModel gravm, string imagePath)
        {

            var guestItems = new List<ItemRow>();

           foreach (var rm in gravm.DisplayList.OrderBy(x => x.TransactionDate))
           {
                guestItems.Add(new ItemRow { TransactionDate = rm.TransactionDate, Amount = rm.Balance,  Description = rm.Detail,  Credit = rm.Credit, Debit = rm.Debit, Balance = rm.Balance });
           }
            

            var guest = gravm.Guest;


            string[] splitShopDetails = null;

            try
            {
                var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                splitShopDetails = shopDetails.Split('@').ToArray();

                if (splitShopDetails.Count() < 7)
                {
                    splitShopDetails = null;
                }

            }
            catch (Exception)
            {
            }

            if (splitShopDetails == null)
            {
                splitShopDetails = new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }

            string currency = "NGN", reference = guest.Id.ToString() + "_" + DateTime.Now.ToShortTimeString().Replace(":", ""), companyReg = splitShopDetails[6];
            string[] companyDetails = splitShopDetails, clientDetails = GetClientInformation(gravm);
            List<ItemRow> itemsRows = new List<ItemRow>();
            List<TotalRow> totalRows = new List<TotalRow>();
            List<DetailRow> detailRows = new List<DetailRow>();
            string footerWebsite = splitShopDetails[5];

            var initialDeposit = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).FirstOrDefault(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit);

            var gr = guest.GuestRooms.FirstOrDefault();
            var noOfNights = gr.CheckoutDate.Subtract(gr.CheckinDate).Days;
            var roomDetails = "Room " + gr.RoomNumber + "-" + gr.Room.RoomType1.Name;
            var amountPaidCredit = decimal.Zero;
            var amountPaidDebit = decimal.Zero;
            var balance = decimal.Zero;

            amountPaidCredit = guestItems.Where(x => x.Credit > 0).Sum(x => x.Credit);

            amountPaidDebit = guestItems.Where(x => x.Debit > 0).Sum(x => x.Debit);

            balance = amountPaidCredit - amountPaidDebit;

            if (string.IsNullOrEmpty(reference))
            {
                reference = DateTime.Now.ToString();
            }

            var totalRowsTotal = new List<TotalRow>();

            var taxRow = guestItems.FirstOrDefault(x => x.Description.ToUpper().Contains("TAX"));

            if(taxRow != null)
            {
                amountPaidDebit = amountPaidDebit - taxRow.Debit;
            }

            totalRowsTotal.Add(TotalRow.Make("Payments Total", amountPaidCredit));

            //2347057706701
            totalRowsTotal.Add(TotalRow.Make("Sub Total", amountPaidDebit));

            if(taxRow != null)
            {
                totalRowsTotal.Add(TotalRow.Make(taxRow.Description, taxRow.Debit));
            }

            totalRowsTotal.Add(TotalRow.Make("Balance", balance, true));

            string filename = new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, currency)
                 .TextColor("#CC0000")
                 .BackColor("#FFD6CC")
                 .Image(imagePath, 125, 27)
                 .Reference(reference)
                 .Company(Address.Make("FROM", companyDetails, companyReg, ""))
                 .Client(Address.Make("BILLING TO", clientDetails))
                 .Items(guestItems)
                 .Totals(totalRowsTotal)
                 .Details(new List<DetailRow>
                  {
                    DetailRow.Make("CHECK-OUT INFORMATION", "A copy of this receipt will also be emailed to you.", "", "If you have any questions concerning this receipt, contact our front office or a duty manager.", "", "Thank you for your business."),
                    DetailRow.Make("INFORMATION", "")
                  })
                 .Footer(footerWebsite)
                 .Save(path,1);

            return reference;
        }

        private static List<string> GetTAC()
        {
            var tac = string.Empty;

            try
            {
                tac = ConfigurationManager.AppSettings["TermsAndConditions"].ToString();
            }
            catch
            {
                tac = "";
            }

            if (string.IsNullOrEmpty(tac))
            {
                return new List<string>();
            }
            else
            {
                var splitter = tac.Split('@');

                if (splitter.Length > 0)
                    return splitter.ToList();

            }

            return new List<string>();
        }


        private static List<string> GetAcknowledge()
        {
            var tac = string.Empty;

            try
            {
                tac = ConfigurationManager.AppSettings["Acknowledge"].ToString();
            }
            catch
            {
                tac = "";
            }

            if (string.IsNullOrEmpty(tac))
            {
                return new List<string>();
            }
            else
            {
                var splitter = tac.Split('@');

                if (splitter.Length > 0)
                    return splitter.ToList();

            }

            return new List<string>();
        }

        public static string PrintInvoiceChecking(string path, Guest guest)
        {
            string[] splitShopDetails = null;

            var termsAndConditions = GetTAC();

            var acknowledge = GetAcknowledge();

            var bottomDetailsRows = new List<DetailRow>();

            bottomDetailsRows.Add(DetailRow.Make("CHECK-IN PAYMENT INFORMATION", "A copy of this receipt will also be emailed to you.", "", "If you have any questions concerning this receipt, contact our front office or a duty manager.", "", "Thank you for your business."));

            if (termsAndConditions.Any())
            {
                bottomDetailsRows.Add(DetailRow.Make("TERMS && CONDITIONS", termsAndConditions.ToArray()));
            }

            if(acknowledge.Any())
            {
                bottomDetailsRows.Add(DetailRow.Make("ACKNOWLEDGEMENT", acknowledge.ToArray()));
            }

            try
            {
                var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                splitShopDetails = shopDetails.Split('@').ToArray();

                if (splitShopDetails.Count() < 7)
                {
                    splitShopDetails = null;
                }

            }
            catch (Exception)
            {
            }

            if (splitShopDetails == null)
            {
                splitShopDetails = new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }

            string currency = "NGN",  reference = guest.Id.ToString() + "_" + DateTime.Now.ToShortTimeString().Replace(":",""),   companyReg = splitShopDetails[6];
            string[] companyDetails = splitShopDetails, clientDetails = GetClientInformation(guest);
            List<ItemRow> itemsRows = new List<ItemRow>();
            List<TotalRow> totalRows = new List<TotalRow>();
            List<DetailRow> detailRows = new List<DetailRow>();
            string footerWebsite = splitShopDetails[5];

            var initialDeposit = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).FirstOrDefault(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit);

            var gr = guest.GuestRooms.FirstOrDefault();
            var noOfNights = gr.CheckoutDate.Subtract(gr.CheckinDate).Days;
            var roomDetails = "Room " + gr.RoomNumber + "-" + gr.Room.RoomType1.Name;
            var amountPaid = decimal.Zero;

            if(initialDeposit != null)
            {
                amountPaid = initialDeposit.Amount;
            }

            if (string.IsNullOrEmpty(reference))
            {
                reference = DateTime.Now.ToString();
            }

            string filename = new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, currency)
                 .TextColor("#CC0000")
                 .BackColor("#FFD6CC")
                 .Image(@"..\..\images\berkshire.jpg", 125, 27)
                 .Reference(reference)
                 .Company(Address.Make("FROM", companyDetails, companyReg, ""))
                 .Client(Address.Make("BILLING TO", clientDetails))
                 .Items(new List<ItemRow>
                 {
                    ItemRow.Make("Initial Deposit", roomDetails , (decimal)1, 0, amountPaid, amountPaid)
                 })
                   .Totals(new List<TotalRow> {
                    TotalRow.Make("Sub Total", amountPaid),
                    TotalRow.Make("TAX @ 0%", decimal.Zero),
                    TotalRow.Make("Total", amountPaid, true),
               })
               .Details(bottomDetailsRows)
               .Footer(footerWebsite)
               .Save(path,0);

            return reference;
        }

        public static string PrintInvoiceCheckingFuture(string path, Guest guest)
        {

            int qty = guest.GuestRooms.Count;

            string[] splitShopDetails = null;

            var termsAndConditions = GetTAC();

            var acknowledge = GetAcknowledge();

            var bottomDetailsRows = new List<DetailRow>();

            bottomDetailsRows.Add(DetailRow.Make("RESERVATION INFORMATION", "A copy of this receipt will also be emailed to you.", "", "If you have any questions concerning this receipt, contact our front office or a duty manager.", "", "Thank you for your business."));

            if (termsAndConditions.Any())
            {
                bottomDetailsRows.Add(DetailRow.Make("TERMS && CONDITIONS", termsAndConditions.ToArray()));
            }

            if (acknowledge.Any())
            {
                bottomDetailsRows.Add(DetailRow.Make("ACKNOWLEDGEMENT", acknowledge.ToArray()));
            }

            try
            {
                var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                splitShopDetails = shopDetails.Split('@').ToArray();

                if (splitShopDetails.Count() < 7)
                {
                    splitShopDetails = null;
                }

            }
            catch (Exception)
            {
            }

            if (splitShopDetails == null)
            {
                splitShopDetails = new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }

            string currency = "NGN", reference = guest.Id.ToString() + "_" + DateTime.Now.ToShortTimeString().Replace(":", ""), companyReg = splitShopDetails[6];
            string[] companyDetails = splitShopDetails, clientDetails = GetClientInformation(guest);
            List<ItemRow> itemsRows = new List<ItemRow>();
            List<TotalRow> totalRows = new List<TotalRow>();
            List<DetailRow> detailRows = new List<DetailRow>();
            string footerWebsite = splitShopDetails[5];

            var initialDeposit = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).FirstOrDefault(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit);

            var gr = guest.GuestRooms.FirstOrDefault();
            var noOfNights = gr.CheckoutDate.Subtract(gr.CheckinDate).Days;
            var roomDetails = "From " + gr.CheckinDate.ToShortDateString() + " To " + gr.CheckoutDate.ToShortDateString() + " Room Type - " + gr.Room.RoomType1.Name;
            var amountPaid = decimal.Zero;

            if (initialDeposit != null)
            {
                amountPaid = initialDeposit.Amount;
            }

            if (string.IsNullOrEmpty(reference))
            {
                reference = DateTime.Now.ToString();
            }

            string filename = new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, currency)
                 .TextColor("#CC0000")
                 .BackColor("#FFD6CC")
                 .Image(@"..\..\images\berkshire.jpg", 125, 27)
                 .Reference(reference)
                 .Company(Address.Make("FROM", companyDetails, companyReg, ""))
                 .Client(Address.Make("BILLING TO", clientDetails))
                 .Items(new List<ItemRow>
                 {
                    ItemRow.Make("Resevation Deposit", roomDetails , (decimal)qty, 0, amountPaid, amountPaid)
                 })
                   .Totals(new List<TotalRow> {
                    TotalRow.Make("Sub Total", amountPaid),
                    TotalRow.Make("TAX @ 0%", decimal.Zero),
                    TotalRow.Make("Total", amountPaid, true),
               })
               .Details(bottomDetailsRows)
               .Footer(footerWebsite)
               .Save(path, 0);

            return reference;
        }
        private static string[] GetClientInformation(GuestRoomAccountViewModel model)
        {
            List<string> strList = new List<string>();

            try
            {
                strList.Add("Room No. :" + model.GuestRoomNumber);
                strList.Add("Guest Name :" + model.GuestName);
                strList.Add("Arrival Date :" + model.ArrivalDate);
                strList.Add("Departure Date :" + model.DepartureDate);
                strList.Add("No Of Night :" + model.NoOfNight);
                strList.Add("Bill No :" + model.BillNo);

                return strList.ToArray();
            }
            catch
            {
                return new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }
        }

        private static string[] GetClientInformation(Guest guest)
        {
            List<string> strList = new List<string>();

            try
            {
                strList.Add(guest.FullName);
                strList.Add(guest.Email);
                strList.Add(guest.Telephone);
                strList.Add(guest.Address);
                //strList.Add("No. Of Nights : 2");

                return strList.ToArray();
            }
            catch
            {
                return new string[] { "AcademyVista Ltd. @ B03 Eleganza, V.G.C, @ Lagos State @ Nigeria @ 08105387515 @ www.academyvista.com @ 6543210" };
            }
        }
    }
}