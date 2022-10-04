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

    public TagDTO? Find(int tagId)
    {
        var tagToFind = _context.Tags.FirstOrDefault(t => t.Id == tagId);
        
        return tagToFind is null ? null : new TagDTO(tagToFind.Id, tagToFind.Name);
    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        var tags = from t in _context.Tags
            orderby t.Id
            select new TagDTO(t.Id, t.Name);
            
        return tags.ToArray();
    }

    public Response Update(TagUpdateDTO tag)
    {
        var tagToUpdate = _context.Tags.FirstOrDefault(t => t.Id == tag.Id);
        
        if (tagToUpdate is null) 
        {
            return NotFound;
        }

        tagToUpdate.Name = tag.Name;
        _context.SaveChanges();

        return Updated;
    }
}
