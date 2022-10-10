﻿// <auto-generated />
using System;
using DMUCalendarSync.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DMUCalendarSync.Database.Migrations
{
    [DbContext(typeof(DcsDbContext))]
    [Migration("20221010115203_initialMigration")]
    partial class initialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.9");

            modelBuilder.Entity("DMUCalendarSync.Database.Models.MyDmuCookie", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Domain")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpiryTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("MyDmuCookieSetId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MyDmuUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MyDmuCookieSetId");

                    b.HasIndex("MyDmuUserId");

                    b.ToTable("MyDmuCookies");
                });

            modelBuilder.Entity("DMUCalendarSync.Database.Models.MyDmuCookieSet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EarliestCookieExpiry")
                        .HasColumnType("TEXT");

                    b.Property<int>("MyDmuUserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("MyDmuUserId");

                    b.ToTable("MyDmuCookieSets");
                });

            modelBuilder.Entity("DMUCalendarSync.Database.Models.MyDmuUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("MyDmuUsers");
                });

            modelBuilder.Entity("DMUCalendarSync.Database.Models.MyDmuCookie", b =>
                {
                    b.HasOne("DMUCalendarSync.Database.Models.MyDmuCookieSet", null)
                        .WithMany("Cookies")
                        .HasForeignKey("MyDmuCookieSetId");

                    b.HasOne("DMUCalendarSync.Database.Models.MyDmuUser", "MyDmuUser")
                        .WithMany()
                        .HasForeignKey("MyDmuUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MyDmuUser");
                });

            modelBuilder.Entity("DMUCalendarSync.Database.Models.MyDmuCookieSet", b =>
                {
                    b.HasOne("DMUCalendarSync.Database.Models.MyDmuUser", "MyDmuUser")
                        .WithMany("CookieSets")
                        .HasForeignKey("MyDmuUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MyDmuUser");
                });

            modelBuilder.Entity("DMUCalendarSync.Database.Models.MyDmuCookieSet", b =>
                {
                    b.Navigation("Cookies");
                });

            modelBuilder.Entity("DMUCalendarSync.Database.Models.MyDmuUser", b =>
                {
                    b.Navigation("CookieSets");
                });
#pragma warning restore 612, 618
        }
    }
}