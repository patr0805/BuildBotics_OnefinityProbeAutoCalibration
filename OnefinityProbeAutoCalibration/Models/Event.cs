using System;
using System.Collections.Generic;
using System.Text;

namespace OnefinityProbeAutoCalibration.Models
{
    public class Event
    {
        public delegate void MoveToComplete();
        public event MoveToComplete MoveToCompleted;
        public string id = null;
        public decimal? expectedXMoveTo = null;
        public decimal? expectedYMoveTo = null;
        public decimal? expectedZMoveTo = null;

        public void ExectureComplete()
        {
            MoveToCompleted();
        }
    }
}
