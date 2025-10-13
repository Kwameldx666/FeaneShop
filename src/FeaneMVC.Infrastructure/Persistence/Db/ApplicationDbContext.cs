using FeaneMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<UserData> Users { get; set; }
        public DbSet<ReservationHistory> ReservationsHistory { get; set; }
        public DbSet<PaymentDetails> PaymentDetails { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Filter> Filters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связь один-к-одному между UserData и DeliveryAddress
            modelBuilder.Entity<UserData>()
                .HasOne(u => u.Delivery)
                .WithOne(d => d.User)
                .HasForeignKey<DeliveryAddress>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Измените на подходящее поведение

            // Определение составного ключа для CartItem
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => new { ci.CartId, ci.DishId });

            // Связь между UserData и Cart
            modelBuilder.Entity<UserData>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)  // Указываем связь обратно, если она существует
                .HasForeignKey<UserData>(u => u.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь между Cart и User
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)  // Указываем связь обратно, если она существует
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Измените на подходящее поведение

            // Связь между Cart и CartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь между CartItem и Dish
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Dish)
                .WithMany(d => d.CartItems)
                .HasForeignKey(ci => ci.DishId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь между CartItem и User
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь между Reservation и User
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь между ReservationHistory и User
            modelBuilder.Entity<ReservationHistory>()
                .HasOne(rh => rh.User)
                .WithMany()
                .HasForeignKey(rh => rh.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Настройки ключей
            modelBuilder.Entity<Dish>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<PaymentDetails>()
                .HasKey(p => p.CardNumber);

            modelBuilder.Entity<DeliveryAddress>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<Cart>()
                .HasKey(c => c.CartId);

            modelBuilder.Entity<PaymentRecord>()
             .HasKey(pr => pr.Id);
        }







    }
}
