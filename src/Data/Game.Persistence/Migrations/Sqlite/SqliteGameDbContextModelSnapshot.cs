﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuantumCore.Game.Persistence;

#nullable disable

namespace QuantumCore.Game.Persistence.Migrations.Sqlite
{
    [DbContext(typeof(SqliteGameDbContext))]
    partial class SqliteGameDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.DeletedPlayer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<int>("BodyPart")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<DateTime>("DeletedAt")
                        .HasColumnType("TEXT");

                    b.Property<byte>("Dx")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Experience")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Gold")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HairPart")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Health")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Ht")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Iq")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Mana")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("TEXT");

                    b.Property<int>("PlayTime")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("PlayerClass")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositionX")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositionY")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("SkillGroup")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("St")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Stamina")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.HasKey("Id");

                    b.ToTable("DeletedPlayers");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Item", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<byte>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<uint>("ItemId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Position")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<byte>("Window")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermAuth", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("PermissionGroups");

                    b.HasData(
                        new
                        {
                            Id = new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"),
                            Name = "Operator"
                        });
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermUser", b =>
                {
                    b.Property<Guid>("GroupId")
                        .HasColumnType("TEXT");

                    b.Property<uint>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GroupId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("PermissionUsers");

                    b.HasData(
                        new
                        {
                            GroupId = new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"),
                            PlayerId = 1u
                        });
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Player", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<uint>("AvailableStatusPoints")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("BodyPart")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.Property<byte>("Dx")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Empire")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Experience")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("GivenStatusPoints")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Gold")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("HairPart")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Health")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Ht")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Iq")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Mana")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("TEXT");

                    b.Property<ulong>("PlayTime")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("PlayerClass")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositionX")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositionY")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("SkillGroup")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("St")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Stamina")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("current_timestamp");

                    b.HasKey("Id");

                    b.ToTable("Players");

                    b.HasData(
                        new
                        {
                            Id = 1u,
                            AccountId = new Guid("e34fd5ab-fb3b-428e-935b-7db5bd08a3e5"),
                            AvailableStatusPoints = 0u,
                            BodyPart = 0u,
                            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            Dx = (byte)99,
                            Empire = (byte)0,
                            Experience = 0u,
                            GivenStatusPoints = 0u,
                            Gold = 2000000000u,
                            HairPart = 0u,
                            Health = 99999L,
                            Ht = (byte)99,
                            Iq = (byte)99,
                            Level = (byte)99,
                            Mana = 99999L,
                            Name = "Admin",
                            PlayTime = 0u,
                            PlayerClass = (byte)0,
                            PositionX = 958870,
                            PositionY = 272788,
                            SkillGroup = (byte)0,
                            St = (byte)99,
                            Stamina = 0L,
                            UpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Item", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermAuth", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.PermGroup", "Group")
                        .WithMany("Permissions")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermUser", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.PermGroup", "Group")
                        .WithMany("Users")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuantumCore.Game.Persistence.Entities.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermGroup", b =>
                {
                    b.Navigation("Permissions");

                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
