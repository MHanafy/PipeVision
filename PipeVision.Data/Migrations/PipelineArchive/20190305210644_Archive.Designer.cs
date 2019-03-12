﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PipeVision.Data;

namespace PipeVision.Data.Migrations.PipelineArchive
{
    [DbContext(typeof(PipelineArchiveContext))]
    [Migration("20190305210644_Archive")]
    partial class Archive
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PipeVision.Data.Test", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Arc_Tests");
                });

            modelBuilder.Entity("PipeVision.Data.TestRun", b =>
                {
                    b.Property<int>("TestId");

                    b.Property<int>("PipelineJobId");

                    b.Property<string>("CallStack");

                    b.Property<long>("Duration");

                    b.Property<string>("Error");

                    b.HasKey("TestId", "PipelineJobId");

                    b.HasIndex("PipelineJobId");

                    b.ToTable("Arc_TestRuns");
                });

            modelBuilder.Entity("PipeVision.Domain.ChangeList", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Comment");

                    b.Property<DateTime>("ModifiedDate");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.ToTable("Arc_ChangeLists");
                });

            modelBuilder.Entity("PipeVision.Domain.Pipeline", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("Counter");

                    b.Property<bool>("InProgress");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Arc_Pipelines");
                });

            modelBuilder.Entity("PipeVision.Domain.PipelineChangelist", b =>
                {
                    b.Property<int>("PipelineId");

                    b.Property<int>("ChangelistId");

                    b.HasKey("PipelineId", "ChangelistId");

                    b.HasIndex("ChangelistId");

                    b.ToTable("Arc_PipelineChangeLists");
                });

            modelBuilder.Entity("PipeVision.Domain.PipelineJob", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Agent");

                    b.Property<DateTime>("EndDate");

                    b.Property<int>("LogStatus");

                    b.Property<string>("Name");

                    b.Property<int>("PipelineId");

                    b.Property<string>("Result");

                    b.Property<int>("StageCounter");

                    b.Property<string>("StageName");

                    b.Property<DateTime>("StartDate");

                    b.Property<int>("TestType");

                    b.HasKey("Id");

                    b.HasIndex("PipelineId");

                    b.ToTable("Arc_PipelineJobs");
                });

            modelBuilder.Entity("PipeVision.Data.TestRun", b =>
                {
                    b.HasOne("PipeVision.Domain.PipelineJob", "PipelineJob")
                        .WithMany()
                        .HasForeignKey("PipelineJobId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PipeVision.Data.Test", "Test")
                        .WithMany("TestRuns")
                        .HasForeignKey("TestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PipeVision.Domain.PipelineChangelist", b =>
                {
                    b.HasOne("PipeVision.Domain.ChangeList", "ChangeList")
                        .WithMany("PipelineChangeLists")
                        .HasForeignKey("ChangelistId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PipeVision.Domain.Pipeline", "Pipeline")
                        .WithMany("PipelineChangeLists")
                        .HasForeignKey("PipelineId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PipeVision.Domain.PipelineJob", b =>
                {
                    b.HasOne("PipeVision.Domain.Pipeline", "Pipeline")
                        .WithMany("PipelineJobs")
                        .HasForeignKey("PipelineId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}