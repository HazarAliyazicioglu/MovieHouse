using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MovieHouse.Models;

namespace MovieHouse.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Film> Films { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FilmActor> FilmActors { get; set; }
        public DbSet<FilmCategory> FilmCategories { get; set; }
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<UserListFilm> UserListFilms { get; set; }
        public DbSet<UserRating> UserRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // FilmActor many-to-many configuration
            modelBuilder.Entity<FilmActor>()
                .HasKey(fa => new { fa.FilmId, fa.ActorId });

            modelBuilder.Entity<FilmActor>()
                .HasOne(fa => fa.Film)
                .WithMany(f => f.FilmActors)
                .HasForeignKey(fa => fa.FilmId);

            modelBuilder.Entity<FilmActor>()
                .HasOne(fa => fa.Actor)
                .WithMany(a => a.FilmActors)
                .HasForeignKey(fa => fa.ActorId);

            // FilmCategory many-to-many configuration
            modelBuilder.Entity<FilmCategory>()
                .HasKey(fc => new { fc.FilmId, fc.CategoryId });

            modelBuilder.Entity<FilmCategory>()
                .HasOne(fc => fc.Film)
                .WithMany(f => f.FilmCategories)
                .HasForeignKey(fc => fc.FilmId);

            modelBuilder.Entity<FilmCategory>()
                .HasOne(fc => fc.Category)
                .WithMany(c => c.FilmCategories)
                .HasForeignKey(fc => fc.CategoryId);

            // UserListFilm configuration
            modelBuilder.Entity<UserListFilm>()
                .HasOne(ulf => ulf.UserList)
                .WithMany(ul => ul.UserListFilms)
                .HasForeignKey(ulf => ulf.UserListId);

            modelBuilder.Entity<UserListFilm>()
                .HasOne(ulf => ulf.Film)
                .WithMany(f => f.UserListFilms)
                .HasForeignKey(ulf => ulf.FilmId);

            // UserRating configuration
            modelBuilder.Entity<UserRating>()
                .HasKey(ur => ur.Id);

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRatings)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRating>()
                .HasOne(ur => ur.Film)
                .WithMany(f => f.UserRatings)
                .HasForeignKey(ur => ur.FilmId);

            // Unique constraint: Bir kullanıcı bir filmi sadece bir kez puanlayabilir
            modelBuilder.Entity<UserRating>()
                .HasIndex(ur => new { ur.UserId, ur.FilmId })
                .IsUnique();
        }
    }
}
