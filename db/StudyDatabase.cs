using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace TaskMain
{
    /// <summary>
    /// 공부 기록을 저장하는 SQLite 데이터베이스.
    /// 파일 위치: %LocalAppData%\StudyCafe\study.db
    /// </summary>
    internal class StudyDatabase
    {
        /* 테이블 구조를 바꾸면 이 숫자를 올리고 Migrate()에 변경 내용을 추가한다 */
        private const int SchemaVersion = 1;

        /* 날짜는 이 형식의 문자열로 저장한다. 문자열 비교로 기간 검색이 된다 */
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        private readonly string connectionString;

        public string FilePath { get; }

        public StudyDatabase() : this(DefaultPath())
        {
        }

        public StudyDatabase(string filePath)
        {
            FilePath = filePath;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = filePath,
                ForeignKeys = true
            }.ToString();
            Migrate();
        }

        public static string DefaultPath()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(folder, "StudyCafe", "study.db");
        }

        private SQLiteConnection Open()
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        /* 처음 실행이면 테이블을 만든다. PRAGMA user_version으로 버전을 기억한다 */
        private void Migrate()
        {
            using (SQLiteConnection connection = Open())
            {
                long version;
                using (SQLiteCommand command = new SQLiteCommand("PRAGMA user_version;", connection))
                {
                    version = (long)command.ExecuteScalar();
                }

                if (version >= SchemaVersion)
                {
                    return;
                }

                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    if (version < 1)
                    {
                        Execute(connection, @"
                            CREATE TABLE IF NOT EXISTS subjects (
                                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                                name       TEXT NOT NULL UNIQUE,
                                created_at TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
                            );

                            CREATE TABLE IF NOT EXISTS study_sessions (
                                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                                subject_id INTEGER NOT NULL REFERENCES subjects(id),
                                started_at TEXT NOT NULL,
                                ended_at   TEXT NOT NULL,
                                seconds    INTEGER NOT NULL
                            );

                            CREATE INDEX IF NOT EXISTS idx_sessions_started
                                ON study_sessions(started_at);");
                    }

                    Execute(connection, "PRAGMA user_version = " + SchemaVersion + ";");
                    transaction.Commit();
                }
            }
        }

        private static void Execute(SQLiteConnection connection, string sql)
        {
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static string Format(DateTime time)
        {
            return time.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        /* 과목 이름으로 id를 찾고, 없으면 새로 만든다 */
        private static long GetSubjectId(SQLiteConnection connection, string subject)
        {
            using (SQLiteCommand insert = new SQLiteCommand(
                "INSERT OR IGNORE INTO subjects(name) VALUES (@name);", connection))
            {
                insert.Parameters.AddWithValue("@name", subject);
                insert.ExecuteNonQuery();
            }
            using (SQLiteCommand select = new SQLiteCommand(
                "SELECT id FROM subjects WHERE name = @name;", connection))
            {
                select.Parameters.AddWithValue("@name", subject);
                return (long)select.ExecuteScalar();
            }
        }

        /// <summary>
        /// 공부한 구간 하나를 저장한다. 자정을 넘긴 구간은 날짜별로 나눠 저장한다.
        /// </summary>
        public void SaveSession(string subject, DateTime start, DateTime end)
        {
            if (end <= start)
            {
                return;
            }

            using (SQLiteConnection connection = Open())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                long subjectId = GetSubjectId(connection, subject);
                /* 날짜별로 나눠도 합계가 전체 초와 같도록 시작부터의 누적 초 차이로 계산한다 */
                DateTime partStart = start;
                int secondsSoFar = 0;
                while (partStart < end)
                {
                    DateTime nextMidnight = partStart.Date.AddDays(1);
                    DateTime partEnd = end < nextMidnight ? end : nextMidnight;
                    int secondsToEnd = (int)Math.Round((partEnd - start).TotalSeconds);
                    int seconds = secondsToEnd - secondsSoFar;
                    secondsSoFar = secondsToEnd;

                    if (seconds > 0)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(@"
                            INSERT INTO study_sessions(subject_id, started_at, ended_at, seconds)
                            VALUES (@subject, @start, @end, @seconds);", connection))
                        {
                            command.Parameters.AddWithValue("@subject", subjectId);
                            command.Parameters.AddWithValue("@start", Format(partStart));
                            command.Parameters.AddWithValue("@end", Format(partEnd));
                            command.Parameters.AddWithValue("@seconds", seconds);
                            command.ExecuteNonQuery();
                        }
                    }
                    partStart = partEnd;
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// 해당 날짜에 공부한 시간(초)의 합.
        /// </summary>
        public int GetSecondsOnDate(DateTime date)
        {
            using (SQLiteConnection connection = Open())
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT COALESCE(SUM(seconds), 0) FROM study_sessions
                WHERE started_at >= @from AND started_at < @to;", connection))
            {
                command.Parameters.AddWithValue("@from", Format(date.Date));
                command.Parameters.AddWithValue("@to", Format(date.Date.AddDays(1)));
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// date가 속한 주(월~일)의 요일별 공부 시간(초). [0]이 월요일.
        /// </summary>
        public int[] GetWeekSeconds(DateTime date)
        {
            DateTime monday = WeekStart(date);
            int[] result = new int[7];

            using (SQLiteConnection connection = Open())
            using (SQLiteCommand command = new SQLiteCommand(@"
                SELECT substr(started_at, 1, 10) AS day, SUM(seconds)
                FROM study_sessions
                WHERE started_at >= @from AND started_at < @to
                GROUP BY day;", connection))
            {
                command.Parameters.AddWithValue("@from", Format(monday));
                command.Parameters.AddWithValue("@to", Format(monday.AddDays(7)));
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime day = DateTime.ParseExact(
                            reader.GetString(0), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        int index = (int)(day - monday).TotalDays;
                        if (index >= 0 && index < 7)
                        {
                            result[index] = Convert.ToInt32(reader.GetValue(1));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// date가 속한 주의 월요일 0시.
        /// </summary>
        public static DateTime WeekStart(DateTime date)
        {
            int fromMonday = ((int)date.DayOfWeek + 6) % 7;
            return date.Date.AddDays(-fromMonday);
        }
    }
}
