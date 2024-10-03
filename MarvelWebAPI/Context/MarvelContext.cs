using MarvelWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MarvelWebAPI.Models.CharactersModel;

namespace MarvelWebAPI.Context
{
    public class MarvelContext : DbContext
    {
        public MarvelContext(DbContextOptions<MarvelContext> options)
            : base(options)
        {
        }

        public DbSet<Character> Personagens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            
        }

    }

}
