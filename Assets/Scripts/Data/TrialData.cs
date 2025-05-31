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

namespace MouseLog
{
    /// <summary>
    /// This class encapsulates the data associated with a single trial (single click) within a
    /// condition within a Fitts law study. The class holds all information necessary for defining
    /// a single trial, including its target locations.
    /// </summary>
    public class TrialData: IXmlLoggable
    {
        #region Fields
        protected int _number; // 1-based number of this trial; trial 0 is reserved for the start area for the condition
        protected bool _practice; // true if this is a practice trial; false otherwise
        protected long _tInterval; // the normative movement time interval in milliseconds, or -1L. (메트로놈 안 쓰면 필요 X)

        protected TimePointR _start; // the click point that started this trial
        protected TimePointR _end; // the click point that ended this trial

        protected MovementData _movement; // the movement associated with this trial
        private Target3D _thisTarget;
        private Target3D _lastTarget;
        private PointR _isoCenter;

        private ConditionData _owner;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for an abstract Fitts' law trial.
        /// </summary>
        public TrialData()
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
        public TrialData(int index, bool practice, ConditionData owner, Target3D lastTarget, Target3D thisTarget, PointR center, long tInterval)
        {
            _number = index;
            _practice = practice;
            _owner = owner; // the condition that owns this trial
            _tInterval = tInterval;
            _start = TimePointR.Empty;
            _end = TimePointR.Empty;
            _movement = new MovementData(this);
            _lastTarget = lastTarget;
            _thisTarget = thisTarget;
            _isoCenter = center;
        }

        /// <summary>
        /// This static method is to be used by the ConditionData instance when creating the
        /// special start-area trial after reading trial number 1 in a XML log file. The special
        /// start-area trial is not logged explicitly, but it can be created from the information
        /// in trial number 1.
        /// </summary>
        /// <param name="trialOne">The first trial in the condition whose last target is used to
        /// create the special start-area trial.</param>
        /// <returns>The special start-area trial for a 2D task.</returns>
        internal static TrialData CreateStartTarget(TrialData trialOne)
        {
            TrialData st = new TrialData();
            st._number = 0;
            st._practice = true;
            st._thisTarget = trialOne._lastTarget;
            st._isoCenter = trialOne._isoCenter;
            st._tInterval = trialOne._tInterval;
            return st;
        }
        #endregion

        #region Movement
        public MovementData Movement
        {
            get { return _movement; }
            set { _movement = value; }
        }
        #endregion

        #region Condition Values: Number, IsStartAreaTrial, IsPractice, UsedMetronome, ID(unused), MT, Axis, Circular, A, W

        public int Number { get { return _number; } }

        public bool IsStartAreaTrial { get { return _number == 0; } }

        /// <summary> Practice Trial은 condition-level calculations에서 제외됩니다. </summary>
        public bool IsPractice { get { return _practice; } }

        /// <summary> Gets whether or not this trial used a metronome to govern movement time. </summary>
        public bool UsedMetronome { get { return _tInterval != -1L; } }

        public double ID { get { return Math.Log((double)A /W + 1.0, 2.0); } }

        /// <summary>
        /// Gets the normative movement time in milliseconds for this trial. The normative 
        /// movement time is the time of a desired movement "encouraged" by the metronome 
        /// interval tick time. If a normative time isn't used, this value is -1L.
        /// </summary>
        public long MT { get { return _tInterval; } }

        /// <summary> Gets the angle of the nominal movement axis for this trial, in radians. </summary>
        public double Axis { get { return PointR.Angle(_lastTarget.CenterP, _thisTarget.CenterP, true); } }

        public bool Circular { get { return true; } } // 1D 조건 취급 안 하니 무조건 true;

        public int A { get { return _owner.A; } }

        public int W { get { return _owner.W; } }
        #endregion

        #region Measured Values

