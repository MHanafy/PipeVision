using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PipeVision.Domain
{


    public class ChangeList : IApplicable<ChangeList, int>
    {
        public void Apply(ChangeList updated)
        {
            ModifiedDate = updated.ModifiedDate;
            UserName = updated.UserName;
            Comment = updated.Comment;
        }

        /// <summary>
        /// This is Perforce changelist Id, not Go's
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id{get;set;}
        public DateTime ModifiedDate{get;set;}
        public string UserName{get;set;}
        public string Comment{get;set;}

        public virtual List<PipelineChangelist> PipelineChangeLists { get; set; }

        public ChangeList Clone()
        {
            var result = (ChangeList) MemberwiseClone();
            //Build = null;
            return result; 
        }
    }

    public class PipelineChangelist : IApplicable<PipelineChangelist, Tuple<int, int>>
    {
        public int PipelineId { get; set; }
        public int ChangelistId { get; set; }
        public virtual Pipeline Pipeline { get; set; }
        public virtual ChangeList ChangeList { get; set; }
        public Tuple<int, int> Id => new Tuple<int, int>(PipelineId, ChangelistId);
        public void Apply(PipelineChangelist updated)
        {
            //Apply additional properties if needed
        }
    }
}
