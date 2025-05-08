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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Condition 순서를 무작위로 섞기 위한 임시 구조체입니다.
    /// </summary>
    public struct ConditionConfig
    {
        public int A; // Diameter of the circle which targets are aligned along
        public int W; // Target width

        public ConditionConfig(int a, int w) { A = a; W = w; }
    }

    public class ConditionData
    {
        private int _block; // the index number of the block to which this condition belongs
        private int _index; // the index number of this condition
        private bool _circular; // circular ISO 9241-9 or vertical ribbons
        private int _a; // the nominal movement amplitude for this condition
        private int _w; // the nominal movement width for this condition
        private double _mtpct; // the percent of the predicted Fitts' movement time to make the normative time
        private long _mtpred; // the predicted movement time in milliseconds for this trial
        private long _tAppeared; // the millisecond time-stamp of when this condition appeared to the user
        private List<TrialData> _trials; // the collection of performed trials for this condition

        public ConditionData(int block, int index, int A, int W, int trials, int practice, bool is2D)
        {
            _block = block;
            _index = index;
            _a = A;
            _w = W;
        }

        public void AddMove(Vector2 pos)
        {

        }
    }
}

