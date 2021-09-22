﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Schwabra;

namespace Schwabra.Migrations
{
    [DbContext(typeof(ElectionContext))]
    [Migration("20210922220720_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("Schwabra.Result", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Stationid")
                        .HasColumnType("INTEGER");

                    b.Property<int>("num")
                        .HasColumnType("INTEGER");

                    b.Property<string>("title")
                        .HasColumnType("TEXT");

                    b.Property<int>("value")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("value_percent")
                        .HasColumnType("REAL");

                    b.HasKey("id");

                    b.HasIndex("Stationid");

                    b.ToTable("result");
                });

            modelBuilder.Entity("Schwabra.Station", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("filename")
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .HasColumnType("TEXT");

                    b.Property<string>("path")
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("station");
                });

            modelBuilder.Entity("Schwabra.Result", b =>
                {
                    b.HasOne("Schwabra.Station", null)
                        .WithMany("rows")
                        .HasForeignKey("Stationid");
                });

            modelBuilder.Entity("Schwabra.Station", b =>
                {
                    b.Navigation("rows");
                });
#pragma warning restore 612, 618
        }
    }
}
