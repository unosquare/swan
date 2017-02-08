using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Unosquare.Swan.AspNetCore.Sample.Database;

namespace Unosquare.Swan.AspNetCore.Sample.Migrations
{
    [DbContext(typeof(SampleDbContext))]
    [Migration("20170208202520_Create_Tables")]
    partial class Create_Tables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Unosquare.Swan.AspNetCore.Models.LogEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Browser")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<DateTime>("Date");

                    b.Property<string>("Exception")
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<string>("HostAddress")
                        .HasAnnotation("MaxLength", 20);

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("Logger")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 4000);

                    b.Property<string>("Thread")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("Url")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<string>("Username")
                        .HasAnnotation("MaxLength", 50);

                    b.HasKey("Id");

                    b.ToTable("LogEntries");
                });

            modelBuilder.Entity("Unosquare.Swan.AspNetCore.Sample.Database.AuditTrailEntry", b =>
                {
                    b.Property<int>("AuditId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Action");

                    b.Property<DateTime>("DateCreated");

                    b.Property<string>("JsonBody");

                    b.Property<string>("TableName");

                    b.Property<string>("UserId");

                    b.HasKey("AuditId");

                    b.ToTable("AuditTrailEntries");
                });

            modelBuilder.Entity("Unosquare.Swan.AspNetCore.Sample.Database.Product", b =>
                {
                    b.Property<int>("ProductID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ProductID");

                    b.ToTable("Products");
                });
        }
    }
}
