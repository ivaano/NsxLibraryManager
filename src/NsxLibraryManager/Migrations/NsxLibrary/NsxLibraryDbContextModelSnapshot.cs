﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NsxLibraryManager.Data;

#nullable disable

namespace NsxLibraryManager.Migrations.NsxLibrary
{
    [DbContext(typeof(NsxLibraryDbContext))]
    partial class NsxLibraryDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.13");

            modelBuilder.Entity("LanguageTitle", b =>
                {
                    b.Property<int>("LanguagesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitlesId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LanguagesId", "TitlesId");

                    b.HasIndex("TitlesId");

                    b.ToTable("LanguageTitle", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FriendlyName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Xml")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("Categories", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.CategoryLanguage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("CategoryLanguages", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Collection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("Collections", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("Languages", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.LibraryUpdate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BaseTitleCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("TEXT");

                    b.Property<int>("DlcTitleCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LibraryPath")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<int>("UpdateTitleCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("LibraryUpdates", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.PersistentTitle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApplicationId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Collection")
                        .HasMaxLength(100)
                        .HasColumnType("VARCHAR");

                    b.Property<DateTime>("FirstSeen")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserRating")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PersistentTitles", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.RatingsContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("RatingsContent", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Screenshot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EditionId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TitleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.HasIndex("TitleId");

                    b.ToTable("Screenshots", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Settings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Key")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Settings", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Title", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApplicationId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("BannerUrl")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("CollectionId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("ContentType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(5000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Developer")
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("DlcCount")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Favorite")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("VARCHAR");

                    b.Property<string>("IconUrl")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("Intro")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<bool>("IsDemo")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastWriteTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LatestMissingDlcDate")
                        .HasColumnType("TEXT");

                    b.Property<uint>("LatestOwnedUpdateVersion")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("LatestVersion")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("Notes")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<long?>("NsuId")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("NumberOfPlayers")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OtherApplicationId")
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("OwnedDlcs")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("OwnedUpdates")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PackageType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Publisher")
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR");

                    b.Property<int?>("Rating")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Region")
                        .HasMaxLength(2)
                        .HasColumnType("VARCHAR");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<long?>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TitleName")
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpdatesCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserRating")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Version")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.ToTable("Titles", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.TitleCategory", b =>
                {
                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.HasKey("CategoryId", "TitleId");

                    b.HasIndex("TitleId");

                    b.ToTable("TitleCategory", (string)null);
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Version", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VersionDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<uint>("VersionNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TitleId");

                    b.ToTable("Versions", (string)null);
                });

            modelBuilder.Entity("RatingsContentTitle", b =>
                {
                    b.Property<int>("RatingsContentsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TitlesId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RatingsContentsId", "TitlesId");

                    b.HasIndex("TitlesId");

                    b.ToTable("RatingsContentTitle", (string)null);
                });

            modelBuilder.Entity("LanguageTitle", b =>
                {
                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Language", null)
                        .WithMany()
                        .HasForeignKey("LanguagesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Title", null)
                        .WithMany()
                        .HasForeignKey("TitlesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.CategoryLanguage", b =>
                {
                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Category", "Category")
                        .WithMany("Languages")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Screenshot", b =>
                {
                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Title", "Title")
                        .WithMany("Screenshots")
                        .HasForeignKey("TitleId");

                    b.Navigation("Title");
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Title", b =>
                {
                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Collection", "Collection")
                        .WithMany("Titles")
                        .HasForeignKey("CollectionId");

                    b.Navigation("Collection");
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.TitleCategory", b =>
                {
                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Category", "Category")
                        .WithMany("TitleCategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Title", "Title")
                        .WithMany("TitleCategories")
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Title");
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Version", b =>
                {
                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Title", "Title")
                        .WithMany("Versions")
                        .HasForeignKey("TitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Title");
                });

            modelBuilder.Entity("RatingsContentTitle", b =>
                {
                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.RatingsContent", null)
                        .WithMany()
                        .HasForeignKey("RatingsContentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NsxLibraryManager.Models.NsxLibrary.Title", null)
                        .WithMany()
                        .HasForeignKey("TitlesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Category", b =>
                {
                    b.Navigation("Languages");

                    b.Navigation("TitleCategories");
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Collection", b =>
                {
                    b.Navigation("Titles");
                });

            modelBuilder.Entity("NsxLibraryManager.Models.NsxLibrary.Title", b =>
                {
                    b.Navigation("Screenshots");

                    b.Navigation("TitleCategories");

                    b.Navigation("Versions");
                });
#pragma warning restore 612, 618
        }
    }
}
