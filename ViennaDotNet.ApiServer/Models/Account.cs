using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViennaDotNet.ApiServer.Models;

public class Account
{
    public string Id { get; set; }

    public required string Username { get; set; }

    [MaxLength(16)]
    public required byte[] PasswordSalt { get; set; }

    [MaxLength(64)]
    public required byte[] PasswordHash { get; set; }
}
