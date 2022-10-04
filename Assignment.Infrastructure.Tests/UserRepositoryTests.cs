namespace Assignment.Infrastructure.Tests;

using static Response;
public class UserRepositoryTests
{
    private readonly KanbanContext _context;
    private readonly UserRepository _repo;

    public UserRepositoryTests()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<KanbanContext>()
                          .UseSqlite(connection)
                          .Options;
        var context = new KanbanContext(options);

        context.Database.EnsureCreated();

        _context = context;
        _repo = new UserRepository(_context);
    }

    [Fact]
    public void Update_non_existing_User(){
        // Arrange
        var toUpdate = new UserUpdateDTO(20000, "Billy", "Billy@example.com");

        // Act
        var response = _repo.Update(toUpdate);

        // Assert
        response.Should().Be(NotFound);
    }

    [Fact]
    public void Update_existing_User()
    {
        // Arrange
        string name = "Billy";
        string email = "Billy@example.com";

        var newUser = new User(name, email);
        _context.Users.Add(newUser);
        _context.SaveChanges();

        string newName = "Billy Bob";
        var userUpdate = new UserUpdateDTO(1, newName, email);

        // Act
        var response = _repo.Update(userUpdate);

        // Assert
        response.Should().Be(Updated);
    }

    [Fact]
    public void Delete_non_existing_User()
    {
        // Arrange

        // Act
        var response = _repo.Delete(20000);

        // Assert
        response.Should().Be(NotFound);
    }

        [Fact]
    public void Delete_user_assigned_to_workItem_without_force_returns_conflict()
    {
        // Arrange
        var newUser = new User("Billy", "Billy@example.com");
        var wi = new WorkItem("Do Maths");
        wi.AssignedTo = newUser;
        newUser.Items.Add(wi);
        
        _context.Items.Add(wi);
        _context.Users.Add(newUser);
        _context.SaveChanges();

        // Act
        var response = _repo.Delete(newUser.Id);

        // Assert
        response.Should().Be(Conflict);
        _context.Users.Count().Should().Be(1);
    }

    [Fact]
    public void Delete_user_assigned_to_workItem_with_force_returns_deleted()
    {
        // Arrange
        var newUser = new User("Billy", "Billy@example.com");
        var wi = new WorkItem("Do Maths");
        wi.AssignedTo = newUser;
        newUser.Items.Add(wi);
        
        _context.Items.Add(wi);
        _context.Users.Add(newUser);
        _context.SaveChanges();

        // Act
        var response = _repo.Delete(newUser.Id, true);

        // Assert
        response.Should().Be(Deleted);
        _context.Users.Should().BeEmpty();
    }

    [Fact]
    public void Read_users_get_response_null(){
        // Arrange

        // Act
        var userCollection = _repo.Read();

        // Assert
        userCollection.Should().BeEmpty();
    }

    [Fact]
    public void Read_users_get_response_not_null(){

        // Arrange
        string name = "Billey";
        string email = "Billy@example.com";

        User user = new User(name, email);
        WorkItem wi = new WorkItem("Do Maths");
        user.Items.Add(wi);
        wi.AssignedTo = user;
        _context.Users.Add(user);
        _context.Items.Add(wi);
        _context.SaveChanges();

        // Act
        var userColl = _repo.Read();

        // Assert
        userColl.Should().NotBeEmpty();
        userColl.First().Should().Be(new UserDTO(user.Id, name, email));
    }

     [Fact]
    public void Create_User_returns_created()
    {
        // Arrange
        var newUser = new UserCreateDTO("Billy", "Billy@example.com");

        // Act
        var (response, newTagId) = _repo.Create(newUser);

        // Assert
        response.Should().Be(Created);
        newTagId.Should().Be(1);
    }

    [Fact]
    public void Create_User_with_already_existing_email_returns_conflict()
    {
        // Arrange
        var email = "billy@example.com";

        var user = new User("Billy", email);
        _context.Users.Add(user);
        _context.SaveChanges();
        
        var newTagDTO = new UserCreateDTO("Hans", email);

        // Act
        var (response, userId) = _repo.Create(newTagDTO);

        // Assert
        response.Should().Be(Conflict);
    }

}
