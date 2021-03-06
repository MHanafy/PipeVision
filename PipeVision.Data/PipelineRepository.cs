﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PipeVision.Domain;

namespace PipeVision.Data
{
    public class PipelineRepository : IPipelineRepository
    {
        private readonly PipelineContext _context;

        public PipelineRepository(PipelineContext context)
        {
            _context = context;
        }

        public async Task<int?> GetLastUpdatedPipelineCounter(string pipelineName)
        {
            return (await _context.Pipelines
                .OrderByDescending(x => x.Counter)
                .FirstOrDefaultAsync(x => x.Name == pipelineName && x.InProgress == false))?.Counter;
        }

        public async Task AddPipeline(Pipeline pipeline)
        {
            await _context.Pipelines.AddAsync(pipeline);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePipeline(Pipeline pipeline, bool saveChanges = true)
        {
            foreach (var pipelineChange in pipeline.PipelineChangeLists)
            {
                var exChangelist = await _context.ChangeLists.FindAsync(pipelineChange.ChangeList.Id);
                //this assumes that ChangeLists are never changed
                if (exChangelist != null) pipelineChange.ChangeList = exChangelist;
                pipelineChange.PipelineId = pipelineChange.Pipeline.Id;
                pipelineChange.ChangelistId = pipelineChange.ChangeList.Id;
            }
            var existing = await GetPipeline(pipeline.Id);
            if (existing == null)
            {
                await _context.Pipelines.AddAsync(pipeline);
            }
            else
            {
                existing.Apply(pipeline);
            }

            if (saveChanges) await _context.SaveChangesAsync();
        }

        public async Task UpdatePipelines(IEnumerable<Pipeline> pipelines)
        {
            foreach (var pipeline in pipelines)
            {
                await UpdatePipeline(pipeline, false);
            }

           await _context.SaveChangesAsync();
        }

        public Task<Pipeline> GetPipeline(int id)
        {
            return _context.Pipelines.Where(x => x.Id == id)
                .Include(x => x.PipelineJobs)
                .Include(x => x.PipelineChangeLists)
                .FirstOrDefaultAsync();
        }

    }
}