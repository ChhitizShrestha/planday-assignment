namespace Planday.Schedule
{
    public class Shift
    {
        public Shift(long id, long? employeeId, DateTime start, DateTime end)
        {
            Id = id;
            EmployeeId = employeeId;
            Start = start;
            End = end;
        }

        public long Id { get; }
        public long? EmployeeId { get; }
        public DateTime Start { get; }
        public DateTime End { get; }

        public bool IsAssignedToEmployee => EmployeeId.HasValue;
        public bool IsOpenShift => !IsAssignedToEmployee;

        public bool IsOnSameDay => Start.Date == End.Date;

        public bool OverlapsWith(DateTime start, DateTime end)
        {
            return Start < end && End > start;
        }
    }
}