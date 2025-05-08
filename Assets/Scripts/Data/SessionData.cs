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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public struct ScreenData
    {
        public int W;
        public int H;
    }

    public class SessionData
    {
        public int _subject; // subject ID
        public bool _circular; // circular ISO 9241-9 or vertical ribbons
        public Screen _screen; // screen size
        public int[] _ASet; // movement amplitudes in pixels
        public int[] _WSet; // target widths in pixels
        public List<ConditionData> _conditions; // ordered list of (A x W) conditions
        public int condIdx;

        public SessionData(int subject, bool circular, Screen screen, int[] a, int[] w)
        {
            _subject = subject;
            _circular = circular;
            _screen = screen;
            _ASet = a;
            _WSet = w;

            // Get shuffled condition sequence
            List<ConditionConfig> conditions = CreateConditionSequence(true);
            for (int i = 0; i < conditions.Count; i++)
            {
                //ConditionData cond = new ConditionData();
            }
        }

        public List<ConditionConfig> CreateConditionSequence(bool shuffle)
        {
            List<ConditionConfig> conditionList = new List<ConditionConfig>();
            foreach (int A in _ASet)
            {
                foreach (int W in _WSet)
                    conditionList.Add(new ConditionConfig(A, W));
            }

            // Shuffle Condition Sequence
            if (shuffle)
            {
                ConditionConfig temp;
                int length = conditionList.Count;
                int i, j;
                for (i = 0; i < length; i++)
                {
                    j = UnityEngine.Random.Range(i, length);
                    temp = conditionList[i];
                    conditionList[i] = conditionList[j];
                    conditionList[j] = temp;
                }
            }

            return conditionList;
        }
    }

        

}

