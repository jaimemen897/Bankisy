﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TFG.Context.Context;

#nullable disable

namespace TFG.Context.Migrations
{
    [DbContext(typeof(BankContext))]
    partial class BankContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TFG.Context.Models.BankAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("AccountType")
                        .HasColumnType("integer")
                        .HasColumnName("account_type");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric")
                        .HasColumnName("balance");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.HasKey("Id");

                    b.ToTable("bank_accounts");
                });

            modelBuilder.Entity("TFG.Context.Models.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric")
                        .HasColumnName("amount");

                    b.Property<string>("Concept")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("concept");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date");

                    b.Property<Guid>("IdAccountDestination")
                        .HasColumnType("uuid")
                        .HasColumnName("id_account_destination");

                    b.Property<Guid>("IdAccountOrigin")
                        .HasColumnType("uuid")
                        .HasColumnName("id_account_origin");

                    b.HasKey("Id");

                    b.HasIndex("IdAccountDestination");

                    b.HasIndex("IdAccountOrigin");

                    b.ToTable("transactions");
                });

            modelBuilder.Entity("TFG.Context.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("avatar");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Dni")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("dni");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<int>("Gender")
                        .HasColumnType("integer")
                        .HasColumnName("gender");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<int>("Role")
                        .HasColumnType("integer")
                        .HasColumnName("role");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("UserBankAccount", b =>
                {
                    b.Property<Guid>("UsersId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("BankAccountsId")
                        .HasColumnType("uuid");

                    b.HasKey("UsersId", "BankAccountsId");

                    b.HasIndex("BankAccountsId");

                    b.ToTable("UserBankAccount");
                });

            modelBuilder.Entity("TFG.Context.Models.Transaction", b =>
                {
                    b.HasOne("TFG.Context.Models.BankAccount", "BankAccountDestination")
                        .WithMany()
                        .HasForeignKey("IdAccountDestination")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TFG.Context.Models.BankAccount", "BankAccountOrigin")
                        .WithMany("Transactions")
                        .HasForeignKey("IdAccountOrigin")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("BankAccountDestination");

                    b.Navigation("BankAccountOrigin");
                });

            modelBuilder.Entity("UserBankAccount", b =>
                {
                    b.HasOne("TFG.Context.Models.BankAccount", null)
                        .WithMany()
                        .HasForeignKey("BankAccountsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TFG.Context.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TFG.Context.Models.BankAccount", b =>
                {
                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
