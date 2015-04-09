﻿using System;
using System.Collections.Generic;
using System.Text;
using System.util;
using System.util.collections;
using iTextSharp.awt.geom;

namespace iTextSharp.text.pdf.parser {

    /**
     * As subpath is a part of a path comprising a sequence of connected segments.
     *
     * @since 5.5.6
     */
    public class Subpath {

        private Point2D startPoint;
        private IList<IShape> segments = new List<IShape>();
        private bool closed;

        public Subpath() {
        }

        public Subpath(Subpath subpath) {
            this.startPoint = subpath.startPoint;
            Util.AddAll(this.segments, subpath.GetSegments());
            this.closed = subpath.closed;
        }

        /**
         * Constructs a new subpath starting at the given point.
         */
        public Subpath(Point2D startPoint) : this((float) startPoint.GetX(), (float) startPoint.GetY()) {
        }

        /**
         * Constructs a new subpath starting at the given point.
         */
        public Subpath(float startPointX, float startPointY) {
            this.startPoint = new Point2D.Float(startPointX, startPointY);
        }

        public virtual void SetStartPoint(Point2D startPoint) {
            SetStartPoint((float) startPoint.GetX(), (float) startPoint.GetY());
        }

        public virtual void SetStartPoint(float x, float y) {
            this.startPoint = new Point2D.Float(x, y);
        }

        public virtual Point2D GetStartPoint() {
            return startPoint;
        }

        public virtual Point2D GetLastPoint() {
            Point2D lastPoint = null;

            if (segments.Count > 0) {
                if (closed) {
                    IShape shape = segments[0];
                    lastPoint = shape.GetBasePoints()[0];
                } else {
                    IShape shape = segments[segments.Count - 1];
                    lastPoint = shape.GetBasePoints()[shape.GetBasePoints().Count - 1];
                }
            }

            return lastPoint;
        }

        public virtual void AddSegment(IShape segment) {
            if (closed) {
                return;
            }

            if (IsSinglePointOpen()) {
                startPoint = segment.GetBasePoints()[0];
            }

            segments.Add(segment);
        }

        public virtual IList<IShape> GetSegments() {
            return segments;
        }

        public virtual bool IsEmpty() {
            return startPoint == null;
        }

        /**
         * @return <CODE>true</CODE> if this subpath contains only one point and it is not closed,
         *         <CODE>false</CODE> otherwise
         */
        public virtual bool IsSinglePointOpen() {
            return segments.Count == 0 && !closed;
        }

        public virtual bool IsSinglePointClosed() {
            return segments.Count == 0 && closed;
        }

        /**
         * Returns or sets a <CODE>bool</CODE> value indicating whether the subpath must be closed or not.
         * Ignore this value if the subpath is a rectangle because in this case it is already closed
         * (of course if you paint the path using <CODE>re</CODE> operator)
         *
         * @return <CODE>bool</CODE> value indicating whether the path must be closed or not.
         * @since 5.5.6
         */

        public virtual bool Closed {
            get { return closed; }
            set { closed = value; }
        }

        /**
         * Returns a <CODE>bool</CODE> indicating whether the subpath is degenerate or not.
         * A degenerate subpath is the subpath consisting of a single-point closed path or of
         * two or more points at the same coordinates.
         *
         * @return <CODE>bool</CODE> value indicating whether the path is degenerate or not.
         * @since 5.5.6
         */
        public virtual bool IsDegenerate() {
            if (segments.Count > 0 && closed) {
                return false;
            }

            foreach (IShape segment in segments) {
                HashSet2<Point2D> points = new HashSet2<Point2D>(segment.GetBasePoints());

                // The first segment of a subpath always starts at startPoint, so...
                if (points.Count != 1) {
                    return false;
                }
            }

            // the second clause is for case when we have single point
            return segments.Count > 0 || closed;
        }

        public virtual IList<Point2D> GetPiecewiseLinearApproximation() {
            IList<Point2D> result = new List<Point2D>();

            foreach (IShape segment in segments) {
                if (segment is Line) {
                    Util.AddAll(result, segment.GetBasePoints());
                } else {
                    Util.AddAll(result, ((BezierCurve) segment).GetPiecewiseLinearApproximation());
                }
            }

            return result;
        }
    }
}
