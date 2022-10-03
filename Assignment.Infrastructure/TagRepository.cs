namespace Assignment.Infrastructure;

using static Assignment.Core.Response;
public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;
    
    public TagRepository(KanbanContext context) => _context = context;

    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var newTag = _context.Tags.FirstOrDefault(t => t.Name == t.Name);
        Response response;
    
        if(newTag is null)
        {
            newTag = new Tag(tag.Name);
            _context.Tags.Add(newTag);
            _context.SaveChanges();

            response = Created;
        }
        else 
        {
            response = Conflict;
        }

        return (response, newTag.Id);
    }

    public Response Delete(int tagId, bool force = false)
    {
        var tagToDelete = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        Response response;
        
        if(tagToDelete is null)
        {
            response = NotFound;
        }
        else if(tagToDelete.WorkItems.Any() && !force)
        {
            response = Conflict;
        }
        else {
            _context.Tags.Remove(tagToDelete);
            _context.SaveChanges();

            response = Deleted;
        }

        return response;
    }

    public TagDTO Find(int tagId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        throw new NotImplementedException();
    }

    public Response Update(TagUpdateDTO tag)
    {
        throw new NotImplementedException();
    }
}
