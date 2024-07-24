using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Principal;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using MCBA_Web_App.Models;

namespace MCBA_Web_App.Data
{
    public class MCBAContext : DbContext
    {
        public MCBAContext(DbContextOptions<MCBAContext> options) : base(options)
        { 
        }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Login> Login { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<Transactions> Transaction { get; set; }
        public DbSet<BillPay> BillPay { get; set; }
        public DbSet<Payee> Payee { get; set; }
    }
}