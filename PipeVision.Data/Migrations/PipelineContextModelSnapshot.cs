﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PipeVision.Data;

namespace PipeVision.Data.Migrations
{
    [DbContext(typeof(PipelineContext))]
    partial class PipelineContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PipeVision.Domain.ChangeList", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Comment");

                    b.Property<DateTime>("ModifiedDate");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.ToTable("ChangeLists");
                });

            modelBuilder.Entity("PipeVision.Domain.Pipeline", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int>("Counter");

                    b.Property<bool>("InProgress");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Pipelines");
                });

            modelBuilder.Entity("PipeVision.Domain.PipelineChangelist", b =>
                {
                    b.Property<int>("PipelineId");

                    b.Property<int>("ChangelistId");

                    b.HasKey("PipelineId", "ChangelistId");

                    b.HasIndex("ChangelistId");

                    b.ToTable("PipelineChangeLists");
                });

            modelBuilder.Entity("PipeVision.Domain.PipelineJob", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Agent");

                    b.Property<DateTime>("EndDate");

                    b.Property<string>("Name");

                    b.Property<int>("PipelineId");

                    b.Property<string>("Result");

                    b.Property<int>("StageCounter");

                    b.Property<string>("StageName");

                    b.Property<DateTime>("StartDate");

                    b.Property<int>("TestType");

                    b.HasKey("Id");

                    b.HasIndex("PipelineId");

                    b.ToTable("PipelineJobs");
                });

            modelBuilder.Entity("PipeVision.Domain.Test", b =>
                {
                    b.Property<int>("PipelineJobId");

                    b.Property<string>("Name");

                    b.Property<string>("CallStack");

                    b.Property<long>("Duration");

                    b.Property<string>("Error");

                    b.HasKey("PipelineJobId", "Name");

                    b.ToTable("Tests");
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

            modelBuilder.Entity("PipeVision.Domain.Test", b =>
                {
                    b.HasOne("PipeVision.Domain.PipelineJob", "PipelineJob")
                        .WithMany("Tests")
                        .HasForeignKey("PipelineJobId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
