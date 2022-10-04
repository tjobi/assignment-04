namespace Assignment.Infrastructure;

using static State;
public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
        throw new NotImplementedException();
    }

    public Response Delete(int itemId)
    {
        throw new NotImplementedException();
    }

    public WorkItemDetailsDTO Find(int itemId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> Read()
        => _context.Items.Select(x =>
                            new WorkItemDTO(
                                x.Id, x.Title, x.AssignedTo!.Name,
                                x.Tags.Select(x => x.Name).ToArray(),
                                x.State))
                         .ToArray();

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
        => _context.Items.Where(wi => wi.State == state)
                         .Select(x =>
                            new WorkItemDTO(
                                x.Id, x.Title, x.AssignedTo!.Name,
                                x.Tags.Select(x => x.Name).ToArray(),
                                x.State))
                         .ToArray();

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
        => _context.Items.Where(wi => wi.Tags.Any(t => t.Name == tag))
                         .Select(x =>
                            new WorkItemDTO(
                                x.Id, x.Title, x.AssignedTo!.Name,
                                x.Tags.Select(x => x.Name).ToArray(),
                                x.State))
                         .ToArray();

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
        => _context.Items.Where(wi => wi.AssignedTo != null && wi.AssignedTo.Id == userId)
                         .Select(x =>
                            new WorkItemDTO(
                                x.Id, x.Title, x.AssignedTo!.Name,
                                x.Tags.Select(x => x.Name).ToArray(),
                                x.State))
                         .ToArray();

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
        => _context.Items.Where(wi => wi.State == Removed)
                        .Select(x =>
                            new WorkItemDTO(
                                x.Id, x.Title, x.AssignedTo!.Name,
                                x.Tags.Select(x => x.Name).ToArray(),
                                x.State))
                        .ToArray();

    public Response Update(WorkItemUpdateDTO item)
    {
        throw new NotImplementedException();
    }
}
