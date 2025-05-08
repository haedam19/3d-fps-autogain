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
using UnityEngine.UIElements;

namespace Data
{
    public class MovementData
    {
        public struct Profiles
        {
            public static readonly Profiles Empty;
            public bool IsEmpty { get { return Position == null && Velocity == null && Acceleration == null && Jerk == null; } }
            public List<float> timeStamp;
            public List<Vector2> Position;
            public List<Vector2> Velocity;
            public List<Vector2> Acceleration;
            public List<Vector2> Jerk;
        }

        public List<Vector2> mousePos;
        public List<long> time; // ms 단위

        #region Properties: NumMoves, Travel, Duration
        public int NumMoves { get { return mousePos.Count; } }

        /// <summary>
        /// Gets the total distance traveled during movement.
        /// </summary>
        public float Travel
        {
            get
            {
                float travel = 0f;
                for (int i = 1; i < NumMoves; i++)
                    travel += Vector2.Distance(mousePos[i], mousePos[i - 1]);
                return travel;
            }
        }

        /// <summary>
        /// Gets the duration of this movement, in milliseconds.
        /// </summary>
        public long Duration
        {
            get
            {
                if (time != null || time.Count > 0)
                    return time[time.Count - 1] - time[0];
                else return 0L;
            }
        }
        #endregion

        /// <param name="pos">좌하단 원점 기준</param>
        /// <param name="t">Trial 시작 시점을 기준, PerformanceCounter 단위로 기록합니다.</param>
        public void AddMove(Vector2 pos, long t)
        {
            // 마우스 움직임 X -> 시간만 업데이트
            if (mousePos.Count > 0 && mousePos[mousePos.Count - 1] == pos)
            {
                time.RemoveAt(time.Count - 1);
                time.Add(t);
            }
            else
            {
                mousePos.Add(pos);
                time.Add(t);
            }
        }

        public void ClearMoves()
        {
            mousePos.Clear();
            time.Clear();
            
        }
    }
}

