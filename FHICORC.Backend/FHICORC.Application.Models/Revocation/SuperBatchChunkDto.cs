using System;
using System.Collections.Generic;

namespace FHICORC.Application.Models.Revocation
{
    public class SuperBatchChunkDto
    {

        public SuperBatchChunkDto(DateTime nextLastModified, IEnumerable<SuperBatch> superBatches) {
            N = nextLastModified;
            S = superBatches;

        }
        /// <summary>
        /// NextLastModified
        /// The modified dateTime of the first Superbatch in the next chunk
        /// If ther there are noe more chunks to download this is set to null.
        /// </summary>
        public DateTime N { get; private set; }

        // SuperBatches
        public IEnumerable<SuperBatch> S { get; private set; }

    }
}
