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
        // Find user
        var assignedTo = _context.Users.Find(item.AssignedToId);

        if (assignedTo is null) 
        {
            return (BadRequest, -1);
        }

        // Find tags
        var tags = new List<Tag>();
        foreach (var tagName in item.Tags)
        {
            var tag = _context.Tags.Where(t => t.Name == tagName).FirstOrDefault();
            if (tag is null)
            {
                tag = new Tag(tagName);
                _context.Tags.Add(tag);
            }
            tags.Add(tag);
        }

        // Create task entity
        var entity = new WorkItem(item.Title);
        entity.AssignedTo = assignedTo;
        entity.Description = item.Description; 
        entity.State = New;
        entity.Tags = tags;
        entity.Created = DateTime.Now; 
        entity.StateUpdated = DateTime.Now;

        // Add task to user
        assignedTo.Items.Add(entity);
        
        // Add task to tags
        tags.ForEach(t => t.WorkItems.Add(entity));

        _context.Items.Add(entity);
        _context.SaveChanges();
        return (Created, entity.Id);
    }

    public Response Delete(int itemId)
    {
        var entity = _context.Items.Find(itemId);
        if (entity is null)
        {
            return NotFound;
        }

        switch (entity.State)
        {
            case New:
                _context.Items.Remove(entity);
                break;
            case Active:
                entity.State = Removed;
                break;
            default:
                return Conflict;
        }

        _context.SaveChanges();
        return Deleted;
    }

    public WorkItemDetailsDTO? Find(int itemId)
    {
        var entity = _context.Items.Find(itemId);
        if (entity is null)
        {
            return null;
        }
        return new WorkItemDetailsDTO(entity.Id, entity.Title, entity.Description ?? string.Empty, entity.Created, entity.AssignedTo?.Name ?? string.Empty, entity.Tags.Select(t => t.Name).ToList().AsReadOnly(), entity.State, entity.StateUpdated);
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
        var entity = _context.Items.Find(item.Id);
        if (entity is null)
        {
            return NotFound;
        }

        // Find user
        var assignedTo = _context.Users.Find(item.AssignedToId);
        if (assignedTo is null) 
        {
            return BadRequest;
        }

        // Find tags
        var tags = new List<Tag>();
        foreach (var tagName in item.Tags)
        {
            var tag = _context.Tags.FirstOrDefault(t => t.Name == tagName);
            if (tag is null)
            {
                tag = new Tag(tagName);
                _context.Tags.Add(tag);
            }
            tags.Add(tag);
        }

        // Unassign previous user
        var oldAssignee = entity.AssignedTo;
        oldAssignee?.Items.Remove(entity);

        // Unassign old tags
        var oldTags = entity.Tags;
        foreach (var oldTag in oldTags) {
            oldTag.WorkItems.Remove(entity);
        }

        // Update properties
        entity.Title = item.Title;
        entity.Description = item.Description;
        entity.AssignedTo = assignedTo;
        entity.Tags = tags;
        entity.State = item.State;
        entity.StateUpdated = DateTime.Now;
        
        // Add task to user
        assignedTo.Items.Add(entity);
        
        // Add task to tags
        tags.ForEach(t => t.WorkItems.Add(entity));

        _context.SaveChanges();
        return Updated;
    }
}
