namespace Assignment.Infrastructure.Tests;

using static Assignment.Core.Response;
public class TagRepositoryTests
{
    private readonly TagRepository _repo;
    private readonly KanbanContext _context;
    public TagRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<KanbanContext>()
                          .UseSqlite(connection)
                          .Options;
        var context = new KanbanContext(options);

        context.Database.EnsureCreated();

        _context = context;
        _repo = new TagRepository(_context);
    }

    [Fact]
    public void Update_non_existing_tag(){
        // Arrange
        var toUpdate = new TagUpdateDTO(20000, "Maths");

        // Act
        var response = _repo.Update(toUpdate);

        // Assert
        response.Should().Be(NotFound);
    }

    [Fact]
    public void Update_existing_tag()
    {
        // Arrange
        var newTag = new Tag("Maths");  
        _context.Tags.Add(newTag);
        _context.SaveChanges();  

        var tagUpdate = new TagUpdateDTO(1, "Maths >:(");

        // Act
        var response = _repo.Update(tagUpdate);

        // Assert
        response.Should().Be(Updated);
    }

    [Fact]
    public void Delete_non_existing_tag()
    {
        // Arrange

        // Act
        var response = _repo.Delete(20000);

        // Assert
        response.Should().Be(NotFound);
    }

    [Fact]
    public void Delete_tag_assigned_to_workItem_without_force_returns_conflict()
    {
        // Arrange
        Tag tag = new Tag("Maths");
        WorkItem wi = new WorkItem("Do Maths");
        tag.WorkItems.Add(wi);
        wi.Tags.Add(tag);
        _context.Tags.Add(tag);
        _context.Items.Add(wi);
        _context.SaveChanges();

        // Act
        var response = _repo.Delete(tag.Id);

        // Assert
        response.Should().Be(Conflict);
        _context.Tags.Count().Should().Be(1);
    }

    [Fact]
    public void Delete_tag_assigned_to_workItem_with_force_returns_deleted()
    {
        // Arrange
        Tag tag = new Tag("Maths");
        WorkItem wi = new WorkItem("Do Maths");
        tag.WorkItems.Add(wi);
        wi.Tags.Add(tag);
        _context.Tags.Add(tag);
        _context.Items.Add(wi);
        _context.SaveChanges();

        // Act
        var response = _repo.Delete(tag.Id, true);

        // Assert
        response.Should().Be(Deleted);
        _context.Tags.Count().Should().Be(0);
    }

    [Fact]
    public void Read_tag_get_response_null(){
        // Arrange

        // Act
        var tagList = _repo.Read();

        // Assert
        Assert.True(tagList.Count() == 0);
    }

    [Fact]
    public void Read_tag_get_response_not_null(){

        // Arrange
        Tag tag = new Tag("Maths");
        WorkItem wi = new WorkItem("Do Maths");
        tag.WorkItems.Add(wi);
        wi.Tags.Add(tag);
        _context.Tags.Add(tag);
        _context.Items.Add(wi);
        _context.SaveChanges();

        // Act
        var tagList = _repo.Read();

        // Assert
        Assert.True(tagList.Count() == 1);
    }
    
    [Fact]
    public void Tag_not_found() {
        // Arrange
        
        // Act
        var tag = _repo.Find(100);

        // Assert
        Assert.Null(tag);
    }

    [Fact]
    public void Tag_found() {
        // Arrange
        Tag tag = new Tag("Maths");
        WorkItem wi = new WorkItem("Do Maths");
        tag.WorkItems.Add(wi);
        wi.Tags.Add(tag);
        _context.Tags.Add(tag);
        _context.Items.Add(wi);
        _context.SaveChanges();
        
        // Act
        var tagTest = _repo.Find(1);

        // Assert()
        Assert.Equal(new TagDTO(tag.Id, tag.Name), tagTest);
    }

    [Fact]
    public void Create_Tag_returns_created()
    {
        // Arrange
        var newTagDTO = new TagCreateDTO("BDSA - Assignment");

        // Act
        var (response, newTagId) = _repo.Create(newTagDTO);

        // Assert
        Assert.Equal(Created, response);
        Assert.Equal(1, newTagId);
    }

    [Fact]
    public void Create_Tag_returns_conflict()
    {
        // Arrange
        var tagName = "BDSA - Assignment04";
        Tag t = new Tag(tagName);
        _context.Tags.Add(t);
        _context.SaveChanges();
        
        var newTagDTO = new TagCreateDTO(tagName);

        // Act
        var (response, tagId) = _repo.Create(newTagDTO);

        // Assert
        response.Should().Be(Conflict);
        tagId.Should().Be(1);
    }
}
