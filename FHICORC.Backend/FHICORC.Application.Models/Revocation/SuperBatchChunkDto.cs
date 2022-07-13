using System;
using System.Collections.Generic;

namespace FHICORC.Application.Models.Revocation
{
    public class SuperBatchChunkDto
    {

        public SuperBatchChunkDto(DateTime nextLastModified, IEnumerable<SuperBatch> superBatches, bool hasMore = false) {
            N = nextLastModified;
            S = superBatches;
            M = hasMore;
        }

        // More revocationbatches available
        public bool M { get; private set; }

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