        /// <summary>
        /// Clears the performance data associated with this trial. Does not clear the
        /// independent variables that define the settings for this trial.
        /// </summary>
        public virtual void ClearData()
        {
            _start = TimePointR.Empty;
            _end = TimePointR.Empty;
            _movement.ClearMoves();
        }

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
        /// Gets a ratio indicating how the actual movement time (MTe) corresponded to the normative
        /// movement time (MT). The ratio is <c>MTe / MT</c>. Thus, values &gt;1 indicate that the actual movement
        /// time was too slow. Values &lt;1 indicate that the actual movement time was too fast. A value
        /// of 1.00 indicates the actual movement time was the same as that prescribed.
        /// </summary>
        /// <remarks>
        /// The maximum possible number of actual metronome ticks that could have been heard is the <b>Math.Ceiling</b> 
        /// of this property. The minimum number of ticks that could have been heard is the <b>Math.Floor</b> of this 
        /// property.
        /// </remarks>
        public double MTRatio
        {
            get
            {
                if (this.UsedMetronome)
                {
                    double diff = _end.Time - _start.Time;
                    return diff / _tInterval;
                }
                return -1.0; // unused
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
                    if (_thisTarget.Contains((PointR)pt)) // now inside
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

                double radians = PointR.Angle((PointR)_start, _thisTarget.CenterP, true); // angle of the task axis

                for (int i = 0; i < _movement.NumMoves; i++)
                {
                    TimePointR pt = PointR.RotatePoint((PointR)_movement[i], _thisTarget.CenterP, -radians); // rotate for 0-degree task
                    if (pt.X > _thisTarget.CenterP.X + _thisTarget.Radius) // if we've broken the line tangent to the far side of the circle 
                    {
                        n++; // overshoot
                        radians = PointR.Angle((PointR)_movement[i], _thisTarget.CenterP, true); // update for new angle from this point
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
            return bivariate ? PointR.Distance(_thisTarget.CenterP, (PointR)_end) : this.NormalizedEnd.X; // nend is relative to (0,0)
        }

        #endregion

        #region Error and Outlier

        public bool IsError
        {
            get
            {
                return !_thisTarget.Contains((PointR)_end);
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
                    (this.GetAe(true) < _owner.A / 2.0) || (Math.Abs(this.GetDx(true)) > 2.0 * _owner.W)
                    );
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this trial had a movement time 
        /// that is a temporal outlier. A temporal outlier is considered unduly far
        /// from the normative time interval. It is defined by a movement time that 
        /// is less than 75% of the normative movement time (too fast), or more than 
        /// 125% of the normative movement time (too slow).
        /// </summary>
        /// <example>
        /// If a metronome movement time (MT) is set to 500 ms, a temporal outlier
        /// will occur if the effective movement time (MTe) is either less than 375 ms
        /// or greater than 625 ms. That amounts to 500 +/- 125 ms.
        /// </example>
        public bool IsTemporalOutlier
        {
            get
            {
                if (_tInterval != -1.0)
                {
                    double diff = _end.Time - _start.Time;
                    return (
                        (diff < _tInterval * 0.75) ||
                        (diff > _tInterval * 1.25)
                        );
                }
                return false;
            }
        }
        #endregion

        #region Target

        public Target3D ThisTarget => _thisTarget;
        public Target3D LastTarget => _lastTarget;

        public PointR TargetCenter
        {
            get { return _thisTarget.CenterP; }
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
                    return _thisTarget.CenterP;
                }
                return PointR.Empty;
            }
        }

        public bool TargetContains(PointR pt)
        {
            return _thisTarget.Contains(pt);
        }

        #endregion

        #region IXmlLoggable Members

        /// <summary>
        /// Writes all or part of this data object to XML. If this data object owns other
        /// data objects that will also be written, this method may leave some XML elements
        /// open, which will be closed with a later call to <i>WriteXmlFooter</i>.
        /// </summary>
        /// <param name="writer">An open XML writer. The writer will be left open by this method
        /// after writing.</param>
        /// <returns>Returns <b>true</b> if successful; <b>false</b> otherwise.</returns>
        public bool WriteXmlHeader(XmlTextWriter writer)
        {
            writer.WriteStartElement("Trial");
            writer.WriteAttributeString("number", XmlConvert.ToString(_number));
            writer.WriteAttributeString("circular", XmlConvert.ToString(true)); // 2D
            writer.WriteAttributeString("metronome", XmlConvert.ToString(this.UsedMetronome));
            writer.WriteAttributeString("completed", XmlConvert.ToString(this.IsComplete));
            writer.WriteAttributeString("practice", XmlConvert.ToString(_practice));

            writer.WriteAttributeString("lastCircle", _lastTarget.ToString());
            writer.WriteAttributeString("thisCircle", _thisTarget.ToString());
            writer.WriteAttributeString("isoCenter", _isoCenter.ToString());

            writer.WriteAttributeString("MT", XmlConvert.ToString(_tInterval));
            writer.WriteAttributeString("A", XmlConvert.ToString(_owner.A));
            writer.WriteAttributeString("W", XmlConvert.ToString(_owner.W));
            writer.WriteAttributeString("axis", XmlConvert.ToString(Math.Round((double)Mathf.Rad2Deg * this.Axis, 4)));

            writer.WriteAttributeString("angle", XmlConvert.ToString(Math.Round((double)Mathf.Rad2Deg * this.Axis, 4)));
            writer.WriteAttributeString("ae_1d", XmlConvert.ToString(Math.Round(this.GetAe(true), 4))); // 2d 값으로 강제 입력
            writer.WriteAttributeString("dx_1d", XmlConvert.ToString(Math.Round(this.GetDx(true), 4))); // 2d 값으로 강제 입력

            writer.WriteAttributeString("ae_2d", XmlConvert.ToString(Math.Round(this.GetAe(true), 4)));
            writer.WriteAttributeString("dx_2d", XmlConvert.ToString(Math.Round(this.GetDx(true), 4)));

            writer.WriteAttributeString("MTe", XmlConvert.ToString(this.MTe));
            writer.WriteAttributeString("MTRatio", XmlConvert.ToString(Math.Round(this.MTRatio, 4)));

            writer.WriteAttributeString("entries", XmlConvert.ToString(this.TargetEntries));
            writer.WriteAttributeString("overshoots", XmlConvert.ToString(this.TargetOvershoots));
            writer.WriteAttributeString("error", XmlConvert.ToString(this.IsError));
            writer.WriteAttributeString("spatialOutlier", XmlConvert.ToString(this.IsSpatialOutlier));
            writer.WriteAttributeString("temporalOutlier", XmlConvert.ToString(this.IsTemporalOutlier));

            writer.WriteAttributeString("start", _start.ToString());
            writer.WriteAttributeString("end", _end.ToString());

            // write out the movement for this trial
            _movement.WriteXmlHeader(writer);

            writer.WriteEndElement(); // </Trial>

            return true;
        }

        /// <summary>
        /// Writes any closing XML necessary for this data object. This method can simply
        /// return <b>true</b> if all data was already written in the header.
        /// </summary>
        /// <param name="writer">An open XML writer. The writer will be closed by this method
        /// after writing.</param>
        /// <returns>Returns <b>true</b> if successful; <b>false</b> otherwise.</returns>
        public bool WriteXmlFooter(XmlTextWriter writer)
        {
            return true; // do nothing
        }

        /// <summary>
        /// Reads a data object from XML and returns an instance of the object.
        /// </summary>
        /// <param name="reader">An open XML reader. The reader will be closed by this
        /// method after reading.</param>
        /// <returns>Returns <b>true</b> if successful; <b>false</b> otherwise.</returns>
        /// <remarks>Clients should first create a new instance using a default constructor, and then
        /// call this method to populate the data fields of the default instance.</remarks>
        public bool ReadFromXml(XmlTextReader reader)
        {
            reader.Read(); // <Trial>
            if (reader.Name != "Trial")
                throw new XmlException("XML format error: Expected the <Trial> tag.");

            _number = XmlConvert.ToInt32(reader.GetAttribute("number"));
            _practice = XmlConvert.ToBoolean(reader.GetAttribute("practice"));
            //_lastTarget = CircleR.FromString(reader.GetAttribute("lastCircle"));
            //_thisTarget = CircleR.FromString(reader.GetAttribute("thisCircle"));
            _isoCenter = PointR.FromString(reader.GetAttribute("isoCenter"));
            _tInterval = XmlConvert.ToInt64(reader.GetAttribute("MT"));
            _start = TimePointR.FromString(reader.GetAttribute("start"));
            _end = TimePointR.FromString(reader.GetAttribute("end"));

            // read in the movement and add it to the trial
            _movement = new MovementData(this);
            _movement.ReadFromXml(reader);

            reader.Read(); // </Trial>
            if (reader.Name != "Trial" || reader.NodeType != XmlNodeType.EndElement)
                throw new XmlException("XML format error: Expected the </Trial> tag.");

            return true;
        }

        /// <summary>
        /// Performs any analyses on this data object and writes the results to a comma-delimitted
        /// file for subsequent copy-and-pasting into a spreadsheet like Microsoft Excel or SAS JMP.
        /// </summary>
        /// <param name="writer">An open stream writer pointed to a text file. The writer will be closed 
        /// by this method after writing.</param>
        /// <returns>True if writing is successful; false otherwise.</returns>
        public bool WriteResultsToTxt(StreamWriter writer)
        {
            return true; // do nothing
        }

        #endregion

    }
}

