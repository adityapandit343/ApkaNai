namespace CutBook.API.DTOs;

public class JoinQueueDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class QueueEntryResponseDto
{
    public int Id { get; set; }
    public int TokenNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public int Position { get; set; }           // kitne log aage hain
    public int EstimatedWaitMinutes { get; set; } // estimated wait
}

public class QueueStatusDto
{
    public int TotalWaiting { get; set; }
    public int CurrentToken { get; set; }        // abhi kisko serve ho raha hai
    public List<QueueEntryResponseDto> WaitingList { get; set; } = new();
}
