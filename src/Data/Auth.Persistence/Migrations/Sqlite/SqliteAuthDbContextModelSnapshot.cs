﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuantumCore.Auth.Persistence;

#nullable disable

namespace QuantumCore.Auth.Persistence.Migrations.Sqlite
{
    [DbContext(typeof(SqliteAuthDbContext))]
    partial class SqliteAuthDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("QuantumCore.Auth.Persistence.Entities.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("DeleteCode")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("TEXT");

                    b.Property<short>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Status");

                    b.ToTable("accounts");

                    b.HasData(
                        new
                        {
                            Id = new Guid("e34fd5ab-fb3b-428e-935b-7db5bd08a3e5"),
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            DeleteCode = "1234567",
                            Email = "admin@test.com",
                            Password = "$2y$10$5e9nP50E64iy8vaSMwrRWO7vCfnA7.p5XpIDHC3hPdi6BCtTF7rBS",
                            Status = (short)1,
                            UpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Username = "admin"
                        });
                });

            modelBuilder.Entity("QuantumCore.Auth.Persistence.Entities.AccountStatus", b =>
                {
                    b.Property<short>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AllowLogin")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClientStatus")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("account_status");

                    b.HasData(
                        new
                        {
                            Id = (short)1,
                            AllowLogin = true,
                            ClientStatus = "OK",
                            Description = "Default Status"
                        });
                });

            modelBuilder.Entity("QuantumCore.Auth.Persistence.Entities.Account", b =>
                {
                    b.HasOne("QuantumCore.Auth.Persistence.Entities.AccountStatus", "AccountStatus")
                        .WithMany("Accounts")
                        .HasForeignKey("Status")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AccountStatus");
                });

            modelBuilder.Entity("QuantumCore.Auth.Persistence.Entities.AccountStatus", b =>
                {
                    b.Navigation("Accounts");
                });
#pragma warning restore 612, 618
        }
    }
}
