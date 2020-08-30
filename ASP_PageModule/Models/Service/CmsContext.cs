using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP_PageModule.Models.Service
{
    public class CmsContext : DbContext
    {
        public DbSet<Page.Page> Pages { get; set; }

        public CmsContext(DbContextOptions<CmsContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
