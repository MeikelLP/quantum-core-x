﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuantumCore.Game.Persistence;

#nullable disable

namespace QuantumCore.Game.Persistence.Migrations.Mysql
{
    [DbContext(typeof(MySqlGameDbContext))]
    partial class MySqlGameDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.DeletedPlayer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<int>("BodyPart")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.Property<DateTime>("DeletedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<byte>("Dx")
                        .HasColumnType("tinyint unsigned");

                    b.Property<int>("Experience")
                        .HasColumnType("int");

                    b.Property<byte>("Gold")
                        .HasColumnType("tinyint unsigned");

                    b.Property<int>("HairPart")
                        .HasColumnType("int");

                    b.Property<long>("Health")
                        .HasColumnType("bigint");

                    b.Property<byte>("Ht")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("Iq")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("Level")
                        .HasColumnType("tinyint unsigned");

                    b.Property<long>("Mana")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("varchar(24)");

                    b.Property<int>("PlayTime")
                        .HasColumnType("int");

                    b.Property<byte>("PlayerClass")
                        .HasColumnType("tinyint unsigned");

                    b.Property<int>("PositionX")
                        .HasColumnType("int");

                    b.Property<int>("PositionY")
                        .HasColumnType("int");

                    b.Property<byte>("SkillGroup")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("St")
                        .HasColumnType("tinyint unsigned");

                    b.Property<long>("Stamina")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.HasKey("Id");

                    b.ToTable("deleted_players");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Item", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<byte>("Count")
                        .HasColumnType("tinyint unsigned");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.Property<uint>("ItemId")
                        .HasColumnType("int unsigned");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("char(36)");

                    b.Property<uint>("Position")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.Property<byte>("Window")
                        .HasColumnType("tinyint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("items");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermAuth", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("perm_auth");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("perm_groups");

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
                        .HasColumnType("char(36)");

                    b.Property<Guid>("PlayerId")
                        .HasColumnType("char(36)");

                    b.HasKey("GroupId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("perm_users");

                    b.HasData(
                        new
                        {
                            GroupId = new Guid("45bff707-1836-42b7-956d-00b9b69e0ee0"),
                            PlayerId = new Guid("fefa4396-c5d1-4d7f-bc84-5df40867eac8")
                        });
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Player", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<uint>("AvailableStatusPoints")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("BodyPart")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.Property<byte>("Dx")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("Empire")
                        .HasColumnType("tinyint unsigned");

                    b.Property<uint>("Experience")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("GivenStatusPoints")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Gold")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("HairPart")
                        .HasColumnType("int unsigned");

                    b.Property<long>("Health")
                        .HasColumnType("bigint");

                    b.Property<byte>("Ht")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("Iq")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("Level")
                        .HasColumnType("tinyint unsigned");

                    b.Property<long>("Mana")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("varchar(24)");

                    b.Property<uint>("PlayTime")
                        .HasColumnType("int unsigned");

                    b.Property<byte>("PlayerClass")
                        .HasColumnType("tinyint unsigned");

                    b.Property<int>("PositionX")
                        .HasColumnType("int");

                    b.Property<int>("PositionY")
                        .HasColumnType("int");

                    b.Property<byte>("SkillGroup")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("St")
                        .HasColumnType("tinyint unsigned");

                    b.Property<long>("Stamina")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.HasKey("Id");

                    b.ToTable("players");

                    b.HasData(
                        new
                        {
                            Id = new Guid("fefa4396-c5d1-4d7f-bc84-5df40867eac8"),
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
