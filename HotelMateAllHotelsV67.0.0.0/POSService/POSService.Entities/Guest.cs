using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace POSService.Entities
{
    public class Guest
{
    // Properties
    public string FullName { get; set; }

    public int GuestRoomId { get; set; }

    //[Browsable(false), Key, Column("Id")]
    public long Id { get; set; }

    public string RoomNumber { get; set; }
}

 

}
