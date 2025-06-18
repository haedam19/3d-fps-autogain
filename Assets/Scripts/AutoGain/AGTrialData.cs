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
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Xml;
using MouseLog;

/// <summary>
/// This class encapsulates the data associated with a single trial (single click) within a
/// condition within a Fitts law study. The class holds all information necessary for defining
/// a single trial, including its target locations.
/// </summary>
public class AGTrialData
{
    #region Fields
    protected int _number; // 1-based number of this trial; trial 0 is reserved for the start area for the condition
    protected bool _practice; // true if this is a practice trial; false otherwise

    protected TimePointR _start; // the click point that started this trial
    protected TimePointR _end; // the click point that ended this trial

    protected AGMovementData _movement; // the movement associated with this trial
    private AGTargetData _thisTargetData;
    private AGTargetData _lastTargetData;

    public double A;
    public double W;

    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor for an abstract Fitts' law trial.
    /// </summary>
    public AGTrialData()
    {
        // do nothing
    }

    /// <summary>
    /// Constructor for an abstract Fitts' law trial. Actual trial instances must be created from
    /// subclasses that define the specific trial mechanics.
    /// </summary>
    /// <param name="index">The 0-based index number of this trial.</param>
    /// <param name="practice">True if this trial is practice; false otherwise. Practice trials aren't included in any calculations.</param>
    /// <param name="tInterval">The metronome time interval in milliseconds, or -1L if unused.</param>
    public AGTrialData(int index, bool practice, AGTargetData lastTarget, AGTargetData thisTarget)
    {
        _number = index;
        _practice = practice;
        _start = TimePointR.Empty;
        _end = TimePointR.Empty;
        _movement = new AGMovementData(this);
        _lastTargetData = lastTarget;
        _thisTargetData = thisTarget;
    }

    #endregion

    #region Movement
    public AGMovementData Movement
    {
        get { return _movement; }
        set { _movement = value; }
    }
    #endregion

    #region Condition Values: ID, Axis

    public double ID { get { return Math.Log((double)A / W + 1.0, 2.0); } }

    /// <summary> Gets the angle of the nominal movement axis for this trial, in radians. </summary>
    public double Axis { get { return PointR.Angle(_lastTargetData.posR, _thisTargetData.posR, true); } }

    #endregion

    #region Measured Values

    /// <summary>
    /// Gets whether or not this trial has been completed. A completed trial has been
    /// performed and therefore has a non-zero ending time-stamp.
    /// </summary>
    public bool IsComplete
    {
        get
        {
            return (_end.Time != 0L);
        }
    }

    /// <summary>
    /// Gets or sets the start click point and time that began this trial.
    /// </summary>
    public TimePointR Start
    {
        get { return _start; }
        set { _start = value; }
    }

    /// <summary>
    /// Gets or sets the selection endpoint and time that ended this trial.
    /// </summary>
    public TimePointR End
    {
        get { return _end; }
        set { _end = value; }
    }

    /// <summary>
    /// Gets the actual movement angle for this trial, in radians.
    /// </summary>
    public double Angle
    {
        get
        {
            return PointR.Angle((PointR)_start, (PointR)_end, true);
        }
    }

    /// <summary>
    /// Gets the trial start point normalized relative to this trial's target as if the target
    /// were centered at (0,0) and movement towards it was at 0 degrees, that is, straight right
    /// along the +x-axis. Thus, normalized start points will always begin at (-x,0).
    /// </summary>
    public PointR NormalizedStart
    {
        get
        {
            PointR center = this.TargetCenterFromStart;
            double radians = PointR.Angle((PointR)_start, (PointR)center, true);
            PointR newStart = PointR.RotatePoint((PointR)_start, (PointR)center, -radians);
            newStart.X -= center.X;
            newStart.Y -= center.Y;
            return newStart;
        }
    }

    /// <summary>
    /// Gets the selection endpoint normalized relative to this trial's target as if the target
    /// were centered at (0,0) and movement towards it was at 0 degrees, that is, straight right
    /// along the +x-axis. This allows endpoint distributions of trials within a condition to be
    /// compared despite not any of the actual target locations in each condition being the same.
    /// </summary>
    /// <remarks>This property is used in the calculation of the effective target width for the condition, We.</remarks>
    public PointR NormalizedEnd
    {
        get
        {
            // find the angle of the ideal task axis for this trial
            PointR center = this.TargetCenterFromStart;
            double radians = PointR.Angle((PointR)_start, center, true);

            // rotate the endpoint around the target center so that it would have come from 
            // a task whose task-axis was at 0 degrees (+x, straight to the right).
            PointR newEnd = PointR.RotatePoint((PointR)_end, center, -radians);

            // translate the endpoint so that it is as if the target was centered at (0,0).
            newEnd.X -= center.X;
            newEnd.Y -= center.Y;

            return newEnd;
        }
    }

