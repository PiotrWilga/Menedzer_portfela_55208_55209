using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceManager.WebApi.Models;

public class AccountPermission
{
    public int AccountPermissionId { get; set; }

    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Account Account { get; set; }

    public int AppUserId { get; set; }
    [ForeignKey("AppUserId")]
    public AppUser AppUser { get; set; }

    public PermissionType PermissionType { get; set; }
}

public enum PermissionType
{
    ReadOnly,
    ReadAndWrite
}