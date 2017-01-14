using System;
using System.Collections.Generic;
using Couchbase;

namespace EternalChess
{
    public class DatabaseContoller
    {
        private static readonly Cluster Cluster = new Cluster();

        public void WriteToDatabase(string ferBoard, BoardState state)
        {
            using (var bucket = Cluster.OpenBucket("positions"))
            {
                var document = new Document<BoardState>
                {
                    Id = ferBoard,
                    Content = state
                };

                bucket.Upsert(document);
            }
        }

        public BoardState GetMovesById(string ferBoard)
        {
            using (var bucket = Cluster.OpenBucket("positions"))
            {
                var get = bucket.GetDocument<BoardState>(ferBoard);
                return get.Content;
            }
        }

    }
}