    /// <summary>
    /// Normalizes the trial time so that the start time is zero and each move time
    /// and the end time are relative to that. Only works on completed trials.
    /// </summary>
    public void NormalizeTimes()
    {
        if (!IsComplete)
            return;

        _movement.NormalizeTimes(_start.Time);
        _end.Time -= _start.Time;
        _start.Time = 0L;
    }

    /// <summary>
    /// Gets the actual movement time in milliseconds for this trial.
    /// </summary>
    public long MTe
    {
        get
        {
            return _end.Time - _start.Time;
        }
    }

    /// <summary>
    /// For a completed trial, gets the number of target entries. If the trial was an error,
    /// it may be that the target was never entered. The most successful trials will have
    /// a target entered once and only once. If target re-entry occurs, the target was entered 
    /// multiple times.
    /// </summary>
    public int TargetEntries
    {
        get
        {
            int n = 0;
            bool inside = false;

            for (int i = 0; i < _movement.NumMoves; i++)
            {
                TimePointR pt = _movement[i];
                if (_thisTargetData.Contains((PointR)pt)) // now inside
                {
                    if (!inside) // were not yet inside
                    {
                        inside = true;
                        n++; // entry
                    }
                }
                else inside = false; // not inside
            }
            return n;
        }
    }

    /// <summary>
    /// Gets the number of times the mouse passed beyond the target's far edge, whether inside or 
    /// outside the target. For 2D trials, this is conceptually like putting a line tangent to the
    /// far side of the circle perpendicular to the movement direction. At every overshoot occurrence,
    /// the tangent line is re-computed to the new far side of the target, and so on.
    /// </summary>
    public int TargetOvershoots
    {
        get
        {
            int n = 0;

            double radians = PointR.Angle((PointR)_start, _thisTargetData.posR, true); // angle of the task axis

            for (int i = 0; i < _movement.NumMoves; i++)
            {
                TimePointR pt = PointR.RotatePoint((PointR)_movement[i], _thisTargetData.posR, -radians); // rotate for 0-degree task
                if (pt.X > _thisTargetData.posR.X + _thisTargetData.radius) // if we've broken the line tangent to the far side of the circle 
                {
                    n++; // overshoot
                    radians = PointR.Angle((PointR)_movement[i], _thisTargetData.posR, true); // update for new angle from this point
                }
            }
            return n;
        }
    }

    #endregion

    #region Other Measures
    /// <summary>
    /// Gets the actual trial amplitude for this trial as the effective amplitude (Ae).
    /// </summary>
    public double GetAe(bool bivariate)
    {
        if (bivariate) // two-dimensional distance
        {
            return PointR.Distance((PointR)_start, (PointR)_end);
        }
        else // only consider x-coordinate
        {
            PointR nstart = this.NormalizedStart;  // relative to a target at (0,0)
            PointR nend = this.NormalizedEnd;
            return Math.Abs(nend.X - nstart.X);
        }
    }

    /// <summary>
    /// Gets the distance from the center of the target. For circle targets, the bivariate outcome is
    /// the Euclidean distance to the target center. The univariate outcome is the x-distance to the
    /// x-coordinate of the target center.
    /// </summary>
    /// <remarks>This is NOT used to compute We as 4.133 * SDx. Instead, we must compute We
    /// more carefully using the standard deviation of normalized distances from the normalized selection
    /// mean.</remarks>
    public double GetDx(bool bivariate)
    {
        return bivariate ? PointR.Distance(_thisTargetData.posR, (PointR)_end) : this.NormalizedEnd.X; // nend is relative to (0,0)
    }

    #endregion

    #region Error and Outlier

    public bool IsError
    {
        get
        {
            return !_thisTargetData.Contains((PointR)_end);
        }
    }

    /// <summary>
    /// Gets a value indicating whether or not this trial is defined as a 
    /// "spatial outlier," which means that the selection point was outside
    /// the target, and (a) the actual distance moved was less than half the
    /// nominal distance required, and/or (b) the distance from the selection
    /// point to the target center was more than twice the width of the target.
    /// </summary>
    public bool IsSpatialOutlier
    {
        get
        {
            return this.IsError && (
                (this.GetAe(true) < A / 2.0) || (Math.Abs(this.GetDx(true)) > 2.0 * W)
                );
        }
    }

    #endregion

    #region Target

    public AGTargetData ThisTarget => _thisTargetData;
    public AGTargetData LastTarget => _lastTargetData;

    public PointR TargetCenter
    {
        get { return _thisTargetData.posR; }
    }

    /// <summary>
    /// Gets the center point of this target relative to the start of the trial.
    /// However, for circular targets, the center is the same regardless of approach
    /// angle.
    /// </summary>
    /// <remarks>If the trial has not been run yet, there will not be a start point
    /// and this property's value is meaningless. In this case, <b>PointF.Empty</b> is
    /// its value.</remarks>
    public PointR TargetCenterFromStart
    {
        get
        {
            if (this.IsComplete)
            {
                return _thisTargetData.posR;
            }
            return PointR.Empty;
        }
    }

    public bool TargetContains(PointR pt)
    {
        return _thisTargetData.Contains(pt);
    }

    #endregion

}