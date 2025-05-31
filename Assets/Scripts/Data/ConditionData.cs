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
using System.IO;
using System.Xml;
using TMPro;
using UnityEngine;

namespace MouseLog
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

    public class ConditionData: IXmlLoggable
    {
        #region Fields
        private int _block; // the index number of the block to which this condition belongs
        private int _index; // the index number of this condition
        private bool _circular; // circular ISO 9241-9 or vertical ribbons
        private int _a; // the nominal movement amplitude for this condition
        private int _w; // the nominal movement width for this condition
        private double _mtpct; // the percent of the predicted Fitts' movement time to make the normative time
        private long _mtpred; // the predicted movement time in milliseconds for this trial
        private long _tAppeared; // the millisecond time-stamp of when this condition appeared to the user
        private List<TrialData> _trials; // the collection of performed trials for this condition

        #endregion

        #region Constructor
        public ConditionData() { }
        public ConditionData(int block, int index, int A, int W, double MTpct, long MTpred
            , int trials, int practice, bool is2D)
        {
            _block = block;
            _index = index;
            _a = A;
            _w = W;
            _circular = is2D;
            _tAppeared = -1L;

            _mtpct = MTpct;
            _mtpred = MTpred;
            long mt = (_mtpct != -1.0) ? (long)(_mtpct * _mtpred) : -1L;

            PointR center = new PointR((double)Screen.width / 2, (double)Screen.height / 2);

            // 원본에서는 1D vs 2D 차이에 따라 달라졌지만 여기선 항상 2D
            List<Target3D> targets = GameManager3D.Instance.targetManager.CreateTargets(index, trials, new ConditionConfig(A, W));

            // Order the targets appropriately according to the ISO 9241-9 standard #########################################
            List<Target3D> orderedTargets = new List<Target3D>(targets.Count + 1);
            if(trials % 2 == 0) // trial 짝수
            {
                int k = trials / 2;
                for (int i = 0; i < k; i ++)
                {
                    int idx1 = i; // 0 ~ k-1
                    int idx2 = i + k; // k ~ 2k-1
                    orderedTargets.Add(targets[idx1]); // 2i-th target
                    orderedTargets.Add(targets[idx2]); // (2i+1)-th target
                }
                orderedTargets.Add(targets[k]); // 마지막 타겟은 k번째 (정 아래방향) 타겟과 동일

            }
            else // trial 홀수
            {
                int k = (trials + 1) / 2;
                for (int i = 0; i < k; i++)
                {
                    int idx1 = i;
                    int idx2 = i + k;
                    if(idx2 == trials) // 마지막 타겟은 첫 번째 타겟과 동일
                        idx2 = 0;
                    orderedTargets.Add(targets[idx1]); // 2i-th target
                    orderedTargets.Add(targets[idx2]); // (2i+1)-th target

                }
            }
                //int k = trials % 2 == 0 ? trials / 2 : (trials + 1) / 2;


                // Create the trial instances that represent the trials with the given targets. This includes
                // creating the special start-area trial at index 0 in the condition representing the starting
                // area that, when clicked, starts the actual trials.
                _trials = new List<TrialData>(trials + 1);
            TrialData td;
            for(int i = 0; i < trials + 1; i++)
            {
                td = new TrialData(i, i <= practice, this, i == 0 ? null : orderedTargets[i - 1], orderedTargets[i], center, mt);
                _trials.Add(td);
            }
        }

        #endregion

        #region Trials
        public TrialData this[int number]
        {
            get
            {
                if (0 <= number && number < _trials.Count)
                {
                    return _trials[number];
                }
                return null;
            }
        }

        /// <summary>
        /// Clears all the trial data currently stored by this condition, if any. Does not clear
        /// the trial condition values that specify the trials.
        /// </summary>
        public void ClearTrialData()
        {
            for (int i = 1; i < _trials.Count; i++)
                _trials[i].ClearData();
        }

        public int NumTrials { get { return _trials.Count - 1; } } // index 0은 시작 trial이므로 제외

        public int NumTestTrials
        {
            get
            {
                int num = 0;
                for (int i = 1; i < _trials.Count; i++) // start past the start area trial
                {
                    if (!_trials[i].IsPractice)
                        num++;
                }
                return num;
            }
        }

        /// <summary>
        /// Gets the number of completed trials in this condition. This does not include the special start 
        /// area trial occupying index 0.
        /// </summary>
        public int NumCompletedTrials
        {
            get
            {
                int num = 0;
                for (int i = 1; i < _trials.Count; i++) // start past the start area trial
                {
                    if (_trials[i].IsComplete)
                        num++;
                }
                return num;
            }
        }

        /// <summary>
        /// Gets the number of completed test trials in this condition. This does not include the 
        /// special start area trial occupying index 0.
        /// </summary>
        public int NumCompletedTestTrials
        {
            get
            {
                int num = 0;
                for (int i = 1; i < _trials.Count; i++) // start past the start area trial
                {
                    if (!_trials[i].IsPractice && _trials[i].IsComplete)
                        num++;
                }
                return num;
            }
        }
        #endregion

        #region Condition Values
        //  해당 region은 원본 코드 원문 100% 재사용

        /// <summary>
        /// Gets the 0-based block index number.
        /// </summary>
        public int Block
        {
            get { return _block; }
        }

        /// <summary>
        /// Gets the 0-based condition index number within its block. Condition at index 0 is a valid condition.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets whether or not the trials in this condition are 2D circles in the
        /// ISO 9241-9 standard pattern, or vertical ribbons in the Fitts' traditional
        /// horizontal 1D task.
        /// </summary>
        public bool Is2D
        {
            get { return _circular; }
        }

        /// <summary>
        /// Gets whether or not this condition used a metronome to govern movement time as an
        /// independent variable.
        /// </summary>
        public bool UsedMetronome
        {
            get { return _mtpct != -1.0; }
        }

        /// <summary>
        /// Gets the nominal movement amplitude for trials in this condition. All trials in this
        /// condition contain targets that exhibit this amplitude.
        /// </summary>
        public int A
        {
            get { return _a; }
        }

        /// <summary>
        /// Gets the nominal target size for trials in this condition. All trials in this
        /// condition contain targets that exhibit this width.
        /// </summary>
        public int W
        {
            get { return _w; }
        }

        /// <summary>
        /// Gets the nominal index of difficulty for this condition. All trials in this
        /// condition contain targets that exhibit this index of difficulty. It has units 
        /// in "bits."
        /// </summary>
        public double ID
        {
            get
            {
                return Math.Log((double)_a / _w + 1.0, 2.0);
            }
        }

        /// <summary>
        /// Gets the movement time percent (MT%) for trials in this condition. This value
        /// is an independent variable just like A or W. It represents the percentage of
        /// the Fitts' law-predicted MT that these trials would require. This predicted 
        /// value is multiplied by MT% to discover the raw movement time. Using MT% rather 
        /// than a raw MT as the independent variable allows MT% to be crossed with any 
        /// number of A's and W's. This value is -1.0 if this condition does not use a
        /// metronome.
        /// </summary>
        /// <example>
        /// Let MT% = 0.90 and MTPred = 2000 ms. The raw MT is therefore 0.90 * 2000 = 1800 ms, or 10% 
        /// faster than the predicted movement time.
        /// </example>
        public double MTPct
        {
            get { return _mtpct; }
        }

        /// <summary>
        /// Gets the Fitts' law-predicted movement time in milliseconds for trials in this condition, 
        /// using the formula MTPred = a + b log2(A/W + 1). The MT% independent variable is multiplied 
        /// by this value to compute the raw normative movement time that is used by the metronome.
        /// This value is -1L if this condition does not use a metronome.
        /// </summary>
        /// <example>
        /// Let MT% = 0.90 and MTPred = 2000 ms. The raw MT is therefore 0.90 * 2000 = 1800 ms, or 10% 
        /// faster than the predicted movement time.
        /// </example>
        public long MTPred
        {
            get { return _mtpred; }
        }

        /// <summary>
        /// Gets the normative raw movement time in milliseconds for trials in this condition, 
        /// or -1L if this condition does not use a metronome.
        /// </summary>
        /// <example>
        /// Let MT% = 0.90 and MTPred = 2000 ms. The raw MT is therefore 0.90 * 2000 = 1800 ms, or 10% 
        /// faster than the predicted movement time.
        /// </example>
        public long MT
        {
            get
            {
                if (_mtpct != -1.0 && _mtpred != 1L)
                {
                    return (long)(_mtpct * _mtpred);
                }
                return -1L;
            }
        }

        #endregion

        #region Measured Values
        /// <summary>
        /// Gets the effective movement amplitude for completed trials in this condition. The
        /// effective movement amplitude is the average of the actual movement distances 
        /// of the trials in this condition.
        /// </summary>
        /// <remarks>Spatial outliers are excluded from this calculation so that the
        /// measure remains consistent with its counterpart, We.</remarks>
        public double GetAe(bool bivariate)
        {
            int trials = 0;
            double ae = 0.0;

            if (bivariate && !_circular)
                return 0.0;

            for (int i = 1; i < _trials.Count; i++) // start beyond the special start-area trial
            {
                if (!_trials[i].IsPractice && _trials[i].IsComplete && !_trials[i].IsSpatialOutlier)
                {
                    ae += _trials[i].GetAe(bivariate);
                    trials++;
                }
            }
            return (trials > 0) ? ae / trials : 0.0;
        }
        /// <summary>
        /// Computes the endpoint deviation of the trials in this condition. For ribbon targets, the univariate
        /// result is the standard deviation of x-coordinates. The bivariate result is undefined. For circle 
        /// targets, the univariate result is the standard deviation of x-coordinates. The bivariate result is
        /// the deviation of points around the centroid.
        /// </summary>
        /// <param name="bivariate">If true, a result considering deviation in both x- and y-dimensions is given.
        /// If false, only the deviation in x, defined as "along the task axis," is given.</param>
        /// <returns>The endpoint deviation for normalized trials in this condition.</returns>
        /// <remarks>Spatial outliers are excluded from this calculation so that the
        /// measure remains consistent with its counterpart, We.</remarks>
        public double GetSD(bool bivariate)
        {
            double stdev = 0.0;

            // compute the normalized endpoint locations for each trial in this condition
            List<PointR> pts = new List<PointR>(_trials.Count);
            for (int i = 0; i < _trials.Count; i++)
            {
                if (!_trials[i].IsPractice && _trials[i].IsComplete && !_trials[i].IsSpatialOutlier)
                {
                    pts.Add(_trials[i].NormalizedEnd); // normalized selection endpoints
                }
            }

            // compute the desired endpoint deviation
            if (pts.Count > 0)
            {
                if (bivariate)
                {
                    stdev = _circular ? StatsEx.StdDev2D(pts) : 0.0; // 0.0 is for ribbon task--no bivariate deviation
                }
                else
                {
                    double[] dxs = new double[pts.Count];
                    for (int j = 0; j < pts.Count; j++)
                        dxs[j] = pts[j].X;
                    stdev = StatsEx.StdDev(dxs); // stdev in x-dimension only
                }
            }
            return stdev;
        }

        /// <summary>
        /// Compues Crossman's (1957) effective target width, which reflects the endpoint deviation
        /// around the target and is used to normalize for subjects' speed-accuracy biases.
        /// </summary>
        /// <param name="bivariate">If true, a result considering deviation in both x- and y-dimensions is given.
        /// If false, only the deviation in x, defined as "along the task axis," is given.</param>
        /// <returns>The effective target width based on the endpoint distribution for this condition.</returns>
        public double GetWe(bool bivariate)
        {
            return (_circular || !bivariate) ? 4.133 * this.GetSD(bivariate) : 0.0;
        }

        /// <summary>
        /// Gets the effective index of difficulty based on trials and movement times in this
        /// condition.
        /// </summary>
        /// <param name="bivariate">If true, a result considering deviation in both x- and y-dimensions is given.
        /// If false, only the deviation in x, defined as "along the task axis," is given.</param>
        /// <returns>The effective difficulty of this task based on target utilization.</returns>
        public double GetIDe(bool bivariate)
        {
            double ae = this.GetAe(bivariate);
            double we = this.GetWe(bivariate);
            if (ae > 0.0 && we > 0.0)
            {
                return Math.Log(ae / we + 1.0, 2.0);
            }
            return 0.0;
        }

        /// <summary>
        /// Gets the Fitts' law throughput based on the speed and accuracy of trials in this condition.
        /// </summary>
        /// <param name="bivariate">If true, a result considering deviation in both x- and y-dimensions is given.
        /// If false, only the deviation in x, defined as "along the task axis," is given.</param>
        /// <returns>The throughput for this condition in bits/s.</returns>
        public double GetTP(bool bivariate)
        {
            long mte = this.GetMTe(ExcludeOutliersType.Spatial);
            if (mte != 0L)
            {
                double sec = mte / 1000.0;
                return (this.GetIDe(bivariate) / sec);
            }
            return 0.0;
        }

        /// <summary>
        /// Gets the average movement time in milliseconds of completed trials in this condition.
        /// </summary>
        /// <param name="ex">Indicates which types of outliers, spatial or temporal (or both) are excluded.</param>
        /// <returns>The effective movement time in milliseconds.</returns>
        public long GetMTe(ExcludeOutliersType ex)
        {
            int trials = 0;
            long mte = 0L;

            for (int i = 1; i < _trials.Count; i++) // start beyond the special start-area trial
            {
                if (!_trials[i].IsPractice && _trials[i].IsComplete
                    && ((ex & ExcludeOutliersType.Spatial) == 0 || !_trials[i].IsSpatialOutlier)
                    && ((ex & ExcludeOutliersType.Temporal) == 0 || !_trials[i].IsTemporalOutlier))
                {
                    mte += _trials[i].MTe;
                    trials++;
                }
            }
            return (trials > 0) ? mte / trials : 0L;
        }

        /// <summary>
        /// Gets the number of completed error trials in this condition.
        /// </summary>
        /// <param name="ex">Indicates which types of outliers, spatial or temporal (or both) are excluded.</param>
        /// <returns>The number of error trials in this condition.</returns>
        public int GetNumErrors(ExcludeOutliersType ex)
        {
            int errors = 0;
            for (int i = 1; i < _trials.Count; i++) // start beyond the special start area trial
            {
                if (!_trials[i].IsPractice && _trials[i].IsComplete
                    && ((ex & ExcludeOutliersType.Spatial) == 0 || !_trials[i].IsSpatialOutlier)
                    && ((ex & ExcludeOutliersType.Temporal) == 0 || !_trials[i].IsTemporalOutlier))
                {
                    if (_trials[i].IsError)
                    {
                        errors++;
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Gets the rate of errors among completed trials in this condition. The range is 0.0 to 1.00. If 
        /// outliers are excluded, this is the rate of errors among only those trials that are not outliers.
        /// </summary>
        /// <param name="ex">Indicates which types of outliers, spatial or temporal (or both) are excluded.</param>
        /// <returns>The error rate for trials in this condition.</returns>
        public double GetErrorRate(ExcludeOutliersType ex)
        {
            int errors = 0;
            int trials = 0;

            for (int i = 1; i < _trials.Count; i++) // start beyond the special start area trial
            {
                if (!_trials[i].IsPractice && _trials[i].IsComplete
                    && ((ex & ExcludeOutliersType.Spatial) == 0 || !_trials[i].IsSpatialOutlier)
                    && ((ex & ExcludeOutliersType.Temporal) == 0 || !_trials[i].IsTemporalOutlier))
                {
                    if (_trials[i].IsError)
                    {
                        errors++;
                    }
                    trials++;
                }
            }
            return (trials > 0) ? (double)errors / trials : 0.0;
        }

        /// <summary>
        /// Gets or sets the millisecond time-stamp indicating the time when this condition was
        /// presented to the user.
        /// </summary>
        /// <remarks>
        /// This value is not logged or used in condition-level calculations in any way. It is provided
        /// for convenience for user interface clients (e.g., MainForm).
        /// </remarks>
        internal long TAppeared
        {
            get
            {
                return _tAppeared;
            }
            set
            {
                _tAppeared = value;
            }
        }
        #endregion

        #region Outliers

        /// <summary>
        /// Counts the number of completed spatial outlier trials in this condition.
        /// </summary>
        public int NumSpatialOutliers
        {
            get
            {
                int outliers = 0;
                for (int i = 1; i < _trials.Count; i++) // start beyond the special start area trial
                {
                    if (!_trials[i].IsPractice && _trials[i].IsComplete && _trials[i].IsSpatialOutlier)
                    {
                        outliers++;
                    }
                }
                return outliers;
            }
        }

        /// <summary>
        /// Counts the number of completed temporal outlier trials in this condition.
        /// </summary>
        public int NumTemporalOutliers
        {
            get
            {
                int outliers = 0;
                for (int i = 1; i < _trials.Count; i++) // start beyond the special start area trial
                {
                    if (!_trials[i].IsPractice && _trials[i].IsComplete && _trials[i].IsTemporalOutlier)
                    {
                        outliers++;
                    }
                }
                return outliers;
            }
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
            writer.WriteStartElement("Condition");
            writer.WriteAttributeString("block", XmlConvert.ToString(_block));
            writer.WriteAttributeString("index", XmlConvert.ToString(_index));
            writer.WriteAttributeString("circular", XmlConvert.ToString(_circular));
            writer.WriteAttributeString("metronome", XmlConvert.ToString(this.UsedMetronome));
            writer.WriteAttributeString("trials", XmlConvert.ToString(this.NumTrials));
            writer.WriteAttributeString("completed", XmlConvert.ToString(this.NumCompletedTrials));
            writer.WriteAttributeString("practice", XmlConvert.ToString(this.NumTrials - this.NumTestTrials));
            writer.WriteAttributeString("test", XmlConvert.ToString(this.NumTestTrials));

            writer.WriteAttributeString("MTPct", XmlConvert.ToString(_mtpct));
            writer.WriteAttributeString("MTPred", XmlConvert.ToString(_mtpred));
            writer.WriteAttributeString("MT", XmlConvert.ToString(this.MT));

            writer.WriteAttributeString("A", XmlConvert.ToString(_a));
            writer.WriteAttributeString("W", XmlConvert.ToString(_w));
            writer.WriteAttributeString("ID", XmlConvert.ToString(Math.Round(this.ID, 4)));

            writer.WriteAttributeString("Ae_1d", XmlConvert.ToString(Math.Round(this.GetAe(false), 4)));
            writer.WriteAttributeString("SD_1d", XmlConvert.ToString(Math.Round(this.GetSD(false), 4)));
            writer.WriteAttributeString("We_1d", XmlConvert.ToString(Math.Round(this.GetWe(false), 4)));
            writer.WriteAttributeString("IDe_1d", XmlConvert.ToString(Math.Round(this.GetIDe(false), 4)));
            writer.WriteAttributeString("TP_1d", XmlConvert.ToString(Math.Round(this.GetTP(false), 4)));

            writer.WriteAttributeString("Ae_2d", XmlConvert.ToString(Math.Round(this.GetAe(true), 4)));
            writer.WriteAttributeString("SD_2d", XmlConvert.ToString(Math.Round(this.GetSD(true), 4)));
            writer.WriteAttributeString("We_2d", XmlConvert.ToString(Math.Round(this.GetWe(true), 4)));
            writer.WriteAttributeString("IDe_2d", XmlConvert.ToString(Math.Round(this.GetIDe(true), 4)));
            writer.WriteAttributeString("TP_2d", XmlConvert.ToString(Math.Round(this.GetTP(true), 4)));

            writer.WriteAttributeString("MTe", XmlConvert.ToString(this.GetMTe(ExcludeOutliersType.None)));
            writer.WriteAttributeString("MTe_sx", XmlConvert.ToString(this.GetMTe(ExcludeOutliersType.Spatial)));
            writer.WriteAttributeString("MTe_tx", XmlConvert.ToString(this.GetMTe(ExcludeOutliersType.Temporal)));

            writer.WriteAttributeString("errors", XmlConvert.ToString(this.GetNumErrors(ExcludeOutliersType.None)));
            writer.WriteAttributeString("errors_sx", XmlConvert.ToString(this.GetNumErrors(ExcludeOutliersType.Spatial)));
            writer.WriteAttributeString("errors_tx", XmlConvert.ToString(this.GetNumErrors(ExcludeOutliersType.Temporal)));
            writer.WriteAttributeString("errorPct", XmlConvert.ToString(Math.Round(GetErrorRate(ExcludeOutliersType.None), 4)));
            writer.WriteAttributeString("errorPct_sx", XmlConvert.ToString(Math.Round(GetErrorRate(ExcludeOutliersType.Spatial), 4)));
            writer.WriteAttributeString("errorPct_tx", XmlConvert.ToString(Math.Round(GetErrorRate(ExcludeOutliersType.Temporal), 4)));

            writer.WriteAttributeString("spatialOutliers", XmlConvert.ToString(this.NumSpatialOutliers));
            writer.WriteAttributeString("temporalOutliers", XmlConvert.ToString(this.NumTemporalOutliers));

            // write each trial out in turn. do not write out the special start-area trial at index 0.
            for (int i = 1; i < _trials.Count; i++)
            {
                TrialData td = _trials[i];
                td.WriteXmlHeader(writer);
            }

            writer.WriteEndElement(); // </Condition>

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
            reader.Read(); // <Condition>
            if (reader.Name != "Condition")
                throw new XmlException("XML format error: Expected the <Condition> tag.");

            _block = XmlConvert.ToInt32(reader.GetAttribute("block"));
            _index = XmlConvert.ToInt32(reader.GetAttribute("index"));
            _circular = XmlConvert.ToBoolean(reader.GetAttribute("circular"));
            int trials = XmlConvert.ToInt32(reader.GetAttribute("trials"));
            _mtpct = XmlConvert.ToDouble(reader.GetAttribute("MTPct"));
            _mtpred = XmlConvert.ToInt64(reader.GetAttribute("MTPred"));
            _a = XmlConvert.ToInt32(reader.GetAttribute("A"));
            _w = XmlConvert.ToInt32(reader.GetAttribute("W"));

            // read in the trials and add them to the condition
            _trials = new List<TrialData>(trials + 1); // include slot for special start-area trial
            for (int i = 1; i <= trials; i++)
            {
                TrialData td = new TrialData();
                if (!td.ReadFromXml(reader))
                    throw new XmlException("Failed to read the TrialData2D.");
                if (i == 1)
                {
                    TrialData sa = TrialData.CreateStartTarget((TrialData)td);
                    _trials.Add(sa); // add special start-area trial
                }

                // now add the trial just read in
                _trials.Add(td);
            }

            reader.Read(); // </Condition>
            if (reader.Name != "Condition" || reader.NodeType != XmlNodeType.EndElement)
                throw new XmlException("XML format error: Expected the </Condition> tag.");

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

