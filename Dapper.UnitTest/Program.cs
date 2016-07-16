using Repository.DTO_Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Dapper.UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BaseRepository<UserRoles> roles = new BaseRepository<UserRoles>();

            UserRoles newUser = new UserRoles();

            roles.Find(1);
            newUser.RoleName = "Operations";
            var abc = roles.Insert(newUser);
            Console.ReadKey();
        }

    }

    public class UserRoles
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public IEnumerable<ABC> MyProperty { get; set; }
    }

    public class ABC
    {

    }


}
