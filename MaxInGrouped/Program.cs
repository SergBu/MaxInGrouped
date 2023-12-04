var yearStarted = DateTime.UtcNow.AddYears(-1);

var events = new List<EventType>() { EventType.TerminalInUnloading, EventType.TerminalOutUnloading };

var terminalEvents = await context.TerminalEvents.AsNoTracking()
                     .Where(p => p.DateCreated > yearStarted)
                     .Where(p => events.Contains(p.EventType))
                     .ToListAsync();

var terminalEventsByGateDict = from te in terminalEvents
                               group te by (te.TerminalId, te.GateNumber)
                            into eventsDict
                               select eventsDict;

var terminalGateStates = (from eventsDict in terminalEventsByGateDict
                          select eventsDict.OrderBy(p => p.DateCreated)
                         //.LastOrDefault(p => (p.TerminalId, p.GateNumber) == (eventsDict.Key)))
                         .LastOrDefault())
                            .Select(p => new TerminalGateState { TerminalId = p.TerminalId, GateNumber = p.GateNumber, State = p.EventType.ToString(), UpdatedAt = p.DateCreated });

foreach (var terminalGateState in terminalGateStates)
{
    var entity = await context.TerminalGateStates
    //.SingleOrDefaultAsync(p => ((p.TerminalId, p.GateNumber) == (terminalGateState.TerminalId, terminalGateState.GateNumber)));
    .SingleOrDefaultAsync(p => $"{p.TerminalId}_{p.GateNumber}" == $"{terminalGateState.TerminalId}_{terminalGateState.GateNumber}");

    if (entity is null)
    {
        await context.TerminalGateStates.AddAsync(terminalGateState);
    }
}

await context.SaveChangesAsync();