/**
 * FittsStudy
 *
 *		Jacob O. Wobbrock, Ph.D.
 * 		The Information School
 *		University of Washington
 *		Mary Gates Hall, Box 352840
 *		Seattle, WA 98195-2840
 *		wobbrock@uw.edu
 *		
 * This software is distributed under the "New BSD License" agreement:
 * 
 * Copyright (C) 2007-2022, Jacob O. Wobbrock. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the name of the University of Washington nor the names of its 
 *      contributors may be used to endorse or promote products derived from 
 *      this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Jacob O. Wobbrock
 * BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
**/
using System;

namespace MouseLog
{
    /// <summary>
    /// Defines an enumerated type for which type of outliers to exclude for 
    /// certain computations regarding trials.
    /// </summary>
    [Flags]
    public enum ExcludeOutliersType
    {
        /// <summary>
        /// No outliers should be excluded from the given calculation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Simple spatial outliers should be excluded from the given calculation. Simple
        /// spatial outliers are those whose effective amplitude of movement is less than
        /// half the normative amplitude of movement, or whose selection endpoint is more
        /// than twice the target width from the target center.
        /// </summary>
        Spatial = 0x01,

        /// <summary>
        /// Temporal outliers should be excluded from the given calculation. Temporal outliers
        /// are those whose movement time is less than 75% of the normative movement time, or
        /// greater than 125% of the normative movement time.
        /// </summary>
        Temporal = 0x02,

        /// <summary>
        /// Both spatial and temporal outliers should be excluded from the given calculation.
        /// </summary>
        Both = Spatial | Temporal
    }
}