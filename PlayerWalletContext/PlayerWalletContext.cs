using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PlayerWalletContext.Entities;
using static System.Guid;

namespace PlayerWalletContext
{
    public static class Helper
    {
        public static string? GetAspNetCoreEnvironment() =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }

#nullable disable
    public class CliDbContext : IDesignTimeDbContextFactory<PlayerWalletContext>
    {
        public PlayerWalletContext CreateDbContext(string[] args)
        {
            var environment = Helper.GetAspNetCoreEnvironment() ?? "Production";

            var optionsBuilder = new DbContextOptionsBuilder<PlayerWalletContext>();
            optionsBuilder.UseSqlite(PlayerWalletContext.SqliteConnectionString);

            if (!environment.ToLowerInvariant().Contains("production"))
            {
                // Enable Sensitive data logging and detailed errors outside Production environment
                optionsBuilder
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
            }

            return new PlayerWalletContext(optionsBuilder.Options);
        }
    }

    public class PlayerWalletContext : DbContext
    {
        private const string DatabaseName = "PlayerWalletDB";

        public static string SqliteConnectionString => $"Data Source={DatabaseName}.db;";

        private readonly string _environment;

        // private constructor invisible from the outside
        private PlayerWalletContext()
        {
            _environment = Helper.GetAspNetCoreEnvironment() ?? "Production";
        }

        // constructor for mocking in tests
        public PlayerWalletContext(DbContextOptions<PlayerWalletContext> options) : base(options)
        {
            _environment = Helper.GetAspNetCoreEnvironment() ?? "Production";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // return if already configured (during mocking with InMemory provider)
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder.UseSqlite(SqliteConnectionString);

            if (_environment.ToLowerInvariant().Contains("production"))
            {
                return;
            }

            // Enable Sensitive data logging and detailed errors outside Production environments
            Console.WriteLine($"Enabling detailed errors and sensitive data logging for environment \"{_environment}\"");
            optionsBuilder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Player

            // index on Active status for state validation
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Active);

            // unique index on player's name
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.PlayerName)
                .IsUnique();

            #endregion

            #region MyRegion

            modelBuilder.Entity<Wallet>()
                .Property(wallet => wallet.RowVersion)
                .IsRequired(false);

            #endregion

            // Seed data to have something to start with
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seedId = ParseExact("11111111111111111111111111111111", "N");

            modelBuilder.Entity<Player>()
                .HasData(
                    new Player
                    {
                        Id = seedId,
                        Active = true,
                        PlayerName = "NicknameJim"
                    });

            modelBuilder.Entity<Wallet>()
                .HasData(
                    new Wallet
                    {
                        Id = seedId,
                        Balance = 0
                    }
                );

            modelBuilder.Entity<WalletLog>()
                .HasKey(walletLog => new
                {
                    walletLog.TransactionId,
                    walletLog.WalletId
                });

            // TODO: Add wallet transactions
            // modelBuilder.Entity<WalletLog>()
            //     .HasData(
            //         new List<WalletLog>
            //         {
            //             new WalletLog
            //             {
            //                 WalletId = seedId,
            //                 TransactionId = Guid.NewGuid(),
            //                 ResultType = ResultType.Created,
            //                 Memento = ""
            //             }
            //         }
            //     );
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<WalletLog> WalletLogs { get; set; }
    }
}
