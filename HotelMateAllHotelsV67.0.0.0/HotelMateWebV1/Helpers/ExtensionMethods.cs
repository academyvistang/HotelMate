using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using HotelMateWeb.Dal.DataCore;
using HotelMateWebV1.Helpers.Enums;


namespace HotelMateWebV1.Helpers
{
    public static class ExtensionMethods
    {
        public static decimal GetTotalHotelRecievable(this Person person, DateTime salesDate)
        {
            return person.GuestRoomAccounts.Where(x => x.TransactionDate.ToShortDateString().Equals(salesDate.ToShortDateString()) &&
                (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.Cash)).Sum(x => x.Amount);
        }

        public static decimal GetTotalBarRecievable(this Person person, DateTime salesDate)
        {
            return person.SoldItemsAlls.Where(x => (x.TillOpen) && (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.Cash)).Sum(x => x.TotalPrice);
        }

        public static decimal TotalAccounts(this BusinessAccount company)
        {
            var accounts = company.GuestRooms.SelectMany(x => x.GuestRoomAccounts);
            return accounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL) 
                || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit 
                || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit 
                )).Summation();
        }


        public static decimal TotalAccounts(this Guest guest)
        {
            var accounts = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts);
            return accounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL) 
                || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit 
                || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit 
                || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit 
                )).Summation();
            //return decimal.Zero;
        }
        public static string GetRoomGuest(this Room room)
        {
            var gr = room.GuestRooms.Where(x => x.Guest.IsActive && x.IsActive).OrderByDescending(x => x.CheckinDate).ToList();
            return gr.Count == 0 ? "EMPTY ROOM" : gr.FirstOrDefault().Guest.FullName;
        }

        public static Guest GetActualRoomGuest(this Room room)
        {
            var gr = room.GuestRooms.Where(x => x.Guest.IsActive && x.IsActive).OrderByDescending(x => x.CheckinDate).ToList();
            return gr.Count == 0 ? null : gr.FirstOrDefault().Guest;
        }

        public static long TicksNonNeg(this DateTime dt)
        {
            var ticks = dt.Ticks;
            return ticks < 0 ? ticks*(-1) : ticks;
        }

        public static string ToDelimitedString(this IEnumerable<string> list, string delimiter)
        {
            return list == null ? string.Empty : string.Join(delimiter, list.ToArray());
        }

        public static bool IsBetween(this DateTime valToCheck, DateTime startdate, DateTime endDate)
        {
            var hotelAccountsTime = 14;
            int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out hotelAccountsTime);
            valToCheck = new DateTime(valToCheck.Year, valToCheck.Month, valToCheck.Day, hotelAccountsTime, 1, 0);
            startdate = new DateTime(startdate.Year, startdate.Month, startdate.Day, hotelAccountsTime, 0, 0);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, hotelAccountsTime, 0, 0);
            var conflicting = valToCheck >= startdate && valToCheck <= endDate;
            //var otherConflicts = startdate <= valToCheck && endDate <= valToCheck;

            return conflicting;// || otherConflicts;
        }

        public static bool IsBetweenStartEnd(this DateTime valToCheck, DateTime valToCheckEnd, DateTime startdate, DateTime endDate)
        {
            var hotelAccountsTime = 14;
            int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out hotelAccountsTime);
            valToCheck = new DateTime(valToCheck.Year, valToCheck.Month, valToCheck.Day, hotelAccountsTime, 1, 0);
            startdate = new DateTime(startdate.Year, startdate.Month, startdate.Day, hotelAccountsTime, 0, 0);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, hotelAccountsTime, 0, 0);
            //var conflicting = valToCheck >= startdate && valToCheck <= endDate;
            //var otherConflicts = startdate <= valToCheck && endDate <= valToCheck;

            if (valToCheck <= startdate && valToCheckEnd >= endDate)
                return true;

            return false;// || otherConflicts;
        }

        

        public static bool ReportIsBetween(this DateTime valToCheck, DateTime startdate, DateTime endDate)
        {
            var conflicting = valToCheck >= startdate && valToCheck <= endDate;
            return conflicting;
        }

        public static IList<GuestReservation> SelectAvailable(this IEnumerable<GuestReservation> guestReservations, DateTime startDateTime, DateTime endDateTime, int? roomTypeId)
        {       

            if (roomTypeId.HasValue && roomTypeId.Value > 0)
            {
                var conflictingReservations = guestReservations.Where(x => ((x.Guest.IsActive && x.IsActive) && (startDateTime.IsBetween(x.StartDate, x.EndDate) || endDateTime.IsBetween(x.StartDate, x.EndDate))) && x.Room.RoomType == roomTypeId).ToList();
                return conflictingReservations;
            }
            else
            {
                var conflictingReservations = guestReservations.Where(x => (x.Guest.IsActive && x.IsActive) && (startDateTime.IsBetween(x.StartDate, x.EndDate) || endDateTime.IsBetween(x.StartDate, x.EndDate))).ToList();
                return conflictingReservations;
            }
        }

        public static string GetSymbolPath(this GuestRoomAccount guestRoomAccount, string scheme, string authourity, string content)
        {
            
            string url = string.Format("{0}://{1}{2}", scheme,authourity,content);
            string strPathPlus = url + "images/" + "plus_16.png";
            string strPathMinus = url + "images/" + "minus_16.png";
            return guestRoomAccount.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit ? strPathPlus : strPathMinus;
        }


        public static string GetSymbol(this GuestRoomAccount guestRoomAccount)
        {
            return guestRoomAccount.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit ? "plus_16.png" : "minus_16.png";
        }

        public static bool GuestRoomsBeRemoved(this Guest guest)
        {
            var cancellationHours = 4;
            int.TryParse(ConfigurationManager.AppSettings["HotelCancellationHours"].ToString(CultureInfo.InvariantCulture), out cancellationHours);
            var removeableGuestRooms = guest.GuestRooms.Count(x => !x.GuestRoomAccounts.Any() && (x.CheckinDate.AddHours(cancellationHours) > DateTime.Now));
            return removeableGuestRooms > 0;
        }

        public static IList<GuestReservation> RoomAvailability(this Room room, DateTime startDateTime, DateTime endDateTime, int? roomTypeId)
        {
            if (roomTypeId.HasValue && roomTypeId.Value > 0)
            {
                var conflictingReservations =
                   room.GuestReservations.Where(
                       x =>
                       ((x.IsActive) &&
                       (startDateTime.IsBetween(x.StartDate, x.EndDate) ||
                        endDateTime.IsBetween(x.StartDate, x.EndDate))) && x.Room.RoomType == roomTypeId.Value).ToList();

                if (conflictingReservations.Count > 0)
                return conflictingReservations;

                conflictingReservations =
                   room.GuestReservations.Where(
                       x =>
                       ((x.IsActive) &&
                       (startDateTime.IsBetweenStartEnd(endDateTime, x.StartDate, x.EndDate))) && x.Room.RoomType == roomTypeId.Value).ToList();

                return conflictingReservations;
            }
            else
            {
                var conflictingReservations =
                    room.GuestReservations.Where(
                        x =>
                        (x.IsActive) &&
                        (startDateTime.IsBetween(x.StartDate, x.EndDate) ||
                         endDateTime.IsBetween(x.StartDate, x.EndDate))).ToList();

                if (conflictingReservations.Count > 0)
                    return conflictingReservations;

                conflictingReservations =
                   room.GuestReservations.Where(
                       x =>
                       ((x.IsActive) &&
                       (startDateTime.IsBetweenStartEnd(endDateTime, x.StartDate, x.EndDate)))).ToList();

                return conflictingReservations;
            }
        }

        public static IList<GuestReservation> RoomAvailability(this Room room, DateTime startDateTime, DateTime endDateTime, int guestId, int? roomTypeId)
        {
            if (roomTypeId.HasValue && roomTypeId.Value > 0)
            {
                var conflictingReservations =
                    room.GuestReservations.Where(
                        x =>
                        (((x.IsActive) &&
                         (startDateTime.IsBetween(x.StartDate, x.EndDate) ||
                          endDateTime.IsBetween(x.StartDate, x.EndDate))) && x.GuestId != guestId) && roomTypeId == x.Room.RoomType).ToList();

                if (conflictingReservations.Count > 0)
                    return conflictingReservations;

                conflictingReservations =
                   room.GuestReservations.Where(
                       x =>
                       ((x.IsActive) &&
                       (startDateTime.IsBetweenStartEnd(endDateTime, x.StartDate, x.EndDate))) && x.Room.RoomType == roomTypeId.Value).ToList();

                return conflictingReservations;
            }
            else
            {
                var conflictingReservations =
                   room.GuestReservations.Where(
                       x =>
                       ((x.IsActive) &&
                        (startDateTime.IsBetween(x.StartDate, x.EndDate) ||
                         endDateTime.IsBetween(x.StartDate, x.EndDate))) && x.GuestId != guestId).ToList();
                if (conflictingReservations.Count > 0)
                    return conflictingReservations;

                conflictingReservations =
                   room.GuestReservations.Where(
                       x =>
                       ((x.IsActive) &&
                       (startDateTime.IsBetweenStartEnd(endDateTime, x.StartDate, x.EndDate)))).ToList();

                return conflictingReservations;
            }
        }

        public static string BookRoom(this Room room)
        {
            if (room.GuestRooms.Any(x => x.IsActive))
                return "Update Guest Details For Room " + room.RoomNumber;
            return "Book New Guest Into Room " + room.RoomNumber;
        }

        
        public static string GetStatusBaccGroundColor(this Room room)
        {
            var statusId = room.RoomStatu.Id;

            switch (statusId)
            {
                case (int)RoomStatusEnum.Repair:
                    return "#ff0000";
                case (int)RoomStatusEnum.Dirty:
                    return "#778899";
                case (int)RoomStatusEnum.Unknown:
                    return "#778899";
                default:
                    return "";
            }
        }

        public static string GetCompanyBalanceColour(this BusinessAccount company)
        {
            var balance = company.GetGuestBalance();

            if (balance < 0)
                return "#ff0000";
            else if (balance == 0)
                return "#7aa37a";
            else
                return "#99f893";
        }

        public static string GetGuestBalanceColour(this BusinessAccount company)
        {
            var balance = company.GetGuestBalance();

            if (balance < 0)
                return "#ff0000";
            else if (balance == 0)
                return "#7aa37a";
            else
                return "#99f893";
        }


        public static string GetGuestBalanceWithTaxColour(this Guest guest)
        {
            var balance = guest.GuestRooms.TotallySummation("-1", "-1");

            if (balance < 0)
                return "#ff0000";
            else if (balance == 0)
                return "#7aa37a";
            else
                return "#99f893";
        }

        public static string GetGuestBalanceColour(this Guest guest)
        {
            var balance = guest.GetGuestBalance();

            if (balance < 0)
                return "#ff0000";
            else if (balance == 0)
                return "#7aa37a";
            else
                return "#99f893";
        }

        
        public static decimal TotalSpent(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.DebitSummation();
            return totalRooms;
        }

        public static decimal TotalPaidSoFarCash(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.CreditSummationCash();
            return totalRooms;
        }

        public static decimal TotalPaidSoFar(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.CreditSummation();
            return totalRooms;
        }

        public static decimal GetGuestRoomBalance(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.Summation();
            return totalRooms;
        }

        public static decimal GetGuestReservationBalance(this Guest guest)
        {
            
            var accountTotal = decimal.Zero;

            foreach (var rm in guest.GuestRooms.Where(x => x.GuestRoomAccounts.Sum(y => y.Amount) > 0))
            {
                accountTotal += rm.GuestRoomAccounts.Where(x => x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit).Sum(x => x.Amount);
            }

            var total = accountTotal;

            return total;
        }

        public static decimal GetCompanyBalanceWithTax(this BusinessAccount company)
        {
            var totalRooms = company.GuestRooms.Summation();

            var halfDay = company.GuestRooms.SelectMany(x => x.GuestRoomAccounts).Where(y => y.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay).Sum(z => z.Amount);

            var hotelTax = GetHotelTax(company.Id);

            var reservationTax = 0;

            var rg = company.Guests.FirstOrDefault(x => !string.IsNullOrEmpty(x.PassportNumber));

            if(rg != null)
            {
                reservationTax = GetReservationTax(company.Id);
            }

            var restTax = GetRestaurantTax();

            if (company.Id > 0)
            {
                restTax = hotelTax;
            }

            var totalRoomsTax = decimal.Zero;

            if (totalRooms > 0 && hotelTax > 0)
            {
                totalRoomsTax = ((totalRooms) * hotelTax / 100);
            }

            if (halfDay > 0 && hotelTax > 0)
            {
                totalRoomsTax += ((halfDay) * hotelTax / 100);
            }


            var accountTotal = decimal.Zero;

            var allItems = company.SoldItemsAlls.ToList();

            var restaurantTotal = allItems.Sum(x => x.TotalPrice);

            var restaurantTotalTax = decimal.Zero;

            if (totalRooms > 0 && hotelTax > 0)
            {
                restaurantTotalTax = ((restaurantTotal) * restTax / 100);
            }

            foreach (var rm in company.GuestRooms.Where(x => x.GuestRoomAccounts.Sum(y => y.Amount) > 0))
            {
                accountTotal += rm.GuestRoomAccounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL)
                    || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay)).Summation();
            }

            totalRooms = totalRooms + restaurantTotalTax + totalRoomsTax;

            if(reservationTax > 0)
            {
                totalRooms = ((totalRooms) * reservationTax / 100);
            }

            var total = accountTotal - totalRooms;

            return total;
        }

        public static decimal GetCompanyBalance(this BusinessAccount company)
        {
            var totalPaid = company.BusinessCorporateAccounts.Sum(x => x.Amount);

            var total = company.GetCompanyBalanceWithTax();

            return totalPaid - Decimal.Negate(total);
        }

        public static decimal GetFullTax(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.Summation();

            var halfDay = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).Where(y => y.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay).Sum(z => z.Amount);

            var hotelTax = GetHotelTax(guest.CompanyId);


            var restTax = GetRestaurantTax();

            if(guest.CompanyId.HasValue)
            {
                restTax = hotelTax;
            }

            var totalRoomsTax = decimal.Zero;

            if (totalRooms > 0 && hotelTax > 0)
            {
                totalRoomsTax = ((totalRooms) * hotelTax / 100);
            }

            if (halfDay > 0 && hotelTax > 0)
            {
                totalRoomsTax += ((halfDay) * hotelTax / 100);
            }

            var allItems = guest.SoldItemsAlls.ToList();

            var restaurantTotal = allItems.Sum(x => x.TotalPrice);

            var restaurantTotalTax = decimal.Zero;

            if (totalRooms > 0 && hotelTax > 0)
            {
                restaurantTotalTax = ((restaurantTotal) * restTax / 100);
            }

            var totalTax = restaurantTotalTax + totalRoomsTax;

            return totalTax;
        }


        public static decimal GetGuestBalanceWithFullTax(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.Summation();

            var halfDay = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).Where(y => y.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay).Sum(z => z.Amount);

            var hotelTax = GetHotelTax(guest.CompanyId);

            var restTax = GetRestaurantTax();

            if(guest.CompanyId.HasValue)
            {
                restTax = hotelTax;
            }

            
            var reservationTax = 0;

            if (!string.IsNullOrEmpty(guest.PassportNumber))
            {
                reservationTax = GetReservationTax(null);
            }

            var totalRoomsTax = decimal.Zero;

            if (totalRooms > 0 && hotelTax > 0)
            {
                totalRoomsTax = ((totalRooms) * hotelTax / 100);
            }

            if (halfDay > 0 && hotelTax > 0)
            {
                totalRoomsTax += ((halfDay) * hotelTax / 100);
            }

            var accountTotal = decimal.Zero;

            var allItems = guest.SoldItemsAlls.ToList();

            var restaurantTotal = allItems.Sum(x => x.TotalPrice);

            var postTotal = decimal.Zero;

            foreach (var rm in guest.GuestRooms.Where(x => x.GuestRoomAccounts.Sum(y => y.Amount) > 0))
            {
                postTotal += rm.GuestRoomAccounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL)).Summation();

                if (postTotal < 0)
                    postTotal = decimal.Negate(postTotal);
            }

            restaurantTotal = restaurantTotal + postTotal;

            var restaurantTotalTax = decimal.Zero;

            if (restTax > 0)
            {
                restaurantTotalTax = ((restaurantTotal) * restTax / 100) + restaurantTotal;
            }
            else
            {
                restaurantTotalTax = restaurantTotal;
            }

            

            foreach (var rm in guest.GuestRooms.Where(x => x.GuestRoomAccounts.Sum(y => y.Amount) > 0))
            {
                accountTotal += rm.GuestRoomAccounts.Where(x => (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay)).Summation();
            }

            totalRooms = totalRooms + restaurantTotalTax + totalRoomsTax;

            if (reservationTax > 0)
            {
                totalRooms += ((totalRooms) * reservationTax / 100);
            }

            var gd = guest.GuestRooms.FirstOrDefault(x => x.Children > 0);

            if (gd != null)
            {
                var guestDiscount = gd.Children;

                var thisTotal = totalRooms;

                if (thisTotal < 0)
                {
                    thisTotal = Decimal.Negate(totalRooms);
                    decimal discountedRate = decimal.Divide((gd.Children), 100);
                    var discount = discountedRate * thisTotal;
                    totalRooms = totalRooms + discount;
                }
                else
                {
                    decimal discountedRate = decimal.Divide((gd.Children), 100);
                    var discount = discountedRate * thisTotal;
                    totalRooms = totalRooms - discount;
                }

            }

            var total = accountTotal - totalRooms;
           
            return total;
            //total = total + restaurantTotalTax + totalRoomsTax;

            
        }

        public static decimal GetGuestBalance(this BusinessAccount company)
        {
            return company.GetCompanyBalanceWithTax();
        }

        private static int GetReservationTax(int? companyId)
        {
            int hTax = 0;

            try
            {
                int.TryParse(ConfigurationManager.AppSettings["ReservationTax"].ToString(), out hTax);
            }
            catch
            {
                hTax = 0;
            }

            return hTax;

            
        }

        private static int GetHotelTax(int? companyId)
        {
            int hTax = 0;

            try
            {
                if (companyId.HasValue)
                {
                    int.TryParse(ConfigurationManager.AppSettings["HotelCorporateTax"].ToString(), out hTax);
                }
                else
                {
                    int.TryParse(ConfigurationManager.AppSettings["HotelTax"].ToString(), out hTax);
                }
                
            }
            catch
            {
                hTax = 0;
            }

            return hTax;
        }

        private static int GetRestaurantTax()
        {
            int hTax = 0;

            try
            {
                int.TryParse(ConfigurationManager.AppSettings["RestaurantTax"].ToString(), out hTax);
            }
            catch
            {
                hTax = 0;
            }

            return hTax;
        }

        public static decimal GetGuestBalanceOld(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.Summation();

            var accountTotal = decimal.Zero;

            foreach (var rm in guest.GuestRooms.Where(x => x.GuestRoomAccounts.Sum(y => y.Amount) > 0))
            {
                accountTotal += rm.GuestRoomAccounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL) 
                    || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay)).Summation();
            }

            var total = accountTotal - totalRooms;

            return total;
        }

        public static decimal GetGuestBalance(this Guest guest)
        {
            return guest.GetGuestBalanceWithFullTax();
        }

        public static decimal GetGuestTotalPaid(this Guest guest)
        {
            var totalRooms = guest.GuestRooms.Summation();

            var accountTotal = decimal.Zero;

            foreach (var rm in guest.GuestRooms.Where(x => x.GuestRoomAccounts.Sum(y => y.Amount) > 0))
            {
                accountTotal += rm.GuestRoomAccounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL) 
                    || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit 
                    || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay)).Summation();
            }

            var total = accountTotal - totalRooms;

            return total;
        }

        public static string GetBalance(this Guest guest)
        {
            //var totalRooms = guest.GuestRooms.Summation();

            //var accountTotal = decimal.Zero;

            //foreach (var rm in guest.GuestRooms.Where(x => x.GuestRoomAccounts.Sum(y => y.Amount) > 0))
            //{
            //    accountTotal += rm.GuestRoomAccounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL) 
            //        || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit 
            //        || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit 
            //        || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit 
            //        || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay)).Summation();
            //}

            //var total = accountTotal - totalRooms;

            var total = guest.GetGuestBalanceWithFullTax();

            if(total < 0)
                return guest.FullName + " has to pay a balance of NGN " + decimal.Negate(total).ToString(CultureInfo.InvariantCulture);
            else
            {
                if (total == 0)
                    return "Check out " + guest.FullName;

                return "Refund NGN " + total.ToString(CultureInfo.InvariantCulture) + " and Check out " + guest.FullName;
            }
        }

        
        public static decimal PaymentsSummation(this IEnumerable<GuestRoomAccount> roomAccounts)
        {
            var total = decimal.Zero;

            foreach (var guestRoomAccount in roomAccounts.Where(x => (x.PaymentTypeId ==
                    (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit)
                    || (x.PaymentTypeId ==
                    (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit)
                    || (x.PaymentTypeId ==
                    (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit)))
            {
                total += guestRoomAccount.Amount;
            }

            return total;
        }

        public static decimal ExpensesSummation(this IEnumerable<GuestRoomAccount> roomAccounts)
        {
            var total = decimal.Zero;

            foreach (var guestRoomAccount in roomAccounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL)))
            {
                total += guestRoomAccount.Amount;
            }

            //var restaurantAndBarTotalPost = roomAccounts.Select(x => x.GuestRoom).Select(x => x.Guest).SelectMany(x => x.Payments).Where(x => x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL).Sum(x => x.Total);

            return total;
        }

        public static decimal Summation(this IEnumerable<GuestRoomAccount> roomAccounts)
        {
            var total = decimal.Zero;

            foreach (var guestRoomAccount in roomAccounts.Where(x => (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL)
                || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit 
                || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit 
                || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit 
                || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.HalfDay)))
            {
                if (guestRoomAccount.RoomPaymentType.PaymentStatusId == (int) RoomPaymentStatusEnum.Credit)
                    total += guestRoomAccount.Amount;
                else
                    total -= guestRoomAccount.Amount;
            }

            return total;
        }

        public static int GetNumberOfNightsFutureBooking(this GuestRoom guestRoom)
        {
            var hotelAccountsTime = 14;

            int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out hotelAccountsTime);

            var dtCheckoutDate = DateTime.Now;

            
            dtCheckoutDate = guestRoom.CheckoutDate;
            

            var extraDay = 0;

            if (DateTime.Now.Hour > hotelAccountsTime)
                extraDay = 1;

            //Use Exact Times to Calculate exact lenght of stay

            var exactCheckingDate = new DateTime(guestRoom.CheckinDate.Year, guestRoom.CheckinDate.Month, guestRoom.CheckinDate.Day, hotelAccountsTime, 0, 0);
            var exactCheckoutDate = new DateTime(dtCheckoutDate.Year, dtCheckoutDate.Month, dtCheckoutDate.Day, hotelAccountsTime, 0, 0);


            var totalNumberOfDays = exactCheckoutDate.Subtract(exactCheckingDate).Days + extraDay;

            totalNumberOfDays = (totalNumberOfDays == 0) ? 1 : totalNumberOfDays;

            return totalNumberOfDays;
        }

        public static int GetNumberOfNights(this GuestRoom guestRoom)
        {
            var hotelAccountsTime = 14;

            int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out hotelAccountsTime);

            var dtCheckoutDate = DateTime.Now;

            var useCheckoutExactDate = false;

            var extraDay = 0;

            if (DateTime.Now.Hour > hotelAccountsTime)
                extraDay = 1;

            if (!guestRoom.IsActive)
            {
                dtCheckoutDate = guestRoom.CheckoutDate;
                useCheckoutExactDate = true;
                extraDay = 0;
            }

            //Use Exact Times to Calculate exact lenght of stay

            var exactCheckingDate = new DateTime(guestRoom.CheckinDate.Year, guestRoom.CheckinDate.Month, guestRoom.CheckinDate.Day, hotelAccountsTime, 0, 0);
            var exactCheckoutDate = new DateTime(dtCheckoutDate.Year, dtCheckoutDate.Month, dtCheckoutDate.Day, hotelAccountsTime, 0, 0);

            if(useCheckoutExactDate)
            {
                exactCheckoutDate = dtCheckoutDate;
            }

            if (guestRoom.CheckinDate.Hour >= 0 && guestRoom.CheckinDate.Hour < hotelAccountsTime)
            {
                var hotelDayToday = DateTime.Today;

                if (hotelDayToday.Day == guestRoom.CheckinDate.Day && hotelDayToday.Month == guestRoom.CheckinDate.Month && hotelDayToday.Year == guestRoom.CheckinDate.Year)
                {
                    extraDay++;
                }
            }


            var totalNumberOfDays = exactCheckoutDate.Subtract(exactCheckingDate).Days + extraDay;

            totalNumberOfDays = (totalNumberOfDays == 0) ? 1 : totalNumberOfDays;

            return totalNumberOfDays;
        }

        public static int GetNumberOfNightsFuture(this GuestRoom guestRoom)
        {
           
            var dtCheckoutDate = DateTime.Now;

            if (!guestRoom.IsActive)
            {
                dtCheckoutDate = guestRoom.CheckoutDate;
            }
            else
            {
                dtCheckoutDate = guestRoom.CheckoutDate;
            }

            var extraDay = 0;

            var exactCheckingDate = new DateTime(guestRoom.CheckinDate.Year, guestRoom.CheckinDate.Month, guestRoom.CheckinDate.Day, 12, 0, 0);
            var exactCheckoutDate = new DateTime(dtCheckoutDate.Year, dtCheckoutDate.Month, dtCheckoutDate.Day, 12, 0, 0);

            var totalNumberOfDays = exactCheckoutDate.Subtract(exactCheckingDate).Days + extraDay;

            totalNumberOfDays = (totalNumberOfDays == 0) ? 1 : totalNumberOfDays;

            return totalNumberOfDays;
        }

        public static int GetNumberOfNightsCheckin(this GuestRoom guestRoom)
        {
            var hotelAccountsTime = 14;

            int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out hotelAccountsTime);

            var dtCheckoutDate = DateTime.Now;

            if (!guestRoom.IsActive)
            {
                dtCheckoutDate = guestRoom.CheckoutDate;
            }
            else
            {
                dtCheckoutDate = guestRoom.CheckoutDate;
            }

            var extraDay = 0;

            //if (DateTime.Now.Hour > hotelAccountsTime)
            //    extraDay = 1;

            //Use Exact Times to Calculate exact lenght of stay

            var exactCheckingDate = new DateTime(guestRoom.CheckinDate.Year, guestRoom.CheckinDate.Month, guestRoom.CheckinDate.Day, hotelAccountsTime, 0, 0);
            var exactCheckoutDate = new DateTime(dtCheckoutDate.Year, dtCheckoutDate.Month, dtCheckoutDate.Day, hotelAccountsTime, 0, 0);


            var totalNumberOfDays = exactCheckoutDate.Subtract(exactCheckingDate).Days + extraDay;

            totalNumberOfDays = (totalNumberOfDays == 0) ? 1 : totalNumberOfDays;

            return totalNumberOfDays;
        }

        public static decimal RoomCharge(this GuestRoom guestRoom, bool guestJustCheckedOut = false)
        {            
            var hotelAccountsTime = 14;
        
            int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out hotelAccountsTime);

            var dtCheckoutDate = DateTime.Now;

            var guestIsStillActive = true;

            var extraDay = 0;

            if (DateTime.Now.Hour > hotelAccountsTime)
                extraDay = 1;

            if (!guestRoom.IsActive && !guestJustCheckedOut)
            {
                dtCheckoutDate = guestRoom.CheckoutDate;
                guestIsStillActive = false;

                if (!guestRoom.GroupBooking)
                {
                    extraDay = 0;
                }
            }

            //Use Exact Times to Calculate exact lenght of stay
            var exactCheckingDate = new DateTime(guestRoom.CheckinDate.Year, guestRoom.CheckinDate.Month, guestRoom.CheckinDate.Day, hotelAccountsTime, 0, 0);

            var exactCheckoutDate = new DateTime(dtCheckoutDate.Year, dtCheckoutDate.Month, dtCheckoutDate.Day, hotelAccountsTime, 0, 0);

            if(guestRoom.CheckinDate.Hour >= 0 && guestRoom.CheckinDate.Hour < hotelAccountsTime)
            {
                var hotelDayToday = DateTime.Today;

                if(hotelDayToday.Day == guestRoom.CheckinDate.Day && hotelDayToday.Month == guestRoom.CheckinDate.Month  && hotelDayToday.Year == guestRoom.CheckinDate.Year)
                {
                    extraDay++;
                }
            }

            var totalNumberOfDays = exactCheckoutDate.Subtract(exactCheckingDate).Days + extraDay;

            if (guestIsStillActive)
            {
                totalNumberOfDays = (totalNumberOfDays == 0) ? 1 : totalNumberOfDays;
            }

            return totalNumberOfDays * guestRoom.RoomRate;
        }


        public static decimal DebitSummation(this IEnumerable<GuestRoom> guestRooms)
        {
            return guestRooms.Sum(guestRoom => guestRoom.GuestRoomAccounts.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit && (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL) || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit)).Summation());
        }

        //
        public static decimal PercentageOccupancy(this IEnumerable<GuestRoom> guestRooms)
        {
            int totalNumberOfNights = guestRooms.Sum(guestRoom => guestRoom.GetNumberOfNights());
            if (totalNumberOfNights == 0)
                return decimal.Zero;

            decimal ave = totalNumberOfNights / 365 ;
            return ave;
        }

        public static decimal CreditSummation(this IEnumerable<GuestRoom> guestRooms)
        {
            return guestRooms.Sum(guestRoom => guestRoom.GuestRoomAccounts.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && 
            (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.POSTBILL) || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit)).Summation());
        }

        
        public static decimal GrandTotal(this IEnumerable<GuestRoom> guestRooms)
        {
            var rooms = guestRooms.Summation();
            var payments = guestRooms.PaymentsSummation();
            var expenses = guestRooms.ExpensesSummation();

            var ret = payments - (rooms + expenses);

            return ret;
        }

        public static decimal CreditSummationCash(this IEnumerable<GuestRoom> guestRooms)
        {
            return guestRooms.Sum(guestRoom => guestRoom.GuestRoomAccounts.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && (x.PaymentMethodId == (int)HotelMateWebV1.Helpers.Enums.PaymentMethodEnum.Cash) || (x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.CashDeposit || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.InitialDeposit || x.PaymentTypeId == (int)HotelMateWebV1.Helpers.Enums.RoomPaymentTypeEnum.ReservationDeposit)).Summation());
        }

        public static decimal ExpensesSummation(this IEnumerable<GuestRoom> guestRooms)
        {
            var inHouse = guestRooms.SelectMany(x => x.GuestRoomAccounts).Where(x => x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL)
                .Sum(guestRoom => guestRoom.Amount);

            //var bar = guestRooms.Select(x => x.Guest).SelectMany(x => x.Payments).Sum(x => x.Total);

            return inHouse;
        }

        
        public static decimal SubTotal(this IEnumerable<GuestRoom> guestRooms)
        {
            return guestRooms.TotallySummation("0%", "0%");
        }

        public static decimal TotallySummation(this IEnumerable<GuestRoom> guestRooms, string strTax, string strDiscount)
        {
            if(strTax == "-1" && strDiscount == "-1" && guestRooms.Any())
            {
                var thisGuest = guestRooms.FirstOrDefault().Guest;
                if(thisGuest != null)
                {
                    var payment = thisGuest.Payments.LastOrDefault(x => x.Type == 2);
                    if(payment != null)
                    {
                        if(payment.PaymentTypeId == (int)RoomPaymentTypeEnum.Refund)
                        {
                            return payment.Total;
                        }
                        else
                        {
                            return decimal.Zero;
                        }
                        //strTax = payment.Tax;
                        //strDiscount = payment.Discount;
                    }
                }
            }
            var findPercent = strTax.IndexOf('%');

            int tax = 0;

            if (findPercent >= 0)
            {
                strTax = strTax.Substring(0, findPercent);

                int.TryParse(strTax, out tax);
            }

            findPercent = strDiscount.IndexOf('%');

            int discount = 0;

            if (findPercent >= 0)
            {
                strDiscount = strDiscount.Substring(0, findPercent);

                int.TryParse(strDiscount, out discount);
            }

            var totalBar = guestRooms.ExpensesSummation();

            var Payments = guestRooms.PaymentsSummation();

            var totalRooms = guestRooms.Summation();

            var appliedDiscount = decimal.Zero;

            var appliedTax = decimal.Zero;


            if (discount > 0)
            {
                appliedDiscount = decimal.Divide(discount, 100) * totalRooms;
            }

            //discountAmount = appliedDiscount;

            if (tax > 0)
            {
                appliedTax = decimal.Divide(tax, 100) * totalRooms;
            }

            //taxAmount = appliedTax;

            totalRooms = (totalRooms - appliedDiscount) + appliedTax;

            var returnTotal = (Payments - (totalRooms + totalBar));

            return returnTotal;
        }


        public static decimal TotalSummation(this IEnumerable<GuestRoom> guestRooms, string strTax, string strDiscount, out decimal? taxAmount, out decimal? discountAmount)
        {
            var findPercent = strTax.IndexOf('%');

            int tax = 0;

            if(findPercent >= 0)
            {
                strTax = strTax.Substring(0, findPercent);

                int.TryParse(strTax, out tax);
            }

            findPercent = -1;

            findPercent = strDiscount.IndexOf('%');

            int discount = 0;

            if (findPercent >= 0)
            {
                strDiscount = strDiscount.Substring(0, findPercent);

                int.TryParse(strDiscount, out discount);
            }

            var totalBar = guestRooms.ExpensesSummation();

            var Payments = guestRooms.PaymentsSummation();

            var totalRooms = guestRooms.Summation();

            var appliedDiscount = decimal.Zero;

            var appliedTax = decimal.Zero;


            if (discount > 0)
            {
                appliedDiscount = decimal.Divide(discount, 100) * totalRooms;
            }

            discountAmount = appliedDiscount;

            if (tax > 0)
            {
                appliedTax = decimal.Divide(tax, 100) * totalRooms;
            }

            taxAmount = appliedTax;

            totalRooms = (totalRooms - appliedDiscount) + appliedTax;

            var returnTotal = (Payments - (totalRooms + totalBar));

            return returnTotal;
        }


        public static decimal PaymentsSummation(this IEnumerable<GuestRoom> guestRooms)
        {
            return guestRooms.SelectMany(x => x.GuestRoomAccounts).Where(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit
            || x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit
            || x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit)
                .Sum(guestRoom => guestRoom.Amount);
        }
        public static decimal Summation(this IEnumerable<GuestRoom> guestRooms, bool guestJustCheckedOut = false)
        {
            return guestRooms.Sum(guestRoom => guestRoom.RoomCharge(guestJustCheckedOut));
        }
    }
}