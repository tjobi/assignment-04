namespace Assignment.Infrastructure.Tests;

using static Response;
using static State;
public class WorkItemRepositoryTests
{
    private readonly KanbanContext _context;
    private readonly WorkItemRepository _repo;

    public WorkItemRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<KanbanContext>()
                          .UseSqlite(connection)
                          .Options;
        var context = new KanbanContext(options);

        context.Database.EnsureCreated();

        _context = context;
        _repo = new WorkItemRepository(_context);
    }

    [Fact]
    public void Updating_non_existing_work_item_returns_NotFound() 
    {
        // Arrange
        var updateDTO = new WorkItemUpdateDTO(0, "fake", null, null, new List<string>(), New);

        // Act
        var result = _repo.Update(updateDTO);

        // Assert
        Assert.Equal(NotFound, result);
    }

    [Fact]
    public void Deleting_non_existing_work_item_returns_NotFound() 
    {
        // Arrange
        var deleteId = 0;

        // Act
        var result = _repo.Delete(deleteId);

        // Assert
        Assert.Equal(NotFound, result);
    }

    [Fact]
    public void Creating_a_work_item_returns_Created() 
    {
        // Arrange
        var createDTO = new WorkItemCreateDTO("new", null, null, new List<string>());

        // Act
        var (result, _) = _repo.Create(createDTO);

        // Assert
        Assert.Equal(Created, result);
    }

    [Fact]
    public void Assigning_non_existing_user_returns_BadRequest()
    {
        // Arrange
        var wi = new WorkItem("Do Maths");
        _context.Items.Add(wi);
        _context.SaveChanges();

        var updateWiDTO = new WorkItemUpdateDTO(
            1, "Do Maths", 1, null, new List<string>(), Active 
        );

        // Act
        var reponse = _repo.Update(updateWiDTO);

        // Assert
        reponse.Should().Be(BadRequest);
    }

    [Fact]
    public void Read_returns_empty()
    {
        // Arrange

        // Act
        var wiColl = _repo.Read();

        // Assert
        wiColl.Should().BeEmpty();
    }

    [Fact]
    public void Read_returns_non_empty()
    {
        // Arrange
        WorkItem[] items = {
            new WorkItem("Do Maths"),
            new WorkItem("Do more Maths"),
            new WorkItem("Do even more Maths")
        };
        _context.Items.AddRange(items);
        _context.SaveChanges();

        // Act
        var wiColl = _repo.Read();

        // Assert
        wiColl.Should().NotBeEmpty();
        wiColl.Count.Should().Be(3);
    }

    [Fact]
    public void Read_by_state_removed_returns_2_workitems()
    {
        // Arrange
        WorkItem[] items = {
            new WorkItem("Do Maths"){State = New},
            new WorkItem("Do more Maths"){State = Active},
            new WorkItem("Do even more Maths"){State = Active}
        };
        _context.Items.AddRange(items);
        _context.SaveChanges();

        // Act
        var wiColl = _repo.ReadByState(Active);

        // Assert
        wiColl.Should().NotBeEmpty();
        wiColl.Count.Should().Be(2);
    }

    [Fact]
    public void Read_removed_returns_2_elements()    
    {
        // Arrange
        WorkItem[] items = {
            new WorkItem("Do Maths"){State = New},
            new WorkItem("Do more Maths"){State = Removed},
            new WorkItem("Do even more Maths"){State = Removed}
        };
        _context.Items.AddRange(items);
        _context.SaveChanges();

        // Act
        var wiColl = _repo.ReadRemoved();

        // Assert
        wiColl.Should().NotBeEmpty();
        wiColl.Count.Should().Be(2);
    }

    [Fact]
    public void Read_by_user_returns_3_elements()    
    {
        // Arrange
        User user = new User("Billy", "Billy@example.com");

        WorkItem w1 = new WorkItem("Do Maths"){AssignedTo = user};
        WorkItem w2 = new WorkItem("Do more Maths"){AssignedTo = user};
        WorkItem w3 = new WorkItem("Do even more Maths"){AssignedTo = user};

        WorkItem[] items = {
            w1,
            w2,
            w3,
            new WorkItem("Do something"),
            new WorkItem("Do something else")
        };
        _context.Users.Add(user);
        _context.Items.AddRange(items);
        _context.SaveChanges();

        // Act
        var wiColl = _repo.ReadByUser(user.Id);

        // Assert
        wiColl.Should().NotBeEmpty();
        wiColl.Count.Should().Be(3);
    }

    public void Read_by_tag_returns_2_elements()    
    {
        // Arrange
        string tagName = "Easy";
        Tag tag = new Tag(tagName);

        WorkItem w1 = new WorkItem("Do Maths");
        WorkItem w2 = new WorkItem("Do more Maths");
        WorkItem w3 = new WorkItem("Do even more Maths");

        w1.Tags.Add(tag);
        w2.Tags.Add(tag);
        tag.WorkItems.Add(w1);
        tag.WorkItems.Add(w2);

        WorkItem[] items = { w1, w2, w3 };
        _context.Tags.Add(tag);
        _context.Items.AddRange(items);
        _context.SaveChanges();

        // Act
        var wiColl = _repo.ReadByTag(tagName);

        // Assert
        wiColl.Should().NotBeEmpty();
        wiColl.Count.Should().Be(2);
    }
}
