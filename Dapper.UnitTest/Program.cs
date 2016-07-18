using Repository.DTO_Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperDataAnnotation;

namespace Dapper.UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BaseRepository<UserRoles> roles = new BaseRepository<UserRoles>();

            UserRoles newUser = new UserRoles();

            UserRoles ac = roles.Find(1);
            newUser.RoleName = "Operations";
            var abc = roles.Insert(newUser);
            Console.ReadKey();
        }

    }

    public class UserRoles
    {
        [PK]
        public int RoleId { get; set; }
        public string RoleName { get; set; }


        public UserRoles()
        {
            this.Bridge_Role_Pages = new List<Bridge_Role_Pages>();
            this.Bridge_User_Roles = new List<Bridge_User_Roles>();
        }

    

        [ForeignTable("RoleId")]
        public virtual List<Bridge_Role_Pages> Bridge_Role_Pages { get; set; }
        [ForeignTable("RoleId")]
        public virtual List<Bridge_User_Roles> Bridge_User_Roles { get; set; }
    }

    public partial class Bridge_Role_Pages
    {
        [PK]
        public int BridgeRolePagesId { get; set; }
        public Nullable<int> RoleId { get; set; }
        public Nullable<int> PageId { get; set; }

        [ForeignTable("PageId")]
        public virtual App_Pages App_Pages { get; set; }
        [ForeignTable("RoleId")]
        public virtual UserRoles UserRole { get; set; }
    }

    public partial class App_Pages
    {
        public App_Pages()
        {
            this.Bridge_Role_Pages = new HashSet<Bridge_Role_Pages>();
        }
        [PK]
        public int PageId { get; set; }
        public string LinkText { get; set; }
        public string ActionName { get; set; }
        public string Controller { get; set; }
        public Nullable<bool> IsFullPathAvailable { get; set; }
        public string FullActionControllerPath { get; set; }
        public Nullable<bool> IsParent { get; set; }
        public Nullable<int> ParentId { get; set; }
        public Nullable<bool> ShowInMenu { get; set; }
        public Nullable<int> OrderNumberInMenu { get; set; }
        public string GlyphiconFont { get; set; }
        public string IconPath { get; set; }

        [ForeignTable("PageId")]
        public virtual ICollection<Bridge_Role_Pages> Bridge_Role_Pages { get; set; }
    }

    public partial class Bridge_User_Roles
    {
        [PK]
        public int BridgeUserRolesId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> RoleId { get; set; }

        [ForeignTable("UserId")]
        public virtual App_User App_User { get; set; }
        [ForeignTable("RoleId")]
        public virtual UserRoles UserRole { get; set; }
    }

    public partial class App_User
    {
        public App_User()
        {
            this.Bridge_User_Roles = new HashSet<Bridge_User_Roles>();
        }

        [PK]
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CNIC { get; set; }
        public string ContactNumber { get; set; }
        public string EmailAddress { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [ForeignTable("UserId")]
        public virtual ICollection<Bridge_User_Roles> Bridge_User_Roles { get; set; }
    }


}
