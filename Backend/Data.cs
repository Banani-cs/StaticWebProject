using Microsoft.EntityFrameworkCore;

// This is a modeel cclass, basically what the database will look like
public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Currency { get; set; }

    public int ClickPower { get; set; }

    public int UpgradeCost { get; set; }
}

//Manages the connection to the database and allows us to query it
public class ApplicationDbContext : DbContext //Inheriting from DbContext
{
    //This constructor is used to configure the database connection and other options for the DbContext.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        //Nothing yet
    }

    //Create a table in the database called Users, which will be of type User
    public DbSet<User> Users { get; set; }
}