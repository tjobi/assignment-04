namespace Assignment.Infrastructure;
using Assignment.Core;

public class UserRepository : IUserRepository
{
    private readonly KanbanContext _context;

    public UserRepository(KanbanContext context) => _context = context;
    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        var newUser = _context.Users.FirstOrDefault(c => c.Email.Equals(user.Email));
        Response response;

        if (newUser is null)
        {
            newUser  = new User(user.Name, user.Email);
            _context.Users.Add(newUser);
            _context.SaveChanges();

            response = Response.Created;
        }
        else
        {
            response = Response.Conflict;
        }

        var userCreated = new UserDTO(newUser.Id, newUser.Name, newUser.Email);
        var userId = userCreated.Id;

        return (response, userId);
    }


    public Response Delete(int userId, bool force = false)
    {
        var user = _context.Users.Find(userId);
        Response response;

        if (user is null) 
        {
            response = Response.NotFound;
        }
        else if(user.Items.Any() && !force)
        {
            response = Response.Conflict;
        }
        else 
        {
            _context.Users.Remove(user);
            _context.SaveChanges();

            response = Response.Deleted;
        }

        return response;
    }

    public UserDTO Find(int userId)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        return (user is not null 
            ? new UserDTO(user.Id, user.Name, user.Email)
            : null)!;
    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        var users = from u in _context.Users
                     orderby u.Id
                     select new UserDTO(u.Id, u.Name, u.Email);

        return users.ToArray();
    }

    public Response Update(UserUpdateDTO user)
    {
        var userUpdate = _context.Users.Find(user.Id);
        Response response;

        if (userUpdate is null) 
        {
            response = Response.NotFound;
        }
        else 
        {
            userUpdate.Name = user.Name;
            userUpdate.Email = user.Email;
            response = Response.Updated;
        }

        return response;

    }
}
