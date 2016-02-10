﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Core.Memory;
using GoodAI.Core.Utils;

namespace GoodAI.Core.Memory
{
    public abstract class TensorDimensionsBase
    {
        protected readonly IImmutableList<int> m_dims;

        protected const int MaxDimensions = 100;  // ought to be enough for everybody

        protected TensorDimensionsBase()
        {
            m_dims = ImmutableList<int>.Empty;  // This means default dimensions.
        }

        protected TensorDimensionsBase(IImmutableList<int> immutableDimensions)
        {
            m_dims = immutableDimensions;
        }

        protected bool Equals(TensorDimensionsBase other)
        {
            if (other.Rank != Rank)
                return false;

            if ((Rank == 1) && (m_dims == null) && (other.m_dims == null))
                return true;

            return (m_dims != null) && (other.m_dims != null) && m_dims.SequenceEqual(other.m_dims);
        }

        public override int GetHashCode()
        {
            return IsEmpty ? 0 : m_dims.Aggregate(19, (hashCode, item) => 31 * hashCode + item);
        }

        public bool IsEmpty
        {
            get { return (m_dims == null) || (m_dims.Count == 0); }
        }

        public int Rank
        {
            get { return IsEmpty ? 1 : m_dims.Count; }
        }

        /// <summary>
        /// Rank synonym. (This is required by tests because this class has an the indexer. Please use Rank if possible.)
        /// </summary>
        [Obsolete("Preferably use Rank instead")]
        public int Count
        { get { return Rank; } }

        public int ElementCount
        {
            get
            {
                if (m_cachedElementCount == -1)
                {
                    m_cachedElementCount = IsEmpty ? 0 : Math.Abs(m_dims.Aggregate(1, (acc, item) => acc * item));
                }

                return m_cachedElementCount;
            }
        }
        private int m_cachedElementCount = -1;

        public int this[int index]
        {
            get
            {
                if (IsEmpty)
                {
                    if (index == 0)
                        return 0;  // We pretend we have one dimension of size 0.

                    throw GetIndexOutOfRangeException(index, 0);
                }

                if (index >= m_dims.Count)
                    throw GetIndexOutOfRangeException(index, m_dims.Count - 1);

                return m_dims[index];
            }
        }

        private static IndexOutOfRangeException GetIndexOutOfRangeException(int index, int maxIndex)
        {
            return new IndexOutOfRangeException(string.Format(
                "Index {0} is greater than max index {1}.", index, maxIndex));
        }
    }
}
