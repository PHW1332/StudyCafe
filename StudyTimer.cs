using System;

namespace TaskMain
{
    /// <summary>
    /// 공부 시간을 재고, 일시정지나 종료할 때 공부한 구간을 DB에 저장한다.
    /// 타이머에 보이는 초(소수점 버림)를 그대로 저장해서 누적 시간과 타이머가 항상 같게 한다.
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
            SaveSegment(DateTime.Now);
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
                segmentStart = SaveSegment(DateTime.Now);
            }
            subject = newSubject;
        }

        /// <summary>
        /// 이번 공부(시작부터 종료까지)의 경과 시간(초). 타이머에 표시하는 값.
        /// </summary>
        public int GetElapsedSeconds(DateTime now)
        {
            return savedSeconds + GetUnsavedSeconds(now);
        }

        /// <summary>
        /// 아직 저장하지 않은 진행 중 구간의 초. 멈춰 있으면 0.
        /// </summary>
        public int GetUnsavedSeconds(DateTime now)
        {
            return IsRunning ? WholeSecondsSinceSegmentStart(now) : 0;
        }

        /// <summary>
        /// 진행 중 구간 중 오늘(자정 이후)에 해당하는 초. 오늘 누적 시간 계산에 쓴다.
        /// </summary>
        public int GetUnsavedSecondsToday(DateTime now)
        {
            int unsaved = GetUnsavedSeconds(now);
            if (segmentStart >= now.Date)
            {
                return unsaved;
            }
            /* 자정 전에 시작했으면 구간 끝(타이머 기준)에서 자정까지만 센다 */
            DateTime shownEnd = segmentStart.AddSeconds(unsaved);
            return Math.Max(0, (int)(shownEnd - now.Date).TotalSeconds);
        }

        /// <summary>
        /// 타이머에 보인 만큼(초 단위 버림)만 저장하고, 저장한 구간의 끝 시각을 돌려준다.
        /// </summary>
        private DateTime SaveSegment(DateTime now)
        {
            int seconds = WholeSecondsSinceSegmentStart(now);
            DateTime end = segmentStart.AddSeconds(seconds);
            savedSeconds += seconds;
            db.SaveSession(subject, segmentStart, end);
            return end;
        }

        private int WholeSecondsSinceSegmentStart(DateTime now)
        {
            return Math.Max(0, (int)(now - segmentStart).TotalSeconds);
        }
    }
}
