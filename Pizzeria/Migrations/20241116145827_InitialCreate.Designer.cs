﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pizzeria.Database;

#nullable disable

namespace Pizzeria.Migrations
{
    [DbContext(typeof(PizzeriaContext))]
    [Migration("20241116145827_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Pizzeria.Database.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
