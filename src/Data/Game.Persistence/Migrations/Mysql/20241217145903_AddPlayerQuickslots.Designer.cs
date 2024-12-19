﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QuantumCore.Game.Persistence;

#nullable disable

namespace QuantumCore.Game.Persistence.Migrations.Mysql
{
    [DbContext(typeof(MySqlGameDbContext))]
    [Migration("20241217145903_AddPlayerQuickslots")]
    partial class AddPlayerQuickslots
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
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

                    b.ToTable("DeletedPlayers");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.Guild", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.Property<uint>("Experience")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Gold")
                        .HasColumnType("int unsigned");

                    b.Property<byte>("Level")
                        .HasColumnType("tinyint unsigned");

                    b.Property<ushort>("MaxMemberCount")
                        .HasColumnType("smallint unsigned");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");

                    b.Property<uint>("OwnerId")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.GuildMember", b =>
                {
                    b.Property<uint>("GuildId")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("PlayerId")
                        .HasColumnType("int unsigned");

                    b.Property<bool>("IsLeader")
                        .HasColumnType("tinyint(1)");

                    b.Property<byte>("RankPosition")
                        .HasColumnType("tinyint unsigned");

                    b.Property<uint>("SpentExperience")
                        .HasColumnType("int unsigned");

                    b.HasKey("GuildId", "PlayerId");

                    b.HasIndex("PlayerId")
                        .IsUnique();

                    b.HasIndex("GuildId", "RankPosition");

                    b.ToTable("GuildMembers");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.GuildNews", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.Property<uint?>("GuildId")
                        .HasColumnType("int unsigned");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<uint>("PlayerId")
                        .HasColumnType("int unsigned");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.HasIndex("PlayerId");

                    b.ToTable("GuildNews");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.GuildRank", b =>
                {
                    b.Property<uint>("GuildId")
                        .HasColumnType("int unsigned");

                    b.Property<byte>("Position")
                        .HasColumnType("tinyint unsigned");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("varchar(8)");

                    b.Property<byte>("Permissions")
                        .HasColumnType("tinyint unsigned");

                    b.HasKey("GuildId", "Position");

                    b.ToTable("GuildRanks");
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

                    b.Property<uint>("PlayerId")
                        .HasColumnType("int unsigned");

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

                    b.ToTable("Items");
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

                    b.ToTable("Permissions");
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
                        .HasColumnType("char(36)");

                    b.Property<uint>("PlayerId")
                        .HasColumnType("int unsigned");

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
                        .HasColumnType("int unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<uint>("Id"));

                    b.Property<Guid>("AccountId")
                        .HasColumnType("char(36)");

                    b.Property<uint>("AvailableSkillPoints")
                        .HasColumnType("int unsigned");

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

                    b.Property<uint?>("GuildId")
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

                    b.Property<ulong>("PlayTime")
                        .HasColumnType("bigint unsigned");

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

                    b.HasIndex("GuildId");

                    b.ToTable("Players");

                    b.HasData(
                        new
                        {
                            Id = 1u,
                            AccountId = new Guid("e34fd5ab-fb3b-428e-935b-7db5bd08a3e5"),
                            AvailableSkillPoints = 99u,
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
                            PlayTime = 0ul,
                            PlayerClass = (byte)0,
                            PositionX = 958870,
                            PositionY = 272788,
                            SkillGroup = (byte)0,
                            St = (byte)99,
                            Stamina = 0L,
                            UpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PlayerQuickSlot", b =>
                {
                    b.Property<uint>("PlayerId")
                        .HasColumnType("int unsigned");

                    b.Property<byte>("Slot")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("Type")
                        .HasColumnType("tinyint unsigned");

                    b.Property<uint>("Value")
                        .HasColumnType("int unsigned");

                    b.HasKey("PlayerId", "Slot");

                    b.ToTable("PlayerQuickSlots");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PlayerSkill", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.Property<byte>("Level")
                        .HasColumnType("tinyint unsigned");

                    b.Property<byte>("MasterType")
                        .HasColumnType("tinyint unsigned");

                    b.Property<int>("NextReadTime")
                        .HasColumnType("int");

                    b.Property<uint>("PlayerId")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("ReadsRequired")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("SkillId")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");

                    b.HasKey("Id");

                    b.ToTable("PlayerSkills");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.Guild", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.Player", "Leader")
                        .WithMany("GuildsToLead")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Leader");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.GuildMember", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.Guilds.Guild", "Guild")
                        .WithMany("Members")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuantumCore.Game.Persistence.Entities.Player", "Player")
                        .WithMany("Members")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuantumCore.Game.Persistence.Entities.Guilds.GuildRank", "Rank")
                        .WithMany("Members")
                        .HasForeignKey("GuildId", "RankPosition")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("Player");

                    b.Navigation("Rank");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.GuildNews", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.Guilds.Guild", "Guild")
                        .WithMany("News")
                        .HasForeignKey("GuildId");

                    b.HasOne("QuantumCore.Game.Persistence.Entities.Player", "Player")
                        .WithMany("WrittenGuildNews")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.GuildRank", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.Guilds.Guild", "Guild")
                        .WithMany("Ranks")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");
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

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Player", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.Guilds.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PlayerQuickSlot", b =>
                {
                    b.HasOne("QuantumCore.Game.Persistence.Entities.Player", "Player")
                        .WithMany("QuickSlots")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.Guild", b =>
                {
                    b.Navigation("Members");

                    b.Navigation("News");

                    b.Navigation("Ranks");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Guilds.GuildRank", b =>
                {
                    b.Navigation("Members");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.PermGroup", b =>
                {
                    b.Navigation("Permissions");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("QuantumCore.Game.Persistence.Entities.Player", b =>
                {
                    b.Navigation("GuildsToLead");

                    b.Navigation("Members");

                    b.Navigation("QuickSlots");

                    b.Navigation("WrittenGuildNews");
                });
#pragma warning restore 612, 618
        }
    }
}
