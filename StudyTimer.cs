using System;

namespace TaskMain
{
    /// <summary>
    /// 공부 시간을 재고, 일시정지나 종료할 때 공부한 구간을 DB에 저장한다.
    /// </summary>
    internal class StudyTimer
    {
        private readonly StudyDatabase db;
        private string subject;
        private DateTime segmentStart;  // 지금 재고 있는 구간의 시작 시각
        private int savedSeconds;       // 이번 공부에서 이미 저장한 구간들의 합

        public StudyTimer(StudyDatabase db)
        {
            this.db = db;
        }

        public bool IsRunning { get; private set; }

        public void Start(string subject)
        {
            if (IsRunning)
            {
                return;
            }
            this.subject = subject;
            segmentStart = DateTime.Now;
            IsRunning = true;
        }

        public void Pause()
        {
            if (!IsRunning)
            {
                return;
            }
            IsRunning = false;
            SaveSegment();
        }

        public void Stop()
        {
            Pause();
            savedSeconds = 0;
        }

        /// <summary>
        /// 공부 중에 과목을 바꾸면 지금까지를 이전 과목으로 저장하고 새 구간을 시작한다.
        /// </summary>
        public void ChangeSubject(string newSubject)
        {
            if (IsRunning && newSubject != subject)
            {
                SaveSegment();
                segmentStart = DateTime.Now;
            }
            subject = newSubject;
        }

        /// <summary>
        /// 이번 공부(시작부터 종료까지)의 경과 시간(초).
        /// </summary>
        public int GetElapsedSeconds()
        {
            return savedSeconds + GetUnsavedSecondsSince(DateTime.MinValue);
        }

        /// <summary>
        /// 아직 저장하지 않은 구간 중 from 이후의 시간(초). 오늘 합계 계산에 쓴다.
        /// </summary>
        public int GetUnsavedSecondsSince(DateTime from)
        {
            if (!IsRunning)
            {
                return 0;
            }
            DateTime start = segmentStart > from ? segmentStart : from;
            return Math.Max(0, (int)(DateTime.Now - start).TotalSeconds);
        }

        private void SaveSegment()
        {
            DateTime end = DateTime.Now;
            savedSeconds += (int)(end - segmentStart).TotalSeconds;
            db.SaveSession(subject, segmentStart, end);
        }
    }
}
