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
                    b.Property<string>("Iban")
                        .HasMaxLength(34)
                        .HasColumnType("character varying(34)")
                        .HasColumnName("iban");

                    b.Property<bool>("AcceptBizum")
                        .HasColumnType("boolean")
                        .HasColumnName("accept_bizum");

                    b.Property<int>("AccountType")
                        .HasColumnType("integer")
                        .HasColumnName("account_type");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric")
                        .HasColumnName("balance");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.HasKey("Iban");

                    b.ToTable("bank_accounts");
                });

            modelBuilder.Entity("TFG.Context.Models.Card", b =>
                {
                    b.Property<string>("CardNumber")
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("card_number");

                    b.Property<string>("BankAccountIban")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("character varying(24)")
                        .HasColumnName("bank_account_iban");

                    b.Property<int>("CardType")
                        .HasColumnType("integer")
                        .HasColumnName("card_type");

                    b.Property<string>("Cvv")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("cvv");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expiration_date");

                    b.Property<bool>("IsBlocked")
                        .HasColumnType("boolean")
                        .HasColumnName("is_blocked");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<string>("Pin")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pin");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("CardNumber");

                    b.HasIndex("BankAccountIban");

                    b.HasIndex("UserId");

                    b.ToTable("cards");
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

                    b.Property<string>("IbanAccountDestination")
                        .IsRequired()
                        .HasMaxLength(34)
                        .HasColumnType("character varying(34)")
                        .HasColumnName("iban_account_destination");

                    b.Property<string>("IbanAccountOrigin")
                        .HasMaxLength(34)
                        .HasColumnType("character varying(34)")
                        .HasColumnName("iban_account_origin");

                    b.HasKey("Id");

                    b.HasIndex("IbanAccountDestination");

                    b.HasIndex("IbanAccountOrigin");

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
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("avatar");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Dni")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("character varying(9)")
                        .HasColumnName("dni");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("email");

                    b.Property<int>("Gender")
                        .HasColumnType("integer")
                        .HasColumnName("gender");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_deleted");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("password");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("character varying(9)")
                        .HasColumnName("phone");

                    b.Property<int>("Role")
                        .HasColumnType("integer")
                        .HasColumnName("role");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("UserBankAccount", b =>
                {
                    b.Property<Guid>("UsersId")
                        .HasColumnType("uuid");

                    b.Property<string>("BankAccountsId")
                        .HasColumnType("character varying(34)");

                    b.HasKey("UsersId", "BankAccountsId");

                    b.HasIndex("BankAccountsId");

                    b.ToTable("UserBankAccount");
                });

            modelBuilder.Entity("TFG.Context.Models.Card", b =>
                {
                    b.HasOne("TFG.Context.Models.BankAccount", "BankAccount")
                        .WithMany("Cards")
                        .HasForeignKey("BankAccountIban")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TFG.Context.Models.User", "User")
                        .WithMany("Cards")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BankAccount");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TFG.Context.Models.Transaction", b =>
                {
                    b.HasOne("TFG.Context.Models.BankAccount", "BankAccountDestinationIban")
                        .WithMany("TransactionsDestination")
                        .HasForeignKey("IbanAccountDestination")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TFG.Context.Models.BankAccount", "BankAccountOriginIban")
                        .WithMany("TransactionsOrigin")
                        .HasForeignKey("IbanAccountOrigin");

                    b.Navigation("BankAccountDestinationIban");

                    b.Navigation("BankAccountOriginIban");
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
                    b.Navigation("Cards");

                    b.Navigation("TransactionsDestination");

                    b.Navigation("TransactionsOrigin");
                });

            modelBuilder.Entity("TFG.Context.Models.User", b =>
                {
                    b.Navigation("Cards");
                });
#pragma warning restore 612, 618
        }
    }
}
