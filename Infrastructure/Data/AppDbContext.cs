using Core.Models.Blogs;
using Core.Models.Error;
using Core.Models.Identity;
using Core.Models.Settings;
using Core.Models.Uploads;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    /*
    if we want customise IdentityUser class we need to create new class inherent from IdentityUser and pass it :
        IdentityDbContext<AppUser>
    IdentityDbContext inherits from DbContext for that we just need this AppDbContext class to work with database
     */

    public class AppDbContext : IdentityDbContext<AppUser>
    {
        // for all Tables of EntityFrameworkCore initialize here by default, IdentityDbContext took care of all of the work to create tables and relational between them.

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // add index for Blog's columns
            builder.Entity<Blog>()
                .HasIndex(b => b.Id);
            builder.Entity<Blog>()
                .HasIndex(b => new { b.ReleaseDate, b.Title });

            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=standardDB.db");
        }

        //
        public DbSet<Address> Address { get; set; }
        public DbSet<AppError> AppError { get; set; }
        public DbSet<ErrorType> ErrorType { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<BlogSourceCategoryName> BlogSourceCategoryName { get; set; }
        public DbSet<Blog> Blog { get; set; }
        public DbSet<BlogCategory> BlogCategory { get; set; }
        public DbSet<BlogCategoryList> BlogCategoryList { get; set; }
        public DbSet<BlogLike> BlogLike { get; set; }
        public DbSet<BlogComment> BlogComment { get; set; }
        public DbSet<UploadType> UploadType { get; set; }
        public DbSet<Upload> Upload { get; set; }
        public DbSet<UploadUserImagesList> UploadUserImagesList { get; set; }
        public DbSet<UploadBlogImagesList> UploadBlogImagesList { get; set; }
    }


    // to initialize an object from data base to using it in some class where can't inject the AppDbContext class this.
    public class AppAllDbContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=standardDB.db");
        }

        public DbSet<Address> Address { get; set; }
        public DbSet<AppError> AppError { get; set; }
        public DbSet<ErrorType> ErrorType { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Blog> Blog { get; set; }
        public DbSet<BlogSourceCategoryName> BlogSourceCategoryName { get; set; }
        public DbSet<BlogCategory> BlogCategory { get; set; }
        public DbSet<BlogCategoryList> BlogCategoryList { get; set; }
        public DbSet<BlogLike> BlogLike { get; set; }
        public DbSet<BlogComment> BlogComment { get; set; }
        public DbSet<UploadType> UploadType { get; set; }
        public DbSet<Upload> Upload { get; set; }
        public DbSet<UploadUserImagesList> UploadUserImagesList { get; set; }
        public DbSet<UploadBlogImagesList> UploadBlogImagesList { get; set; }
    }
}
