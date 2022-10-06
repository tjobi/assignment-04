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

        var user1 = new User("Sigurd", "shho@itu.dk");
        _context.Users.Add(user1);
        _context.SaveChanges();
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
    public void Updating_exiting_work_item_updates_stateupdated()
    {
        // Arrange
        var wi = new WorkItem("Maths");
        _context.Items.Add(wi);
        _context.SaveChanges();

        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var wiUpdated = new WorkItemUpdateDTO(1, "Maths", user?.Id, null, 
                                              new string[]{}, Closed);

        // Act
        var response = _repo.Update(wiUpdated);

        // Assert
        response.Should().Be(Updated);

        /* Where is the StateUpdated :eyes: */
        //_context.Items.Find(1).StateUpdated.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5))
    }

    [Fact]
    public void Update_workitem_edit_tags()
    {
        // Arrange
        var wi = new WorkItem("Maths");
        var tag = new Tag("Easy");
        _context.Tags.Add(tag);
        _context.Items.Add(wi);
        _context.SaveChanges();

        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var wiUpdated = new WorkItemUpdateDTO(wi.Id, "Maths", user?.Id, null, 
                                              new string[]{ tag.Name }, 
                                              Closed);
        // Act
        var reponse = _repo.Update(wiUpdated);

        // Assert
        reponse.Should().Be(Updated);

        _context.Items.Find(wi.Id)!.Tags.Should().Contain(tag);
    }

    [Fact]
    public void Update_workitem_adds_a_new_tag_to_context()
    {
        /* Hvordan skal 
            "Create/update workItem must allow for editing tags."
           forstås?
        */ 

        // Arrange
        string newTagName = "Easy";

        var wi = new WorkItem("Maths");
        _context.Items.Add(wi);
        _context.SaveChanges();

        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");

        var wiUpdated = new WorkItemUpdateDTO(wi.Id, "Maths", user?.Id, null, 
                                              new string[]{ newTagName }, 
                                              Closed);
        // Act
        var reponse = _repo.Update(wiUpdated);

        // Assert
        reponse.Should().Be(Updated);

        // assuming the update added a new tag to the Tags dbset.
        _context.Items.Find(wi.Id)!.Tags.Should().Contain(t => t.Name == newTagName);
        _context.Tags.Count().Should().Be(1);
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
    public void Deleting_work_item_with_state_New_deletes_it()
    {
        // Arrange
        var wi = new WorkItem("Fish");
        _context.Items.Add(wi);
        _context.SaveChanges();

        // Act
        var entity = _repo.Read().FirstOrDefault(i => i.Title == wi.Title);
        Assert.NotNull(entity);
        _repo.Delete(entity.Id);
        _context.SaveChanges();
        var result = _repo.Find(entity.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Deleting_work_item_with_state_Active_sets_state_to_Removed()
    {
        // Arrange
        var wi = new WorkItem("Drink");
        _context.Items.Add(wi);
        _context.SaveChanges();
        
        var entity = _repo.Read().FirstOrDefault(i => i.Title == wi.Title);
        Assert.NotNull(entity);
        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var updateDTO = new WorkItemUpdateDTO(entity.Id, "Updated title", user?.Id, null, new List<string>(), Active);
        _repo.Update(updateDTO);
        _context.SaveChanges();

        // Act
        _repo.Delete(entity.Id);
        _context.SaveChanges();
        var result = _repo.Find(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Removed, result.State);
    }

    [Fact]
    public void Deleting_work_item_with_state_Resolved_sets_state_to_Conflict()
    {
        // Arrange
        var wi = new WorkItem("Resolved");
        _context.Items.Add(wi);
        _context.SaveChanges();
        
        var entity = _repo.Read().FirstOrDefault(i => i.Title == wi.Title);
        Assert.NotNull(entity);
        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var updateDTO = new WorkItemUpdateDTO(entity.Id, "Updated title", user?.Id, null, new List<string>(), Resolved);
        _repo.Update(updateDTO);
        _context.SaveChanges();

        // Act
        var result = _repo.Delete(entity.Id);

        // Assert
        Assert.Equal(Conflict, result);
    }

    [Fact]
    public void Deleting_work_item_with_state_Closed_sets_state_to_Conflict()
    {
        // Arrange
        var wi = new WorkItem("Closed");
        _context.Items.Add(wi);
        _context.SaveChanges();
        
        var entity = _repo.Read().FirstOrDefault(i => i.Title == wi.Title);
        Assert.NotNull(entity);
        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var updateDTO = new WorkItemUpdateDTO(entity.Id, "Updated title", user?.Id, null, new List<string>(), Closed);
        _repo.Update(updateDTO);
        _context.SaveChanges();

        // Act
        var result = _repo.Delete(entity.Id);

        // Assert
        Assert.Equal(Conflict, result);
    }

    [Fact]
    public void Deleting_work_item_with_state_Removed_sets_state_to_Conflict()
    {
        // Arrange
        var wi = new WorkItem("Removed");
        _context.Items.Add(wi);
        _context.SaveChanges();
        
        var entity = _repo.Read().FirstOrDefault(i => i.Title == wi.Title);
        Assert.NotNull(entity);
        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var updateDTO = new WorkItemUpdateDTO(entity.Id, "Updated title", user?.Id, null, new List<string>(), Removed);
        _repo.Update(updateDTO);
        _context.SaveChanges();

        // Act
        var result = _repo.Delete(entity.Id);

        // Assert
        Assert.Equal(Conflict, result);
    }

    [Fact]
    public void Creating_a_work_item_returns_Created() 
    {
        // Arrange
        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var createDTO = new WorkItemCreateDTO("new", user?.Id, null, new List<string>());

        // Act
        var (result, _) = _repo.Create(createDTO);

        // Assert
        Assert.Equal(Created, result);
    }

    [Fact]
    public void Creating_a_work_item_sets_Created_and_StateUpdated_to_now() 
    {
        // Arrange
        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var createDTO = new WorkItemCreateDTO("created time", user?.Id, null, new List<string>());

        // Act
        _repo.Create(createDTO);
        _context.SaveChanges();
        var entity = _repo.Read().FirstOrDefault(i => i.Title == createDTO.Title);
        Assert.NotNull(entity);
        var entityDetails = _repo.Find(entity.Id);
        
        // Assert
        Assert.NotNull(entityDetails);
        entityDetails.Created.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(1.0));
        entityDetails.StateUpdated.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(1.0));
    }

    [Fact]
    public void Updating_a_work_item_sets_StateUpdated_to_now() 
    {
        // Arrange
        var user = _context.Users.FirstOrDefault(u => u.Name == "Sigurd");
        var createDTO = new WorkItemCreateDTO("updated time", user?.Id, null, new List<string>());
        _repo.Create(createDTO);
        _context.SaveChanges();
        var entity = _repo.Read().FirstOrDefault(i => i.Title == createDTO.Title);
        Assert.NotNull(entity);
        var updateDTO = new WorkItemUpdateDTO(entity.Id, entity.Title, user?.Id, "UPDATE", new List<string>(), Active);

        // Act
        _repo.Update(updateDTO);
        var entityDetails = _repo.Find(entity.Id);

        // Assert
        Assert.NotNull(entityDetails);
        entityDetails.StateUpdated.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(1.0));
    }

    [Fact]
    public void Assigning_non_existing_user_returns_BadRequest()
    {
        // Arrange
        var wi = new WorkItem("Do Maths");
        _context.Items.Add(wi);
        _context.SaveChanges();

        var updateWiDTO = new WorkItemUpdateDTO(
            1, "Do Maths", 123, null, new List<string>(), Active 
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

    [Fact]
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
