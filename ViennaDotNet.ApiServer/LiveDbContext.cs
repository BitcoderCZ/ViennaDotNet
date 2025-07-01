using Microsoft.EntityFrameworkCore;
using System;
using ViennaDotNet.ApiServer.Models;

namespace ViennaDotNet.ApiServer;

public class LiveDbContext : DbContext
{
    public LiveDbContext(DbContextOptions<LiveDbContext> options) 
        : base(options)
    { 
    }

    public DbSet<Account> Accounts { get; set; }
}
